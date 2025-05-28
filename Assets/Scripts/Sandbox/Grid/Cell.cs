using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] protected GameObject highlightObject;
        [SerializeField] protected TMPro.TMP_Text entityListText; //TODO(caspar) -> Entity management in its own component?

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
        public void AddEntity(string id)
        {
            entityListText.text += id + " :100\n";
        }
        #endregion
    }
}
