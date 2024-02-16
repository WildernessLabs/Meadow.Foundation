using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics
{
    public partial class MicroGraphics
    {
        /// <summary>
        /// Represents a canvas state
        /// </summary>
        protected class CanvasState
        {
            /// <summary>
            /// The current font
            /// </summary>
            public IFont? CurrentFont { get; set; }

            /// <summary>
            /// The current stroke when drawing primitives
            /// </summary>
            public int Stroke { get; set; }

            /// <summary>
            /// The canvas rotation
            /// </summary>
            public RotationType Rotation { get; set; }

            /// <summary>
            /// The current pen color
            /// </summary>
            public Color PenColor { get; set; }
        }

        /// <summary>
        /// The current canvas state
        /// </summary>
        protected CanvasState? canvasState;

        /// <summary>
        /// Save any state variables
        /// Includes: CurrentFont, Stroke, and Rotation
        /// </summary>
        public void SaveState()
        {
            canvasState ??= new CanvasState();

            canvasState.CurrentFont = currentFont;
            canvasState.Stroke = Stroke;
            canvasState.Rotation = Rotation;
            canvasState.PenColor = PenColor;
        }

        /// <summary>
        /// Restore saved state variables and apply them to the MicroGraphics instance 
        /// Includes: CurrentFont, Stroke, and Rotation
        /// </summary>
        public void RestoreState()
        {
            if (canvasState == null)
            {
                throw new NullReferenceException("GraphicsLibary: State not saved, no state to restore.");
            }

            currentFont = canvasState.CurrentFont;
            Stroke = canvasState.Stroke;
            Rotation = canvasState.Rotation;
            PenColor = canvasState.PenColor;
        }
    }
}