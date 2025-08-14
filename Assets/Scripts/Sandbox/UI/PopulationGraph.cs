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

        //TODO(caspar): Template Graph to copy style from?

        public bool IsVisible => _graphContainer != null ? _graphContainer.gameObject.activeSelf : false;

        private const string LogChannel = "[PopulationGraph]";

        private void Start()
        {
            HideGraph();
        }

        public void Init()
        {
            HideGraph();

            if (ScenarioLoader.Instance != null)
            {
                SetTitle(ScenarioLoader.Instance.LastPlayedScenario.Name);
            }
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

        private void SetTitle(string title)
        {
            if (_lineChart != null)
            {
                Title chartTitle = _lineChart.GetChartComponent<Title>();
                if (chartTitle != null)
                {
                    chartTitle.text = title;
                }
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
                        string categoryName = i.ToString();
                        if (i == 0)
                        {
                            categoryName = "START";
                        }

                        xAxis.AddData(categoryName);
                    }
                }
            }
        }

        //NOTE(caspar): The LineChart automatically figures out a good max Y Value for the axis
        /*private void SetYAxis_Population(int maxPopulation)
        {
            //TODO
        }*/

        private void ClearLegendEntries()
        {
            if (_lineChart != null)
            {
                Legend legend = _lineChart.GetChartComponent<Legend>();
                if (legend != null)
                {
                    legend.ClearData();
                    legend.icons.Clear();
                    legend.colors.Clear();
                }
            }
        }

        private void AddLegendEntry(string spritePath, Color colour)
        {
            if (_lineChart != null)
            {
                Legend legend = _lineChart.GetChartComponent<Legend>();
                if (legend != null)
                {
                    Sprite resource = Resources.Load<Sprite>(spritePath);
                    if (resource != null)
                    {
                        legend.icons.Add(resource);
                        legend.colors.Add(colour);
                    }
                    else
                    {
                        Debug.LogError($"{LogChannel} Failed to find icon for legend at path {spritePath}!");
                    }
                }
            }
        }

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

                    //_lineChart.ClearSerieData();
                    _lineChart.RemoveAllSerie();
                    ClearLegendEntries();

                    EntityManager entityManager = sandboxManager.EntityManager;
                    if (entityManager != null)
                    {
                        //Get our entity types
                        Entity[] entityTypes = entityManager.GetEntityTypeList();

                        foreach(Entity entity in entityTypes)
                        {
                            //Create array of populations from the event data that match entity type id
                            int[] populations = eventData.Where(x => x.Populations.ContainsKey(entity.ID)).Select(x => x.Populations[entity.ID]).ToArray();

                            Color entityColour = Color.blue;
                            ColorUtility.TryParseHtmlString("#" + entity.Colour, out entityColour);

                            Line line = AddNewLineSerie(entity.ID, entityColour);
                            if (line != null)
                            {
                                for (int i = 0; i < populations.Count(); i++)
                                {
                                    line.AddXYData(i, populations[i]);
                                }
                            }

                            AddLegendEntry(entity.Icon, entityColour);
                        }

                    }
                }

                //TODO(caspar): Later
                //Calculate Harvest/Introduce changes per round
                //Look for HARVEST and INTRODUCE event types
            }
        }

        private Line AddNewLineSerie(string serieName, Color colour)
        {
            if (_lineChart == null)
            {
                //TODO(caspar): Error
                return null;
            }

            Line line = _lineChart.AddSerie<Line>(serieName);

            /*if (_defaultLine != null)
            {
                line.lineStyle = _defaultLine.lineStyle;
                line.itemStyle = _defaultLine.itemStyle;
                line.symbol = _defaultLine.symbol;
            }*/

            line.lineStyle.color = colour;
            line.itemStyle.color = colour;
            line.symbol.color = colour;

            line.symbol.type = SymbolType.Circle; //TODO(caspar): Would be nice if we had templates to copy from

            return line;
        }
        #endregion

    }
}
