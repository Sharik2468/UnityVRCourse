using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static CoreTypes;
using static Valve.VR.SteamVR_Skybox;

public class MazeSpawner : MonoBehaviour
{
    #region PublicSection

    #region VariableSeciton

    // ������� ��� ������� ����������� ������
    public delegate void RestartCompletedEventHandler();

    // �������, ������� ����������, ����� �������� ���������
    public event RestartCompletedEventHandler OnRestartCompleted;
    #endregion

    /// <summary>
    /// ��������� ���������
    /// </summary>
    public SpawnItem Character;
    /// <summary>
    /// ��������� ������
    /// </summary>
    public SpawnItem Exit;

    /// <summary>
    /// ����� ������� ������� �������������, � ������� ����� ���������� ��������
    /// </summary>
    [Header("GridSettings")]
    // ������� ��������������
    public Vector3 topLeft;
    /// <summary>
    /// ������ ������ ������� ��������������, � ������� ����� ���������� ��������
    /// </summary>
    public Vector3 bottomRight;

    /// <summary>
    /// ��������� ���������
    /// </summary>
    [Header("DifficultSettings")]
    public DifficultType Difficult;
    /// <summary>
    /// ��������� ����� ���������
    /// </summary>
    public DifficultStruct easyDifficulty;
    /// <summary>
    /// ��������� ������� ��������� ���������
    /// </summary>
    public DifficultStruct normalDifficulty;
    /// <summary>
    /// ��������� ������� ��������� ���������
    /// </summary>
    public DifficultStruct hardDifficulty;

    [Header("Wall Settings")]
    public SpawnItem[] Walls;

    /// <summary>
    /// ������� ��� ������
    /// </summary>
    [Header("SpawnSettings")]
    public SpawnItem[] Blocks;

    /// <summary>
    /// ������� ��� �������
    /// </summary>
    public BonusItem[] Bonuses;
    /// <summary>
    /// ���������� �������
    /// </summary>
    public int bonusCount = 0;

    /// <summary>
    /// ��������� ����� ��� ������� ��������
    /// </summary>
    public bool randomSpawn;

    /// <summary>
    /// ��������� ����� ������ ��� �� ���������� ������
    /// </summary>
    public List<CellRow> spawnCells = new List<CellRow>();
    /// <summary>
    /// � ������ ������ ������
    /// </summary>
    public int currentSpawnCellIndex = 0;
    /// <summary>
    /// ������ �� ������ ������������� ����� �����������
    /// </summary>
    public bool isAutoIncrementSpawnIndex = false;

    [Header("Audio")]

    /// <summary>
    /// ������ ��������������� ����� ������.
    /// </summary>
    public AudioSource successAudioSource;

    /// <summary>
    /// ������ ��������������� ����� �������.
    /// </summary>
    public AudioSource failureAudioSource;

    /// <summary>
    /// ������ ��������������� ����� �������.
    /// </summary>
    public AudioSource bonusAudioSource;

    #endregion

    #region PrivateSection

    /// <summary>
    /// ���������� �����
    /// </summary>
    private int rows;
    /// <summary>
    /// ���������� ��������
    /// </summary>
    private int columns;

    /// <summary>
    /// ���������� �������� ��� ������
    /// </summary>
    private int spawnCount;

    /// <summary>
    /// ��� ������
    /// </summary>
    private Cell ExitCache;

    /// <summary>
    /// ������� � ����� ������ � ��� ���������
    /// </summary>
    private SortedDictionary<ExitRotationType, int> rotationExitVariant
        = new SortedDictionary<ExitRotationType, int>()
        {
            { ExitRotationType.Top, 0 },
            { ExitRotationType.Right, 90 },
        };

