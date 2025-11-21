using System;
using System.Windows.Forms;
using System.Drawing;

namespace MinigameMenu
{
    public class MainMenuForm : Form
    {
        private ListBox gameList;
        private Button playButton;
        private Button exitButton;

        public MainMenuForm()
        {
            Text = "Minigame Arcade";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterScreen;

            // Set the window icon
            this.Icon = new Icon("icon.ico");

            // Game list
            gameList = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 150
            };
            gameList.Items.AddRange(new string[]
            {
                "2048",
                "Snake",
                "Minesweeper",
                "Word Scramble"
            });

            // Play button
            playButton = new Button
            {
                Text = "Play",
                Dock = DockStyle.Left,
                Width = 100
            };
            playButton.Click += PlayButton_Click;

            // Exit button
            exitButton = new Button
            {
                Text = "Exit",
                Dock = DockStyle.Right,
                Width = 100
            };
            exitButton.Click += (s, e) => Close();

            // Layout panel for buttons
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            panel.Controls.Add(playButton);
            panel.Controls.Add(exitButton);

            Controls.Add(gameList);
            Controls.Add(panel);
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (gameList.SelectedItem == null)
            {
                MessageBox.Show("Please select a game first!");
                return;
            }

            string selectedGame = gameList.SelectedItem.ToString();

            if (selectedGame == "Snake")
            {
                new SnakeForm().ShowDialog();
            }
            else if (selectedGame == "2048")
            {
                // new Game2048Form().ShowDialog();
            }
            else if (selectedGame == "Minesweeper")
            {
                new MinesweeperForm().ShowDialog();
            }
            else if (selectedGame == "Word Scramble")
            {
                new WordScrambleForm().ShowDialog();
            }
        }
    }
}