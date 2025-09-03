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

        [Header("Player Info")]
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private Button _playerNameButton;

        public Button PlayerNameButton => _playerNameButton;
        public string PlayerName => _playerNameInput != null ? _playerNameInput.text : null;

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
