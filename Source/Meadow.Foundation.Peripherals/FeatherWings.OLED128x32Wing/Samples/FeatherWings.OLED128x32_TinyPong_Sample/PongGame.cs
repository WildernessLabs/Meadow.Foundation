using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeatherWings.OLED128x32_TinyPong_Sample
{

    public class PongGame
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public int PaddleWidth => 3;
        public int PaddleHeight => 12;

        public int BallX { get; protected set; }
        public int BallY { get; protected set; }
        public int BallRadius  => 2;

        public int PlayerX { get; protected set; }
        public int PlayerY { get; protected set; }
        public int CpuX { get; protected set; }
        public int CpuY { get; protected set; }

        public int PlayerScore { get; protected set; }

        public int CpuScore { get; protected set; }

        int ballXVelocity = 5;
        int ballYVelocity = 2;

        public PongGame(int width, int height)
        {
            Width = width;
            Height = height;

            InitGame();
            Reset();
        }

        void InitGame ()
        {
            PlayerX = 4;
            CpuX = Width - 4 - PaddleWidth;
        }

        public void Reset()
        {
            ResetBall();

            PlayerScore = 0;
            CpuScore = 0;
        }

        void ResetBall ()
        {
            BallX = Width / 2;
            BallY = Height / 2;
        }

        void UpdateCPUPlayer()
        {
            if (CpuY < BallY)
                CpuY++;
            else if (CpuY > BallY)
                CpuY--;
        }

        public void PlayerUp()
        {
            if(PlayerY > 0)
            {
                PlayerY -= 4;
            }
        }

        public void PlayerDown()
        {
            if(PlayerY + PaddleHeight < Height)
            {
                PlayerY += 4;
            }
        }

        public void Update()
        {
            UpdateCPUPlayer();

            //X collisions
            BallX += ballXVelocity;
            if (BallY >= PlayerY &&
                BallY <= PlayerY + PaddleHeight &&
                BallX - BallRadius <= PlayerX)
            {
                ballXVelocity *= -1;
                BallX += ballXVelocity;
            }
            else if (BallX - BallRadius < 0)
            {
                CpuScore++;
                ballXVelocity *= -1;
                ResetBall();
            }
            else if (BallY >= CpuY &&
                BallY <= CpuY + PaddleHeight &&
                BallX + BallRadius > CpuX)
            {
                ballXVelocity *= -1;
                BallX += ballXVelocity;
            }
            else if (BallX + BallRadius > Width)
            {
                PlayerScore++;
                ballXVelocity *= -1;
                ResetBall();
            }

            BallY += ballYVelocity;
            if (BallY - BallRadius < 0)
            {
                ballYVelocity *= -1;
                BallY += ballYVelocity;
            }
            else if (BallY + BallRadius >= Height)
            {
                ballYVelocity *= -1;
                BallY += ballYVelocity;
            }
        }
    }
}