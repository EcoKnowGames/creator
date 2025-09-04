using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class Button_PlayerSelect : MonoBehaviour
    {
        [Header("Player Select Buttons")]
        [SerializeField] private RectTransform _maxPlayerButtonContainer;
        [SerializeField] private Button _maxPlayerButton_Deselected;
        [SerializeField] private Button _maxPlayerButton_Selected;

        public Button PlayerSelectButton => _maxPlayerButton_Deselected;

        [Header("Player Info")]
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private Button _playerNameButton;
        [SerializeField] private GameObject _playerNameConfirmCheck;

        public Button PlayerNameButton => _playerNameButton;
        public string PlayerName => _playerNameInput != null ? _playerNameInput.text : null;

        public void SetPlayerCount(int count)
        {
            string playerCount = count == 0 ? "Single Player" : string.Format($"{count + 1} Players"); //Account for 0

            if (_maxPlayerButton_Deselected != null)
            {
                TMP_Text deselectedText = _maxPlayerButton_Deselected.GetComponentInChildren<TMP_Text>();
                if (deselectedText != null)
                {
                    deselectedText.text = playerCount;
                }
            }

            if (_maxPlayerButton_Selected != null)
            {
                TMP_Text deselectedText = _maxPlayerButton_Selected.GetComponentInChildren<TMP_Text>();
                if (deselectedText != null)
                {
                    deselectedText.text = playerCount;
                }
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_maxPlayerButton_Deselected != null)
            {
                _maxPlayerButton_Deselected.interactable = interactable;
            }

            if (_maxPlayerButton_Selected != null)
            {
                _maxPlayerButton_Selected.interactable = interactable;
            }
        }

        public void SetSelected(bool isSelected)
        {
            _maxPlayerButton_Deselected?.gameObject.SetActive(!isSelected);
            _maxPlayerButton_Selected?.gameObject.SetActive(isSelected);

            StartCoroutine(RefreshLayout());
        }

        public void SetPlayerFieldInteractable(bool interactable)
        {
            if (_playerNameInput != null)
            {
                _playerNameInput.interactable = interactable;
            }

            if (_playerNameButton != null)
            {
                _playerNameButton.interactable = interactable;
            }

            if (interactable == false)
            {
                SetCheckmarkVisible(false);
            }
        }

        public void SetCheckmarkVisible(bool visible)
        {
            if (_playerNameConfirmCheck != null)
            {
                _playerNameConfirmCheck.SetActive(visible);
            }
        }

        public void ResetNameInput(int playerIndex)
        {
            if (_playerNameInput != null)
            {
                _playerNameInput.text = string.Empty;
                if (_playerNameInput.placeholder is TMP_Text placeholderText)
                {
                    placeholderText.text = string.Format($"Player {playerIndex + 1}"); //Account for 0
                }
            }

            SetCheckmarkVisible(false);
        }

        public void OnNameSet()
        {
            SetCheckmarkVisible(true);
        }

        public void OnInputChanged()
        {
            SetCheckmarkVisible(false);
        }

        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            if (_maxPlayerButtonContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_maxPlayerButtonContainer);
            }
        }
    }
}
