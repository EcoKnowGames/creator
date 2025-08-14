using Glitchers.EcoKnow.Sandbox.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XCharts.Runtime;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class PopulationGraph : MonoBehaviour
    {
        [SerializeField] private GameObject _graphContainer;
        [SerializeField] private LineChart _lineChart;

        public bool IsVisible => _graphContainer != null ? _graphContainer.gameObject.activeSelf : false;

        private void Start()
        {
            HideGraph();
        }

        public void Init()
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
            SetupGraph();

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
            if (_lineChart != null)
            {
                XAxis xAxis = _lineChart.GetChartComponent<XAxis>();
                if (xAxis != null)
                {
                    xAxis.ClearData();

                    for (int i = 0; i < rounds; i++)
                    {
                        xAxis.AddData(i.ToString());
                    }
                }
            }
        }

        //NOTE(caspar): The LineChart automatically figures out a good max Y Value for the axis
        /*private void SetYAxis_Population(int maxPopulation)
        {
            //TODO
        }*/

        private void SetupGraph()
        {
            DataManager dataManager = DataManager.Instance;
            SandboxManager sandboxManager = SandboxManager.Instance;
            if ((dataManager != null) && (sandboxManager != null) && (_lineChart != null))
            {                
                //sandboxManager.WinConditions;

                List<EventDataObject> eventData = dataManager.FetchDataPoints( new Data.EventType[]{ Data.EventType.GAME_START, Data.EventType.ROUND_END } );
                if (eventData != null)
                {
                    SetXAxis_Rounds(eventData.Count); //TODO(caspar): We probably need total rounds + 1 rather than eventCount

                    _lineChart.ClearSerieData();

                    EntityManager entityManager = sandboxManager.EntityManager;
                    if (entityManager != null)
                    {
                        //Get our entity types
                        Entity[] entityTypes = entityManager.GetEntityTypeList();

                        foreach(Entity entity in entityTypes)
                        {
                            //Create array of populations from the event data that match entity type id
                            int[] populations = eventData.Where(x => x.Populations.ContainsKey(entity.ID)).Select(x => x.Populations[entity.ID]).ToArray();
                            Line line = _lineChart.AddSerie<Line>(entity.ID);
                            for (int i = 0; i < populations.Count(); i++)
                            {
                                line.AddXYData(i, populations[i]);
                            }
                        }

                    }
                }

                //TODO(caspar): Later
                //Calculate Harvest/Introduce changes per round
                //Look for HARVEST and INTRODUCE event types
            }
        }
        #endregion

    }
}
