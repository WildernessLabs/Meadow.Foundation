namespace MicroGraphics
{
    public struct Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public bool IsEmpty => X == 0 && Y == 0;

        public Point (int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }

        public void Offset(Point point)
        {
            X += point.X;
            Y += point.Y;
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }
}