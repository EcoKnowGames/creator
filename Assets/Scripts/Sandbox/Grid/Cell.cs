using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class CellEntity
    {
        public enum State { EXTINCT, VULNERABLE, STABLE, ABUNDANT};
        private State _currentState;
        public State CurrentState => _currentState;

        private int _index;
        public int Index => _index;

        private string _id;
        public string ID => _id;

        private int _population;
        public int Population => _population;

        public CellEntity(int index, string id, int population, State state)
        {
            _index = index;
            _id = id;
            _population = population;
            _currentState = state;
        }
    };

    public class Cell : MonoBehaviour
    {
        [SerializeField] protected GameObject highlightObject;

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
            UpdateTokens(entities);
        }

        public void SetupEntityTokens()
        {
            //We don't care about the exact entities in the cell
            //Just populate with a token for each entity type

            if ((_cellTokenPrefab == null) || (_cellTokenContainer == null))
            {
                Debug.LogError($"{LogChannel} Failed to spawn tokens at Cell Row {_row} / Column {_column}, token prefab or token container is null!");
                return;
            }

            foreach (Cell_Token child in _cellTokenContainer.GetComponentsInChildren<Cell_Token>(true))
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
                    int totalPopulation = entityManager.GetTotalPopulationOfEntityType(i);

                    Cell_Token token = Instantiate(_cellTokenPrefab, _cellTokenContainer);
                    token.Init(i, type);
                    token.UpdatePopulation(cellEntities[i].Population, totalPopulation, cellEntities[i].CurrentState);
                    token.SetVisible(cellEntities[i].Population > 0);
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
                Debug.LogError($"{LogChannel} Failed to update tokens at Cell Row {_row} / Column {_column}, mismatch between token count and entity list length!");
                return;
            }

            EntityManager entityManager = SandboxManager.Instance.EntityManager;


            if ((entityList != null) && (entityManager != null))
            {
                Cell_Token[] tokens = _cellTokenContainer.GetComponentsInChildren<Cell_Token>(true);
                CellEntity[] orderedEntities = entityList.OrderByDescending(x => x.Population).ThenByDescending(x => x.CurrentState).ToArray();

                if (tokens.Length > 0)
                {
                    for (int i = 0; i < orderedEntities.Length; i++)
                    {
                        int totalPopulation = entityManager.GetTotalPopulationOfEntityType(orderedEntities[i].Index);
                        Cell_Token token = tokens.FirstOrDefault(x => x.Index == orderedEntities[i].Index); // Make sure we match before adjusting any numbers
                        if (token != null)
                        {
                            token.UpdatePopulation(orderedEntities[i].Population, totalPopulation, orderedEntities[i].CurrentState);
                            token.transform.SetSiblingIndex(i);

                            //We only want to set visible once population has increased
                            //We never set invisible at zero population, only extinct
                            if (orderedEntities[i].Population > 0)
                            {
                                token.SetVisible(true);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
