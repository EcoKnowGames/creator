using UnityEngine;
using XCharts.Runtime;

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

        public void ShowGraph()
        {
            if (_graphContainer != null)
            {
                _graphContainer.gameObject.SetActive(true);
            }

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
    }
}
