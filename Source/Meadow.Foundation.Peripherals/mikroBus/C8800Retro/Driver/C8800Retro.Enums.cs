namespace Meadow.Foundation.mikroBUS.Displays
{
    public partial class C8800Retro
    {
        /// <summary>
        /// Button array columns (1-4)
        /// </summary>
        public enum ButtonColumn
        {
            /// <summary>
            /// Button column 1
            /// </summary>
            _1,
            /// <summary>
            /// Button column 2
            /// </summary>
            _2,
            /// <summary>
            /// Button column 3
            /// </summary>
            _3,
            /// <summary>
            /// Button column 4
            /// </summary>
            _4,
        }

        /// <summary>
        /// Button array rows (A-D)
        /// </summary>
        public enum ButtonRow
        {
            /// <summary>
            /// Button row A
            /// </summary>
            A,
            /// <summary>
            /// Button row B
            /// </summary>
            B,
            /// <summary>
            /// Button row C
            /// </summary>
            C,
            /// <summary>
            /// Button row D
            /// </summary>
            D,
        }
    }
}