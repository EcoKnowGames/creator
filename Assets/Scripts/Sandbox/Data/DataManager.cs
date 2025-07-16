using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SimpleFileBrowser;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType { GAME_START, GAME_END, ROUND_END, INTRODUCE, HARVEST, SELL};

    /*public record CellDataObject
        (
            int X,
            int Y,
            Dictionary<string, int> Populations
        );*/

    public record MapDataObject
        (
            Dictionary<string, int>[,] Populations
        );

    public record WinConditionDataObject
        (
            string Title,
            bool Completed,
            int Streak
        );

    public record EventDataObject
        (
            EventType Type,
            int Player,
            Dictionary<string, int> Populations,
            Dictionary<string, int> Inventory,
            List<WinConditionDataObject> WinConditions,
            MapDataObject Map,
            System.Object Meta
        );

    public record GameDataObject
        (
            ScenarioConfig Config,
            EventDataObject[] Events
        );


    public class DataManager : MonoBehaviour
    {
        #region Singleton
        protected static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetInstance();

                    if (instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(DataManager) +
                            " is needed in the scene, but there is none.");
                    }
                }

                return instance;
            }
        }

        public static bool Exists
        {
            get
            {
                return instance;
            }
        }

        private static DataManager GetInstance()
        {
            if (instance == null)
            {
                return FindFirstObjectByType<DataManager>();
            }
            return instance;
        }
        #endregion

        private List<EventDataObject> _eventLog;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ExportData();
            }
        }

        public void RecordEvent(EventType type, System.Object meta = null)
        {
            //Gather data here
            EventDataObject ev = new EventDataObject(
                type,
                1, //CurrentPlayer
                GetPopulations(),
                GetInventory(),
                GetWinConditions(),
                GetMapPopulations(),
                meta
                ); ;


            if (_eventLog == null)
            {
                _eventLog = new List<EventDataObject>();
            }

            if (ev != null)
            {
                _eventLog.Add(ev);
            }
        }

        private void ExportData(string filePath = null)
        {
            GameDataObject data = new GameDataObject
                (
                    ScenarioLoader.Instance.LoadedConfig,
                    _eventLog.ToArray()
                );

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            if (!string.IsNullOrEmpty(json))
            {
                string path = filePath;
                if (path == null)
                {
                    string fileName = data.Config.Scenario.Name.Replace(" ", "_");
                    path = Application.persistentDataPath + "/" + fileName + "_data.json";
                }

                System.IO.File.WriteAllText(path, json);
                Debug.Log("Data Exported!");
            }
        }

        public void ShowSaveDialog(Action onSuccess, Action onCancel)
        {
            //Get scenario name
            //TODO(caspar): Date/time?
            string scenarioName = "Unknown";
            if ((ScenarioLoader.Instance != null) && (ScenarioLoader.Instance.LastPlayedScenario != null))
            {
                scenarioName = ScenarioLoader.Instance.LastPlayedScenario.Name;
            }

            scenarioName = scenarioName.Replace(" ", "_");
            string defaultFileName = string.Format($"{scenarioName}_data.json");

            //Show dialog
            FileBrowser.SetFilters(false, ".json");
            FileBrowser.ShowSaveDialog(
            (filePaths) =>
            {
                if ((filePaths != null) && (filePaths.Length > 0))
                {
                    //On file saved
                    string filePath = filePaths[0];
                    ExportData(filePath);
                }
            },
            () =>
            {
                //On cancelled
                onCancel?.Invoke();
            },
            FileBrowser.PickMode.Files,
            false,
            null,
            defaultFileName,
            "Export Game Data"
            );
        }

        #region Data Gathering
        private MapDataObject GetMapPopulations()
        {
            Dictionary<string, int>[,] populations = null;
            if (SandboxManager.Instance.EntityManager != null)
            {
                populations = SandboxManager.Instance.EntityManager.GetPopulationsByCell();
            }

            MapDataObject mapData = new MapDataObject
                (
                    populations
                );

            return mapData;
        }

        private Dictionary<string, int> GetPopulations()
        {
            Dictionary<string, int> populationList = new Dictionary<string, int>();

            for (int i = 0; i < SandboxManager.Instance.EntityManager.EntityTypeCount; i++)
            {
                Entity entityType = SandboxManager.Instance.EntityManager.GetEntityType(i);
                if (entityType != null)
                {
                    int total = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(i);
                    populationList.Add(entityType.ID, total);
                }
            }

            return populationList;
        }

        private Dictionary<string, int> GetInventory()
        {
            if (SandboxManager.Instance.PlayerInventory != null)
            {
                return new Dictionary<string, int>(SandboxManager.Instance.PlayerInventory.Inventory);
            }

            return null;
        }

        private List<WinConditionDataObject> GetWinConditions()
        {
            List<WinConditionDataObject> winConditionList = new List<WinConditionDataObject>();

            for (int i = 0; i < SandboxManager.Instance.WinConditions.Count; i++)
            {
                WinCondition winCondition = SandboxManager.Instance.WinConditions[i];
                WinConditionDataObject data = new WinConditionDataObject
                    (
                        winCondition.title,
                        winCondition.Completed,
                        winCondition.ConsecutiveSuccesses
                    );

                winConditionList.Add(data);
            }

            return winConditionList;
        }
        #endregion
    }
}

