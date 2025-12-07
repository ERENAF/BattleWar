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
                new Ship(ShipType.Battelship),
                new Ship(ShipType.Cruiser),
                new Ship(ShipType.Cruiser),
                new Ship(ShipType.Destroyer),
                new Ship(ShipType.Destroyer),
                new Ship(ShipType.Destroyer),
                new Ship(ShipType.PatrolBoat),
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

                    int startX = random.Next(0, Board.Size);
                    int startY = random.Next(0, Board.Size);
                    var orientation = random.Next(0, 2) == 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical;

                    ClearShipPosition(ship);

                    bool valid = TrySetShipPosition(ship, startX, startY, orientation);

                    if (valid && Board.IsValidShipPlacement(ship))
                    {
                        placed = Board.PlaceShip(ship);
                    }
                }
            }
            IsReady = true;
        }

        public bool AllShipsPlaced()
        {
            return true;
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
