using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeaWar.models
{
    public enum GamePhase
    {
        Placement,
        WaitingConnection,
        Player1Turn,
        Player2Turn,
        GameOver
    }
    public class GameSession
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public GamePhase CurrentPhase { get; set; }
        public Player CurrentPlayer { get; set; }
        public DateTime StartTime { get; set; }
        public List<GameMessage> MessageLog { get; set; }

        public GameSession()
        {
            Player1 = null;
            Player2 = null;
            CurrentPhase = GamePhase.Placement;
            CurrentPlayer = null;
            StartTime = DateTime.Now;
            MessageLog = new List<GameMessage>();
        }

        public GameSession(string player1NickName, string player2NickName) : this()
        {
            Player1 = new Player(player1NickName);
            Player2 = new Player(player2NickName);
        }

        public void StartGame()
        {
            if (Player1 == null || Player2 == null)
                throw new InvalidOperationException("Игроки не инициализированы");

            if (!Player1.AllShipsPlaced() || !Player2.AllShipsPlaced())
                throw new InvalidOperationException("Не все корабли расставлены");

            CurrentPhase = GamePhase.Player1Turn;
            CurrentPlayer = Player1;
            Player1.IsMyTurn = true;
            Player2.IsMyTurn = false;
            StartTime = DateTime.Now;

            AddGameMessage("Game", "Начало игры");
        }

        public void SwitchTurn()
        {
            if (CurrentPhase != GamePhase.Player1Turn && CurrentPhase != GamePhase.Player2Turn)
                return;

            if (CurrentPlayer == Player1)
            {
                CurrentPlayer = Player2;
                Player1.IsMyTurn = false;
                Player2.IsMyTurn = true;
                CurrentPhase = GamePhase.Player2Turn;
            }
            else
            {
                CurrentPlayer = Player1;
                Player1.IsMyTurn = true;
                Player2.IsMyTurn = false;
                CurrentPhase = GamePhase.Player1Turn;
            }
            AddGameMessage("Game", $"Ход: {CurrentPlayer.Nickname}");
        }

        public Player CheckWinner()
        {
            if (Player1.Board.AllShipsSunk)
            {
                CurrentPhase = GamePhase.GameOver;
                AddGameMessage("Game", $"{Player2.Nickname} победил!");
                return Player2;
            }
            if (Player2.Board.AllShipsSunk)
            {
                CurrentPhase = GamePhase.GameOver;
                AddGameMessage("Game", $"{Player1.Nickname} победил!");
                return Player1;
            }
            return null;
        }

        public void SaveGameLog(string filename)
        {
            try
            {
                var logData = new
                {
                    StartTime = StartTime,
                    EndTime = DateTime.Now,
                    Winner = CheckWinner()?.Nickname ?? "Не определен",
                    Players = new[]
                    {
                        new { Name = Player1?.Nickname, ShipsRemaining = Player1?.Board.Ships.Count(s => !s.IsSunk) },
                        new { Name = Player2?.Nickname, ShipsRemaining = Player2?.Board.Ships.Count(s => !s.IsSunk) }
                    },
                    Messages = MessageLog.Select(m => new
                    {
                        m.Command,
                        m.X,
                        m.Y,
                        m.PlayerNickName,
                        m.chatText,
                        m.Result,
                        Timestamp = DateTime.Now
                    }).ToList()
                };

                string json = JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filename, json);

                AddGameMessage("System", $"Лог игры сохранен в {filename}");
            }
            catch (Exception ex)
            {
                AddGameMessage("System", $"Ошибка сохранения лога: {ex.Message}");
            }
        }

        public void AddGameMessage(string command, string text)
        {
            MessageLog.Add(new GameMessage
            {
                Command = command,
                chatText = text,
                PlayerNickName = "System",
                Timestamp = DateTime.Now
            });
        }

        public void AddShotMessage(string playerName, int x, int y, ShotResult result)
        {
            MessageLog.Add(new GameMessage
            {
                Command = "Shot",
                PlayerNickName = playerName,
                X = x,
                Y = y,
                Result = result,
                Timestamp = DateTime.Now
            });
        }
    }
}