    /// <summary>
    /// ������� ����
    /// </summary>
    private int currentScore = 0;
    /// <summary>
    /// �� ��������� �������
    /// </summary>
    private int notCollectedScore = 0;
    /// <summary>
    /// ��������� �������
    /// </summary>
    private int collectedScore = 0;
    /// <summary>
    /// ��������� ������� ���������
    /// </summary>
    private Tuple<int, int> startPositionLabyrinth;
    /// <summary>
    /// ������� ��� ����������� �������� �� �������
    /// </summary>
    private Dictionary<Vector2Int, ExitRotationType> cellDirections = new Dictionary<Vector2Int, ExitRotationType>();
    /// <summary>
    /// ������� �� ����
    /// </summary>
    private bool isSoundOn = true;
    /// <summary>
    /// ����� ������
    /// </summary>
    private float roundTime;
    /// <summary>
    /// ������ �� ������ ������ ���?
    /// </summary>
    private bool firstTimeTimerSecondChange = false;

    bool[,] maze;

    /// <summary>
    /// ������� ��� ����������
    /// </summary>
    Dictionary<DifficultType, DifficultStruct> difficultyDictionary = new Dictionary<DifficultType, DifficultStruct>();
    #endregion

    #region UnityEvents
    void Start()
    {
        FirstTimeApplyDifficulty();
        SetDifficulty(Difficult);

        SpawnGrid();
    }

    #endregion

    #region PublicFunc

    /// <summary>
    /// �������� ������� ������ ���������, ���������, ������ � �������� � ���������
    /// </summary>
    public void SpawnGrid()
    {
        roundTime = difficultyDictionary[Difficult].secondTimer;

        //������� ������
        DeleteAllObjectsWithTag("LabBlock");
        DeleteAllBonusesFromScene();

        // ������ ������� ������
        float cellWidth = (bottomRight.x - topLeft.x) / columns;
        float cellHeight = (topLeft.z - bottomRight.z) / rows;

        // ���������� ������ ���������� ��������, ���� �����
        if (randomSpawn)
        {
            GenerateMaze();
        }

        foreach (Cell cell in spawnCells[currentSpawnCellIndex].row)
        {
            if (cell.cellType == LabyrinthObjectType.Bonus)
                SpawnBonuses(cellWidth, cellHeight, cell);

            if (cell.cellType == LabyrinthObjectType.Start && Character.Object)
                RelocateCharacter(cellWidth, cellHeight, cell);

            if (cell.cellType == LabyrinthObjectType.Exit && Exit.Object)
                TransformExit(cellWidth, cellHeight, cell);

            if (cell.cellType == LabyrinthObjectType.Empty)
                SpawnBlock(cellWidth, cellHeight, cell.indexCell.x, cell.indexCell.y);
        }

        // �������� �������, ���� �� ���� ���� ����������
        OnRestartCompleted?.Invoke();
    }

    /// <summary>
    /// ���������� ��� ���������� ������
    /// </summary>
    public void StartLevelRestart()
    {
        currentScore += 5;
        collectedScore++;

        DeleteAllBonusesFromScene();

        if (isSoundOn)
            successAudioSource.Play();
    }

    /// <summary>
    /// ���������� ��� ����������� ���������
    /// </summary>
    public void CompleteLevelRestart()
    {
        if (!IsValidCellCount()) randomSpawn = true;
        if (isAutoIncrementSpawnIndex && IsValidCellCount())
            currentSpawnCellIndex++;

        SpawnGrid();
    }

    #endregion

    #region PrivateFunc

    /// <summary>
    /// ������� ��� ����� ������� �����
    /// </summary>
    /// <param name="arg1">���, ��� ������ �������</param>
    /// <param name="soundOn">������ �����</param>
    private void OnSoundsStatusChanged(object arg1, bool soundOn)
    {
        isSoundOn = soundOn;
    }

    /// <summary>
    /// ������� ����� ���������
    /// </summary>
    /// <param name="arg1">���, ��� ������ �������</param>
    /// <param name="difficulty">���������</param>
    private void OnDifficultyChange(object arg1, int difficulty)
    {
        Difficult = (DifficultType)difficulty;
        SetDifficulty(Difficult);
    }

