using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Glitchers.EcoKnow.Sandbox.Grid;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ModifyCellModal : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _selectedID;
        [SerializeField] private TMP_Text _selectedPopulation;

        [Header("Entity")]
        [SerializeField] private Transform _entityButtonContainer;
        [SerializeField] private Button _entityButtonPrefab;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;

        Action<int, int> onConfirmPressed;

        private int _selectedEntity = 0;

        public void ShowModal(string title, CellEntity[] entities, Action<int, int> onConfirm)
        {
            SetTitle(title);

            onConfirmPressed = onConfirm;
            PopulateEntityOptions(entities);

            if (_inputField != null)
            {
                _inputField.text = "1";
            }

            this.gameObject.SetActive(true);
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }

        #region Callbacks
        public void OnConfirmPressed()
        {
            int modifyAmount = 1;

            if (_inputField != null)
            {
                int.TryParse(_inputField.text, out modifyAmount);
            }

            //Send data to the PlayerToolbar?
            onConfirmPressed?.Invoke(_selectedEntity, modifyAmount);
        }

        public void OnCancelPressed()
        {
            HideModal();
        }
        #endregion

        #region UI
        private void SetTitle(string title)
        {
            if (_title != null)
            {
                _title.text = title;
            }

            if (_confirmButton != null)
            {
                TMP_Text buttonText = _confirmButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = title;
                }
            }
        }
        private void PopulateEntityOptions(CellEntity[] entities)
        {
            _selectedEntity = 0;
            
            if ((_entityButtonPrefab == null) || (_entityButtonContainer == null) || (_selectedID == null) || (_selectedPopulation == null))
            {
                return;
            }

            _selectedID.text = string.Format($"Selected: {entities[_selectedEntity].ID}");
            _selectedPopulation.text = string.Format($"Current Population: {entities[_selectedEntity].Population}");

            foreach(Transform child in _entityButtonContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < entities.Length; i++)
            {
                Button button = Instantiate(_entityButtonPrefab, _entityButtonContainer);

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = entities[i].ID;
                }

                int entityIndex = i;
                button.onClick.AddListener( delegate {
                    _selectedEntity = entityIndex;
                    _selectedID.text = string.Format($"Selected: {entities[entityIndex].ID}");
                    _selectedPopulation.text = string.Format($"Current Population: {entities[entityIndex].Population}");
                } );
            }

            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }
        #endregion
    }
}
