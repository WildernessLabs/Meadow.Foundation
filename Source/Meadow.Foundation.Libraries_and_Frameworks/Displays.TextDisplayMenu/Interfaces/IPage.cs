namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public interface IPage
    {
        bool Next();
        bool Previous();
        bool Select();
    }
}