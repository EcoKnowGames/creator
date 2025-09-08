using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class Button_ScenarioSelect : MonoBehaviour
    {
        [Header("Scenario Info")]
        [SerializeField] private TMP_Text _scenarioNameText;
        [SerializeField] private TMP_Text _authorNameText;
        [SerializeField] private TMP_Text _playerCountText;

        [Header("Button")]
        [SerializeField] private Button _playButton;
        public Button PlayButton => _playButton;

        public void SetScenarioData(int index, string name, string author, int maxActions)
        {
            if (_scenarioNameText != null)
            {
                _scenarioNameText.text = string.Format($"{index}. {name}");
            }

            if (_authorNameText != null)
            {
                string authorName = string.IsNullOrEmpty(author) ? "Unknown Author" : author;
                _authorNameText.text = string.Format($"By {authorName}");
            }

            if (_playerCountText != null)
            {
                string compatiblePlayers = GetCompatiblePlayers(maxActions);
                _playerCountText.text = string.Format($"Supports {compatiblePlayers} Players");
            }
        }

        private string GetCompatiblePlayers(int maxActions)
        {
            string players = "1";

            if (maxActions % 2 == 0)
            {
                players += ", 2";
            }

            if (maxActions % 3 == 0)
            {
                players += ", 3";
            }

            if (maxActions % 4 == 0)
            {
                players += ", 4";
            }

            //Syntax
            int lastComma = players.LastIndexOf(',');
            if (lastComma >= 0)
            {
                players = players.Remove(lastComma, 1).Insert(lastComma, " or");
            }

            return players;
        }

    }
}

