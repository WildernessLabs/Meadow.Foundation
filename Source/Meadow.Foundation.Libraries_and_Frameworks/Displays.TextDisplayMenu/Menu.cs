using System;
using System.Collections;
using Meadow.Foundation.Displays.TextDisplayMenu.InputTypes;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Buttons;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class Menu
    {
        const string INPUT_TYPES_NAMESPACE = "Meadow.Foundation.Displays.TextDisplayMenu.InputTypes.";
        protected ITextDisplay display;
        protected IButton buttonPrevious;
        protected IButton buttonNext;
        protected IButton buttonSelect;
        protected IRotaryEncoder encoder;

        protected int navigatedDepth;
        protected MenuPage rootMenuPage;
        protected MenuPage currentMenuPage;
        protected int topDisplayLine;

        private Stack _menuLevel = null;
        private bool isEditMode = false;
        private bool _showBackOnRoot = false;

        public event MenuSelectedHandler Selected = delegate { };
        public event ValueChangedHandler ValueChanged = delegate { };
        public event EventHandler Exited = delegate { };

        public bool IsEnabled { get; protected set; } = false;
        
        public Menu(ITextDisplay display, IRotaryEncoderWithButton encoder, byte[] menuResource, bool showBackOnRoot = false)
        {
            _showBackOnRoot = showBackOnRoot;
            Init(display, encoder, null, null, encoder, ParseMenuData(menuResource));
        }

        public Menu(ITextDisplay display, IRotaryEncoder encoder, IButton buttonSelect, byte[] menuResource, bool showBackOnRoot = false)
        {
            _showBackOnRoot = showBackOnRoot;
            Init(display, encoder, null, null, buttonSelect, ParseMenuData(menuResource));
        }
        public Menu(ITextDisplay display, IButton buttonNext, IButton buttonPrevious, IButton buttonSelect, byte[] menuResource, bool showBackOnRoot = false)
        {
            _showBackOnRoot = showBackOnRoot;
            Init(display, null, buttonNext, buttonPrevious, buttonSelect, ParseMenuData(menuResource));
        }

        private MenuPage ParseMenuData(byte[] menuResource)
        {
            var menuJson = new string(System.Text.Encoding.UTF8.GetChars(menuResource));
            var menuData = SimpleJsonSerializer.JsonSerializer.DeserializeString(menuJson) as Hashtable; //from nuget package


        //    Hashtable menuData = null;

            if (menuData["menu"] == null)
            {
                throw new ArgumentException("JSON root must contain a 'menu' item");
            }
            return CreateMenuPage((ArrayList)menuData["menu"], _showBackOnRoot);
        }

        private void Init(ITextDisplay display, IRotaryEncoder encoder, IButton buttonNext, IButton buttonPrevious, IButton buttonSelect, MenuPage menuTree)
        {
            if (encoder != null)
            {
                this.encoder = encoder;
            }
            else if (buttonNext != null && buttonPrevious != null)
            {
                this.buttonPrevious = buttonPrevious;
                this.buttonNext = buttonNext;
            }
            else
            {
                throw new ArgumentNullException("Must have either a Rotary Encoder or Next/Previous buttons");
            }

            this.display = display;
            this.buttonSelect = buttonSelect;
            rootMenuPage = menuTree;
            _menuLevel = new Stack();

            // Save our custom characters
            this.display.SaveCustomCharacter(TextCharacters.RightArrow.CharMap, TextCharacters.RightArrow.MemorySlot);
            this.display.SaveCustomCharacter(TextCharacters.RightArrowSelected.CharMap, TextCharacters.RightArrow.MemorySlot);
            this.display.SaveCustomCharacter(TextCharacters.BoxSelected.CharMap, TextCharacters.BoxSelected.MemorySlot);
        }

        public void Enable()
        {
            this.IsEnabled = true;
            if (encoder != null)
            {
                encoder.Rotated += HandlEncoderRotation;
            }
            else if (buttonNext != null && buttonPrevious != null)
            {
                buttonPrevious.Clicked += HandleButtonPrevious;
                buttonNext.Clicked += HandleButtonNext;
            }
            buttonSelect.Clicked += HandleEncoderClick;

            UpdateCurrentMenuPage();
            RenderCurrentMenuPage();
        }

        public void Disable()
        {
            this.IsEnabled = false;
            if (encoder != null)
            {
                encoder.Rotated -= HandlEncoderRotation;
            }
            else if (buttonNext != null && buttonPrevious != null)
            {
                buttonPrevious.Clicked -= HandleButtonPrevious;
                buttonNext.Clicked -= HandleButtonNext;
            }
            buttonSelect.Clicked -= HandleEncoderClick;
            display.ClearLines();
        }

        protected MenuPage CreateMenuPage(ArrayList nodes, bool addBack)
        {
            MenuPage menuPage = new MenuPage();

            if (addBack)
            {
                menuPage.MenuItems.Add(new MenuItem("< Back"));
            }

            if (nodes != null)
            {
                foreach (Hashtable node in nodes)
                {
                    var item = new MenuItem(node["text"].ToString(), node["command"]?.ToString(), node["id"]?.ToString(), node["type"]?.ToString(), node["value"]?.ToString());
                    if (node["sub"] != null)
                    {
                        item.SubMenu = CreateMenuPage((ArrayList)node["sub"], true);
                    }
                    menuPage.MenuItems.Add(item);
                }
            }
            return menuPage;
        }

        protected void RenderCurrentMenuPage()
        {
            if (!IsEnabled) { return; } 

            // clear the display
            display.ClearLines();

            // if there are no items to render, get out.
            if (currentMenuPage.MenuItems.Count <= 0) return;

            // if the scroll position is above the display area, move the display "window"
            if (currentMenuPage.ScrollPosition < topDisplayLine)
            {
                topDisplayLine = currentMenuPage.ScrollPosition;
            }

            // if the scroll position is below the display area, move the display "window"
            if (currentMenuPage.ScrollPosition > topDisplayLine + display.DisplayConfig.Height - 1)
            {
                topDisplayLine = currentMenuPage.ScrollPosition - display.DisplayConfig.Height + 1;
            }

            Console.WriteLine("Scroll: " + currentMenuPage.ScrollPosition.ToString() + ", start: " + topDisplayLine.ToString() + ", end: " + (topDisplayLine + display.DisplayConfig.Height - 1).ToString());

            byte lineNumber = 0;

            for (int i = topDisplayLine; i <= (topDisplayLine + display.DisplayConfig.Height - 1); i++)
            {
                if (i < currentMenuPage.MenuItems.Count)
                {
                    IMenuItem item = currentMenuPage.MenuItems[i] as IMenuItem;

                    // trim and add selection
                    string lineText = GetItemText(item, (i == currentMenuPage.ScrollPosition));
                    display.WriteLine(lineText, lineNumber);
                    lineNumber++;
                }
            }
        }

        protected string GetItemText(IMenuItem item, bool isSelected)
        {
            string itemText = string.Empty;
            string displayText = item.Text;
            if (InputHelpers.Contains(displayText, "{value}") && item.Value != null)
            {
                displayText = InputHelpers.Replace(displayText, "{value}", item.Value.ToString());
            }

            if (isSelected)
            {
                // calculate any neccessary padding to put selector on far right
                int paddingLength = (display.DisplayConfig.Width - 1 - displayText.Length);
                string padding = string.Empty;
                if (paddingLength > 0) padding = new string(' ', paddingLength);
                //
                itemText = displayText.Substring(0, (displayText.Length >= display.DisplayConfig.Width - 1) ? display.DisplayConfig.Width - 1 : displayText.Length) + padding + TextCharacters.BoxSelected.ToChar();
            }
            else
            {
                itemText = displayText.Substring(0, (displayText.Length >= display.DisplayConfig.Width) ? display.DisplayConfig.Width : displayText.Length);
            }

            return itemText;
        }

        /// <summary>
        /// Updates the currentMenuPage based on the current navigation depth
        /// </summary>
        protected void UpdateCurrentMenuPage()
        {
            if (navigatedDepth == 0) currentMenuPage = rootMenuPage;
            else
            {
                MenuPage page = rootMenuPage;
                for (int i = 0; i < navigatedDepth; i++)
                {
                    page = (page.MenuItems[page.ScrollPosition] as IMenuItem).SubMenu;
                }
                currentMenuPage = page;
            }
        }

        public bool MoveNext()
        {
            // if outside of valid range return false
            if (currentMenuPage.ScrollPosition >= currentMenuPage.MenuItems.Count - 1) return false;

            // increment scroll position
            currentMenuPage.ScrollPosition++;

            // re-render menu
            RenderCurrentMenuPage();

            return true;
        }

        public bool MovePrevious()
        {
            // if outside of valid range return false
            if (currentMenuPage.ScrollPosition <= 0) return false;

            // increment scroll position
            currentMenuPage.ScrollPosition--;

            // re-render menu
            RenderCurrentMenuPage();

            return true;
        }

        public bool SelectCurrentItem()
        {
            if(currentMenuPage.ScrollPosition==0 && _menuLevel.Count == 0 && _showBackOnRoot)
            {
                this.Disable();
                Exited(this, new EventArgs());
                return true;
            }

            // if currently on a subpage and user selects back, pop back to parent page.
            if (currentMenuPage.ScrollPosition == 0 && _menuLevel.Count > 0)
            {
                MenuPage parent = _menuLevel.Pop() as MenuPage;
                currentMenuPage = parent;
                RenderCurrentMenuPage();
                return true;
            }

            int pos = currentMenuPage.ScrollPosition;
            MenuItem child = ((MenuItem)currentMenuPage.MenuItems[pos]);

            // go to the submenu if children are present
            if (child.SubMenu.MenuItems.Count > 0)
            {
                _menuLevel.Push(currentMenuPage);
                currentMenuPage = child.SubMenu;
                RenderCurrentMenuPage();
                return true;
            }
            // if there is a command, notify the subscribers 
            else if (child.Command != string.Empty)
            {
                Selected(this, new MenuSelectedEventArgs(child.Command));
                return true;
            }
            // if there is a type, then let the type handle the input
            else if (child.Type != string.Empty)
            {
                // preserve current menu state and remove handlers
                if (encoder != null)
                {
                    encoder.Rotated -= HandlEncoderRotation;
                }
                else
                {
                    buttonPrevious.Clicked -= HandleButtonPrevious;
                    buttonNext.Clicked -= HandleButtonNext;
                }
                buttonSelect.Clicked -= HandleEncoderClick;

                _menuLevel.Push(currentMenuPage);
                isEditMode = true;

                // create the new input type
                var type = Type.GetType(INPUT_TYPES_NAMESPACE + child.Type);
                if (type == null)
                {
                    throw new ArgumentException(child.Type + " was not found");
                }

                var constructor = type.GetConstructor(new Type[] { });
                IMenuInputItem inputItem = constructor.Invoke(new object[] { }) as IMenuInputItem;

                // setup callback
                inputItem.ValueChanged += delegate (object sender, ValueChangedEventArgs e)
                {
                    // set the value and notify the eager listeners
                    child.Value = e.Value;
                    ValueChanged(this, new ValueChangedEventArgs(e.ItemID, e.Value));

                    // setup handlers again
                    if(encoder != null)
                    {
                        encoder.Rotated += HandlEncoderRotation;
                    }
                    else
                    {
                        buttonPrevious.Clicked += HandleButtonPrevious;
                        buttonNext.Clicked += HandleButtonNext;
                    }
                    buttonSelect.Clicked += HandleEncoderClick;

                    // reload the parent menu
                    MenuPage parent = _menuLevel.Pop() as MenuPage;
                    currentMenuPage = parent;
                    RenderCurrentMenuPage();
                    isEditMode = false;
                };

                // initialize input mode and get new value
                if(encoder != null)
                {
                    inputItem.Init(display, encoder, buttonSelect);
                }
                else
                {
                    inputItem.Init(display, buttonNext, buttonPrevious, buttonSelect);
                }
                
                inputItem.GetInput(child.ItemID, child.Value);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateItemValue(string id, object value)
        {
            Hashtable values = new Hashtable(1);
            values[id] = value;
            UpdateItemValue(values);
        }

        public void UpdateItemValue(Hashtable values)
        {
            foreach(DictionaryEntry item in values)
            {
                UpdateMenuItemValue(item.Key.ToString(), item.Value);
            }
            if (!isEditMode)
            {
                RenderCurrentMenuPage();
            }
        }

        private void UpdateMenuItemValue(string id, object value)
        {
            MenuItem node = null;
            foreach (var menuItem in rootMenuPage.MenuItems)
            {
                node = FindNodeById(menuItem as MenuItem, id);
                if (node != null) { break; }
            }

            if (node != null)
            {
                node.Value = value;
            }
            else
            {
                throw new ArgumentNullException("Item with id: " + id + " does not exist");
            }
        }

        private MenuItem FindNodeById(MenuItem menuItem, string id)
        {
            if (menuItem.ItemID == id)
            {
                return menuItem;
            }
            else if (menuItem.SubMenu.MenuItems.Count > 0)
            {
                foreach (var subMenuItem in menuItem.SubMenu.MenuItems)
                {
                    var node = FindNodeById(subMenuItem as MenuItem, id);
                    if (node != null) return node;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        private void HandleEncoderClick(object sender, EventArgs e)
        {
            SelectCurrentItem();
        }

        private void HandlEncoderRotation(object sender, RotaryTurnedEventArgs e)
        {
            bool moved;

            if (e.Direction == RotationDirection.Clockwise)
            {
                moved = MoveNext();
            }
            else
            {
                moved = MovePrevious();
            }

            if (!moved)
            {
                Console.WriteLine("end of items");
            }
        }

        private void HandleButtonPrevious(object sender, EventArgs e)
        {
            MovePrevious();
        }

        private void HandleButtonNext(object sender, EventArgs e)
        {
            MoveNext();
        }

        public byte Max(byte a, byte b)
        {
            return (a > b) ? a : b;
        }
    }
}