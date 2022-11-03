using System;

namespace Meadow.Foundation.Graphics
{
    public partial class MicroGraphics
    {
        protected class CanvasState
        {
            public IFont CurrentFont { get; set; }
            public int Stroke { get; set; }
            public RotationType Rotation { get; set; }
            public Color PenColor { get; set; }
        }

        protected CanvasState canvasState;

        /// <summary>
        /// Save any state variables
        /// Includes: CurrentFont, Stroke, and Rotation
        /// </summary>
        public void SaveState()
        {
            if (canvasState == null)
            {
                canvasState = new CanvasState();
            }

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
