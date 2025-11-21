using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MinigameMenu
{
    public class SnakeForm : Form
    {
        private Timer timer;
        private List<Point> snake;
        private Point food;
        private int cellSize = 20;
        private int cols, rows;
        private int dx = 1, dy = 0;
        private Random rand = new Random();

        private Label scoreLabel;
        private int score = 0;

        public SnakeForm()
        {
            Text = "Snake";
            Width = 400;
            Height = 450; // extra space for score label
            DoubleBuffered = true;

            cols = ClientSize.Width / cellSize;
            rows = (ClientSize.Height - 30) / cellSize; // leave room for label

            snake = new List<Point> { new Point(5, 5) };
            SpawnFood();

            // Score label
            scoreLabel = new Label
            {
                Text = "Score: 0",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            Controls.Add(scoreLabel);

            timer = new Timer { Interval = 150 };
            timer.Tick += (s, e) => MoveSnake();
            timer.Start();

            Paint += SnakeForm_Paint;
            KeyDown += SnakeForm_KeyDown;
        }

        private void MoveSnake()
        {
            Point head = snake[0];
            Point newHead = new Point(head.X + dx, head.Y + dy);

            // Check collisions
            if (newHead.X < 0 || newHead.Y < 0 || newHead.X >= cols || newHead.Y >= rows || snake.Contains(newHead))
            {
                timer.Stop();
                MessageBox.Show($"Game Over!\nFinal Score: {score}");
                Close();
                return;
            }

            snake.Insert(0, newHead);

            if (newHead == food)
            {
                // Eat food and grow
                score++;
                scoreLabel.Text = $"Score: {score}";
                SpawnFood();
            }
            else
            {
                // Move forward (remove tail)
                snake.RemoveAt(snake.Count - 1);
            }

            Invalidate();
        }

        private void SpawnFood()
        {
            food = new Point(rand.Next(cols), rand.Next(rows));
            while (snake.Contains(food))
                food = new Point(rand.Next(cols), rand.Next(rows));
        }

        private void SnakeForm_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            foreach (var segment in snake)
                g.FillRectangle(Brushes.Green, segment.X * cellSize, segment.Y * cellSize + 30, cellSize, cellSize);

            g.FillRectangle(Brushes.Red, food.X * cellSize, food.Y * cellSize + 30, cellSize, cellSize);
        }

        private void SnakeForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: if (dy == 0) { dx = 0; dy = -1; } break;
                case Keys.Down: if (dy == 0) { dx = 0; dy = 1; } break;
                case Keys.Left: if (dx == 0) { dx = -1; dy = 0; } break;
                case Keys.Right: if (dx == 0) { dx = 1; dy = 0; } break;
            }
        }
    }
}