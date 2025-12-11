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
        private GameSession game;
        private NetworkManager network;
        private GameBoardVisual playerBoard;
        private GameBoardVisual enemyBoard;
        private Button btnPlaceShips;
        private Button btnHostGame;
        private Button btnConnect;
        private Button btnStartGame;
        private Label lblStatus;

        public GameForm()
        {
            // Настройка окна
            this.Text = "Морской Бой";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создаем игру и сеть
            game = new GameSession("Игрок", "Соперник");
            network = new NetworkManager();
            network.MessageReceived += OnNetworkMessage;

            // Создаем доски
            playerBoard = new GameBoardVisual(10, 25, 50, 100, false);
            enemyBoard = new GameBoardVisual(10, 25, 400, 100, true);

            this.Controls.Add(playerBoard.BoardPanel);
            this.Controls.Add(enemyBoard.BoardPanel);

            enemyBoard.CellClicked += OnEnemyCellClicked;

            CreateControls();
        }

        private void CreateControls()
        {
            // Статус
            lblStatus = new Label();
            lblStatus.Location = new Point(50, 50);
            lblStatus.Size = new Size(300, 30);
            lblStatus.Text = "Расставьте корабли";
            this.Controls.Add(lblStatus);

            // Кнопка расстановки
            btnPlaceShips = new Button();
            btnPlaceShips.Location = new Point(400, 50);
            btnPlaceShips.Size = new Size(120, 30);
            btnPlaceShips.Text = "Расставить корабли";
            btnPlaceShips.Click += BtnPlaceShips_Click;
            this.Controls.Add(btnPlaceShips);

            // Кнопка создания игры
            btnHostGame = new Button();
            btnHostGame.Location = new Point(530, 50);
            btnHostGame.Size = new Size(120, 30);
            btnHostGame.Text = "Создать игру";
            btnHostGame.Click += BtnHostGame_Click;
            this.Controls.Add(btnHostGame);

            // Кнопка подключения
            btnConnect = new Button();
            btnConnect.Location = new Point(660, 50);
            btnConnect.Size = new Size(120, 30);
            btnConnect.Text = "Подключиться";
            btnConnect.Click += BtnConnect_Click;
            this.Controls.Add(btnConnect);

            // Кнопка старта
            btnStartGame = new Button();
            btnStartGame.Location = new Point(400, 90);
            btnStartGame.Size = new Size(120, 30);
            btnStartGame.Text = "Начать игру";
            btnStartGame.Enabled = false;
            btnStartGame.Click += BtnStartGame_Click;
            this.Controls.Add(btnStartGame);

            enemyBoard.SetInteractive(false);
        }

        private void BtnPlaceShips_Click(object sender, EventArgs e)
        {
            game.Player1.PlaceShipsAutomatically();
            playerBoard.UpdateFromGameBoard(game.Player1.Board);
            lblStatus.Text = "Корабли расставлены";
        }

        private async void BtnHostGame_Click(object sender, EventArgs e)
        {
            try
            {
                await network.StartHostAsync();
                lblStatus.Text = "Ожидаем подключения...";
                btnHostGame.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                await network.ConnectAsync("localhost", 8888);
                lblStatus.Text = "Подключено к игре";
                btnConnect.Enabled = false;
                btnStartGame.Enabled = true;

                // Отправляем сообщение о готовности
                await network.SendMessageAsync(NetworkMessage.CreateReadyMessage("Игрок"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void BtnStartGame_Click(object sender, EventArgs e)
        {
            try
            {
                game.StartGame();
                lblStatus.Text = $"Ход: {game.CurrentPlayer.Nickname}";
                btnStartGame.Enabled = false;

                if (network.IsConnected)
                {
                    await network.SendMessageAsync(NetworkMessage.CreateStartGameMessage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void OnEnemyCellClicked(int x, int y)
        {
            if (!network.IsConnected || game.CurrentPlayer != game.Player1)
            {
                return;
            }

            // Отправляем выстрел
            var message = NetworkMessage.CreateShotMessage(x, y, "Игрок");
            await network.SendMessageAsync(message);

            // Проверка локально
            var result = game.Player2.Shoot(x, y);
            enemyBoard.UpdateFromGameBoard(game.Player2.Board);

            lblStatus.Text = $"Выстрел: {result}";

            // Проверяем победу
            var winner = game.CheckWinner();
            if (winner != null)
            {
                MessageBox.Show($"Победил {winner.Nickname}");
                return;
            }

            game.SwitchTurn();
            lblStatus.Text = $"Ход: {game.CurrentPlayer.Nickname}";
        }

        private async void OnNetworkMessage(NetworkMessage message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnNetworkMessage(message)));
                return;
            }

            if (message.Type == "Connect")
            {
                lblStatus.Text = "Кто то подключился";
                btnStartGame.Enabled = true;
            }
            else if (message.Type == "Shot")
            {
                // Получили выстрел от соперника
                var data = System.Text.Json.JsonSerializer.Deserialize<dynamic>(message.Data);
                int x = int.Parse(data.X.ToString());
                int y = int.Parse(data.Y.ToString());

                var result = game.Player1.Shoot(x, y);
                playerBoard.UpdateFromGameBoard(game.Player1.Board);

                // Отправляем результат
                var resultMsg = NetworkMessage.CreateResultMessage(x, y, result.ToString());
                await network.SendMessageAsync(resultMsg);

                // Проверяем победу
                var winner = game.CheckWinner();
                if (winner != null)
                {
                    MessageBox.Show($"Победил {winner.Nickname}");
                    return;
                }

                game.SwitchTurn();
                lblStatus.Text = $"Ход: {game.CurrentPlayer.Nickname}";
            }
            else if (message.Type == "Result")
            {
                // Получили результат нашего выстрела
                lblStatus.Text = $"Результат: {message.Data}";
            }
            else if (message.Type == "StartGame")
            {
                // Начинаем игру
                game.StartGame();
                lblStatus.Text = $"Ход: {game.CurrentPlayer.Nickname}";
                enemyBoard.SetInteractive(true);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            network?.Stop();
            base.OnFormClosing(e);
        }
    }

    public class GameBoardVisual
    {
        public Panel BoardPanel { get; private set; }
        public Button[,] Cells { get; private set; }
        public event Action<int, int> CellClicked;

        public GameBoardVisual(int size, int cellSize, int x, int y, bool isEnemy)
        {
            BoardPanel = new Panel();
            BoardPanel.Location = new Point(x, y);
            BoardPanel.Size = new Size(size * cellSize + 20, size * cellSize + 20);
            BoardPanel.BorderStyle = BorderStyle.FixedSingle;

            Cells = new Button[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Cells[i, j] = new Button();
                    Cells[i, j].Location = new Point(i * cellSize + 10, j * cellSize + 10);
                    Cells[i, j].Size = new Size(cellSize, cellSize);
                    Cells[i, j].Tag = new Point(i, j);
                    Cells[i, j].Click += CellButton_Click;
                    Cells[i, j].BackColor = isEnemy ? Color.LightBlue : Color.White;

                    BoardPanel.Controls.Add(Cells[i, j]);
                }
            }
        }

        private void CellButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var point = (Point)button.Tag;
            CellClicked?.Invoke(point.X, point.Y);
        }

        public void UpdateFromGameBoard(GameBoard board)
        {
            if (board == null) return;

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    var cell = board.Cells[x, y];
                    if (cell != null)
                    {
                        UpdateCell(x, y, cell.State);
                    }
                }
            }
        }

        private void UpdateCell(int x, int y, CellState state)
        {
            var button = Cells[x, y];

            switch (state)
            {
                case CellState.Empty:
                    button.BackColor = Color.White;
                    button.Text = "";
                    break;

                case CellState.Ship:
                    button.BackColor = Color.Gray;
                    button.Text = "#";
                    break;

                case CellState.Miss:
                    button.BackColor = Color.LightBlue;
                    button.Text = "•";
                    break;

                case CellState.Hit:
                    button.BackColor = Color.Red;
                    button.Text = "X";
                    break;

                case CellState.Sunk:
                    button.BackColor = Color.DarkRed;
                    button.Text = "X";
                    break;
            }
        }

        public void SetInteractive(bool interactive)
        {
            foreach (var cell in Cells)
            {
                cell.Enabled = interactive;
            }
        }
    }
}