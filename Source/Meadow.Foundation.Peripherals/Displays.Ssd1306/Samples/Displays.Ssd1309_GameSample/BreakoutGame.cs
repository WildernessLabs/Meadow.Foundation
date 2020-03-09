using System;

namespace Displays.Ssd1309_3DCube_Sample
{
    public class BreakoutGame
    {
        Random rand = new Random();

        public Paddle Paddle { get; private set; }
        public Ball Ball { get; private set; }
        public Block[] Blocks { get; private set; }

        int width, height;

        public BreakoutGame(int width, int height)
        {
            Blocks = new Block[25];

            this.width = width;
            this.height = height;

            Initialize();

            ResetBoard();
            ResetBall();
        }

        public void Initialize()
        {
            Paddle = new Paddle(16, 5, width / 2, height - 14);
            Ball = new Ball(2);

            for (int i = 0; i < Blocks.Length; i++)
            {
                //    Blocks[i] = new Block(4 + (i % 5) * (5 + BLOCK_WIDTH), 20 + (i / 5) * (4 + BLOCK_HEIGHT),
                //        BLOCK_WIDTH, BLOCK_HEIGHT);

                Blocks[i] = new Block(BLOCK_X_SPACING + (i % 5) * (BLOCK_X_SPACING + BLOCK_WIDTH),
                    10 + (i / 5) * (BLOCK_Y_SPACING + BLOCK_HEIGHT),
                    BLOCK_WIDTH, BLOCK_HEIGHT);
            }
        }

        int BLOCK_WIDTH = 10;
        int BLOCK_HEIGHT = 5;
        int BLOCK_X_SPACING = 2;
        int BLOCK_Y_SPACING = 2;

        public void ResetBoard()
        {
            for (int i = 0; i < Blocks.Length; i++)
            {
                Blocks[i].IsVisible = true;
            }
        }

        public void ResetBall()
        {
            Ball.X = width / 2;
            Ball.Y = height / 2;

            Ball.XSpeed = rand.Next() % 6 - 3;
            Ball.YSpeed = rand.Next() % 2 + 3;
        }

        public void Left(int steps = 1)
        {
            Paddle.X -= steps;
            if (Paddle.X < 0)
                Paddle.X = 0;
        }

        public void Right(int steps = 1)
        {
            Paddle.X += steps;

            if (Paddle.X + Paddle.Width > width)
                Paddle.X = width - Paddle.Width;
        }

        public void Update()
        {
            Ball.X += Ball.XSpeed;
            Ball.Y += Ball.YSpeed;

            CheckCollisions();
        }

        void CheckCollisions()
        {
            CheckBlockCollisions();

            CheckXCollision();
            CheckYCollision();

            CheckState();
        }

        void CheckState()
        {
            bool gameCleared = true;

            foreach (var block in Blocks)
            {
                if (block.IsVisible == true)
                {
                    gameCleared = false;
                    break;
                }
            }

            if(gameCleared)
            {
                ResetBoard();
                ResetBall();
            }
        }

        void CheckBlockCollisions()
        {
            foreach(var block in Blocks)
            {
                if(block.IsVisible == false)
                {
                    continue;
                }

                //down
                if (Ball.YSpeed > 0 &&
                    Ball.X + Ball.Radius >= block.X &&
                    Ball.X - Ball.Radius <= block.X + block.Width &&
                    Ball.Y - Ball.Radius <= block.Y + block.Height &&
                    Ball.Y + Ball.Radius >= block.Y)
                {
                    block.IsVisible = false;
                    Ball.YSpeed *= -1;
                    break;
                }
                //up
                if (Ball.YSpeed < 0 &&
                    Ball.X + Ball.Radius >= block.X &&
                    Ball.X - Ball.Radius <= block.X + block.Width &&
                    Ball.Y - Ball.Radius <= block.Y + block.Height &&
                    Ball.Y + Ball.Radius >= block.Y)
                {
                    block.IsVisible = false;
                    Ball.YSpeed *= -1;
                    break;
                }
            }
        }

        void CheckXCollision()
        {
            //Walls
            if (Ball.X - Ball.Radius <= 0 ||
               Ball.X + Ball.Radius >= width)
            {
                Ball.XSpeed *= -1;
                Ball.X += Ball.XSpeed;
                return;
            }
        }

        void CheckYCollision()
        {
            //walls
            if (Ball.Y - Ball.Radius <= 0 ||
                Ball.Y + Ball.Radius >= height)
            {
                Ball.YSpeed *= -1;
                Ball.Y += Ball.YSpeed;
                return;
            }

            //Paddle
            if(Ball.YSpeed > 0 &&
                Ball.X - Ball.Radius >= Paddle.X &&
                Ball.X + Ball.Radius <= Paddle.X + Paddle.Width &&
                Ball.Y - Ball.Radius <= Paddle.Y + Paddle.Height &&
                Ball.Y + Ball.Radius >= Paddle.Y)
            {
                Ball.YSpeed *= -1;
                Ball.Y += Ball.YSpeed;
                return;
            }
        }
    }

    public class Paddle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Paddle(int width, int height, int x, int y)
        {
            Width = width;
            Height = height;
            X = x;
            Y = y;
        }

    }

    public class Block
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool IsVisible { get; set; }

        public Block(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public class Ball
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int XSpeed { get; set; }
        public int YSpeed { get; set; }
        public int Radius { get; private set; }

        public Ball(int radius)
        {
            Radius = radius;
        }
    }
}