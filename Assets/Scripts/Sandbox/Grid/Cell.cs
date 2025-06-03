using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class CellEntity
    {
        private string _id;
        public string ID => _id;
        private int _population;
        public int Population { get => _population; set { _population = value; } }

        public CellEntity(string id, int population)
        {
            _id = id;
            _population = population;
        }
    }

    public class Cell : MonoBehaviour
    {
        [SerializeField] protected GameObject highlightObject;

        [SerializeField] protected TMPro.TMP_Text entityListText;

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
        public void UpdateEntityCount()
        {
            List<CellEntity> entities = SandboxManager.Instance?.EntityManager?.GetEntitiesForCell(Column, Row);

            UpdateDebugText(entities);
        }

        private void UpdateDebugText(List<CellEntity> entityList)
        {
            if (entityListText == null)
            {
                return;
            }

            entityListText.text = string.Empty;

            if (entityList != null)
            {
                foreach(CellEntity entity in entityList)
                {
                    entityListText.text += entity.ID + ": " + entity.Population + "\n";
                }
            }
        }
        #endregion
    }
}
