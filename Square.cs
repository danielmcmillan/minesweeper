using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minesweeper
{
    [Serializable]
    public class Square
    {
        public Square()
        {
            IsFlagged = false;
            Number = -1;
            IsMine = false;
        }

        /// <summary>
        /// Whether the square has been flagged as a mine by the player
        /// </summary>
        public bool IsFlagged
        {
            get;
            set;
        }

        /// <summary>
        /// The number of the square for after is has been uncovered.
        /// When square is not uncovered, Number equals -1
        /// </summary>
        public int Number
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the square is a mine
        /// </summary>
        public bool IsMine
        {
            get;
            set;
        }
    }
}
