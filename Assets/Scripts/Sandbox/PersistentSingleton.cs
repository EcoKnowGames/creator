using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    /// <summary>
    /// Base class for managers that persist across scene loads.
    /// The first instance to wake becomes the singleton and is moved to DontDestroyOnLoad.
    /// Duplicates created when a scene containing the manager is loaded again destroy themselves.
    /// Subclasses must not declare their own Awake or OnDestroy: override OnSingletonAwake for
    /// start-up work, and call the base implementation if Awake or OnDestroy is overridden.
    /// </summary>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
    {
        protected static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(T) +
                            " is needed in the scene, but there is none.");
                    }
                }

                return instance;
            }
        }

        public static bool Exists => instance != null;

        protected virtual void Awake()
        {
            if ((instance != null) && (instance != this))
            {
                //Duplicate from a re-loaded scene. Disabling stops its Start/Update running before the deferred Destroy
                enabled = false;
                Destroy(gameObject);
                return;
            }

            instance = (T)this;
            DontDestroyOnLoad(gameObject);
            OnSingletonAwake();
        }

        /// <summary>
        /// Initialisation that runs only on the surviving instance.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
