using System;
using System.Collections;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuPage : IPage
    {
        public int ScrollPosition
        {
            get => scrollPosition; 
            set {
                if (value > MenuItems.Count - 1 || value < 0)
                {
                    Console.WriteLine("Attempting to set a scroll position outside of item range: " + value.ToString());
                }
                scrollPosition = value;
            }
        }
        protected int scrollPosition = 0;

        public ArrayList MenuItems { get; set; } = new ArrayList();

        public bool OnNext()
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

        public bool OnPrevious()
        {
            // if outside of valid range return false
            if (scrollPosition <= 0) { return false; }

            // increment scroll position
            scrollPosition--;

            return true;
        }

        public bool OnSelect()
        {   //gives us the ability to respond to select events
            return true;
        }
    }
}