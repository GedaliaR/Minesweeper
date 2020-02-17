using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minesweeper
{     
    public partial class MinesweeperForm : Form
    {
        private static readonly string BOMB_EMOJI = "💣";
        private static readonly string FLAG_EMOJI = "🚩";
        private static readonly string SMILEY_EMOJI = "😁";

        internal int Counter { get; set; }

        internal Button[,] buttons;
        internal bool IsFirstButton { get; set; }

        internal MinesweeperModel model;

        public MinesweeperForm()
        {
            InitializeComponent();

            //the following line gets executed 3 times; this is intentional as it avoids
            //a weird bug that I dont fully understand. (it has to do with the order that WinForms initializes GUI components)
            //TODO: fix it in a way that doesn't require this expensive operation to be repeated
            InitNewGame(MinesweeperModel.DifficultyLevels.MEDIUM);
        }

        private void MinesweeperForm_Load(object sender, EventArgs e)
        {
            InitNewGame(MinesweeperModel.DifficultyLevels.MEDIUM);
        }

        private void InitNewGame(MinesweeperModel.DifficultyLevels difficulty)
        {
            splitContainer1.Panel2.Controls.Clear();

            model = new MinesweeperModel(difficulty);

            comboBox1.Text = difficulty.ToString();

            buttons = new Button[model.Rows, model.Cols];
            AddButtons(model.Rows, model.Cols);

            Size = new Size(model.Cols * 37, model.Rows * 45);

            IsFirstButton = true;

            Counter = 0;
            FlagsUnused.Text = $"{FLAG_EMOJI} = {model.NumOfFlags}";

        }

        private void AddButtons(int rows, int cols)
        {
            int buttonSize = 35;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Button b = new Button();
                    b.Left = col * buttonSize;
                    b.Top = row * buttonSize;
                    b.Width = buttonSize;
                    b.Height = buttonSize;
                    //todo add color
                    b.Name = $"{row} , {col}";
                    b.MouseDown += new MouseEventHandler(ButtonClicked);
                    b.Tag = new Point(row, col);

                    splitContainer1.Panel2.Controls.Add(b);

                    buttons[row, col] = b;
                }
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Counter++;
            Clock.Text = $"{Counter}";
        }

        private void DifficultyChanged(object sender, EventArgs e)
        {
            var difficulty = GetEnumFromString(comboBox1.Text);

            InitNewGame(difficulty);
        }

        private MinesweeperModel.DifficultyLevels GetEnumFromString(string diff)
        {
            if (diff.Equals("Easy"))
            {
                return MinesweeperModel.DifficultyLevels.EASY;
            }
            if (diff.Equals("Medium"))
            {
                return MinesweeperModel.DifficultyLevels.MEDIUM;
            }
            if (diff.Equals("Hard"))
            {
                return MinesweeperModel.DifficultyLevels.HARD;
            }
            else return 0;
        }


        private void ButtonClicked(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            var buttonPoint = (Point)button.Tag;

            if (e.Button == MouseButtons.Left)
            {

                if (IsFirstButton)
                {
                    model.Init(buttonPoint);
                    IsFirstButton = false;
                    GameTimer.Start();
                }

                Dig(buttonPoint);
            //    button.Enabled = false;

            }
            else if (e.Button == MouseButtons.Right)
            {
                model.ToggleFlag(buttonPoint);
                FlagsUnused.Text = $"{FLAG_EMOJI} = {model.NumOfFlags}";
                ToggleFlag(button);
            }

        }

        private void ToggleFlag(Button button)
        {
            if (button.Text == "")
            {
                button.Text = FLAG_EMOJI;
            }
            else
            {
                button.Text = "";
            }
        }

        private void Dig(Point buttonPoint)
        {
            var results = model.Dig(buttonPoint);

            if (results[0].Value == MinesweeperModel.Cell.CellValue.BOMB)
                Lost(results);

            else
            {
                foreach (var cell in results)
                {
                    Button b = buttons[cell.Point.X, cell.Point.Y];

                    while (b.Enabled)
                        b.Enabled = false;
                    
                    switch (cell.Value)
                    {
                        case MinesweeperModel.Cell.CellValue.NONE:
                            b.Text = "";
                            break;
                        case MinesweeperModel.Cell.CellValue.ONE:
                            b.Text = "1";
                            b.BackColor = Color.Blue;
                            break;
                        case MinesweeperModel.Cell.CellValue.TWO:
                            b.Text = "2";
                            b.BackColor = Color.Green;
                            break;
                        case MinesweeperModel.Cell.CellValue.THREE:
                            b.Text = "3";
                            b.BackColor = Color.Red;
                            break;
                        case MinesweeperModel.Cell.CellValue.FOUR:
                            b.Text = "4";
                            b.BackColor = Color.Purple;
                            break;
                        case MinesweeperModel.Cell.CellValue.FIVE:
                            b.Text = "5";
                            b.BackColor = Color.Maroon;
                            break;
                        case MinesweeperModel.Cell.CellValue.SIX:
                            b.Text = "6";
                            b.BackColor = Color.Turquoise;
                            break;
                        case MinesweeperModel.Cell.CellValue.SEVEN:
                            b.Text = "7";
                            b.BackColor = Color.Black;
                            break;
                        case MinesweeperModel.Cell.CellValue.EIGHT:
                            b.Text = "8";
                            b.BackColor = Color.Gray;
                            break;
                        default:
                            break;
                    }
                    
                }
                CheckIfWon();
            }          
        }

        private void CheckIfWon()
        {
            if (model.IsWon())
            {
                GameTimer.Stop();

                foreach (MinesweeperModel.Cell cell in model.bombs)
                {
                    var b = buttons[cell.Point.X, cell.Point.Y];
                    b.Enabled = false;
                    b.Text = SMILEY_EMOJI;
                }
            }
        }

        private void Lost(List<MinesweeperModel.Cell> Bombs)
        {
            GameTimer.Stop();

            foreach (var bomb in Bombs)
            {
                var b = buttons[bomb.Point.X, bomb.Point.Y];
                b.Enabled = false;
                b.Text = BOMB_EMOJI;
            }
        }
    }
}

