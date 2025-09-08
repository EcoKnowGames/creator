using UnityEngine;

[CreateAssetMenu(fileName = "SO_ScenarioConfigList", menuName = "EcoKnow/ScenarioConfigDataList", order = 1)]
public class ScenarioConfigDataList : ScriptableObject
{
    [System.Serializable]
    public class ScenarioAsset
    {
        public TextAsset jsonAsset;
    }

    [SerializeField] private ScenarioAsset[] _scenarioAssets;
    public ScenarioAsset[] ScenarioAssets => _scenarioAssets;
}
