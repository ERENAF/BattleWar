using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaWar.models
{
    public class Player
    {
        public string Nickname { get; set; }
        public GameBoard Board { get; set; }
        public bool IsReady { get; set; }
        public bool IsMyTurn { get; set; }

        public Player(string nickname) 
        { 
            Nickname = nickname;
            Board = new GameBoard();
            IsReady = false;
            IsMyTurn = false;
        }

        public void PlaceShipsAutomatically()
        {
            Board.InitializeBoard();

            Ship[] ships =
            {
                new Ship(ShipType.Battelship),      // 1 корабль 4палубный
                new Ship(ShipType.Cruiser),         // 2 корабля 3палубных
                new Ship(ShipType.Cruiser),
                new Ship(ShipType.Destroyer),       // 3 корабля 2палубных
                new Ship(ShipType.Destroyer),
                new Ship(ShipType.Destroyer),
                new Ship(ShipType.PatrolBoat),      // 4 корабля 1палубных
                new Ship(ShipType.PatrolBoat),
                new Ship(ShipType.PatrolBoat),
                new Ship(ShipType.PatrolBoat)
    };

            Random random = new Random();

            foreach (var ship in ships)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 100)
                {
                    attempts++;

                    // Пробуем случайную позицию
                    int startX = random.Next(0, Board.Size);
                    int startY = random.Next(0, Board.Size);
                    bool isHorizontal = random.Next(0, 2) == 0;

                    // Проверяем можно ли поставить корабль здесь
                    if (CanPlaceShipAt(startX, startY, ship.ShipSize, isHorizontal))
                    {
                        // Размещаем корабль
                        PlaceShipAt(startX, startY, ship, isHorizontal);
                        placed = true;

                        // Добавляем корабль на доску
                        Board.PlaceShip(ship);
                    }
                }

                if (!placed)
                {
                    Console.WriteLine($"Не удалось разместить {ship.Type}");
                }
            }

            IsReady = true;
            Console.WriteLine("Корабли расставлены!");
        }

        private bool CanPlaceShipAt(int startX, int startY, int size, bool isHorizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int x = isHorizontal ? startX + i : startX;
                int y = isHorizontal ? startY : startY + i;

                if (x >= Board.Size || y >= Board.Size)
                    return false;

                if (Board.Cells[x, y].State != CellState.Empty)
                    return false;

                // Чтобы корабли не касались
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && nx < Board.Size && ny >= 0 && ny < Board.Size)
                        {
                            if (Board.Cells[nx, ny].State == CellState.Ship)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        private void PlaceShipAt(int startX, int startY, Ship ship, bool isHorizontal)
        {
            ship.Cells.Clear();
            ship.Orientation = isHorizontal ? ShipOrientation.Horizontal : ShipOrientation.Vertical;

            for (int i = 0; i < ship.ShipSize; i++)
            {
                int x = isHorizontal ? startX + i : startX;
                int y = isHorizontal ? startY : startY + i;

                var cell = Board.Cells[x, y];
                cell.State = CellState.Ship;
                ship.Cells.Add(cell);
            }
        }

        public bool AllShipsPlaced()
        {
            if (Board == null || Board.Ships == null)
                return false;

            return Board.Ships.Count >= 10;
        }

        private bool TrySetShipPosition(Ship ship, int startX, int startY, ShipOrientation orientation)
        {
            ship.Cells.Clear();
            ship.Orientation = orientation;

            for (int i = 0; i < ship.ShipSize; i++)
            {
                int x = orientation == ShipOrientation.Horizontal ? startX + i : startX;
                int y = orientation == ShipOrientation.Horizontal ? startY : startY + i;

                if (x >= Board.Size || y >= Board.Size)
                    return false;

                var cell = Board.Cells[x, y];
                if (cell == null)
                    return false;

                ship.Cells.Add(cell);
            }

            return true;
        }
        private void ClearShipPosition(Ship ship)
        {
            foreach (var cell in ship.Cells)
            {
                if (cell.State == CellState.Ship)
                {
                    cell.State = CellState.Empty;
                }
            }
            ship.Cells.Clear();
        }
        public ShotResult Shoot(int x, int y)
        {
            if (!IsMyTurn) return ShotResult.Invalid;
            return Board.ReceiveShot(x, y);
        }

    }
}
