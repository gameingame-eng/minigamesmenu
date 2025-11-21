using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic; // For list of cells

namespace MinigameMenu
{
    // ##################################################################
    //    NEW MINESWEEPER FORM
    // ##################################################################
    public class MinesweeperForm : Form
    {
        // === Game Settings ===
        private const int GridSize = 10; // 10x10 grid
        private const int NumMines = 12; // 12 mines
        private const int CellSize = 30; // 30x30 pixels

        // === Game State ===
        private Cell[,] grid;
        private int flagsPlaced;
        private bool isGameOver;
        private bool isFirstClick;
        private Random random = new Random();

        // === UI Controls ===
        private TableLayoutPanel gridPanel;
        private Label statusLabel;
        private Button resetButton;

        // This is a helper class to store the logic for each cell
        private class Cell
        {
            public bool IsMine;
            public bool IsRevealed;
            public bool IsFlagged;
            public int AdjacentMines;
            public Button Button; // Each cell knows its own button
        }

        public MinesweeperForm()
        {
            InitializeComponent();
            StartNewGame();
        }

        private void InitializeComponent()
        {
            Text = "Minesweeper";
            // Calculate size based on grid
            int formWidth = GridSize * CellSize + 40; // Add padding
            int formHeight = GridSize * CellSize + 120; // Add space for status/reset
            Size = new Size(formWidth, formHeight);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // --- Top Panel (Status & Reset) ---
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            statusLabel = new Label
            {
                Text = $"Mines: {NumMines}",
                Dock = DockStyle.Left,
                Width = 120,
                Font = new Font("Arial", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            resetButton = new Button
            {
                Text = "🙂", // Smiley face
                Font = new Font("Arial", 14F, FontStyle.Bold),
                Width = 50,
                Height = 50,
                // Center the button
                Left = (formWidth / 2) - 35,
                Anchor = AnchorStyles.Top
            };
            resetButton.Click += (s, e) => StartNewGame();

            topPanel.Controls.Add(statusLabel);
            topPanel.Controls.Add(resetButton);

            // --- Grid Panel ---
            gridPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = GridSize,
                RowCount = GridSize,
                Margin = new Padding(10)
            };
            
            // Set all columns and rows to be the same size
            for (int i = 0; i < GridSize; i++)
            {
                gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / GridSize));
                gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / GridSize));
            }
            
            Controls.Add(gridPanel);
            Controls.Add(topPanel);
        }

        private void StartNewGame()
        {
            // Reset state
            gridPanel.Controls.Clear();
            grid = new Cell[GridSize, GridSize];
            flagsPlaced = 0;
            isGameOver = false;
            isFirstClick = true;
            statusLabel.Text = $"Mines: {NumMines}";
            resetButton.Text = "🙂";

            // 1. Create all Cell objects and Buttons
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    Cell cell = new Cell();
                    Button button = new Button
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(1),
                        Font = new Font("Arial", 10F, FontStyle.Bold),
                    };

                    // Store (x, y) in the button's Tag property
                    // This is how we know which cell was clicked
                    button.Tag = new Point(x, y);
                    button.MouseDown += GridButton_MouseDown; // Use MouseDown for left/right click

                    cell.Button = button;
                    grid[x, y] = cell;
                    gridPanel.Controls.Add(button, x, y); // Add to panel at (col, row)
                }
            }
        }

        private void PlaceMines(int firstClickX, int firstClickY)
        {
            // 2. Place mines randomly, avoiding the first click
            int minesToPlace = NumMines;
            while (minesToPlace > 0)
            {
                int x = random.Next(GridSize);
                int y = random.Next(GridSize);

                // Don't place on first click, and don't place on an existing mine
                if (x == firstClickX && y == firstClickY || grid[x, y].IsMine)
                {
                    continue;
                }

                grid[x, y].IsMine = true;
                minesToPlace--;
            }

            // 3. Calculate adjacent mine counts for all cells
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    if (grid[x, y].IsMine) continue;

                    int count = 0;
                    // Check all 8 neighbors
                    for (int ny = -1; ny <= 1; ny++)
                    {
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            if (nx == 0 && ny == 0) continue; // Skip self

                            int checkX = x + nx;
                            int checkY = y + ny;

                            // Check if neighbor is within bounds
                            if (checkX >= 0 && checkX < GridSize && checkY >= 0 && checkY < GridSize)
                            {
                                if (grid[checkX, checkY].IsMine)
                                {
                                    count++;
                                }
                            }
                        }
                    }
                    grid[x, y].AdjacentMines = count;
                }
            }
        }

        private void GridButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (isGameOver) return; // Game is over, do nothing

            Button button = (Button)sender;
            Point p = (Point)button.Tag;
            Cell cell = grid[p.X, p.Y];

            // --- Handle First Click ---
            // We wait until the first click to place mines to guarantee
            // the player doesn't lose on the first move.
            if (isFirstClick)
            {
                PlaceMines(p.X, p.Y);
                isFirstClick = false;
            }

            // --- Handle Left Click ---
            if (e.Button == MouseButtons.Left)
            {
                if (cell.IsFlagged || cell.IsRevealed)
                {
                    return; // Do nothing if flagged or already seen
                }

                // Boom!
                if (cell.IsMine)
                {
                    GameOver(won: false);
                }
                else
                {
                    RevealCell(p.X, p.Y);
                    CheckForWin();
                }
            }
            // --- Handle Right Click (Flagging) ---
            else if (e.Button == MouseButtons.Right)
            {
                if (cell.IsRevealed) return; // Can't flag a revealed cell

                cell.IsFlagged = !cell.IsFlagged; // Toggle flag

                if (cell.IsFlagged)
                {
                    button.Text = "🚩";
                    flagsPlaced++;
                }
                else
                {
                    button.Text = "";
                    flagsPlaced--;
                }
                statusLabel.Text = $"Mines: {NumMines - flagsPlaced}";
            }
        }
        
        // This is the recursive "flood fill" function
        private void RevealCell(int x, int y)
        {
            // 1. Check bounds
            if (x < 0 || x >= GridSize || y < 0 || y >= GridSize) return;

            Cell cell = grid[x, y];

            // 2. Stop recursion if cell is already revealed or is flagged
            if (cell.IsRevealed || cell.IsFlagged) return;

            // 3. Reveal this cell
            cell.IsRevealed = true;
            cell.Button.Enabled = false; // Disable button
            cell.Button.BackColor = Color.LightGray;

            if (cell.AdjacentMines > 0)
            {
                // It's a number, show it and stop
                cell.Button.Text = cell.AdjacentMines.ToString();
                // Set color for numbers
                cell.Button.ForeColor = GetNumberColor(cell.AdjacentMines);
            }
            else
            {
                // It's an empty cell (0 adjacent mines)
                // Keep revealing neighbors
                cell.Button.Text = "";
                
                // Recurse to all 8 neighbors
                for (int ny = -1; ny <= 1; ny++)
                {
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        if (nx == 0 && ny == 0) continue;
                        RevealCell(x + nx, y + ny);
                    }
                }
            }
        }

        private void GameOver(bool won)
        {
            isGameOver = true;
            resetButton.Text = won ? "😎" : "😵";

            // Reveal all mines
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    Cell cell = grid[x, y];
                    if (cell.IsMine)
                    {
                        if (!won)
                        {
                            // Show all mines if you lost
                            cell.Button.Text = "💣";
                            cell.Button.BackColor = cell.IsRevealed ? Color.Red : Color.White;
                        }
                    }
                    else if (cell.IsFlagged)
                    {
                        // Show incorrectly placed flag
                        cell.Button.Text = "❌"; 
                    }
                }
            }
        }

        private void CheckForWin()
        {
            int revealedCells = 0;
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    if (grid[x, y].IsRevealed && !grid[x, y].IsMine)
                    {
                        revealedCells++;
                    }
                }
            }
            
            // You win if all non-mine cells are revealed
            int nonMineCells = (GridSize * GridSize) - NumMines;
            if (revealedCells == nonMineCells)
            {
                statusLabel.Text = "You Win!";
                GameOver(won: true);
            }
        }

        // Helper to make the numbers colorful like in the original game
        private Color GetNumberColor(int num)
        {
            switch (num)
            {
                case 1: return Color.Blue;
                case 2: return Color.Green;
                case 3: return Color.Red;
                case 4: return Color.DarkBlue;
                case 5: return Color.Maroon;
                case 6: return Color.Cyan;
                case 7: return Color.Black;
                case 8: return Color.Gray;
                default: return Color.Black;
            }
        }
    }
}