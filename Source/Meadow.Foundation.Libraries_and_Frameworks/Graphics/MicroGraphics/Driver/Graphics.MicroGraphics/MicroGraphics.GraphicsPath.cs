namespace Meadow.Foundation.Graphics
{
    public partial class GraphicsLibrary
    {
        public void DrawPath(GraphicsPath path, bool colored)
        {
            DrawPath(path, (colored ? Color.White : Color.Black));
        }

        public void DrawPath(GraphicsPath path, Color color)
        {
            //simple for now 
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
