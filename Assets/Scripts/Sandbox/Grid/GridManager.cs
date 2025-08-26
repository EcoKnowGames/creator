using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public delegate void CellEvent(Cell inCell);

    [System.Serializable]
    public class GridDef
    {
        public int rows;
        public int columns;
        public int[,] tileIDs;
        public int[,][] tilePopulations;
    }


    public class GridManager : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] protected GridCamera gridCamera;

        [Header("Grid")]
        [SerializeField] protected int rows;
        [SerializeField] protected int columns;

        [Header("Cells")]
        [SerializeField] protected Transform cellContainer;
        [SerializeField] protected float cellWidth;
        [SerializeField] protected float cellHeight;
        [SerializeField] protected float cellGap;

        [Header("Prefabs")]
        [SerializeField] protected GameObject cellPrefab;

        //Events
        public CellEvent OnCellClicked;

        private Cell[,] cellList;

        public Vector2 CellScale => new Vector2(cellWidth, cellHeight);
        public Vector2 CellSize => GetActualCellSize();
        public float CellGap => cellGap;
        public int TotalCells => GetComponentsInChildren<Cell>().Length;
        public Vector2 GridSize => new Vector2(columns, rows);
        private Camera Camera => gridCamera == null ? Camera.main : gridCamera.Camera;

        private const string LogChannel = "[GridManager]";


        #region Static Helper Functions
        public static GridDef LoadGridDef(TextAsset mapCSV)
        {
            GridDef def = new GridDef();

            string rawCSV = mapCSV.text;
            rawCSV = rawCSV.Trim(' ', '\n', '\r');

            var regex = @",(?![^[]*\])"; //Look ahead, ignore commas within [] parentheses
            string[] IDs = Regex.Split(rawCSV.Replace("\r", string.Empty).Replace("\n", ","), regex);

            def.rows = rawCSV.Split('\n').Length;
            def.columns = IDs.Length / def.rows;

            def.tileIDs = new int[def.columns, def.rows];
            def.tilePopulations = new int[def.columns, def.rows][];


            int tileIndex = 0;
            for (int y = 0; y < def.rows; y++)
            {
                for (int x = 0; x < def.columns; x++)
                {
                    string[] tileDef = IDs[tileIndex].Replace("]", string.Empty).Split('[');

                    int tileID = -1;
                    int.TryParse(IDs[tileIndex], out tileID);

                    int[] populations = tileDef.Length > 1 ? Array.ConvertAll(tileDef[1].Split(','), int.Parse) : null;

                    def.tilePopulations[x, y] = populations;
                    def.tileIDs[x, y] = tileID;
                    tileIndex++;
                }
            }

            Debug.Log($"Map: {mapCSV.name} / Rows: {def.rows} Columns: {def.columns}");

            return def;
        }
        #endregion


        public void Start()
        {
            DisableGrid();
        }

        public void Init()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                SandboxManager.Instance.EntityManager.onEntityHarvested += OnEntityUpdated;
                SandboxManager.Instance.EntityManager.onEntityIntroduced += OnEntityUpdated;
            }
        }

        public void Cleanup()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                SandboxManager.Instance.EntityManager.onEntityHarvested -= OnEntityUpdated;
                SandboxManager.Instance.EntityManager.onEntityIntroduced -= OnEntityUpdated;
            }
        }

        public void SetupGrid(GridDef gridDef)
        {
            if (cellPrefab == null)
            {
                return;
            }

            rows = gridDef.rows;
            columns = gridDef.columns;

            //Clear all test/debug/old cells
            foreach (Transform cell in cellContainer)
            {
                Destroy(cell.gameObject);
            }

            cellList = new Cell[columns, rows];

            //Generate new ones
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int tileID = gridDef.tileIDs[x, y];
                    if (tileID == -1)
                    {
                        continue;
                    }

                    GameObject cell = Instantiate(cellPrefab, cellContainer, false);
                    cell.transform.localScale = new Vector3(cellWidth, cellHeight, 1f);
                    cell.transform.localPosition = new Vector2(x * (cellWidth + cellGap), y * -(cellHeight + cellGap));

                    cell.GetComponent<Cell>()?.Init(x, y, tileID);
                    cellList[x, y] = cell.GetComponent<Cell>();
                }
            }

            gridCamera.Init(this);
        }

        public void EnableGrid()
        {
            if (cellContainer != null)
            {
                cellContainer.gameObject.SetActive(true);
            }
            if (gridCamera != null)
            {
                gridCamera.gameObject.SetActive(true);
            }
        }

        public void DisableGrid()
        {
            if (cellContainer != null)
            {
                cellContainer.gameObject.SetActive(false);
            }
            if (gridCamera != null)
            {
                gridCamera.gameObject.SetActive(false);
            }
        }

        public void HandleInput()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Cell cell = CastToCell();
                if (cell != null)
                {
                    cell.OnClicked();
                    OnCellClicked?.Invoke(cell);
                }
            }
        }

        private Cell CastToCell()
        {
            //Do we have touch input?
            if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    return null;
                }
            }
            //No touch input, check default pointer
            else
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return null;
                }
            }


            RaycastHit2D hit;
            float distance = 100f;

            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

            //TODO(caspar): Mask?
            hit = Physics2D.Raycast(ray.origin, ray.direction, distance);

            if (hit.transform != null)
            {
                //Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);
                //Debug.Log(hit.transform.gameObject.name);

                Cell cell = hit.transform.gameObject.GetComponent<Cell>();
                return cell;
            }

            return null;
        }

        public Cell FindCellAtPosition(int row, int column)
        {
            if ((cellList != null) && (cellList.Length > 0))
            {
                //Search for our cell
                if ((row >= 0)
                    && (column >= 0)
                    && (row < cellList.GetLength(0))
                    && (column < cellList.GetLength(1)))
                {
                    return cellList[row, column];
                }
            }

            return null;
        }

        public Vector2 GetActualCellSize()
        {
            Vector2 size = Vector2.zero;

            if (cellPrefab != null)
            {
                SpriteRenderer spriteRenderer = cellPrefab.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    size = spriteRenderer.sprite.bounds.size;
                }
            }

            return size;
        }


        #region Camera
        public void CenterCamera()
        {
            gridCamera?.CenterCamera();
        }

        public void FocusCell(Cell inCell)
        {
            if (gridCamera != null)
            {
                gridCamera.FocusCamera(inCell.Column, inCell.Row);
            }
        }
        #endregion

        #region Entities
        public IEnumerator OnEntitiesAdded()
        {
            foreach(Cell cell in cellList)
            {
                if (cell != null)
                {
                    cell.ClearEntityTokens();
                }
            }

            yield return new WaitForEndOfFrame();

            foreach(Cell cell in cellList)
            {
                if (cell != null)
                {
                    cell.SetupEntityTokens();
                }
            }

            yield return new WaitForEndOfFrame();

            UpdateAllCells();
        }

        public void OnEntityUpdated(int column, int row, int id)
        {
            UpdateAllCells(); //We need to update all cells so that the proportional percentage reflects Harvest/Introduce changes
        }

        public void UpdateAllCells()
        {
            foreach(Cell cell in cellList)
            {
                if (cell != null)
                {
                    cell.UpdateEntityCount();
                }
            }
        }
        #endregion

        #region UI
        /*public void RegisterUIEvents(RoundViewController inRoundsVC)
        {
            if (inRoundsVC != null)
            {
                inRoundsVC.onCameraButtonPressed += gridCamera.MoveCamera;
                inRoundsVC.onCameraZoomButtonPressed += gridCamera.UpdateCameraZoom;
            }
        }

        public void DeregisterUIEvents(RoundViewController inRoundsVC)
        {
            if (inRoundsVC != null)
            {
                inRoundsVC.onCameraButtonPressed -= gridCamera.MoveCamera;
                inRoundsVC.onCameraZoomButtonPressed -= gridCamera.UpdateCameraZoom;
            }
        }*/
        #endregion
    }
}
