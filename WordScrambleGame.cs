using System;
using System.Drawing;
using System.Linq; // We'll use this for a simple shuffle
using System.Windows.Forms;

namespace MinigameMenu
{
    // This is the new game form
    public class WordScrambleForm : Form
    {
        // Word list
        private string[] wordList = { 
            "apple", "banana", "orange", "grape", "lemon", 
            "windows", "computer", "program", "snake", "visual" 
        };
        
        private string currentWord;
        private Random random = new Random();

        // Controls
        private Label scrambledWordLabel;
        private TextBox guessTextBox;
        private Button checkButton;
        private Label feedbackLabel;

        public WordScrambleForm()
        {
            InitializeComponent();
            LoadNewWord();
        }

        private void InitializeComponent()
        {
            Text = "Word Scramble";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Arial", 12F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // Scrambled word display
            scrambledWordLabel = new Label
            {
                Text = "scrambled",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Font = new Font("Arial", 24F, FontStyle.Bold),
                Height = 80,
                Padding = new Padding(10)
            };

            // Feedback
            feedbackLabel = new Label
            {
                Text = "Guess the word!",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            // Guess input
            guessTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Margin = new Padding(40, 10, 40, 10), // Add some side margin
                Font = new Font("Arial", 14F)
            };
            // Allow pressing Enter to check
            guessTextBox.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    CheckButton_Click(s, e);
                    e.SuppressKeyPress = true; // Prevents the 'ding' sound
                }
            };


            // Check button
            checkButton = new Button
            {
                Text = "Check",
                Dock = DockStyle.Bottom,
                Height = 50,
                Font = new Font("Arial", 14F, FontStyle.Regular)
            };
            checkButton.Click += CheckButton_Click;

            Controls.Add(scrambledWordLabel);
            Controls.Add(feedbackLabel);
            Controls.Add(guessTextBox);
            Controls.Add(checkButton);
        }

        private void LoadNewWord()
        {
            // Pick a new word
            currentWord = wordList[random.Next(wordList.Length)];
            
            // Scramble it
            string scrambled;
            do
            {
                // Use LINQ to shuffle the characters
                var chars = currentWord.ToCharArray();
                scrambled = new string(chars.OrderBy(x => random.Next()).ToArray());
            } while (scrambled == currentWord); // Ensure it's actually scrambled

            // Update UI
            scrambledWordLabel.Text = scrambled;
            feedbackLabel.Text = "Guess the word!";
            feedbackLabel.ForeColor = Color.Black;
            guessTextBox.Text = "";
            guessTextBox.Enabled = true;
            checkButton.Text = "Check";
        }

        private void CheckButton_Click(object sender, EventArgs e)
        {
            // If button says "Next Word", load a new word
            if (checkButton.Text == "Next Word")
            {
                LoadNewWord();
                return;
            }

            // Otherwise, check the guess
            string guess = guessTextBox.Text.Trim();

            if (string.Equals(guess, currentWord, StringComparison.OrdinalIgnoreCase))
            {
                feedbackLabel.Text = "Correct!";
                feedbackLabel.ForeColor = Color.Green;
                checkButton.Text = "Next Word";
                guessTextBox.Enabled = false;
            }
            else
            {
                feedbackLabel.Text = "Try Again!";
                feedbackLabel.ForeColor = Color.Red;
            }
        }
    }
}