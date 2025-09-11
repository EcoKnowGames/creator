using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MainMenuController : MonoBehaviour
    {
        public enum Screen { MAIN, SELECT_SCENARIO, LOAD_SCENARIO, SETUP_SCENARIO };
        private Screen _activeScreen = Screen.MAIN;

        [SerializeField] private MenuScreen_Main _mainMenuScreen;
        [SerializeField] private MenuScreen_Select _selectScenarioScreen;
        [SerializeField] private MenuScreen_Load _loadScenarioScreen;
        [SerializeField] private MenuScreen_Setup _setupScenarioScreen;

        [SerializeField] private GameObject _pressPlayObject;

        private void Awake()
        {
            _pressPlayObject?.SetActive(false);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _mainMenuScreen?.Show();
            _selectScenarioScreen?.Show();
            _loadScenarioScreen?.Show();
            _setupScenarioScreen?.Show();

            SetScreen(Screen.MAIN);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                RequestLoadSandboxScene();
            }
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
                        _setupScenarioScreen.SetEntryScreen(_activeScreen); //Previous screen
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

            _activeScreen = screenType;
        }
        #endregion

        #region Sandbox
        public void RequestLoadSandboxScene(Action onLoadComplete = null)
        {
            StartCoroutine(LoadSandboxScene(onLoadComplete));
        }

        private IEnumerator LoadSandboxScene(Action onLoadComplete = null)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("scene_Sandbox");
            asyncLoad.completed += delegate { onLoadComplete?.Invoke(); };
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        #endregion
    }
}
