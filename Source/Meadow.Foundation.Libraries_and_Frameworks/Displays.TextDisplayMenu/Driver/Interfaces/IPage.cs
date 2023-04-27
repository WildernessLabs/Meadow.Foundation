namespace Meadow.Foundation.Displays.UI
{
    /// <summary>
    /// Text display menu page abstraction
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// Next input 
        /// </summary>
        /// <returns>true if successful</returns>
        bool Next();

        /// <summary>
        /// Previous input 
        /// </summary>
        /// <returns>true if successful</returns>
        bool Previous();

        /// <summary>
        /// Select input 
        /// </summary>
        /// <returns>true if successful</returns>
        bool Select();
    }
}