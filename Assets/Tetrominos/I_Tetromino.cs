using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TetrisGame;

namespace Assets.Tetrominos
{
    class I_Tetromino : Tetromino
    {
        public int getId()
        {
            return (int)TetrominoID.I;
        }
        public Vector2Int[] GetGridBlocks(int rotation = 0)
        {
            switch (rotation % 4)
            {
                case 0: return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0) };
                case 1: return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3) };
                case 2: return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0) };
                case 3: return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3) };
                default: throw new System.Exception("Invalid rotation!");
            }
        }
    }
}
