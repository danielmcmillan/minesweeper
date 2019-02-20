using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minesweeper
{
    [Serializable]
    public class BoardState
    {
        public Square[,] Squares;
        public int MineCount = 0;
        public int SquaresLeft = 0;
    }
}
