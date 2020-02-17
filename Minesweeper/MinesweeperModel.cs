using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    class MinesweeperModel
    {
        static Random random = new Random();

        internal enum DifficultyLevels { EASY, MEDIUM, HARD }

        internal DifficultyLevels Difficulty { get; set; }

        internal class Cell
        {
            public Cell(CellValue value, Point point)
            {
                Value = value;
                Point = point;
            }

            internal enum CellValue { NONE = 0, ONE = 1, TWO = 2, THREE = 3, FOUR = 4,
                FIVE = 5, SIX = 6, SEVEN = 7, EIGHT = 8, BOMB = 9 }

            internal CellValue Value { get; set; }

            internal bool IsDug { get; set; }

            internal bool IsFlagged { get; set; }

            internal Point Point { get; set; }
        }

        Cell[,] grid;
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int NumOfBombs { get; set; }

        public int NumOfFlags { get; set; }

        internal Cell[] bombs;

        internal MinesweeperModel(DifficultyLevels difficulty)
        {
            Difficulty = difficulty;

            switch (Difficulty)
            {
                case DifficultyLevels.EASY:
                    Rows = 8;
                    Cols = 10;
                    NumOfBombs = 10;                    
                    break;
                case DifficultyLevels.MEDIUM:
                    Rows = 14;
                    Cols = 18;
                    NumOfBombs = 40;
                    break;
                case DifficultyLevels.HARD:
                    Rows = 20;
                    Cols = 24;
                    NumOfBombs = 99;
                    break;
            }
            grid = new Cell[Rows, Cols];
            NumOfFlags = NumOfBombs;
        }

        /**
         * To be called when the player selects the first spot to begin the game.
         * The call to this function should be followed by a call to Dig(), passing in the same Point. 
         */
        internal void Init(Point startPoint)
        {
            grid[startPoint.X, startPoint.Y] = new Cell(Cell.CellValue.NONE, startPoint);

            PlantAndMarkBombs(startPoint);
        }

        private void PlantAndMarkBombs(Point startPoint)
        {
            bombs = PlantBombs(startPoint);

            MarkCells();
        }

        private Cell[] PlantBombs(Point startPoint)
        {
            var bombs = new Cell[NumOfBombs];

            for (int i = 0; i < NumOfBombs; i++)
            {
                int X;
                int Y;

                do
                {
                    X = random.Next(0, Rows);
                    Y = random.Next(0, Cols);

                } while (IsBombTouchingStartPoint(startPoint, X, Y) || IsBombSameAsOtherBomb(X, Y, i, bombs));

                bombs[i] = new Cell(Cell.CellValue.BOMB, new Point(X, Y));

            }

            return bombs;
        }

        /**
         * returns true if the current bomb location is already occupied by another bomb.
         * Can't have two bombs in the same place!
         */
        private bool IsBombSameAsOtherBomb(int x, int y, int i, Cell[] bombs)
        {
            for (int j = 0; j < i; j++)
            {
                if (bombs[j].Point.X == x && bombs[j].Point.Y == y)
                    return true;
            }
            return false;
        }

        /**
         * returns true if the bomb is touching the first spot that the player dug into
         * As I understood it, the rule is that the first spot dug must be an empty space.
         * If there is a bomb touching that spot, the spot will be assigned a number instead.
         */
        private bool IsBombTouchingStartPoint(Point p, int x, int y)
        {
            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (i < 0 || j < 0 || i > Rows || j > Cols)
                        continue;

                    if (p.X == i && p.Y == j)
                        return true;
                }
            }
            return false;
        }

        private void MarkCells()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    MarkCell(new Point(i, j));
                } 
            }
        }

        /**
         * Gives the cell at Point p a value corrosponding to the number of bombs touching it. 
         */
        private void MarkCell(Point p)
        {
            var bombsTouching = 0;

            for (int i = p.X-1; i < p.X+2; i++)
            {
                if (i < 0 || i > Rows)
                    continue;

                for (int j = p.Y-1; j < p.Y+2; j++)
                {
                    if ( j < 0 || j > Cols)
                        continue;

                    foreach (var bomb in bombs)
                    {
                        if (p.X == bomb.Point.X && p.Y == bomb.Point.Y) //if this is a bomb
                        {
                            grid[p.X, p.Y] = bomb;
                            return;
                        }

                        if (bomb.Point.X == i && bomb.Point.Y == j)
                            bombsTouching++;
                    }
                }
            }

            grid[p.X, p.Y] = new Cell((Cell.CellValue) bombsTouching, p);
        }

        /**
         * The action of the player making a move.
         * Returns a list of Cells that were uncovered as a result of this move.
         * Instances when the list contains more than one element:
         *      A) If the dug Cell's value is NONE
         *      B) If the dug Cell's value is BOMB
         * If the dug Cell's value is BOMB, this function will return a list of the locations of all the Bombs
         */
        internal List<Cell> Dig(Point cell)
        {
            Cell cellAsCell = grid[cell.X, cell.Y];

            if(cellAsCell.Value == Cell.CellValue.BOMB)
            {
                return new List<Cell>(bombs);
            }

            if(cellAsCell.Value == Cell.CellValue.NONE)
            {
                return RecursiveDig(new List<Cell>(), cellAsCell);
            }

            else
            {
                cellAsCell.IsDug = true;
                List<Cell> res = new List<Cell>();
                res.Add(cellAsCell);
                return res;
            }
        }
        
        /**
         * A recursive function to Dig all neighboring undug and unflagged Cells.
         * Used to expand an area of NONE cells.
         */
        private List<Cell> RecursiveDig(List<Cell> cells, Cell cell)
        {
            if (cell.IsDug || cell.IsFlagged)
                return cells;
            if (cell.Value != Cell.CellValue.NONE)
            {
                cell.IsDug = true;
                cells.Add(cell);
                return cells;
            }
            else
            {
                cell.IsDug = true;
                cells.Add(cell);

                for (int x = cell.Point.X - 1; x < cell.Point.X + 2; x++)
                {
                    if (x >= Rows || x < 0)
                        continue;
                    for (int y = cell.Point.Y - 1; y < cell.Point.Y + 2; y++)
                    {
                        if (y >= Cols || y < 0)
                            continue;

                        RecursiveDig(cells, grid[x, y]);
                    }
                }
                return cells;
            }
        }

        internal void ToggleFlag(Point cell)
        {
            if (grid[cell.X, cell.Y].IsFlagged)
            {
                grid[cell.X, cell.Y].IsFlagged = false;
                NumOfFlags++;
            }
            else
            {
                grid[cell.X, cell.Y].IsFlagged = true;
                NumOfFlags--;
            }
        }

        /**
         * The winning condition is that all non-bomb spaces were dug.
         */
        internal bool IsWon()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (!grid[i, j].IsDug)
                    {
                        if (grid[i, j].Value == Cell.CellValue.BOMB)
                            continue;
                        else
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
