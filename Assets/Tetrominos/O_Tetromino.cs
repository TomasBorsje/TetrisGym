using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TetrisGame;

namespace Assets.Tetrominos
{
    class O_Tetromino : Tetromino
    {
        public int getId()
        {
            return (int)TetrominoID.O;
        }
        public Vector2Int[] GetGridBlocks(int rotation = 0)
        {
            return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
        }
    }
}
