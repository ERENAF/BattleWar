using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaWar.models
{
    public enum CellState
    {
        Empty,
        Ship,
        Miss,
        Hit,
        Sunk
    }
    public class Cell
    {
        private int _x;
        private int _y;
        public int X
        {
            get => _x; 
            set{
                if (value < 0 || value > 9)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _x = value;
            }
        }
        public int Y
        {
            get => _y;
            set
            {
                if (value < 0 || value > 9)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                _y = value;
            }
        }
        public CellState State;
        public Cell(int x, int y, CellState state = CellState.Empty)
        {
            X = x;
            Y = y;
            State = state;
        }
        public bool IsShip()
        {
            return State == CellState.Ship || State == CellState.Hit || State == CellState.Sunk;
        }
        public bool IsHit()
        {
            return State == CellState.Hit || State == CellState.Sunk || State == CellState.Miss;
        }

        public static string CoordinatesToString(int x, int y)
        {
            if (x < 0 || y < 0 || x>9 || y > 9)
            {
                return "??";
            }

            char column = (char)('A' + x);
            int row = y + 1;
            return $"{column}::{row}";
        }
    }
}