    /// <summary>
    /// ������� ����� ������� �������
    /// </summary>
    /// <param name="arg1">���, ��� ������ �������</param>
    /// <param name="seconds">�����, ������� �����������</param>
    private void OnDifficultyTimerSecondChange(object arg1, float seconds)
    {
        var cachedDifficult = difficultyDictionary[Difficult];
        difficultyDictionary.Remove(Difficult);
        cachedDifficult.secondTimer = (int)seconds;
        difficultyDictionary.Add(Difficult, cachedDifficult);

        if (firstTimeTimerSecondChange) return;
        firstTimeTimerSecondChange = true;
        //RestartCauseWinLevel();
    }

    /// <summary>
    /// ������������� ������� ��������� � ����������� ���������
    /// </summary>
    /// <param name="difficult">���������, ������� ���������� ����������</param>
    private void SetDifficulty(DifficultType difficult)
    {
        for (int i = 0; i < Blocks.Length; i++)
            Blocks[i].scale = difficultyDictionary[difficult].blockScale;
        rows = (int)difficultyDictionary[difficult].gridSize.x;
        columns = (int)difficultyDictionary[difficult].gridSize.y;
    }

    /// <summary>
    /// ��������� ��������� ��������� ��� ����������� �������������
    /// </summary>
    private void FirstTimeApplyDifficulty()
    {
        difficultyDictionary.Add(DifficultType.Easy, easyDifficulty);
        difficultyDictionary.Add(DifficultType.Normal, normalDifficulty);
        difficultyDictionary.Add(DifficultType.Hard, hardDifficulty);
    }

    /// <summary>
    /// ���������� ��������, ������ �� ��������� ������� � �������
    /// </summary>
    private void GenerateMaze()
    {
        spawnCells[currentSpawnCellIndex].row.Clear();

        maze = GenerateMaze(rows, columns);
        CloseAllUnnecessaryExits();
        spawnCells[currentSpawnCellIndex].row.Add
            (new Cell
            {
                indexCell = new Vector2Int(startPositionLabyrinth.Item1, startPositionLabyrinth.Item2),
                cellType = LabyrinthObjectType.Start
            }
            );

        Tuple<int, int> end = FindFurthestCell(maze, startPositionLabyrinth);
        spawnCells[currentSpawnCellIndex].row.Add
            (new Cell
            {
                indexCell = new Vector2Int(end.Item1, end.Item2),
                cellType = LabyrinthObjectType.Exit
            }
            );

        int i = 0;
        int j = 0;

        for (i = 0; i < rows; i++)
            for (j = 0; j < columns; j++)
                if (!maze[i, j])
                    spawnCells[currentSpawnCellIndex].row.Add(new Cell { indexCell = new Vector2Int(i, j), cellType = LabyrinthObjectType.Empty });

        maze[end.Item1, end.Item2] = false;
        maze[startPositionLabyrinth.Item1, startPositionLabyrinth.Item2] = false;
        GenerateBonusesForMaze(maze, out i, out j);
    }

    /// <summary>
    /// ���������� ������������ �������, ������ �� ��������� ���������� � �������
    /// </summary>
    /// <param name="maze">������ ���������</param>
    /// <param name="i"></param>
    /// <param name="j"></param>
    private void GenerateBonusesForMaze(bool[,] maze, out int i, out int j)
    {
        i = UnityEngine.Random.Range(0, rows);
        j = UnityEngine.Random.Range(0, columns);
        int k = 0;
        while (k < bonusCount)
        {
            if (!maze[i, j])
            {
                spawnCells[currentSpawnCellIndex].row.Add
                    (new Cell
                    {
                        indexCell = new Vector2Int(i, j),
                        cellType = (LabyrinthObjectType)(3)
                    }
                    );
                k++;
                maze[i, j] = true;
            }

            i = UnityEngine.Random.Range(0, rows);
            j = UnityEngine.Random.Range(0, columns);
        }
    }

