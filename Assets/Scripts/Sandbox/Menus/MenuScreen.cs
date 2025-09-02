using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen : MonoBehaviour
    {
        protected MainMenuController MainMenuController => this.GetComponentInParent<MainMenuController>();

        private void Awake()
        {
            Hide();
        }

        public virtual void Show()
        {
            this.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            this.gameObject.SetActive(false);
        }
    }
}
