namespace Meadow.Foundation.Graphics
{
    public partial class MicroGraphics
    {
        /// <summary>
        /// Draw a graphics path 
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="enabled">Should pixels be enabled (on) or disabled (off)</param>
        public void DrawPath(GraphicsPath path, bool enabled)
        {
            DrawPath(path, (enabled ? Color.White : Color.Black));
        }

        /// <summary>
        /// Draw a graphics path
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="color">The color to draw the path</param>
        public void DrawPath(GraphicsPath path, Color color)
        {
            for (int i = 0; i < path.PointCount; i++)
            {
                if (path.PathActions[i].Verb == VerbType.Move || i == 0)
                {
                    continue;
                }

                DrawLine(path.PathActions[i - 1].PathPoint.X,
                         path.PathActions[i - 1].PathPoint.Y,
                         path.PathActions[i].PathPoint.X,
                         path.PathActions[i].PathPoint.Y,
                        color);
            }
        }
    }
}