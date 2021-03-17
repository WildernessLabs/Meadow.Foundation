using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
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
        protected IMenuInputItem currentInputItem; //also effectively a "page"
        protected int topDisplayLine;

        private Stack<IPage> pageStack = null;
        private bool isEditMode = false;
        private bool showBackOnRoot = false;

        public event MenuSelectedHandler Selected = delegate { };
        public event ValueChangedHandler ValueChanged = delegate { };
        public event EventHandler Exited = delegate { };

        public bool IsEnabled { get; protected set; } = false;

        public Menu(ITextDisplay display, byte[] menuJson, bool showBackOnRoot = false)
        {
            this.showBackOnRoot = showBackOnRoot;
            var items = ParseMenuData(menuJson);
            Init(display, CreateMenuPage(items, showBackOnRoot));
        }

        public Menu(ITextDisplay display, MenuItem[] menuItems, bool showBackOnRoot = false)
        {
            Init(display, CreateMenuPage(menuItems, showBackOnRoot));
        }

        private MenuItem[] ParseMenuData(byte[] menuJson)
        {
            return JsonSerializer.Deserialize<MenuItem[]>(menuJson);
        }

        private void Init(ITextDisplay display, MenuPage menuPage)
        {
            //good here
            this.display = display;

            rootMenuPage = menuPage;

            pageStack = new Stack<IPage>();

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

            ShowCurrentPage();
        }

        public void Disable()
        {
            IsEnabled = false;

            display.ClearLines();
            display.Show();
        }

        protected MenuPage CreateMenuPage(MenuItem[] items, bool addBack)
        {
            var menuPage = new MenuPage();

            if (addBack)
            {
                menuPage.MenuItems.Add(new MenuItem("< Back"));
            }

            foreach (var item in items)
            {
                menuPage.MenuItems.Add(item);
            } 

            return menuPage;
        }

        protected void ShowCurrentPage()
        {
            if (!IsEnabled) {
                Console.WriteLine("Render not enabled");
                return;
            }

            // clear the display
            display.ClearLines();

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

            byte lineNumber = 0;

            MenuItem item;

            for (int i = topDisplayLine; i <= (topDisplayLine + display.DisplayConfig.Height - 1); i++)
            {
                if (i < currentMenuPage.MenuItems.Count)
                {
                    item = currentMenuPage.MenuItems[i];

                    // trim and add selection
                    string lineText = GetItemText(item, (i == currentMenuPage.ScrollPosition));
                    display.WriteLine(lineText, lineNumber);
                    lineNumber++;
                }
            }

            display.Show();
        }

        protected string GetItemText(MenuItem item, bool isSelected)
        {
            if(item == null)
            {
                Console.WriteLine("GetItemText: item is null");
                return "no item";
            }

            string itemText;
            string displayText = item.Text;

            if (InputHelpers.Contains(displayText, "{value}"))
            {
                if (item.Value != null)
                {
                    displayText = InputHelpers.Replace(displayText, "{value}", item.Value.ToString());
                }
                else
                {
                    displayText = InputHelpers.Replace(displayText, "{value}", string.Empty);
                }
            }

            itemText = displayText.Substring(0, (displayText.Length >= display.DisplayConfig.Width - 1) ? display.DisplayConfig.Width - 1 : displayText.Length);

            if (isSelected || item.HasSubItems)
            {
                // calculate any neccessary padding to put selector on far right
                int paddingLength = (display.DisplayConfig.Width - 1 - displayText.Length);
                string padding = string.Empty;
                if (paddingLength > 0) { padding = new string(' ', paddingLength); }

                itemText += padding + (isSelected?"*":">");
            }

            return itemText;
        }

        /// <summary>
        /// Updates the currentMenuPage based on the current navigation depth
        /// </summary>
        protected void UpdateCurrentMenuPage()
        {
            currentMenuPage = rootMenuPage;

            /*if (navigatedDepth == 0) { currentMenuPage = rootMenuPage; }
            else
            {
                MenuPage page = rootMenuPage;
              //  for (int i = 0; i < navigatedDepth; i++)
              //  {
              //      page = (page.MenuItems[page.ScrollPosition] as IMenuItem).SubMenu;
              //  }
                currentMenuPage = page; */
        } 

        public bool Next()
        {
            if(IsEnabled == false) { return false; }

            if(currentInputItem != null)
            {
                currentInputItem.Next();
            }
            else
            {
                currentMenuPage.Next();
                // re-render menu
                ShowCurrentPage();
            }

            return true;
        }

        public bool Previous()
        {
            if (IsEnabled == false) { return false; }

            if (currentInputItem != null)
            {
                currentInputItem.Previous();
            }
            else
            {
                currentMenuPage.Previous();
                // re-render menu
                ShowCurrentPage();
            }

            return true;
        }

        public bool Back()
        {
            if (IsEnabled == false) { return false; }

            if (currentInputItem != null)
            {
                currentInputItem.Select();
                return true;
            }
            else if (pageStack.Count == 0)
            {
                Disable();
                Exited(this, new EventArgs());
                return true;
            }
            // if currently on a subpage and user selects back, pop back to parent page.
            else if (pageStack.Count > 0)
            {
                MenuPage parent = pageStack.Pop() as MenuPage;
                currentMenuPage = parent;
                ShowCurrentPage();
                return true;
            }

            return false;
        }

        public bool Select()
        {
            if (currentInputItem != null)
            {
                currentInputItem.Select();
                return true;
            }
            else
            {
                currentMenuPage.Select();
            }

            if (currentMenuPage.ScrollPosition == 0 && pageStack.Count == 0 && showBackOnRoot)
            {
                Disable();
                Exited(this, new EventArgs());
                return true;
            }
            
            // if currently on a subpage and user selects back, pop back to parent page.
            if (currentMenuPage.ScrollPosition == 0 && pageStack.Count > 0)
            {
                MenuPage parent = pageStack.Pop() as MenuPage;
                currentMenuPage = parent;
                ShowCurrentPage();
                return true;
            }

            int pos = currentMenuPage.ScrollPosition;
            MenuItem menuItem = currentMenuPage.MenuItems[pos];

            // go to the submenu if children are present
            if (menuItem.HasSubItems)
            {
                pageStack.Push(currentMenuPage);
                // currentMenuPage = child.SubMenu;
                currentMenuPage = CreateMenuPage(menuItem.SubItems, true);
                ShowCurrentPage();
                return true;
            }
            // if there is a command, notify the subscribers 
            else if (menuItem.Command != string.Empty)
            {
                Selected(this, new MenuSelectedEventArgs(menuItem.Command));
                return true;
            }
            // if there is a type, then let the type handle the input
            else if (menuItem.Type != string.Empty)
            {
                pageStack.Push(currentMenuPage);
                
                isEditMode = true;

                // create the new input type
                var type = Type.GetType(INPUT_TYPES_NAMESPACE + menuItem.Type);

                if (type == null)
                {
                    throw new ArgumentException(menuItem.Type + " was not found");
                }

                var constructor = type.GetConstructor(new Type[] { });
                currentInputItem = constructor.Invoke(new object[] { }) as IMenuInputItem;

                // setup callback
                currentInputItem.ValueChanged += delegate (object sender, ValueChangedEventArgs e)
                {
                    // set the value and notify the eager listeners
                    menuItem.Value = e.Value;
                    ValueChanged(this, new ValueChangedEventArgs(e.ItemID, e.Value));

                    // reload the parent menu
                    var parent = pageStack.Pop() as MenuPage;
                    currentMenuPage = parent;
                    currentInputItem = null;
                    ShowCurrentPage();
                    isEditMode = false;
                };

                // initialize input mode and get new value
                currentInputItem.Init(display);

                currentInputItem.GetInput(menuItem.Id, menuItem.Value);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Refresh()
        {
            ShowCurrentPage();
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
                ShowCurrentPage();
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
            if (menuItem.Id == id)
            {
                return menuItem;
            }
            else if (menuItem.SubItems.Length > 0)
            {
                foreach (var subMenuItem in menuItem.SubItems)
                {
                    var node = FindNodeById(subMenuItem as MenuItem, id);
                    if (node != null) return node;
                }
            }
            return null;
        }
    }
}