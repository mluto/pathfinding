using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class PathfindingOptimized : MonoBehaviour
{
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private Transform mapParent;
    [SerializeField] private Transform character;
    [SerializeField] private RectTransform popUpMenu;
    [SerializeField] private TMP_InputField gridHeight_input;
    [SerializeField] private TMP_InputField gridWidth_input;
    [SerializeField] private GameObject path;
    [SerializeField] private float speed = 1;
    [SerializeField][Range(0, 1)] private double wallsProportions = 0.4;

    private int gridWidth;
    private int gridHeight;
    private Cell startPos;
    private Cell endPos;

    private Dictionary<Vector2, Cell> cells;
    private List<Vector2> cellsToSearch;
    private List<Vector2> searchedCells;
    private List<Vector2> finalPath;

    private GameObject clickedCell;
    private GameObject[,] map;
    private bool move = false;
    private Vector3 target;

    enum CellType { Wall, Path, PathStart, PathEnd };

    private class Cell
    { 
        public CellType type;
        public Vector2 position;
        public int fCost = int.MaxValue;
        public int gCost = int.MaxValue;
        public int hCost = int.MaxValue;
        public Vector2 connection;

        public Cell(Vector2 pos)
        {
            position = pos;
            type = CellType.Path;
        }

        public void reset()
        {
            fCost = int.MaxValue;
            gCost = int.MaxValue;
            hCost = int.MaxValue;
        }
    }

    private void Start()
    {
        popUpMenu.gameObject.SetActive(false);
        character.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Input.mousePosition;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.layer != 5)
                {
                    clickedCell = hit.transform.gameObject;
                    print(hit.transform.name + " is clicked by mouse");

                    popUpMenu.anchoredPosition = mousePos;
                    popUpMenu.gameObject.SetActive(true);
                }
            }       
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                popUpMenu.gameObject.SetActive(false);
            }
        }

        if(move)
        {
            float step = speed * Time.deltaTime;
            character.position = Vector3.MoveTowards(character.position, target, step);

            if (character.position == target)
            {
                move = false;
            }
        }
    }

    public void GenerateGrid()
    {
        ClearMap();

        gridWidth = int.Parse(gridWidth_input.text);
        gridHeight = int.Parse(gridHeight_input.text);

        cells = new Dictionary<Vector2, Cell>();
        map = new GameObject[gridWidth, gridHeight];

        cameraControl.SetMax(gridWidth, gridHeight);

        for (float x = 0; x < gridWidth; x++)
        {
            for (float y = 0; y < gridHeight; y++)
            {
                Vector2 pos = new Vector2(x, y);
                cells.Add(pos, new Cell(pos));
            }
        }

        var wallsCount = (gridWidth * gridHeight) * wallsProportions;

        for (int i = 0; i < wallsCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
            cells[pos].type = CellType.Wall;
        }

        Camera.main.transform.position = new Vector3(gridWidth / 2, Camera.main.transform.position.y, gridHeight / 2);
        
        GenerateMap();
    }

    private void ClearMap()
    {
        ClearVisualise();
        if (mapParent.childCount == 0) return;

        startPos = null;
        endPos = null;
        map = null;

        for (int i = mapParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(mapParent.GetChild(i).gameObject);
        }
    }

    private void GenerateMap()
    {
        if (cells == null)
        {
            return;
        }

        foreach (KeyValuePair<Vector2, Cell> kvp in cells)
        {
            GameObject cell;
            cell = Instantiate(path, mapParent);

            if (kvp.Value.type == CellType.Path)
            {
                cell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.gray);
            }
            else
            {
                cell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.black);
            }

            cell.transform.position = new Vector3(kvp.Key.x, 0, kvp.Key.y);
            cell.name = kvp.Key.x.ToString() + ',' + kvp.Key.y.ToString();

            map[(int)kvp.Key.x, (int)kvp.Key.y] = cell;
        }
    }

    public void PathStart()
    {
        ClearVisualise();
        popUpMenu.gameObject.SetActive(false);
        var pos = GetVector2FromName(clickedCell.name);

        if (cells[pos].type == CellType.Path)
        {
            clickedCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);

            if (startPos != null)
            {
                cells[startPos.position].type = CellType.Path;
                var oldCell = map[(int)startPos.position.x, (int)startPos.position.y];
                oldCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.gray);
            }

            startPos = cells[pos];
            cells[pos].type = CellType.PathStart;
        }
    }

    public void PathEnd()
    {
        ClearVisualise();
        popUpMenu.gameObject.SetActive(false);
        var pos = GetVector2FromName(clickedCell.name);

        if (cells[pos].type == CellType.Path)
        {
            clickedCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
            
            if (endPos != null)
            {
                cells[endPos.position].type = CellType.Path;
                var oldCell = map[(int)endPos.position.x, (int)endPos.position.y];
                oldCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.gray);
            }

            endPos = cells[pos];
            cells[pos].type = CellType.PathEnd;
        }
    }

    public void SetPath()
    {
        ClearVisualise();
        popUpMenu.gameObject.SetActive(false);
        var pos = GetVector2FromName(clickedCell.name);

        if (cells[pos].type == CellType.PathStart)
        {
            startPos = null;
        }
        else if (cells[pos].type == CellType.PathEnd)
        {
            endPos = null;
        }

        clickedCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.gray);
        cells[pos].type = CellType.Path;
    }

    public void SetWall()
    {
        ClearVisualise();
        popUpMenu.gameObject.SetActive(false);
        var pos = GetVector2FromName(clickedCell.name);

        if (cells[pos].type == CellType.Path)
        {
            clickedCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.black);
            cells[pos].type = CellType.Wall;
        }
    }

    private Vector2 GetVector2FromName(string name)
    {
        string pattern = ",";
        string[] substrings = Regex.Split(name, pattern);

        return new Vector2(int.Parse(substrings[0]), int.Parse(substrings[1]));
    }

    public void FindPath()
    {
        cellsToSearch = new List<Vector2> { startPos.position };
        searchedCells = new List<Vector2>();
        finalPath = new List<Vector2>();

        ClearVisualise();

        cells[startPos.position].gCost = 0;
        cells[startPos.position].hCost = GetDistance(startPos.position, endPos.position);
        cells[startPos.position].fCost = GetDistance(startPos.position, endPos.position);

        while (cellsToSearch.Count > 0)
        {
            Vector2 cellToSearch = cellsToSearch[0];

            foreach (Vector2 pos in cellsToSearch)
            {
                Cell c = cells[pos];
                if (c.fCost < cells[cellToSearch].fCost ||
                    c.fCost == cells[cellToSearch].fCost && c.hCost == cells[cellToSearch].hCost)
                {
                    cellToSearch = pos;
                }
            }

            cellsToSearch.Remove(cellToSearch);
            searchedCells.Add(cellToSearch);

            if (cellToSearch == endPos.position)
            {
                Cell pathCell = cells[endPos.position];

                while (pathCell.position != startPos.position)
                {
                    finalPath.Add(pathCell.position);
                    pathCell = cells[pathCell.connection];
                }

                finalPath.Add(startPos.position);
                VisualisePath();
                return;
            }

            SearchCellNeighbors(Vector2.left, cellToSearch, endPos.position);
            SearchCellNeighbors(Vector2.right, cellToSearch, endPos.position);
            SearchCellNeighbors(Vector2.up, cellToSearch, endPos.position);
            SearchCellNeighbors(Vector2.down, cellToSearch, endPos.position);
        }

        if (finalPath.Count == 0)
        {
            Debug.Log("Path not found");
        }
    }

    private int GetDistance(Vector2 startPos, Vector2 endPos)
    {
        Vector2Int dist = new Vector2Int(Mathf.Abs((int)startPos.x - (int)endPos.x), Mathf.Abs((int)startPos.y - (int)endPos.y));

        int lowest = Mathf.Min (dist.x, dist.y);
        int highest = Mathf.Max (dist.x, dist.y);
        int horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }

    private void VisualisePath()
    {
        for (int i = 1; i < finalPath.Count - 1; i++)
        {
            var pathCell = map[(int)finalPath[i].x, (int)finalPath[i].y];
            pathCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue);
        }
    }

    private void ClearVisualise()
    {
        character.gameObject.SetActive(false);

        if (cells != null)
        {
            foreach (KeyValuePair<Vector2, Cell> kvp in cells)
            {
                kvp.Value.reset();
            }
        }

        if (finalPath != null)
        {
            for (int i = 1; i < finalPath.Count - 1; i++)
            {
                var pathCell = map[(int)finalPath[i].x, (int)finalPath[i].y];
                pathCell.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.grey);
            }

            finalPath = new List<Vector2>();
        }

        
    }

    private void SearchCellNeighbors(Vector2 direction, Vector2 cellPos, Vector2 endPos)
    {
        Vector2 neighborPos = cellPos + direction;
        if (cells.TryGetValue(neighborPos, out Cell c) && !searchedCells.Contains(neighborPos) && cells[neighborPos].type != CellType.Wall)
        {
            int GcostToNeighbour = cells[cellPos].gCost + GetDistance(cellPos, neighborPos);

            if (GcostToNeighbour < cells[neighborPos].gCost)
            {
                Cell neighbourNode = cells[neighborPos];

                neighbourNode.connection = cellPos;
                neighbourNode.gCost = GcostToNeighbour;
                neighbourNode.hCost = GetDistance(neighborPos, endPos);
                neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;

                if (!cellsToSearch.Contains(neighborPos))
                {
                    cellsToSearch.Add(neighborPos);
                }
            }
        }
    }

    public void MoveCharacter()
    {
        character.position = new Vector3(startPos.position.x, 0, startPos.position.y);
        character.gameObject.SetActive(true);

        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        for (int i = finalPath.Count; i > 0; i--)
        {
            target = new Vector3(finalPath[i-1].x, 0, finalPath[i-1].y);
            move = true;

            yield return new WaitForSeconds(1);
        }
    }

}



