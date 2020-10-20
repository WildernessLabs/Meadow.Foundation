using System;
using System.Collections;
using Meadow.Foundation.Displays.TextDisplayMenu.InputTypes;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class Menu
    {
        const string INPUT_TYPES_NAMESPACE = "Meadow.Foundation.Displays.TextDisplayMenu.InputTypes.";
        protected ITextDisplay display;

        protected int navigatedDepth;
        protected MenuPage rootMenuPage;
        protected MenuPage currentMenuPage;
        protected int topDisplayLine;

        private Stack menuLevel = null;
        private bool isEditMode = false;
        private bool showBackOnRoot = false;

        public event MenuSelectedHandler Selected = delegate { };
        public event ValueChangedHandler ValueChanged = delegate { };
        public event EventHandler Exited = delegate { };

        public bool IsEnabled { get; protected set; } = false;

        public Menu(ITextDisplay display, byte[] menuResource, bool showBackOnRoot = false)
        {
            this.showBackOnRoot = showBackOnRoot;
            Init(display, ParseMenuData(menuResource));
        }

        private MenuPage ParseMenuData(byte[] menuResource)
        {
            var menuJson = new string(System.Text.Encoding.UTF8.GetChars(menuResource));
            var menuData = SimpleJsonSerializer.JsonSerializer.DeserializeString(menuJson) as Hashtable; //from nuget package

            if (menuData["menu"] == null)
            {
                throw new ArgumentException("JSON root must contain a 'menu' item");
            }
            return CreateMenuPage((ArrayList)menuData["menu"], showBackOnRoot);
        }

        private void Init(ITextDisplay display, MenuPage menuTree)
        {
            this.display = display;

            rootMenuPage = menuTree;
            menuLevel = new Stack();

            // Save our custom characters
            // ToDo
            display.SaveCustomCharacter(TextCharacters.RightArrow.CharMap, TextCharacters.RightArrow.MemorySlot);
            display.SaveCustomCharacter(TextCharacters.RightArrowSelected.CharMap, TextCharacters.RightArrow.MemorySlot);
            display.SaveCustomCharacter(TextCharacters.BoxSelected.CharMap, TextCharacters.BoxSelected.MemorySlot);
        }

        public void Enable()
        {
            IsEnabled = true;

            UpdateCurrentMenuPage();
            RenderCurrentPage();
        }

        public void Disable()
        {
            this.IsEnabled = false;

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

        protected void RenderCurrentPage()
        {
            if (!IsEnabled) { return; }

            Console.WriteLine("ClearLines");
            // clear the display
            display.ClearLines();

            Console.WriteLine($"Count: {currentMenuPage.MenuItems.Count}");

            // if there are no items to render, get out.
            if (currentMenuPage.MenuItems.Count <= 0) { return; }

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
            string itemText;
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
            if (navigatedDepth == 0) { currentMenuPage = rootMenuPage; }
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

        public bool OnNext()
        {
            // if outside of valid range return false
            if (currentMenuPage.ScrollPosition >= currentMenuPage.MenuItems.Count - 1)
            {
                return false;
            }

            // increment scroll position
            currentMenuPage.ScrollPosition++;

            // re-render menu
            RenderCurrentPage();

            return true;
        }

        public bool OnPrevious()
        {
            // if outside of valid range return false
            if (currentMenuPage.ScrollPosition <= 0) { return false; }

            // increment scroll position
            currentMenuPage.ScrollPosition--;

            // re-render menu
            RenderCurrentPage();

            return true;
        }

        public bool OnSelect()
        {
            if (currentMenuPage.ScrollPosition == 0 && menuLevel.Count == 0 && showBackOnRoot)
            {
                Disable();
                Exited(this, new EventArgs());
                return true;
            }

            // if currently on a subpage and user selects back, pop back to parent page.
            if (currentMenuPage.ScrollPosition == 0 && menuLevel.Count > 0)
            {
                MenuPage parent = menuLevel.Pop() as MenuPage;
                currentMenuPage = parent;
                RenderCurrentPage();
                return true;
            }

            int pos = currentMenuPage.ScrollPosition;
            MenuItem child = ((MenuItem)currentMenuPage.MenuItems[pos]);

            // go to the submenu if children are present
            if (child.SubMenu.MenuItems.Count > 0)
            {
                menuLevel.Push(currentMenuPage);
                currentMenuPage = child.SubMenu;
                RenderCurrentPage();
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
                menuLevel.Push(currentMenuPage);
                isEditMode = true;

                // create the new input type
                var type = Type.GetType(INPUT_TYPES_NAMESPACE + child.Type);

                if (type == null)
                {
                    throw new ArgumentException(child.Type + " was not found");
                }

                var constructor = type.GetConstructor(new Type[] { });
                var inputItem = constructor.Invoke(new object[] { }) as IMenuInputItem;

                // setup callback
                inputItem.ValueChanged += delegate (object sender, ValueChangedEventArgs e)
                {
                    // set the value and notify the eager listeners
                    child.Value = e.Value;
                    ValueChanged(this, new ValueChangedEventArgs(e.ItemID, e.Value));

                    // reload the parent menu
                    var parent = menuLevel.Pop() as MenuPage;
                    currentMenuPage = parent;
                    RenderCurrentPage();
                    isEditMode = false;
                };

                // initialize input mode and get new value
                inputItem.Init(display);

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
            foreach (DictionaryEntry item in values)
            {
                UpdateMenuItemValue(item.Key.ToString(), item.Value);
            }
            if (!isEditMode)
            {
                RenderCurrentPage();
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
    }
}