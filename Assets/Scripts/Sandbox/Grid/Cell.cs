using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] protected GameObject highlightObject;

        [SerializeField] protected TMPro.TMP_Text entityListText;

        [SerializeField] private Cell_Token _cellTokenPrefab;
        [SerializeField] private Transform _cellTokenContainer;

        private int _row;
        private int _column;
        private bool _mouseOver;

        private const string LogChannel = "[Cell]";

        public int Row => _row;
        public int Column => _column;
        public Vector2 GridPosition => new Vector2(Column, Row);

        public GridManager ParentGrid => GetComponentInParent<GridManager>();

        public void Init(int column, int row, int tileID)
        {
            _row = row;
            _column = column;

            this.gameObject.name = $"Cell {_column}_{_row}";

            entityListText.text = string.Empty;

            ShowHighlight(false);
        }

        #region Cells
        public Cell GetNeighbouringCell(int x, int y)
        {
            //Get cell x and y away
            GridManager gridManager = GetComponentInParent<GridManager>();
            if (gridManager != null)
            {
                return gridManager.FindCellAtPosition(_column + x, _row + y);
            }

            return null;
        }

        public List<Cell> GetAllNeighbouringCells()
        {
            List<Cell> neighbours = new List<Cell>();
            GridManager gridManager = GetComponentInParent<GridManager>();
            {
                if (gridManager != null)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            Cell cell = gridManager.FindCellAtPosition(_column + x, _row + y);
                            if (cell != null)
                            {
                                neighbours.Add(cell);
                            }
                        }
                    }
                }
            }

            return neighbours;
        }
        #endregion

        #region Highlight

        public void ShowHighlight(bool visible)
        {
            highlightObject?.SetActive(visible);
        }
        #endregion



        #region MouseEvents
        void OnMouseEnter()
        {
            _mouseOver = true;
            ShowHighlight(true);
        }

        void OnMouseExit()
        {
            _mouseOver = false;
            ShowHighlight(false);
        }

        private void OnMouseDown()
        {
        }

        private void OnMouseUp()
        {
        }
        #endregion

        #region Entities
        public CellEntity[] GetCellEntities()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                return SandboxManager.Instance?.EntityManager?.GetEntitiesForCell(Column, Row);
            }

            return null;
        }

        public void UpdateEntityCount()
        {
            CellEntity[] entities = GetCellEntities();
            UpdateDebugText(entities);
            UpdateTokens(entities);
        }

        private void UpdateDebugText(CellEntity[] entityList)
        {
            if (entityListText == null)
            {
                return;
            }

            entityListText.text = string.Empty;

            if (entityList != null)
            {
                foreach (CellEntity entity in entityList)
                {
                    entityListText.text += entity.ID + ": " + entity.Population + "\n";
                }
            }
        }

        public void SetupEntityTokens()
        {
            //We don't care about the exact entities in the cell
            //Just populate with a token for each entity type

            if ((_cellTokenPrefab == null) || (_cellTokenContainer == null))
            {
                //Error
                return;
            }

            foreach (Transform child in _cellTokenContainer.transform)
            {
                Destroy(child.gameObject);
            }


            EntityManager entityManager = SandboxManager.Instance.EntityManager;
            if (entityManager != null)
            {
                CellEntity[] cellEntities = entityManager.GetEntitiesForCell(_column, _row);
                for (int i = 0; i < cellEntities.Length; i++)
                {
                    Entity type = entityManager.GetEntityType(i);
                    Cell_Token token = Instantiate(_cellTokenPrefab, _cellTokenContainer);
                    token.Init(i, type);
                    token.UpdatePopulation(cellEntities[i].Population);
                }
            }
        }

        public void UpdateTokens(CellEntity[] entityList)
        {
            if (_cellTokenContainer == null)
            {
                return;
            }

            if (_cellTokenContainer.childCount != entityList.Length)
            {
                //Error?
                return;
            }

            if (entityList != null)
            {
                for (int i = 0; i < entityList.Length; i++)
                {
                    Cell_Token token = _cellTokenContainer.GetChild(i).GetComponent<Cell_Token>();
                    if (token != null)
                    {
                        token.UpdatePopulation(entityList[i].Population);
                    }
                }
            }
        }
        #endregion
    }
}
