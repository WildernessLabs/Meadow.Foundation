namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public interface IPage
    {
        bool OnNext();

        bool OnPrevious();

        bool OnSelect();
    }
}