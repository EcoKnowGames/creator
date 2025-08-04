using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectiveWidget : MonoBehaviour
    {
        [Header("Entity")]
        [SerializeField] private Image _entityBackground;
        [SerializeField] private Image _entityIcon;

        [Header("Round Target")]
        [SerializeField] private TMP_Text _roundTargetText;
        [SerializeField] private GameObject _roundTargetCheck;

        private const string LogChannel = "[ObjectiveWidget]";

        public void SetEntity(Entity entity)
        {
            if (entity == null)
            {
                Debug.LogError($"{LogChannel} Failed setup, Entity is null!");
                return;
            }

            //Set icon
            if (_entityIcon != null)
            {
                Sprite resource = Resources.Load<Sprite>(entity.Icon);
                if (resource != null)
                {
                    _entityIcon.sprite = resource;
                }
                else
                {
                    Debug.LogError($"{LogChannel} Failed to find icon for entity at path {entity.Icon}!");
                }
            }

            //Set Colour
            if (_entityBackground != null)
            {
                Color colour = Color.white;
                ColorUtility.TryParseHtmlString("#" + entity.Colour, out colour);
                _entityBackground.color = colour;
            }
        }
    }
}
