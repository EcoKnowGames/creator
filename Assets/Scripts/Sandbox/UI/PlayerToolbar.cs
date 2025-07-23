using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class PlayerToolbar : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text _activeModeText;
        [SerializeField] private TMP_Text _actionsRemainingText;

        [Header("Buttons")]
        [SerializeField] private Button _harvestButton;
        [SerializeField] private Button _introduceButton;

        private const string LogChannel = "[Toolbar]";

        public void Init()
        {
            HideToolbar();
        }


        public void ShowToolbar(int entityIndex)
        {
            this.gameObject.SetActive(true);
        }

        public void HideToolbar()
        {
            this.gameObject.SetActive(false);
        }

        public void SetModifyModeText(ModifyMode action)
        {
            if (_activeModeText != null)
            {
                _activeModeText.text = action.ToString();
            }
        }

        #region Actions
        public void UpdateActionsRemaining(int actions)
        {
            if (_actionsRemainingText != null)
            {
                _actionsRemainingText.text = string.Format($"Actions Left: {actions}");
            }

            //TODO(caspar): The harvest and introduce buttons should be greyed out if the entity type does not support the action
            //This will come about in further UI implementations

            //Set out buttons active or not
            bool hasActions = actions > 0;
            if (_harvestButton != null)
            {
                _harvestButton.interactable = hasActions;
            }

            if (_introduceButton != null)
            {
                _introduceButton.interactable = hasActions;
            }
        }
        #endregion
    }
}
