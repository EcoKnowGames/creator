using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MainMenuController : MonoBehaviour
    {
        public enum Screen { MAIN, SELECT_SCENARIO, LOAD_SCENARIO, SETUP_SCENARIO };

        [SerializeField] private MenuScreen_Main _mainMenuScreen;
        [SerializeField] private MenuScreen_Select _selectScenarioScreen;
        [SerializeField] private MenuScreen_Load _loadScenarioScreen;
        [SerializeField] private MenuScreen_Setup _setupScenarioScreen;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SetScreen(Screen.MAIN);
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Screens
        public void SetScreen(Screen screenType)
        {
            _mainMenuScreen?.Hide();
            _selectScenarioScreen?.Hide();
            _loadScenarioScreen?.Hide();
            _setupScenarioScreen?.Hide();

            switch (screenType)
            {
                case (Screen.SELECT_SCENARIO):
                    {
                        _selectScenarioScreen?.Show();
                        break;
                    }
                case (Screen.LOAD_SCENARIO):
                    {
                        _loadScenarioScreen?.Show();
                        break;
                    }
                case (Screen.SETUP_SCENARIO):
                    {
                        _setupScenarioScreen?.Show();
                        break;
                    }
                case (Screen.MAIN):
                default:
                    {
                        _mainMenuScreen?.Show();
                        break;
                    }
            }

        }
        #endregion
    }
}
