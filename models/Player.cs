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
        }

        public bool AllShipsPlaced()
        {
            int[] requiredShips = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            var placedShips = Board.Ships.Select(s => s.ShipSize).OrderBy(s => s).ToList();
            var requiredList = requiredShips.OrderBy(s => s).ToList();

            if (placedShips.Count != requiredList.Count)
            {
                return false;
            }
            for (int i = 0; i < placedShips.Count; i++)
            {
                if (placedShips[i] != requiredList[i]) return false;
            }

            return true;
        }

        public ShotResult Shoot(int x, int y)
        {
            if (!IsMyTurn) return ShotResult.Invalid;
            return Board.ReceiveShot(x, y);
        }

    }
}
