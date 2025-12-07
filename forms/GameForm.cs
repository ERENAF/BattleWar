using SeaWar.models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace SeaWar.forms
{
    public partial class GameForm : Form
    {
        
    }

    public class GameBoardVisual
    {
        public int NumCells { get; private set; }
        public int CellSize { get; private set; }
        public bool IsEnemyBoard { get; private set; }

        public Panel BoardPanel { get; private set; }
        public Button[,] Cells { get; private set; }

        // События
        public event Action<int, int> CellClicked;
        public event Action<int, int> CellRightClicked;

        public GameBoardVisual(int numCells, int cellSize, int x, int y, bool isEnemyBoard = false)
        {
            NumCells = numCells;
            CellSize = cellSize;
            IsEnemyBoard = isEnemyBoard;

            int spacing = cellSize / 10;

            BoardPanel = new Panel
            {
                Size = new Size(numCells * (cellSize + spacing), numCells * (cellSize + spacing)),
                Location = new Point(x, y),
                BackColor = Color.DarkGray
            };

            Cells = new Button[numCells, numCells];

            // Создаем клетки-кнопки
            for (int i = 0; i < numCells; i++)
            {
                for (int j = 0; j < numCells; j++)
                {
                    Cells[i, j] = new Button
                    {
                        Location = new Point(i * (cellSize + spacing), j * (cellSize + spacing)),
                        Size = new Size(cellSize, cellSize),
                        BackColor = Color.AliceBlue,
                        ForeColor = Color.Black,
                        Font = new Font("Arial", cellSize / 3, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Tag = new Point(i, j) // Сохраняем координаты в Tag
                    };

                    // Подписываемся на события
                    Cells[i, j].Click += CellButton_Click;
                    Cells[i, j].MouseDown += CellButton_MouseDown;

                    BoardPanel.Controls.Add(Cells[i, j]);
                }
            }

            // Рисуем координаты
            DrawCoordinates();
        }

        private void DrawCoordinates()
        {
            // Буквы сверху
            for (int i = 0; i < NumCells; i++)
            {
                var label = new Label
                {
                    Text = ((char)('A' + i)).ToString(),
                    Location = new Point(
                        i * (CellSize + CellSize / 10) + CellSize / 3,
                        -CellSize / 2),
                    Size = new Size(CellSize / 2, CellSize / 2),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", CellSize / 4, FontStyle.Bold),
                    ForeColor = Color.White
                };
                BoardPanel.Controls.Add(label);
            }

            // Цифры слева
            for (int i = 0; i < NumCells; i++)
            {
                var label = new Label
                {
                    Text = (i + 1).ToString(),
                    Location = new Point(
                        -CellSize / 2,
                        i * (CellSize + CellSize / 10) + CellSize / 3),
                    Size = new Size(CellSize / 2, CellSize / 2),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", CellSize / 4, FontStyle.Bold),
                    ForeColor = Color.White
                };
                BoardPanel.Controls.Add(label);
            }
        }

        private void CellButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag is Point coordinates)
            {
                CellClicked?.Invoke(coordinates.X, coordinates.Y);
            }
        }

        private void CellButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var button = sender as Button;
                if (button != null && button.Tag is Point coordinates)
                {
                    CellRightClicked?.Invoke(coordinates.X, coordinates.Y);
                }
            }
        }

        public void UpdateFromGameBoard(GameBoard gameBoard)
        {
            if (gameBoard == null) return;

            for (int x = 0; x < Math.Min(NumCells, gameBoard.Size); x++)
            {
                for (int y = 0; y < Math.Min(NumCells, gameBoard.Size); y++)
                {
                    var cell = gameBoard.Cells[x, y];
                    if (cell != null)
                    {
                        UpdateCellVisual(x, y, cell.State);
                    }
                }
            }
        }

        private void UpdateCellVisual(int x, int y, CellState state)
        {
            if (x < 0 || x >= NumCells || y < 0 || y >= NumCells)
                return;

            var button = Cells[x, y];

            switch (state)
            {
                case CellState.Empty:
                    button.BackColor = Color.AliceBlue;
                    button.Text = "";
                    break;

                case CellState.Ship:
                    if (!IsEnemyBoard) // На вражеской доске не показываем корабли
                    {
                        button.BackColor = Color.DarkGray;
                        button.Text = "■";
                        button.ForeColor = Color.White;
                    }
                    else
                    {
                        button.BackColor = Color.AliceBlue;
                        button.Text = "";
                    }
                    break;

                case CellState.Miss:
                    button.BackColor = Color.LightBlue;
                    button.Text = "X";
                    button.ForeColor = Color.Blue;
                    break;

                case CellState.Hit:
                    button.BackColor = Color.Orange;
                    button.Text = "✕";
                    button.ForeColor = Color.Red;
                    break;

                case CellState.Sunk:
                    button.BackColor = Color.DarkRed;
                    button.Text = "☠";
                    button.ForeColor = Color.White;
                    break;
            }
        }

        public void SetInteractive(bool interactive)
        {
            foreach (var cell in Cells)
            {
                cell.Enabled = interactive;
                cell.Cursor = interactive ? Cursors.Hand : Cursors.Default;
            }
        }

        public void HighlightCell(int x, int y, Color color)
        {
            if (x >= 0 && x < NumCells && y >= 0 && y < NumCells)
            {
                Cells[x, y].BackColor = color;
            }
        }

        public void ClearHighlights()
        {
            for (int x = 0; x < NumCells; x++)
            {
                for (int y = 0; y < NumCells; y++)
                {
                    UpdateCellVisual(x, y, CellState.Empty);
                }
            }
        }
    }
}