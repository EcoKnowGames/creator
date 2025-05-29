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

        [SerializeField] protected TMPro.TMP_Text entityListText; //TODO(caspar) -> Entity management in its own component?
        [SerializeField] List<CellEntity> _activeEntities;

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
            _activeEntities = new List<CellEntity>();

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
            //NOTE(caspar): Every cell will have one of every entity, but some might have a value of zero

            if (_activeEntities != null)
            {
                CellEntity entity = _activeEntities.FirstOrDefault(x => x.ID.Equals(id));
                if (entity == null)
                {
                    _activeEntities.Add(new CellEntity(id, 100));
                }
            }

            UpdateDebugText();
        }

        private void UpdateDebugText()
        {
            if (entityListText == null)
            {
                return;
            }

            entityListText.text = string.Empty;

            if (_activeEntities != null)
            {
                foreach(CellEntity entity in _activeEntities)
                {
                    entityListText.text += entity.ID + ": " + entity.Population + "\n";
                }
            }
        }

        public void CalculateNewEntityCount()
        {
            //r - growth rates
            //N - 1D matrix of current entity counts
            //A - matrix of all entities and how they relate to each other (passed in via Matrix Node)
            //Nsq = N + N.(r + AN)

            if ((_activeEntities == null) || (_activeEntities.Count <= 0))
            {
                Debug.LogError($"{LogChannel} No entities found for Cell [{this.Row} , {this.Column}]. Aborting calculations...");
                return;
            }

            int entityCount = SandboxManager.Instance.EntityManager.AllEntities.Count;
            if (_activeEntities.Count != entityCount)
            {
                Debug.LogError($"{LogChannel} Entity count [{_activeEntities.Count}] for Cell [{this.Row} , {this.Column}] does not match the Simulation Entity count [{entityCount}]! Aborting calculations...");
                return;
            }

            float[,] A = SandboxManager.Instance.EntityManager.AlphaMatrix;
            if ((A.GetLongLength(0) != entityCount) || (A.GetLongLength(1) != entityCount))
            {
                Debug.LogError($"{LogChannel} Entity count [{_activeEntities.Count}] does not match the entity count of the Alpha Matrix. Aborting calculations...");
                return;
            }

            float[] r = SandboxManager.Instance.EntityManager.AllEntities.Select(x => x.GrowthRate).ToArray();
            float[] N = _activeEntities.Select(x => (float)x.Population).ToArray();

            //AN
            float[] AN = new float[entityCount];
            for (int y = 0; y < A.GetLongLength(1); y++)
            {
                float result = 0f;
                for (int x = 0; x < A.GetLongLength(0); x++)
                {
                    result += A[x, y] * N[x];
                    //Debug.Log(A[x, y]);
                }

                AN[y] = result;
            }

            //N + N.(r + AN)
            float[] NNrAN = new float[entityCount];
            for (int a = 0; a < N.Length; a++)
            {
                NNrAN[a] = N[a] + (N[a] * (r[a] + AN[a]));

                Debug.Log(NNrAN[a]);
            }

            //Update entity numbers
            for(int b = 0; b < NNrAN.Length; b++)
            {
                _activeEntities[b].Population = Mathf.CeilToInt(NNrAN[b]);
            }

            UpdateDebugText();
        }
        #endregion
    }
}
