using UnityEngine;
using SimpleFileBrowser;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace Glitchers.EcoKnow.Sandbox
{
    public class ScenarioLoader : MonoBehaviour
    {
        #region Singleton
        protected static ScenarioLoader instance;
        public static ScenarioLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetInstance();

                    if (instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(ScenarioLoader) +
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

        private static ScenarioLoader GetInstance()
        {
            if (instance == null)
            {
                return FindFirstObjectByType<ScenarioLoader>();
            }
            return instance;
        }
        #endregion

        [Header("Scenario")]
        [SerializeField] private ScenarioConfigDataList _integratedScenarioList;
        [SerializeField] private ScenarioNodeGraph _activeScenarioGraph;
        private ScenarioConfig _loadedConfig;
        public ScenarioConfig LoadedConfig => _loadedConfig;
        public Scenario LastPlayedScenario => _loadedConfig == null ? null : _loadedConfig.Scenario;

        private const string LogChannel = "[ScenarioLoader]";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        #region Loading Scenario Config
        public static void ShowLoadDialog(Action<Scenario> onSuccess, Action onCancel)
        {
            FileBrowser.SetFilters(false, ".json");
            FileBrowser.ShowLoadDialog(
            (filePaths) =>
            {
                //On file chosen
                string rawJson = ParseJsonFromFile(filePaths);
                if (!string.IsNullOrEmpty(rawJson))
                {
                    ScenarioConfig config = LoadConfig(rawJson);
                    if (config != null)
                    {
                        Instance._loadedConfig = config;
                        onSuccess?.Invoke(config.Scenario);
                    }
                }
            },
            () =>
            {
                //On cancelled
                onCancel?.Invoke();

                //TODO(caspar): Temporary while we await a proper frontend
                //Instance.StartNewGameFromGraph();
            },
            FileBrowser.PickMode.Files,
            false,
            null,
            null,
            "Load Scenario JSON File"
            );
        }

        public static void HideLoadDialog()
        {
            FileBrowser.HideDialog();
        }

        private static string ParseJsonFromFile(string[] filePaths)
        {
            if ((filePaths != null) && (filePaths.Length > 0))
            {
                //Ignore multi-select
                string filePath = filePaths[0];

                //Read from file
                string text = System.IO.File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            Debug.LogError($"{LogChannel} Failed to parse Json, either filePath was invalid or the selected file was empty");
            return null;
        }

        //Load ScenarioConfig from raw json
        public static ScenarioConfig LoadConfig(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON, JSON is null or empty");
                return null;
            }

            //Deserialise from JSON
            List<string> errors = new List<string>();
            ScenarioConfig config = JsonConvert.DeserializeObject<ScenarioConfig>(json,
                new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                });

            //Check and print errors, if any
            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.Log($"{LogChannel} Failed to deserialise ScenarioConfig from JSON: {error}");
                }
            }

            if ((config != null) && (config.Scenario != null))
            {
                //Validation
                if (Application.version != config.AppVersion)
                {
                    Debug.LogWarning($"{LogChannel} Scenario {config.Scenario.Name} was exported from a different version of the Application {config.AppVersion} (Current: {Application.version}). Scenario may not work as intended");
                }
#if UNITY_EDITOR
                if (Application.unityVersion != config.UnityVersion)
                {
                    Debug.LogWarning($"{LogChannel} Scenario {config.Scenario.Name} was exported from a different version of the Unity Editor {config.UnityVersion} (Current: {Application.unityVersion}). Scenario may not work as intended");
                }
#endif

                if (config.Scenario == null)
                {
                    Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Scenario is null!");
                    return null;
                }
                else
                {
                    if ((config.Scenario.Map == null) || (config.Scenario.Map.gridDef == null) || (config.Scenario.Map.gridDef.tileIDs == null)) //Check for tileIDs as well
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Map Layout is null! Scenario must have a valid Map Layout");
                        return null;
                    }

                    if (config.Scenario.Matrix == null)
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Matrix is null! Scenario must have a valid Matrix");
                        return null;
                    }

                    if (config.Scenario.Entities == null || config.Scenario.Entities.Count() <= 0)
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. No Entities found in the JSON file. Scenario must have valid Entities");
                        return null;
                    }

                    if (config.Scenario.WinConditions == null || config.Scenario.WinConditions.Count() <= 0)
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. No Win Conditions found in the JSON file. Scenario must have at least one valid Win Condition");
                        return null;
                    }

                    if (config.Scenario.Rounds <= 0)
                    {
                        Debug.LogWarning($"{LogChannel} Invalid number of rounds ({config.Scenario.Rounds}) found in Scenario {config.Scenario.Name}. Using default value");
                    }

                    if (config.Scenario.ActionsPerRound <= 0)
                    {
                        Debug.LogWarning($"{LogChannel} Invalid number of actions per round ({config.Scenario.Rounds}) found in Scenario {config.Scenario.Name}. Using default value");
                    }
                }

                //If we have reached this point we are valid
            }
            else
            {
                Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Likely an error with parsing/deserialisation");
            }

            return config;
        }

        public void SetLoadedConfig(ScenarioConfig config)
        {
            if (config != null)
            {
                _loadedConfig = config;
            }
        }
        #endregion

        #region Integrated Scenarios
        public ScenarioConfigDataList GetIntegratedScenarioList()
        {
            return _integratedScenarioList;
        }

        public List<ScenarioConfig> GetIntegratedScenarioConfigs()
        {
            List<ScenarioConfig> _scenarioConfigs = new List<ScenarioConfig>();

            if (_integratedScenarioList != null)
            {
                foreach (ScenarioConfigDataList.ScenarioAsset asset in _integratedScenarioList.ScenarioAssets)
                {
                    if (asset.jsonAsset != null)
                    {
                        string rawJson = asset.jsonAsset.text;
                        if (!string.IsNullOrEmpty(rawJson))
                        {
                            ScenarioConfig config = LoadConfig(rawJson);
                            if (config != null)
                            {
                                _scenarioConfigs.Add(config);
                            }
                        }
                    }
                }
            }

            return _scenarioConfigs;
        }
        #endregion

        #region Load Sandbox
#if UNITY_EDITOR
        //Would be nice if the current graph config could detect the "open" graph
        public ScenarioConfig GetCurrentGraphConfig()
        {
            if (_activeScenarioGraph != null)
            {
                return _activeScenarioGraph.GetConfig();
            }

            return null;
        }

        public void RequestStartActiveGraph()
        {
            ScenarioConfig config = GetCurrentGraphConfig();
            if (config != null)
            {
                _loadedConfig = config;
                StartLoadedConfig();
            }
        }
#endif
        public void RequestStartLoadedConfig()
        {
            StartLoadedConfig();
        }

        private void StartLoadedConfig()
        {
            if ((_loadedConfig != null) && (SandboxManager.Instance != null))
            {
                SandboxManager.Instance.StartNewGame(_loadedConfig.Scenario);
            }
        }

        #endregion
    }
}
