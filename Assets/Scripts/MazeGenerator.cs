using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] public int mazeRows;
    [SerializeField] public int mazeColumns;

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] public bool disableCellSprite;

    [HideInInspector] private int centreSize = 2;
    [HideInInspector] private Dictionary<Vector2, Cell> allCells = new Dictionary<Vector2, Cell>();
    [HideInInspector] private List<Cell> unvisited = new List<Cell>();
    [HideInInspector] private List<Cell> stack = new List<Cell>();
    [HideInInspector] private Cell[] centreCells = new Cell[4];

    [HideInInspector] private Cell currentCell;
    [HideInInspector] private Cell checkCell;

    [HideInInspector] private Vector2[] neighbourPositions = new Vector2[]
        { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, -1) };

    [HideInInspector] private float cellSize;

    [HideInInspector] private GameObject mazeParent;

    private void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        if (mazeParent != null) DeleteMaze();

        CreateLayout();
    }

    private void CreateLayout()
    {
        InitValues();

        Vector2 startPos = new Vector2(-(cellSize * (mazeColumns / 2)) + (cellSize / 2),
            -(cellSize * (mazeRows / 2)) + (cellSize / 2));
        Vector2 spawnPos = startPos;

        for (int x = 1; x <= mazeColumns; x++)
        {
            for (int y = 1; y <= mazeRows; y++)
            {
                GenerateCell(spawnPos, new Vector2(x, y));

                spawnPos.y += cellSize;
            }

            spawnPos.y = startPos.y;
            spawnPos.x += cellSize;
        }

        CreateCentre();
        RunAlgorithm();
        MakeExit();
    }

    public void RunAlgorithm()
    {
        unvisited.Remove(currentCell);

        while (unvisited.Count > 0)
        {
            List<Cell> unvisitedNeighbours = GetUnvisitedNeighbours(currentCell);
            if (unvisitedNeighbours.Count > 0)
            {
                checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];

                stack.Add(currentCell);

                CompareWalls(currentCell, checkCell);

                currentCell = checkCell;

                unvisited.Remove(currentCell);
            }
            else if (stack.Count > 0)
            {
                currentCell = stack[stack.Count - 1];

                stack.Remove(currentCell);
            }
        }
    }

    private void MakeExit()
    {
        List<Cell> edgeCells = new List<Cell>();

        foreach (KeyValuePair<Vector2, Cell> cell in allCells)
        {
            if (cell.Key.x == 0 || cell.Key.x == mazeColumns || cell.Key.y == 0 || cell.Key.y == mazeRows)
            {
                edgeCells.Add(cell.Value);
            }
        }

        Cell newCell = edgeCells[Random.Range(0, edgeCells.Count)];

        if (newCell.gridPos.x == 0) RemoveWall(newCell.cScript, 1);
        else if (newCell.gridPos.x == mazeColumns) RemoveWall(newCell.cScript, 2);
        else if (newCell.gridPos.y == mazeRows) RemoveWall(newCell.cScript, 3);
        else RemoveWall(newCell.cScript, 4);

        Debug.Log("Maze generation finished.");
    }

    private List<Cell> GetUnvisitedNeighbours(Cell curCell)
    {
        List<Cell> neighbours = new List<Cell>();

        Cell nCell = curCell;

        Vector2 cPos = curCell.gridPos;

        foreach (Vector2 p in neighbourPositions)
        {
            Vector2 nPos = cPos + p;

            if (allCells.ContainsKey(nPos)) nCell = allCells[nPos];

            if (unvisited.Contains(nCell)) neighbours.Add(nCell);
        }

        return neighbours;
    }

    private void CompareWalls(Cell cCell, Cell nCell)
    {
        if (nCell.gridPos.x < cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 2);
            RemoveWall(cCell.cScript, 1);
        }

        else if (nCell.gridPos.x > cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 1);
            RemoveWall(cCell.cScript, 2);
        }

        else if (nCell.gridPos.y > cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 4);
            RemoveWall(cCell.cScript, 3);
        }

        else if (nCell.gridPos.y < cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 3);
            RemoveWall(cCell.cScript, 4);
        }
    }

    private void RemoveWall(CellScript cScript, int wallID)
    {
        if (wallID == 1) cScript.wallL.SetActive(false);
        else if (wallID == 2) cScript.wallR.SetActive(false);
        else if (wallID == 3) cScript.wallU.SetActive(false);
        else if (wallID == 4) cScript.wallD.SetActive(false);
    }

    private void CreateCentre()
    {
        centreCells[0] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2) + 1)];
        RemoveWall(centreCells[0].cScript, 4);
        RemoveWall(centreCells[0].cScript, 2);
        centreCells[1] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2) + 1)];
        RemoveWall(centreCells[1].cScript, 4);
        RemoveWall(centreCells[1].cScript, 1);
        centreCells[2] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2))];
        RemoveWall(centreCells[2].cScript, 3);
        RemoveWall(centreCells[2].cScript, 2);
        centreCells[3] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2))];
        RemoveWall(centreCells[3].cScript, 3);
        RemoveWall(centreCells[3].cScript, 1);

        List<int> rndList = new List<int> { 0, 1, 2, 3 };
        int startCell = rndList[Random.Range(0, rndList.Count)];
        rndList.Remove(startCell);
        currentCell = centreCells[startCell];
        foreach (int c in rndList)
        {
            unvisited.Remove(centreCells[c]);
        }
    }

    private void GenerateCell(Vector2 pos, Vector2 keyPos)
    {
        Cell newCell = new Cell();

        newCell.gridPos = keyPos;

        newCell.cellObject = Instantiate(cellPrefab, pos, cellPrefab.transform.rotation);

        if (mazeParent != null) newCell.cellObject.transform.parent = mazeParent.transform;

        newCell.cellObject.name = "Cell - X:" + keyPos.x + " Y:" + keyPos.y;

        newCell.cScript = newCell.cellObject.GetComponent<CellScript>();

        if (disableCellSprite) newCell.cellObject.GetComponent<SpriteRenderer>().enabled = false;

        allCells[keyPos] = newCell;
        unvisited.Add(newCell);
    }

    private void DeleteMaze()
    {
        if (mazeParent != null) Destroy(mazeParent);
    }

    private void InitValues()
    {
        if (IsOdd(mazeRows)) mazeRows--;
        if (IsOdd(mazeColumns)) mazeColumns--;

        if (mazeRows <= 3) mazeRows = 4;
        if (mazeColumns <= 3) mazeColumns = 4;

        cellSize = cellPrefab.transform.localScale.x;

        mazeParent = new GameObject();
        mazeParent.transform.position = Vector2.zero;
        mazeParent.name = "Maze";
    }

    private bool IsOdd(int value)
    {
        return value % 2 != 0;
    }

    private class Cell
    {
        public Vector2 gridPos;
        public GameObject cellObject;
        public CellScript cScript;
    }
}