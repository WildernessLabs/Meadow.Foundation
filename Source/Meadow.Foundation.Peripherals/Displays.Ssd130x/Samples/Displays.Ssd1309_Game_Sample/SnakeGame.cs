using System;
using System.Collections;

namespace Displays.Ssd1309_3DCube_Sample
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
    }

    public enum SnakeDirection : byte
    {
        Up,
        Down,
        Left,
        Right,
        Stop, //to start
    }

    public class SnakeGame
    {
        public int Score { get; private set; }
        public int Level { get; private set; }

        public int BoardWidth { get; private set; }
        public int BoardHeight { get; private set; }

        public ArrayList SnakePosition { get; private set; }

        public Point FoodPosition { get; private set; }

        public SnakeDirection Direction { get; set; }

        public bool PlaySound { get; private set; }

        Random rand = new Random((int)DateTime.Now.Ticks);

        enum CellType : byte
        {
            Empty,
            Food,
        }

        public SnakeGame(int width, int height)
        {
            BoardWidth = width;
            BoardHeight = height;

            SnakePosition = new ArrayList();

            Reset();
        }

        public void Update()
        {
            PlaySound = false;

            if (Direction == SnakeDirection.Stop)
                return;

            var head = new Point((Point)SnakePosition[0]);
            var tail = new Point((Point)SnakePosition[SnakePosition.Count - 1]);

            if (Direction == SnakeDirection.Left)
                head.X--;
            if (Direction == SnakeDirection.Right)
                head.X++;
            if (Direction == SnakeDirection.Up)
                head.Y--;
            if (Direction == SnakeDirection.Down)
                head.Y++;

            for (int i = 0; i < SnakePosition.Count - 1; i++)
            {
                SnakePosition[SnakePosition.Count - 1 - i] = new Point((Point)SnakePosition[SnakePosition.Count - 2 - i]);
            }

            SnakePosition[0] = head;

            if (IsCellEmpty(head.X, head.Y, true) == false)
            {
                Reset();
            }

            if (head.X == FoodPosition.X && head.Y == FoodPosition.Y)
            {
                SnakePosition.Add(tail);
                UpdateFood();
                PlaySound = true;
            }
        }

        void Reset()
        {
            SnakePosition = new ArrayList();
            SnakePosition.Add(new Point(BoardWidth / 2, BoardHeight / 2));
            Direction = SnakeDirection.Stop;

            Level = 1;

            UpdateFood();
        }

        void UpdateFood()
        {
            int foodX, foodY;
            do
            {
                foodX = rand.Next() % BoardWidth;
                foodY = rand.Next() % BoardHeight;
            }
            while (IsCellEmpty(foodX, foodY) == false);

            FoodPosition = new Point(foodX, foodY);
            Level++;
        }

        bool IsCellEmpty(int x, int y, bool ignoreHead = false)
        {
            Point snakeBody;

            if (x < 0 || y < 0 || x >= BoardWidth || y >= BoardHeight)
                return false;

            for (int i = ignoreHead ? 1 : 0; i < SnakePosition.Count; i++)
            {
                snakeBody = (Point)SnakePosition[i];
                if (snakeBody.X == x && snakeBody.Y == y)
                    return false;
            }
            return true;
        }
    }
}