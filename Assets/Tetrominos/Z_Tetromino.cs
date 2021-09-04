using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TetrisGame;

namespace Assets.Tetrominos
{
    class Z_Tetromino : Tetromino
    {
        public int getId()
        {
            return (int)TetrominoID.Z;
        }
        public Vector2Int[] GetGridBlocks(int rotation = 0)
        {
            switch (rotation % 4)
            {
                case 0: return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) };
                case 1: return new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2) };
                case 2: return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) };
                case 3: return new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2) };
                default: throw new System.Exception("Invalid rotation!");
            }
        }
    }
}
