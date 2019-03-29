using System;
using System.Collections;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Foundation.Displays.TextDisplayMenu.InputTypes;
using Meadow.Foundation.Sensors.Buttons;
using System.Diagnostics;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Buttons;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class Menu
    {
        const string INPUT_TYPES_NAMESPACE = "Meadow.Foundation.Displays.TextDisplayMenu.InputTypes.";
        protected ITextDisplay _display = null;
        protected IButton _buttonPrevious = null;
        protected IButton _buttonNext = null;
        protected IButton _buttonSelect = null;
        protected IRotaryEncoder _encoder = null;

        protected int _navigatedDepth = 0;
        protected MenuPage _rootMenuPage = null;
        protected MenuPage _currentMenuPage = null;
        protected int _topDisplayLine = 0;

        private Stack _menuLevel = null;
        private bool _isEditMode = false;
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
            //Port: TODO var menuData = Json.NETMF.JsonSerializer.DeserializeString(menuJson) as Hashtable;
            Hashtable menuData = null;

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
                _encoder = encoder;
            }
            else if (buttonNext != null && buttonPrevious != null)
            {
                _buttonPrevious = buttonPrevious;
                _buttonNext = buttonNext;
            }
            else
            {
                throw new ArgumentNullException("Must have either a Rotary Encoder or Next/Previous buttons");
            }

            _display = display;
            _buttonSelect = buttonSelect;
            _rootMenuPage = menuTree;
            _menuLevel = new Stack();

            // Save our custom characters
            _display.SaveCustomCharacter(TextCharacters.RightArrow.CharMap, TextCharacters.RightArrow.MemorySlot);
            _display.SaveCustomCharacter(TextCharacters.RightArrowSelected.CharMap, TextCharacters.RightArrow.MemorySlot);
            _display.SaveCustomCharacter(TextCharacters.BoxSelected.CharMap, TextCharacters.BoxSelected.MemorySlot);
        }

        public void Enable()
        {
            this.IsEnabled = true;
            if (_encoder != null)
            {
                _encoder.Rotated += HandlEncoderRotation;
            }
            else if (_buttonNext != null && _buttonPrevious != null)
            {
                _buttonPrevious.Clicked += HandleButtonPrevious;
                _buttonNext.Clicked += HandleButtonNext;
            }
            _buttonSelect.Clicked += HandleEncoderClick;

            UpdatedCurrentMenuPage();
            RenderCurrentMenuPage();
        }

        public void Disable()
        {
            this.IsEnabled = false;
            if (_encoder != null)
            {
                _encoder.Rotated -= HandlEncoderRotation;
            }
            else if (_buttonNext != null && _buttonPrevious != null)
            {
                _buttonPrevious.Clicked -= HandleButtonPrevious;
                _buttonNext.Clicked -= HandleButtonNext;
            }
            _buttonSelect.Clicked -= HandleEncoderClick;
            _display.Clear();
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
            if (!IsEnabled) return;

            // clear the display
            _display.Clear();

            // if there are no items to render, get out.
            if (_currentMenuPage.MenuItems.Count <= 0) return;

            // if the scroll position is above the display area, move the display "window"
            if (_currentMenuPage.ScrollPosition < _topDisplayLine)
            {
                _topDisplayLine = _currentMenuPage.ScrollPosition;
            }

            // if the scroll position is below the display area, move the display "window"
            if (_currentMenuPage.ScrollPosition > _topDisplayLine + _display.DisplayConfig.Height - 1)
            {
                _topDisplayLine = _currentMenuPage.ScrollPosition - _display.DisplayConfig.Height + 1;
            }

            Console.WriteLine("Scroll: " + _currentMenuPage.ScrollPosition.ToString() + ", start: " + _topDisplayLine.ToString() + ", end: " + (_topDisplayLine + _display.DisplayConfig.Height - 1).ToString());

            byte lineNumber = 0;

            for (int i = _topDisplayLine; i <= (_topDisplayLine + _display.DisplayConfig.Height - 1); i++)
            {
                if (i < _currentMenuPage.MenuItems.Count)
                {
                    IMenuItem item = _currentMenuPage.MenuItems[i] as IMenuItem;

                    // trim and add selection
                    string lineText = GetItemText(item, (i == _currentMenuPage.ScrollPosition));
                    _display.WriteLine(lineText, lineNumber);
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
                int paddingLength = (_display.DisplayConfig.Width - 1 - displayText.Length);
                string padding = string.Empty;
                if (paddingLength > 0) padding = new string(' ', paddingLength);
                //
                itemText = displayText.Substring(0, (displayText.Length >= _display.DisplayConfig.Width - 1) ? _display.DisplayConfig.Width - 1 : displayText.Length) + padding + TextCharacters.BoxSelected.ToChar();
            }
            else
            {
                itemText = displayText.Substring(0, (displayText.Length >= _display.DisplayConfig.Width) ? _display.DisplayConfig.Width : displayText.Length);
            }

            return itemText;
        }

        /// <summary>
        /// Updates the _currentMenuPage based on the current navigation depth
        /// </summary>
        protected void UpdatedCurrentMenuPage()
        {
            if (_navigatedDepth == 0) _currentMenuPage = _rootMenuPage;
            else
            {
                MenuPage page = _rootMenuPage;
                for (int i = 0; i < _navigatedDepth; i++)
                {
                    page = (page.MenuItems[page.ScrollPosition] as IMenuItem).SubMenu;
                }
                _currentMenuPage = page;
            }
        }

        public bool MoveNext()
        {
            // if outside of valid range return false
            if (_currentMenuPage.ScrollPosition >= _currentMenuPage.MenuItems.Count - 1) return false;

            // increment scroll position
            _currentMenuPage.ScrollPosition++;

            // re-render menu
            RenderCurrentMenuPage();

            return true;
        }

        public bool MovePrevious()
        {
            // if outside of valid range return false
            if (_currentMenuPage.ScrollPosition <= 0) return false;

            // increment scroll position
            _currentMenuPage.ScrollPosition--;

            // re-render menu
            RenderCurrentMenuPage();

            return true;
        }

        public bool SelectCurrentItem()
        {
            if(_currentMenuPage.ScrollPosition==0 && _menuLevel.Count == 0 && _showBackOnRoot)
            {
                this.Disable();
                Exited(this, new EventArgs());
                return true;
            }

            // if currently on a subpage and user selects back, pop back to parent page.
            if (_currentMenuPage.ScrollPosition == 0 && _menuLevel.Count > 0)
            {
                MenuPage parent = _menuLevel.Pop() as MenuPage;
                _currentMenuPage = parent;
                RenderCurrentMenuPage();
                return true;
            }

            int pos = _currentMenuPage.ScrollPosition;
            MenuItem child = ((MenuItem)_currentMenuPage.MenuItems[pos]);

            // go to the submenu if children are present
            if (child.SubMenu.MenuItems.Count > 0)
            {
                _menuLevel.Push(_currentMenuPage);
                _currentMenuPage = child.SubMenu;
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
                if (_encoder != null)
                {
                    _encoder.Rotated -= HandlEncoderRotation;
                }
                else
                {
                    _buttonPrevious.Clicked -= HandleButtonPrevious;
                    _buttonNext.Clicked -= HandleButtonNext;
                }
                _buttonSelect.Clicked -= HandleEncoderClick;

                _menuLevel.Push(_currentMenuPage);
                _isEditMode = true;

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
                    if(_encoder != null)
                    {
                        _encoder.Rotated += HandlEncoderRotation;
                    }
                    else
                    {
                        _buttonPrevious.Clicked += HandleButtonPrevious;
                        _buttonNext.Clicked += HandleButtonNext;
                    }
                    _buttonSelect.Clicked += HandleEncoderClick;

                    // reload the parent menu
                    MenuPage parent = _menuLevel.Pop() as MenuPage;
                    _currentMenuPage = parent;
                    RenderCurrentMenuPage();
                    _isEditMode = false;
                };

                // initialize input mode and get new value
                if(_encoder != null)
                {
                    inputItem.Init(_display, _encoder, _buttonSelect);
                }
                else
                {
                    inputItem.Init(_display, _buttonNext, _buttonPrevious, _buttonSelect);
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
            if (!_isEditMode)
            {
                RenderCurrentMenuPage();
            }
        }

        private void UpdateMenuItemValue(string id, object value)
        {
            MenuItem node = null;
            foreach (var menuItem in _rootMenuPage.MenuItems)
            {
                node = FindNodeById(menuItem as MenuItem, id);
                if (node != null) break;
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
            this.SelectCurrentItem();
        }

        private void HandlEncoderRotation(object sender, RotaryTurnedEventArgs e)
        {
            bool moved = false;
            if (e.Direction == RotationDirection.Clockwise)
            {
                moved = this.MoveNext();
            }
            else
            {
                moved = this.MovePrevious();
            }

            if (!moved)
            {
                // play a sound?
                Console.WriteLine("end of items");
            }
        }

        private void HandleButtonPrevious(object sender, EventArgs e)
        {
            this.MovePrevious();
        }

        private void HandleButtonNext(object sender, EventArgs e)
        {
            this.MoveNext();
        }

        public byte Max(byte a, byte b)
        {
            if (a > b) return a;
            else return b;
        }
    }


}
