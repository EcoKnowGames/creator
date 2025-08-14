using UnityEngine;
using XCharts.Runtime;
using Glitchers.EcoKnow.Sandbox.Data;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class PopulationGraph : MonoBehaviour
    {
        [SerializeField] private GameObject _graphContainer;
        [SerializeField] private LineChart _lineChart;

        public bool IsVisible => _graphContainer != null ? _graphContainer.gameObject.activeSelf : false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            HideGraph();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (IsVisible)
                {
                    HideGraph();
                }
                else
                {
                    ShowGraph();
                }
            }
        }

        #region Graph Control
        public void ShowGraph()
        {
            if (_graphContainer != null)
            {
                _graphContainer.gameObject.SetActive(true);
            }

            //TODO(caspar): Setup here

            if (_lineChart != null)
            {
                _lineChart.AnimationFadeIn();
            }
        }

        public void HideGraph()
        {
            if (_graphContainer != null)
            {
                _graphContainer.gameObject.SetActive(false);
            }
        }

        private void SetXAxis_Rounds(int rounds)
        {
            //TODO
        }

        private void SetYAxis_Population(int maxPopulation)
        {
            //TODO
        }
        #endregion

        #region Data Manipulation
        private void GatherData() //TODO(caspar): Better func name
        {
            DataManager dataManager = DataManager.Instance;
            if (dataManager != null)
            {
                //dataManager.

                //get initial point (game _start)
                //then get (round_end)
                //whittle down to the population numbers
                //divide into entity types

                //TODO(caspar): Later
                //Calculate Harvest/Introduce changes per round
                //Look for HARVEST and INTRODUCE event types
            }
        }
        #endregion

    }
}
