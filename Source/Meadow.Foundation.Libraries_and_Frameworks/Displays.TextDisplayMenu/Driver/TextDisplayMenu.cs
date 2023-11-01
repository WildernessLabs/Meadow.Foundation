using Meadow.Foundation.Displays.UI.InputTypes;
using Meadow.Peripherals.Displays;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace Meadow.Foundation.Displays.UI
{
    /// <summary>
    /// TextDisplayMenu TextDisplayMenu class
    /// </summary>
    public class TextDisplayMenu
    {
        ITextDisplay? display;

        MenuPage? rootMenuPage;
        MenuPage? currentMenuPage;
        IMenuInputItem? currentInputItem; //also effectively a "page"
        int topDisplayLine;

        Stack<IPage>? pageStack;
        bool isEditMode = false;
        readonly bool showBackOnRoot = false;

        /// <summary>
        /// Raised when the menu receives a selected input
        /// </summary>
        public event MenuSelectedHandler Selected = default!;

        /// <summary>
        /// Raised when a value changes
        /// </summary>
        public event ValueChangedHandler ValueChanged = default!;

        /// <summary>
        /// Raised when the user exits the menu
        /// </summary>
        public event EventHandler? Exited = null;

        /// <summary>
        /// Is the menu enabled
        /// </summary>
        public bool IsEnabled { get; protected set; } = false;

        /// <summary>
        /// Create a new TextDisplayMenu object
        /// </summary>
        /// <param name="display">The display to render the menu</param>
        /// <param name="menuJson">Json to define the menu structure</param>
        /// <param name="showBackOnRoot">True to show Back item on root menu</param>
        public TextDisplayMenu(ITextDisplay display, byte[] menuJson, bool showBackOnRoot = false)
        {
            this.showBackOnRoot = showBackOnRoot;
            var items = ParseMenuData(menuJson);

            Init(display, CreateMenuPage(items, showBackOnRoot));
        }

        /// <summary>
        /// Create a new TextDisplayMenu object
        /// </summary>
        /// <param name="display">The display to render the menu</param>
        /// <param name="menuItems">TextDisplayMenu items array</param>
        /// <param name="showBackOnRoot">True to show Back item on root menu</param>
        public TextDisplayMenu(ITextDisplay display, MenuItem[] menuItems, bool showBackOnRoot = false)
        {
            this.showBackOnRoot = showBackOnRoot;
            Init(display, CreateMenuPage(menuItems, showBackOnRoot));
        }

        MenuItem[]? ParseMenuData(byte[] menuJson)
        {
            var menuString = System.Text.Encoding.Default.GetString(menuJson);

            return JsonSerializer.Deserialize<MenuItem[]>(menuString);
        }

        void Init(ITextDisplay display, MenuPage menuPage)
        {
            this.display = display;

            rootMenuPage = menuPage;

            pageStack = new Stack<IPage>();
        }

        /// <summary>
        /// Enable the menu
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;

            UpdateCurrentMenuPage();

            ShowCurrentPage();
        }

        /// <summary>
        /// Disable the menu
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;

            display?.ClearLines();
            display?.Show();
        }

        /// <summary>
        /// Create a TextDisplayMenu page
        /// </summary>
        /// <param name="items">Items to populate page</param>
        /// <param name="addBack">True to add a back item</param>
        /// <returns>The new MenuPage object</returns>
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

        /// <summary>
        /// Show the current page
        /// </summary>
        protected void ShowCurrentPage()
        {
            if (!IsEnabled)
            {
                Resolver.Log.Warn("Render not enabled");
                return;
            }

            display?.ClearLines();

            // if there are no items to render, get out.
            if (currentMenuPage?.MenuItems.Count <= 0) { return; }

            // if the scroll position is above the display area, move the display "window"
            if (currentMenuPage?.ScrollPosition < topDisplayLine)
            {
                topDisplayLine = currentMenuPage.ScrollPosition;
            }

            // if the scroll position is below the display area, move the display "window"
            if (display != null && currentMenuPage?.ScrollPosition > topDisplayLine + display.DisplayConfig.Height - 1)
            {
                topDisplayLine = currentMenuPage.ScrollPosition - display.DisplayConfig.Height + 1;
            }

            byte lineNumber = 0;

            MenuItem item;

            for (int i = topDisplayLine; i <= (topDisplayLine + display?.DisplayConfig.Height - 1); i++)
            {
                if (i < currentMenuPage?.MenuItems.Count)
                {
                    item = currentMenuPage.MenuItems[i];

                    // trim and add selection
                    string lineText = GetItemText(item, (i == currentMenuPage.ScrollPosition));
                    display?.WriteLine(lineText, lineNumber);
                    lineNumber++;
                }
            }

            display?.Show();
        }

        /// <summary>
        /// Get the text for a menu item
        /// </summary>
        /// <param name="item">The menu item</param>
        /// <param name="isSelected">Is the menu selected</param>
        /// <returns>The item text</returns>
        protected string GetItemText(MenuItem item, bool isSelected)
        {
            if (item == null)
            {
                Resolver.Log.Warn("GetItemText: item is null");
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
                // calculate any necessary padding to put selector on far right
                int paddingLength = (display.DisplayConfig.Width / display.DisplayConfig.FontScale - 1 - displayText.Length);
                string padding = string.Empty;
                if (paddingLength > 0) { padding = new string(' ', paddingLength); }

                itemText += padding + (isSelected ? "*" : ">");
            }

            return itemText;
        }

        /// <summary>
        /// Updates the currentMenuPage based on the current navigation depth
        /// </summary>
        protected void UpdateCurrentMenuPage()
        {
            currentMenuPage = rootMenuPage;
        }

        /// <summary>
        /// Next input - navigates down/forward in the list of items
        /// </summary>
        /// <returns>True if successful, false in menu is disabled</returns>
        public bool Next()
        {
            if (IsEnabled == false) { return false; }

            if (currentInputItem != null)
            {
                currentInputItem.Next();
            }
            else
            {
                currentMenuPage?.Next();
                // re-render menu
                ShowCurrentPage();
            }

            return true;
        }

        /// <summary>
        /// Previous input - navigates up/back in the list of items
        /// </summary>
        /// <returns>True if successful, false in menu is disabled</returns>
        public bool Previous()
        {
            if (IsEnabled == false) { return false; }

            if (currentInputItem != null)
            {
                currentInputItem.Previous();
            }
            else
            {
                currentMenuPage?.Previous();
                // re-render menu
                ShowCurrentPage();
            }

            return true;
        }

        /// <summary>
        /// Back input - navigates back in the navigation stack
        /// </summary>
        /// <returns>True if successful, false in menu is disabled</returns>
        public bool Back()
        {
            if (IsEnabled == false) { return false; }

            if (currentInputItem != null)
            {
                currentInputItem.Back();
                return true;
            }
            else if (pageStack.Count > 0)
            {
                MenuPage parent = pageStack.Pop() as MenuPage;
                currentMenuPage = parent;
                ShowCurrentPage();
                return true;
            }
            else if (pageStack?.Count == 0 && Exited != null)
            {
                Disable();
                Exited?.Invoke(this, new EventArgs());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Select input - selects the current item
        /// </summary>
        /// <returns>True if successful, false in menu is disabled</returns>
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

            if (currentMenuPage.ScrollPosition == 0
                && pageStack.Count == 0
                && showBackOnRoot
                && Exited != null)
            {
                Disable();
                Exited?.Invoke(this, new EventArgs());
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
                Selected?.Invoke(this, new MenuSelectedEventArgs(menuItem.Command));
                return true;
            }
            // if there is a type, then let the type handle the input
            else if (menuItem.Type != string.Empty)
            {
                pageStack.Push(currentMenuPage);

                isEditMode = true;

                currentInputItem = GetMenuInputItemFromName(menuItem.Type);

                // setup callback
                currentInputItem.ValueChanged += delegate (object sender, ValueChangedEventArgs e)
                {
                    // set the value and notify the eager listeners
                    menuItem.Value = e.Value;
                    ValueChanged?.Invoke(this, new ValueChangedEventArgs(menuItem.Id, menuItem.Value));

                    // reload the parent menu
                    var parent = pageStack.Pop() as MenuPage;
                    currentMenuPage = parent;
                    currentInputItem = null;
                    ShowCurrentPage();
                    isEditMode = false;
                };

                // initialize input mode and get new value
                if (display != null)
                {
                    currentInputItem.Init(display);
                }

                currentInputItem.GetInput(menuItem.Id, menuItem.Value);
                return true;
            }
            else
            {
                return false;
            }
        }

        enum DaysOfWeek { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };

        IMenuInputItem? GetMenuInputItemFromName(string name)
        {
            if (Enum.TryParse(name, out InputType inputType) == false)
            {
                throw new ArgumentException(name + " was not found");
            }

            return inputType switch
            {
                InputType.Age => new Age(),
                InputType.Boolean => new InputTypes.Boolean(),
                InputType.Date => new Date(),
                InputType.Numerical => new Numerical(),
                InputType.OnOff => new OnOff(),
                InputType.Temperature => new Temperature(),
                InputType.Time => new Time(),
                InputType.TimeDetailed => new TimeDetailed(),
                InputType.TimeShort => new TimeShort(),
                _ => null,
            };
        }

        /// <summary>
        /// Refresh / redraw the menu
        /// </summary>
        public void Refresh()
        {
            ShowCurrentPage();
        }

        /// <summary>
        /// Update menu item value
        /// </summary>
        /// <param name="id">Item id</param>
        /// <param name="value">New value</param>
        public void UpdateItemValue(string id, object value)
        {
            var values = new Hashtable(1)
            {
                [id] = value
            };
            UpdateItemValue(values);
        }

        /// <summary>
        /// Update menu item value
        /// </summary>
        /// <param name="values">Values for all items</param>
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
            MenuItem? node = null;
            foreach (var menuItem in rootMenuPage.MenuItems)
            {
                node = FindNodeById(menuItem, id);
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

        private MenuItem? FindNodeById(MenuItem menuItem, string id)
        {
            if (menuItem.Id == id)
            {
                return menuItem;
            }
            else if (menuItem.SubItems.Length > 0)
            {
                foreach (var subMenuItem in menuItem.SubItems)
                {
                    var node = FindNodeById(subMenuItem, id);
                    if (node != null) return node;
                }
            }
            return null;
        }
    }
}