    void CloseAllUnnecessaryExits()
    {
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                // ������� ������ �������� ������ ��� ������ �������������.
                bool isVoid = maze[x, y];

                // ���� ������� ������ �� �����, ��������� ��� ��������� �����������
                if (isVoid)
                {
                    CloseExitIfNecessary(x, y, maze);
                }
            }
        }
    }

    void CloseExitIfNecessary(int x, int y, bool[,] maze)
    {
        Vector3[] directionOffsets = new Vector3[]
    {
    new Vector3(0, 0, -0.5f),
    new Vector3(0, 0, 0.5f),
    new Vector3(-0.5f, 0, 0),
    new Vector3(0.5f, 0, 0),
    };
        Vector3[] rotationOffsets = new Vector3[]
        {
    new Vector3(0, 0, 0), // �����
    new Vector3(0, 0, 0),  // ��
    new Vector3(0, 90, 0), // �����
    new Vector3(0, 90, 0)   // ������
        };
        Tuple<int, int>[] directions = new Tuple<int, int>[]
        {
    new Tuple<int, int>(x - 1, y), // �����
    new Tuple<int, int>(x + 1, y), // ��
    new Tuple<int, int>(x, y - 1), // �����
    new Tuple<int, int>(x, y + 1)  // ������
        };

        for (int i = 0; i < directions.Length; i++)
        {
            var dir = directions[i];
            // ���� ����������� ��������� � �������� ��������� � �������� ������
            if (dir.Item1 >= 0 && dir.Item1 < maze.GetLength(0) && dir.Item2 >= 0 && dir.Item2 < maze.GetLength(1) && !maze[dir.Item1, dir.Item2])
            {
                // �������� ������� � ����������� �����
                SpawnWallAt(x, y, directionOffsets[i], rotationOffsets[i]);
            }
        }
    }


    private void SpawnWallAt(int x, int y, Vector3 directionOffset, Vector3 rotationOffsets)
    {
        if (Walls.Length > 0)
        {
            SpawnItem wallItem = Walls[UnityEngine.Random.Range(0, Walls.Length)];
            Vector3 spawnPosition = CalculateWallPosition(x, y, directionOffset);

            GameObject spawnedWall = Instantiate(wallItem.Object, spawnPosition, Quaternion.identity, transform);
            spawnedWall.transform.localScale = wallItem.scale;

            // ������� �����, ���� ���������
            spawnedWall.transform.rotation = Quaternion.Euler(rotationOffsets.x, rotationOffsets.y, rotationOffsets.z);
        }
    }


    private Vector3 CalculateWallPosition(int x, int y, Vector3 directionOffset)
    {
        float cellWidth = (bottomRight.x - topLeft.x) / columns;
        float cellHeight = (topLeft.z - bottomRight.z) / rows;
        Vector3 position = new Vector3(topLeft.x + (y + 0.5f + directionOffset.x) * cellWidth, 0, topLeft.z - (x + 0.5f + directionOffset.z) * cellHeight);
        return position;
    }

    /// <summary>
    /// ���� ����� ������� ����� �� ��������� � ���������
    /// </summary>
    /// <param name="maze">��������</param>
    /// <param name="start">�����, � ������� ���������� ��������</param>
    /// <returns></returns>
    Tuple<int, int> FindFurthestCell(bool[,] maze, Tuple<int, int> start)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        bool[,] visited = new bool[height, width];
        Queue<Tuple<int, int>> queue = new Queue<Tuple<int, int>>();
        Tuple<int, int> current = start;

        queue.Enqueue(start);
        visited[start.Item1, start.Item2] = true;

        while (queue.Count > 0)
        {
            current = queue.Dequeue();

            // ��������� �������
            List<Tuple<int, int>> neighbors = GetNeighbors(current, height, width);
            foreach (Tuple<int, int> neighbor in neighbors)
            {
                if (!maze[neighbor.Item1, neighbor.Item2] && !visited[neighbor.Item1, neighbor.Item2])
                {
                    queue.Enqueue(neighbor);
                    visited[neighbor.Item1, neighbor.Item2] = true;
                }
            }
        }

        return current;
    }

    /// <summary>
    /// ��������� �������� ������
    /// </summary>
    /// <param name="cell">������</param>
    /// <param name="height">������ ������</param>
    /// <param name="width">������ ������</param>
    /// <returns></returns>
    List<Tuple<int, int>> GetNeighbors(Tuple<int, int> cell, int height, int width)
    {
        List<Tuple<int, int>> neighbors = new List<Tuple<int, int>>();

        // �����
        if (cell.Item1 > 0)
        {
            neighbors.Add(new Tuple<int, int>(cell.Item1 - 1, cell.Item2));
            cellDirections[new Vector2Int(cell.Item1 - 1, cell.Item2)] = ExitRotationType.Top;
        }
        // ��
        if (cell.Item1 < height - 1)
        {
            neighbors.Add(new Tuple<int, int>(cell.Item1 + 1, cell.Item2));
            cellDirections[new Vector2Int(cell.Item1 + 1, cell.Item2)] = ExitRotationType.Top;
        }
        // �����
        if (cell.Item2 > 0)
        {
            neighbors.Add(new Tuple<int, int>(cell.Item1, cell.Item2 - 1));
            cellDirections[new Vector2Int(cell.Item1, cell.Item2 - 1)] = ExitRotationType.Right;
        }
        // ������
        if (cell.Item2 < width - 1)
        {
            neighbors.Add(new Tuple<int, int>(cell.Item1, cell.Item2 + 1));
            cellDirections[new Vector2Int(cell.Item1, cell.Item2 + 1)] = ExitRotationType.Right;
        }

        return neighbors;
    }

    /// <summary>
    /// ��������� ������ ���������
    /// </summary>
    /// <param name="width">������ ���������</param>
    /// <param name="height">������ ���������</param>
    /// <returns></returns>
    private bool[,] GenerateMaze(int width, int height)
    {
        bool[,] maze = new bool[height, width];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                maze[y, x] = true;

        Stack<Tuple<int, int>> stack = new Stack<Tuple<int, int>>();
        Tuple<int, int> start = new Tuple<int, int>(0, 0);
        maze[start.Item1, start.Item2] = false;
        startPositionLabyrinth = start;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Tuple<int, int> v = stack.Peek();
            List<Tuple<int, int>> neighbors = new List<Tuple<int, int>>();

            // �����
            if (v.Item1 > 1 && maze[v.Item1 - 2, v.Item2])
                neighbors.Add(new Tuple<int, int>(v.Item1 - 2, v.Item2));
            // ��
            if (v.Item1 < height - 2 && maze[v.Item1 + 2, v.Item2])
                neighbors.Add(new Tuple<int, int>(v.Item1 + 2, v.Item2));
            // �����
            if (v.Item2 > 1 && maze[v.Item1, v.Item2 - 2])
                neighbors.Add(new Tuple<int, int>(v.Item1, v.Item2 - 2));
            // ������
            if (v.Item2 < width - 2 && maze[v.Item1, v.Item2 + 2])
                neighbors.Add(new Tuple<int, int>(v.Item1, v.Item2 + 2));

            if (neighbors.Count > 0)
            {
                System.Random rand = new System.Random();

                int randIndex = rand.Next(neighbors.Count);
                Tuple<int, int> w = neighbors[randIndex];

                // ������� �����
                if (w.Item1 > v.Item1)
                    maze[v.Item1 + 1, v.Item2] = false;
                else if (w.Item1 < v.Item1)
                    maze[v.Item1 - 1, v.Item2] = false;
                else if (w.Item2 > v.Item2)
                    maze[v.Item1, v.Item2 + 1] = false;
                else if (w.Item2 < v.Item2)
                    maze[v.Item1, v.Item2 - 1] = false;

                maze[w.Item1, w.Item2] = false;
                stack.Push(w);
            }
            else
            {
                stack.Pop();
            }
        }

        return maze;
    }

    /// <summary>
    /// ����������� ������ �� ������
    /// </summary>
    /// <param name="cellWidth">������ ������</param>
    /// <param name="cellHeight">������ ������</param>
    /// <param name="cell">���� ��������� ������</param>
    /// <returns></returns>
    private Cell SpawnBonuses(float cellWidth, float cellHeight, Cell cell)
    {
        // �������� ��������� ������ ��� ������
        BonusItem Item = Bonuses[UnityEngine.Random.Range(0, Bonuses.Length)];
        GameObject curBonus = Item.Object;
        switch (Item.Type)
        {
            case BonusType.OnePoint: break;
            case BonusType.TwoPoint: break;
            case BonusType.ThreePoint: break;
        }

        // ������� ��� ������
        Vector3 spawnPosition = new Vector3(topLeft.x + Item.locationOffset.x + cell.indexCell.y * cellWidth + cellWidth / 2,
                                            topLeft.y + Item.locationOffset.y,
                                            topLeft.z + Item.locationOffset.z - cell.indexCell.x * cellHeight - cellHeight / 2);
        // ����� �������
        GameObject spawnedObject =
            Instantiate(curBonus, spawnPosition, Quaternion.identity);
        spawnedObject.transform.localScale =
            new Vector3(Item.scale.x, Item.scale.y, Item.scale.z);

        spawnedObject.tag = Item.Type.ToString();  // ���������� ���� �������
        return cell;
    }



    /// <summary>
    /// �������� ���� ������� � ������. ����������, ��� ��� � ������� ������ ����.
    /// </summary>
    private static void DeleteAllBonusesFromScene()
    {
        for (int i = 0; i < Enum.GetValues(typeof(BonusType)).Cast<int>().Max(); i++)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(((BonusType)i).ToString());

            if (gameObjects.Count() != 0)
            {
                for (var j = 0; j < gameObjects.Length; j++)
                {
                    Destroy(gameObjects[j]);
                }
            }
        }
    }

    private bool IsValidCellCount()
    {
        return spawnCells.Count() - 1 > currentSpawnCellIndex;
    }

    /// <summary>
    /// ������� ������������ ����� �������
    /// </summary>
    private void ClearCellsNearExit()
    {
        Cell currentCell = ExitCache;
        currentCell.indexCell.x++;
        currentCell.cellType = LabyrinthObjectType.Empty;
        spawnCells[currentSpawnCellIndex].row.Remove(currentCell);

        currentCell = ExitCache;
        currentCell.indexCell.x--;
        currentCell.cellType = LabyrinthObjectType.Empty;
        spawnCells[currentSpawnCellIndex].row.Remove(currentCell);

        currentCell = ExitCache;
        currentCell.indexCell.y++;
        currentCell.cellType = LabyrinthObjectType.Empty;
        spawnCells[currentSpawnCellIndex].row.Remove(currentCell);

        currentCell = ExitCache;
        currentCell.indexCell.y--;
        currentCell.cellType = LabyrinthObjectType.Empty;
        spawnCells[currentSpawnCellIndex].row.Remove(currentCell);
    }

    /// <summary>
    /// ������� ����� � ����������� �����
    /// </summary>
    /// <param name="tag">��� ��� ������ ������ ��� ��������</param>
    private void DeleteAllObjectsWithTag(string tag)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);

        for (var i = 0; i < gameObjects.Length; i++)
        {
            Destroy(gameObjects[i]);
        }
    }

    /// <summary>
    /// ����� �������������� ������ ��������� ���������. ������ �������� ��������� ����� � ��������� ������.
    /// </summary>
    private void SpawnNewRandomCells()
    {
        spawnCells[currentSpawnCellIndex].row = new List<Cell>();
        for (int i = 0; i < spawnCount + 2 + bonusCount; i++)
        {
            Cell randomCell;
            do
            {
                randomCell.indexCell = new Vector2Int(
                    UnityEngine.Random.Range(0, rows),
                    UnityEngine.Random.Range(0, columns)
                    );
                randomCell.cellType = LabyrinthObjectType.Empty;
            }
            while (spawnCells[currentSpawnCellIndex].row.Contains(randomCell)); // ���������, ����� �� �������� ��� ������� � ����� ������

            randomCell.cellType = LabyrinthObjectType.Empty;
            if (i == spawnCount - 1)
                randomCell.cellType = LabyrinthObjectType.Start;
            if (i == spawnCount)
            {
                randomCell.cellType = LabyrinthObjectType.Exit;
                ExitCache = randomCell;
            }
            if (IsValueBetween(spawnCount - bonusCount - 2, spawnCount - 1, i))
                randomCell.cellType = LabyrinthObjectType.Bonus;
            spawnCells[currentSpawnCellIndex].row.Add(randomCell);
        }
    }

    static bool IsValueBetween(float p1, float p2, float m)
    {
        return ((p1 < m) && (m < p2)) || ((p2 < m) && (m < p1));
    }

    /// <summary>
    /// �������� ���������, ������ ������
    /// </summary>
    /// <param name="cellWidth">������ ������</param>
    /// <param name="cellHeight">������ ������</param>
    /// <param name="cell">���� ��������� ������</param>
    private void TransformExit(float cellWidth, float cellHeight, Cell cell)
    {
        // ������� ��� ������
        Vector3 spawnPosition = new Vector3(topLeft.x + cell.indexCell.y * cellWidth + cellWidth / 2,
                                            topLeft.y,
                                            topLeft.z - cell.indexCell.x * cellHeight - cellHeight / 2);
        Exit.Object.transform.position = spawnPosition + Exit.locationOffset;
        Exit.Object.transform.localScale = Exit.scale;

        RotateExitForCellLocation(cell);
    }

    /// <summary>
    /// ������������ ����� � ����������� ���������.
    /// </summary>
    /// <param name="cell">������, � ������� ���������� �����</param>
    private void RotateExitForCellLocation(Cell cell)
    {
        int currentRotation = 0;
        if (cellDirections.TryGetValue(cell.indexCell, out ExitRotationType direction))
        {
            rotationExitVariant.TryGetValue(direction, out currentRotation);
        }
        else
        {
            rotationExitVariant.TryGetValue(
                (ExitRotationType)UnityEngine.Random.Range(0, 1),
                out currentRotation);
        }

        Exit.Object.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

    }

    /// <summary>
    /// ���������� ��������� � ������ �����
    /// </summary>
    /// <param name="cellWidth">������ ������</param>
    /// <param name="cellHeight">������ ������</param>
    /// <param name="cell">���� ��������� ������</param>
    private void RelocateCharacter(float cellWidth, float cellHeight, Cell cell)
    {
        // ������� ��� ������
        Vector3 spawnPosition = new Vector3(topLeft.x + cell.indexCell.y * cellWidth + cellWidth / 2,
                                            topLeft.y,
                                            topLeft.z - cell.indexCell.x * cellHeight - cellHeight / 2);
        Character.Object.transform.position = spawnPosition + Character.locationOffset;
        Character.Object.transform.localScale = Character.scale;
    }

    /// <summary>
    /// ����������� ����������� �� �����
    /// </summary>
    /// <param name="cellWidth">������ ������</param>
    /// <param name="cellHeight">������ ������</param>
    /// <param name="i">������ y</param>
    /// <param name="j">������ �� x</param>
    private void SpawnBlock(float cellWidth, float cellHeight, int i, int j)
    {
        // �������� ��������� ������ ��� ������
        SpawnItem Item = Blocks[UnityEngine.Random.Range(0, Blocks.Length)];
        GameObject prefab = Item.Object;

        // ������� ��� ������
        Vector3 spawnPosition = new Vector3(topLeft.x + Item.locationOffset.x + j * cellWidth + cellWidth / 2,
                                            topLeft.y + Item.locationOffset.y,
                                            topLeft.z + Item.locationOffset.z - i * cellHeight - cellHeight / 2);
        // ����� �������
        GameObject spawnedObject =
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        spawnedObject.transform.localScale =
            new Vector3(Item.scale.x, Item.scale.y, Item.scale.z);

        spawnedObject.tag = "LabBlock";  // ���������� ���� �������
    }

    #endregion

}
