using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaWar.models
{
    public class GameBoard
    {
        public int Size { get; set; }
        public Cell[,] Cells { get; set; }
        public List<Ship> Ships { get; set; }
        public List<Cell> Shots { get; set; }
        public bool AllShipsSunk { get; set; }

        public GameBoard(int size = 10)
        {
            Size = size;
            Cells = new Cell[Size,Size];
            Ships = new List<Ship>();
            Shots = new List<Cell>();
            AllShipsSunk = false;
            InitializeBoard();
            
        }

        public void InitializeBoard()
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    Cells[x,y] = new Cell(x, y);
                }
            }

            Ships.Clear();
            Shots.Clear();
            AllShipsSunk = false;
        }

        public bool PlaceShip(Ship ship)
        {
            if (ship == null) return false;
            if (!IsValidShipPlacement(ship)) return false;
            Ships.Add(ship);
            return true;
        }

        public ShotResult ReceiveShot(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Size || y >= Size) return ShotResult.Invalid;

            var cell = Cells[x,y];

            if (cell.IsHit()) return ShotResult.Invalid;

            Shots.Add(cell);

            if (cell.IsShip())
            {
                cell.State = CellState.Hit;
                foreach (var ship in Ships)
                {
                    if (ship.CheckHit(x, y))
                    {
                        if (ship.IsSunk)
                        {
                            MarkSunkenShip(ship);
                            CheckAllShipsSunk();
                            return ShotResult.Sunk;
                        }
                        return ShotResult.Hit;
                        
                    }
                }
            }
            cell.State = CellState.Miss;
            return ShotResult.Miss;
        }

        public bool IsValidShipPlacement(Ship ship)
        {
            if (ship == null || !ship.IsValidPosition()) return false;

            foreach (var cell in ship.Cells)
            {
                if (cell.X < 0 || cell.X >= Size || cell.Y < 0 || cell.Y >= Size) return false;

                if (Cells[cell.X, cell.Y].IsShip()) return false;

                if (!IsCellFreeForShip(cell.X, cell.Y)) return false;
            }
            return true;
        }

        private void MarkSunkenShip(Ship ship)
        {
            if (ship == null || !ship.IsSunk) return;

            foreach (var cell in ship.Cells)
            {
                cell.State = CellState.Sunk;
            }
            MarkMissAroundShip(ship);
        }

        private void MarkMissAroundShip(Ship ship)
        {
            foreach (var cell in ship.Cells)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int newX = cell.X + dx;
                        int newY = cell.Y + dy;

                        if (newX >= 0 && newX < Size && newY >= 0 && newY < Size)
                        {
                            var aroundCell = Cells[newX, newY];
                            if (aroundCell.State == CellState.Empty)
                            {
                                aroundCell.State = CellState.Miss;
                            }
                        }
                    }
                }
            }
        }
        private bool IsCellFreeForShip(int  x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int newX = x + dx;
                    int newY = y + dy;

                    if (newX >= 0 && newX < Size && newY >= 0 && newY < Size)
                    {
                        if (Cells[newY, newX].IsShip()) return false;
                    }
                }
            }
            return true;
        }

        private void CheckAllShipsSunk()
        {
            AllShipsSunk = Ships.All(ship => ship.IsSunk);
        }
    }
}
