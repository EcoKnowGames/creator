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

        private int _lastLegendIndexClicked = -1;
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

            SetupGraph();
            HideMarkArea();

            if (_lineChart != null)
            {
                _lineChart.AnimationFadeIn();
                _lineChart.onLegendClick = OnLegendClick;
            }
        }

        public void HideGraph()
        {
            if (_graphContainer != null)
            {
                _graphContainer.gameObject.SetActive(false);
            }

            if (_lineChart != null)
            {
                _lineChart.onLegendClick = null;
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

        //NOTE: The LineChart automatically figures out a good max Y Value for the axis
        //But we want to control it so that players can "focus" on series without the graph resizing
        private void SetYAxis_Population(int maxPopulation)
        {
            if (_lineChart != null)
            {
                YAxis yAxis = _lineChart.GetChartComponent<YAxis>();
                if (yAxis != null)
                {
                    int units = 1000;
                    int highest = maxPopulation + (units - (maxPopulation % units));

                    yAxis.min = 0;
                    yAxis.max = highest;
                }
            }
        }

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
                List<EventDataObject> eventData = dataManager.FetchDataPoints( new Data.EventType[]{ Data.EventType.GAME_START, Data.EventType.ROUND_END } );
                if (eventData != null)
                {
                    //Clear old data
                    _lineChart.RemoveAllSerie();
                    ClearLegendEntries();

                    //Max number of rounds
                    SetXAxis_Rounds(sandboxManager.MaxRounds + 1); //Include "START" data point

                    //Max chart height
                    int highestPopulation = eventData.Max(x => x.Populations.Values.Max());
                    int highestWinCondition = (int)sandboxManager.WinConditions.Max(x => x.upperLimit);
                    int chartMax = highestPopulation > highestWinCondition ? highestPopulation : highestWinCondition;
                    SetYAxis_Population(chartMax);

                    //Now set up line series
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
            }

            //Refresh everything just in case
            if (_lineChart != null)
            {
                _lineChart.RefreshAllComponent();
            }
        }

        private Line AddNewLineSerie(string serieName, Color colour)
        {
            if (_lineChart == null)
            {
                Debug.LogError($"{LogChannel} Failed to add new LineSerie, _lineChart is null!");
                return null;
            }

            Line line = _lineChart.AddSerie<Line>(serieName);

            line.lineStyle.color = colour;
            line.itemStyle.color = colour;
            line.symbol.color = colour;

            line.symbol.type = SymbolType.None;

            return line;
        }

        private void ShowMarkArea(int index, Entity entityType, float min, float max)
        {
            if (_lineChart != null)
            {
                YAxis yAxis = _lineChart.GetChartComponent<YAxis>();
                MarkArea markArea = _lineChart.GetChartComponent<MarkArea>();
                MarkLine markLine = _lineChart.GetChartComponent<MarkLine>();
                if ((markArea != null) && (markLine != null) && (yAxis != null))
                {
                    int markMax = (int)(max > min ? max : yAxis.max);

                    markArea.serieIndex = index;
                    markArea.start.yValue = markMax;
                    markArea.end.yValue = min;
                    markArea.show = true;
                    markArea.label.show = true;

                    markLine.serieIndex = index;
                    markLine.show = true;

                    if (min <= 0)
                    {
                        markArea.end.type = MarkAreaType.Min;
                        markArea.end.dimension = 0;
                    }
                    else
                    {
                        markArea.end.type = MarkAreaType.None;
                    }

                    //Setup markLines
                    for (int i = 0; i < 2; i++)
                    {
                        MarkLineData data = null;
                        if (i < markLine.data.Count)
                        {
                            data = markLine.data[i];
                        }
                        else
                        {
                            data = new MarkLineData();
                            markLine.data.Add(new MarkLineData());
                        }

                        if (data != null)
                        {
                            data.startSymbol.show = false;
                            data.endSymbol.show = false;
                            data.lineStyle.type = LineStyle.Type.Dashed;
                            data.yValue = i == 0 ? min : markMax;
                        }
                    }
                }

                _lineChart.RefreshAllComponent();
            }
        }

        private void HideMarkArea()
        {
            if (_lineChart != null)
            {
                MarkArea markArea = _lineChart.GetChartComponent<MarkArea>();
                MarkLine markLine = _lineChart.GetChartComponent<MarkLine>();
                if ((markArea != null) && (markLine != null))
                {
                    markArea.show = false;
                    markLine.show = false;
                    markArea.label.show = false;
                }

                _lineChart.RefreshAllComponent();
            }
        }

        #endregion

        #region Graph Events
        private void OnLegendClick(Legend legend, int index, string serieName, bool selected)
        {
            //NOTE: This is called for every item in the legend even if only one button is clicked

            if (_lineChart != null)
            {
                bool allActive = _lineChart.series.All(x => x.show == true);
                bool showAll = false;

                if (index == _lineChart.series.Count - 1)
                {
                    //We have reached the final series and can now process the result of the click interaction
                    for (int i = 0; i < _lineChart.series.Count; i++)
                    {
                        Serie serie = _lineChart.series[i];
                        if (!allActive && serie.show)
                        {
                            if (i == _lastLegendIndexClicked)
                            {
                                showAll = true;
                            }
                        }
                    }

                    //Re-enable all series
                    if (showAll)
                    {
                        _lastLegendIndexClicked = -1;
                        HideMarkArea();

                        foreach (Serie serie in _lineChart.series)
                        {
                            _lineChart.SetSerieActive(serie, true);
                            serie.symbol.type = SymbolType.None;
                        }
                    }
                    //Only display our focused series
                    else
                    {
                        Line activeSerie = (Line)_lineChart.series.FirstOrDefault(x => x.show == true); //Currently active
                        _lastLegendIndexClicked = activeSerie.index;

                        //Show Symbols
                        if (activeSerie != null)
                        {
                            activeSerie.symbol.type = SymbolType.Circle;
                        }

                        //Setup Mark Area
                        SandboxManager sandboxManager = SandboxManager.Instance;
                        if (sandboxManager != null)
                        {
                            EntityManager entityManager = SandboxManager.Instance.EntityManager;
                            if (entityManager != null)
                            {
                                Entity entityType = entityManager.GetEntityTypeList().FirstOrDefault(x => x.ID.Equals(activeSerie.serieName, System.StringComparison.OrdinalIgnoreCase));
                                if (entityType != null)
                                {
                                    WinCondition winCondition = sandboxManager.WinConditions.FirstOrDefault(x => x.EntityIndex == activeSerie.index);
                                    if (winCondition != null)
                                    {
                                        ShowMarkArea(activeSerie.index, entityType, winCondition.lowerLimit, winCondition.upperLimit);
                                    }
                                    else
                                    {
                                        HideMarkArea();
                                    }
                                }
                            }
                        }
                    }
                }
            }         
        }
        #endregion
    }
}
