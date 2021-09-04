using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Tetromino
{
    public Vector2Int[] GetGridBlocks(int rotation = 0);

    public int getId();
}
