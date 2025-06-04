using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class SandboxUI : MonoBehaviour
    {
        [SerializeField] private PlayerToolbar _playerToolbar;
        public void Init()
        {
            _playerToolbar?.Init();
        }
    }
}
