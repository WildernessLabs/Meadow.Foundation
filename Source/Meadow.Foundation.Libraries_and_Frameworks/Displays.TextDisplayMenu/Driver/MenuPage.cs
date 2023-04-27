using System.Collections.Generic;

namespace Meadow.Foundation.Displays.UI
{
    /// <summary>
    /// Text display MenuPage class
    /// </summary>
    public class MenuPage : IPage
    {
        /// <summary>
        /// The current scroll position
        /// </summary>
        public int ScrollPosition
        {
            get => scrollPosition;
            set
            {
                if (value > MenuItems.Count - 1 || value < 0)
                {
                    Resolver.Log.Warn("Attempting to set a scroll position outside of item range: " + value.ToString());
                }
                scrollPosition = value;
            }
        }
        int scrollPosition = 0;

        /// <summary>
        /// The menu items in the page
        /// </summary>
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        /// <summary>
        /// Next input
        /// </summary>
        /// <returns>True if the page can navigate forward in the list</returns>
        public bool Next()
        {
            // if outside of valid range return false
            if (scrollPosition >= MenuItems.Count - 1)
            {
                return false;
            }

            // increment scroll position
            scrollPosition++;

            return true;
        }

        /// <summary>
        /// Previous input
        /// </summary>
        /// <returns>True if the page can navigate backwards in the list</returns>
        public bool Previous()
        {
            // if outside of valid range return false
            if (scrollPosition <= 0) { return false; }

            // increment scroll position
            scrollPosition--;

            return true;
        }

        /// <summary>
        /// Select input
        /// </summary>
        /// <returns>True</returns>
        public bool Select()
        {   //gives us the ability to respond to select events
            return true;
        }
    }
}