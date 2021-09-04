using Assets.Tetrominos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TetrisGame : MonoBehaviour
{
    // Enum to represent an item on the grid
    public enum GridItem : int
    {
        EMPTY = 0,
        BLOCK = 1,
        PLAYERBLOCK = 2
    }

    // Enum to represent every type of tetromino
    public enum TetrominoID : int
    {
        I = 0,
        J = 1,
        L = 2,
        O = 3,
        S = 4,
        T = 5,
        Z = 6
    }

    /// <summary>
    /// Class that represents the currently controlled tetromino.
    /// </summary>
    public class ActiveTetromino {
        public Tetromino tetrominoShape;
        public Vector2Int position;
        
        public int rotation = 0;
        public ActiveTetromino(Tetromino shape, Vector2Int position)
        {
            tetrominoShape = shape;
            this.position = position;
        }
    }

    public static int WIDTH = 10;
    public static int HEIGHT = 20;
    public static int INSTANCE = 0;
    public static Vector3 TOP_LEFT = new Vector3(-150, 170, 0); // Top left of the grid on the UI
    public static Vector2Int SPAWN_POSITION = new Vector2Int(4,1); // The top left of where new pieces spawn
    System.Random random;

    public TetrisAgent Agent; // The agent who will observe the game and make decisions
    public Transform Canvas;
    public GridItem[,] Grid = new GridItem[WIDTH,HEIGHT]; // X left to right, then Y top to bottom

    public ActiveTetromino activeTetromino;

    public bool RenderGUI = true;
    GameObject[,] GUIObjects = new GameObject[WIDTH,HEIGHT];
    //Thread TickThread;
    public float Score = 0;
    public bool GameLost = false;

    float tickRate = 0;

    /// <summary>
    /// Returns a one-hot encoding of the board state.
    /// </summary>
    /// <returns></returns>
    public List<float> GetBoardObservation()
    {
        List<float> list = new List<float>();
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                list.Add(Grid[x, y] == GridItem.EMPTY ? 1 : 0);
                list.Add(Grid[x, y] == GridItem.BLOCK ? 1 : 0);
                list.Add(Grid[x, y] == GridItem.PLAYERBLOCK ? 1 : 0);
            }
        }
        return list;
    }

    /// <summary>
    /// Every tick of the board. Runs every second.
    /// </summary>
    void Tick()
    {
        AttemptMove(0, 1); // Move the player tetromino down so they fall naturally
    }

    /// <summary>
    /// Resets the game state.
    /// </summary>
    public void ResetGame()
    {
        Score = 0;
        GameLost = false;
        //TickThread.Abort();
        Begin();
    }

    /// <summary>
    /// Initialises the game state.
    /// </summary>
    void Begin()
    {
        Grid = new GridItem[WIDTH, HEIGHT];

        SpawnNewTetromino();


        // Do not use background threads as it can be out of sync with FixedUpdate()
        //TickThread = new Thread(() =>
        //{
        //    Thread.CurrentThread.IsBackground = true;
        //    while (true)
        //    {
        //        Tick();
        //        Thread.Sleep(1000);
        //    }
        //});
        //TickThread.Start();

    }

    private void Awake()
    {
        INSTANCE++;
        random = new System.Random(INSTANCE+System.DateTime.Now.Millisecond); // Different random seeds per agent. Allows for multi agent training with different games.
    }

    int linesCleared = 0;
    /// <summary>
    /// Clears finished rows and returns the corresponding reward.
    /// </summary>
    /// <returns>The ML Agent's reward</returns>
    float ClearFinishedRows()
    {
        for(int y = 0; y < HEIGHT; y++)
        {
            bool completedRow = true;
            for(int x = 0; x < WIDTH; x++)
            {
                if(Grid[x,y] != GridItem.BLOCK)
                {
                    completedRow = false;
                    break;
                }
            }
            if (completedRow)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    Grid[x, y] = GridItem.EMPTY;
                }
                for (int rowToCopyAbove = y; rowToCopyAbove > 0; rowToCopyAbove--)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        Grid[x, rowToCopyAbove] = rowToCopyAbove == 0 ? GridItem.EMPTY : Grid[x, rowToCopyAbove - 1];
                    }
                }
                linesCleared++;
                return ClearFinishedRows();
            }
        }
        //Debug.Log("Cleared " + linesCleared + " lines!");
        switch(linesCleared)
        {
            case 1: Score += 0.4f; linesCleared = 0; return 1.0f;
            case 2: Score += 1.0f; linesCleared = 0; return 1.0f;
            case 3: Score += 3.0f; linesCleared = 0; return 1.0f;
            case 4: Score += 12.0f; linesCleared = 0; return 1.0f;
            default: return 0f;
        }
    }

    /// <summary>
    /// Attempts to rotate the current active tetromino.
    /// </summary>
    public void AttemptRotate()
    {
        bool canRotate = true;
        foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks(activeTetromino.rotation+1))
        {
            Vector2Int movedPos = activeTetromino.position + offset;
            if (movedPos.x < 0 || movedPos.x >= WIDTH || movedPos.y < 0 || movedPos.y >= HEIGHT || Grid[movedPos.x, movedPos.y] == GridItem.BLOCK) // If we're at the edge of the grid or there is a block in the way
            {
                canRotate = false;
                break;
            }
        }
        if (canRotate)
        {
            foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks(activeTetromino.rotation)) // Remove old blocks
            {
                Vector2Int newPos = activeTetromino.position + offset;
                Grid[newPos.x, newPos.y] = GridItem.EMPTY;
            }
            foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks(activeTetromino.rotation + 1)) // Re add shifted blocks
            {
                Vector2Int newPos = activeTetromino.position + offset;
                Grid[newPos.x, newPos.y] = GridItem.PLAYERBLOCK;
            }
            activeTetromino.rotation++; // Rotate
            //Debug.Log($"Rotated.");
        }
    }

    /// <summary>
    /// Attempts to move the current tetromino by the given offset.
    /// Returns the award given to the agent for this move.
    /// </summary>
    /// <param name="x">Amount to move right</param>
    /// <param name="y">Amount to move down</param>
    /// <returns>The reward given to the agent</returns>
    public float AttemptMove(int x, int y)
    {
        bool canMove = true;
        foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks(activeTetromino.rotation))
        {
            Vector2Int movedPos = activeTetromino.position + offset + new Vector2Int(x, y);
            if (movedPos.x < 0 || movedPos.x >= WIDTH || movedPos.y < 0 || movedPos.y >= HEIGHT || Grid[movedPos.x, movedPos.y] == GridItem.BLOCK) // If we're at the edge of the grid or there is a block in the way
            {
                canMove = false;
                break;
            }
        }
        if (canMove)
        {
            foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks(activeTetromino.rotation)) // Remove old blocks
            {
                Vector2Int newPos = activeTetromino.position + offset;
                Grid[newPos.x, newPos.y] = GridItem.EMPTY;
            }
            foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks(activeTetromino.rotation)) // Re add shifted blocks
            {
                Vector2Int newPos = activeTetromino.position + offset + new Vector2Int(x, y);
                Grid[newPos.x, newPos.y] = GridItem.PLAYERBLOCK;
            }
            activeTetromino.position += new Vector2Int(x, y); // Move tracker down
            if (y > 0)
            {
                return 0.0006f; // Small reward for moving down without placing
            }
            //Debug.Log($"Moved to ({activeTetromino.position.x}, {activeTetromino.position.y}).");
        }
        else if(y > 0)
        {
            //Debug.Log("Cant move down! Imprinting onto grid.");
            foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks(activeTetromino.rotation)) // Replace with new blocks as we have reached the bottom
            {
                Vector2Int newPos = activeTetromino.position + offset;
                Grid[newPos.x, newPos.y] = GridItem.BLOCK;
            }
            float reward = ClearFinishedRows();
            //float noClearReward = (activeTetromino.position.y / (float)HEIGHT) * 0.015f; // The higher the block is placed, the less the reward 
            SpawnNewTetromino();
            return reward;
            
        }
        return 0;
    }
    /// <summary>
    /// Attempts to create a new tetromino at the spawn point and assigns the player control of it.
    /// Will set the game as lost if the tetromino cannot be spawned.
    /// </summary>
    void SpawnNewTetromino()
    {
        Tetromino next;
        switch(random.Next(0,7))
        {
            case 0: next = new I_Tetromino(); break;
            case 1: next = new O_Tetromino(); break;
            case 2: next = new T_Tetromino(); break;
            case 3: next = new S_Tetromino(); break;
            case 4: next = new Z_Tetromino(); break;
            case 5: next = new J_Tetromino(); break;
            case 6: next = new L_Tetromino(); break;
            default: next = new I_Tetromino(); break;
        }
        activeTetromino = new ActiveTetromino(next, SPAWN_POSITION);

        // TODO: Fix this. Rotating doesn't take into account this random start
        //activeTetromino.rotation = random.Next(0,4); // Randomise starting position so AI has more varied experience

        bool canSpawn = true;
        foreach(Vector2Int offset in next.GetGridBlocks(activeTetromino.rotation))
        {
            Vector2Int spawnGrid = SPAWN_POSITION + offset;
            if (Grid[spawnGrid.x,spawnGrid.y] != GridItem.EMPTY)
            {
                canSpawn = false;
                break;
            }
        }
        if(canSpawn)
        {
            foreach (Vector2Int offset in activeTetromino.tetrominoShape.GetGridBlocks())
            {
                Vector2Int spawnGrid = SPAWN_POSITION + offset;
                Grid[spawnGrid.x, spawnGrid.y] = GridItem.PLAYERBLOCK;
            }
        }
        else 
        {
            GameLost = true;
            Debug.Log("The game has ended! Score: " + Score);
        }
    }

    /// <summary>
    /// Redraws the game onto the in-game GUI.
    /// </summary>
    void RedrawGUI()
    {
        if (RenderGUI)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    GUIObjects[x, y].GetComponent<Image>().color = (Grid[x, y] == GridItem.EMPTY) ? Color.white : (Grid[x, y] == GridItem.PLAYERBLOCK) ? Color.blue : Color.green;
                }
            }
        }
    }
    private void FixedUpdate()
    {
        tickRate += Time.deltaTime;
        if (tickRate > 1) // Tick every second
        {
            Tick();
            tickRate = 0;
        }
    }

    void Start()
    {
        // Create GUI if enabled
        if (RenderGUI)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    GameObject obj = Instantiate(Resources.Load<GameObject>("GridBlock"));
                    obj.transform.SetParent(Canvas);
                    obj.transform.localPosition = TOP_LEFT + new Vector3(x * 20, y * -20);
                    obj.name += $" ({x}, {y})";
                    GUIObjects[x, y] = obj;
                }
            }
        }

        Begin();
    }

    void Update()
    {
        //tickRate += Time.deltaTime;
        //if (tickRate > 1)
        //{
        //    Tick();
        //    tickRate = 0;
        //}
        //if (Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    AttemptMove(-1, 0);
        //}
        //if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    AttemptMove(1, 0);
        //}
        //if (Input.GetKeyDown(KeyCode.DownArrow))
        //{
        //    AttemptMove(0, 1);
        //}
        //if (Input.GetKeyDown(KeyCode.UpArrow))
        //{
        //    AttemptRotate();
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    ResetGame();
        //}
        RedrawGUI();
    }
}