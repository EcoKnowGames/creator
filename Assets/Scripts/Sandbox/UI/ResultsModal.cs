using UnityEngine;
using TMPro;
using Glitchers.EcoKnow.Sandbox;

namespace Glitchers.EcoKnow.Sandbox.UI
{

    public class ResultsModal : MonoBehaviour
    {
        [SerializeField] private TMP_Text _resultsText;
        public void ShowModal(string result)
        {
            _resultsText.text = string.Format($"You {result}!");
            this.gameObject.SetActive(true);
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }

        public void OnReplayPressed()
        {
            SandboxManager.Instance.ReplayCurrentScenario();
        }

        public void OnQuitPressed()
        {
            Application.Quit();
        }

        public void OnExportDataPressed()
        {
#if !UNITY_WEBGL
            Data.DataManager.Instance.ShowSaveDialog(null, null);

            //TODO(caspar): Do we want to allow data export on WebGL? This will likely require some js
#endif
        }
    }
}
