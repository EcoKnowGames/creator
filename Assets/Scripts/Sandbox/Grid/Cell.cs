using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] protected GameObject highlightObject;

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
        #endregion

        #region Tiles
        /*public bool TryPerformAction(TileAction inAction)
        {
            return false;
        }*/
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
        }

        void OnMouseExit()
        {
            _mouseOver = false;
        }

        private void OnMouseDown()
        {
        }

        private void OnMouseUp()
        {
        }
        #endregion;
    }
}
