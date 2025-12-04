using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaWar.models
{
    public enum ShipOrientation
    {
        Horizontal,
        Vertical
    };
    public enum ShipType
    {
        Battelship,
        Cruiser,
        Destroyer,
        PatrolBoat
    }
    public class Ship
    {
        public ShipType Type { get; set; }
        public int ShipSize { get; set; }
        public List<Cell> Cells { get; private set; }
        public ShipOrientation Orientation { get; set; }
        public bool IsSunk { get; private set; }

        public Ship(ShipType type)
        {
            Type = type;
            switch (Type)
            {
                case ShipType.Battelship:
                    ShipSize = 4;
                    break;
                case ShipType.Cruiser:
                    ShipSize = 3;
                    break;
                case ShipType.Destroyer:
                    ShipSize = 2;
                    break;
                case ShipType.PatrolBoat:
                    ShipSize = 1;
                    break;
            }
            Cells = new List<Cell>();
            Orientation = ShipOrientation.Vertical;
            IsSunk = false;
        }

        public bool IsValidPosition()
        {
            if (Cells.Count != ShipSize)
            {
                return false;
            }
            if (Orientation == ShipOrientation.Horizontal)
            {
                var firstCell = Cells.First();
                return Cells.All(cell => cell.Y == firstCell.Y) &&
                        Cells.Select(cell => cell.X).OrderBy(x => x).
                        SequenceEqual(Enumerable.Range(Cells.Min(c => c.X), ShipSize))&&
                        Cells.All(cell => cell.State == CellState.Empty);
            }
            else
            {
                var firstCell = Cells.First();
                return Cells.All(cell => cell.X == firstCell.X) &&
                        Cells.Select(cell => cell.Y).OrderBy(y => y).
                        SequenceEqual(Enumerable.Range(Cells.Min(c => c.Y), ShipSize)) &&
                        Cells.All(cell => cell.State == CellState.Empty);
            }
        }
        
        public bool CheckHit(int x, int y)
        {
            var hitCell = Cells.FirstOrDefault(cell => cell.X == x && cell.Y == y);

            if (hitCell != null && hitCell.State == CellState.Ship)
            {
                hitCell.State = CellState.Hit;
                CheckIfSunk();
                return true;
            }
            return false;
        }
        public void CheckIfSunk()
        {
            IsSunk = Cells.All(cell=>cell.State == CellState.Hit || cell.State == CellState.Sunk);
            if (IsSunk)
            {
                foreach (var cell in Cells)
                {
                    cell.State = CellState.Sunk;
                }
            }
        }
    }
}
