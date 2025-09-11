using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen : MonoBehaviour
    {
        protected MainMenuController MainMenuController => this.GetComponentInParent<MainMenuController>();
        protected RectTransform[] RectTransforms => this.GetComponentsInChildren<RectTransform>();

        private void Awake()
        {
            Hide();
        }

        private void OnEnable()
        {
            OnEnabled();
        }

        protected virtual void OnEnabled()
        {
            StartCoroutine(RefreshLayout());
        }

        public virtual void Show()
        {
            this.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            this.gameObject.SetActive(false);
        }

        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            if ((RectTransforms != null) && (RectTransforms.Length > 0))
            {
                foreach (RectTransform transform in RectTransforms)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
                }
            }
        }
    }
}
