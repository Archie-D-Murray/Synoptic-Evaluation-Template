using UnityEngine;

namespace Utilities {

    ///<summary>Persistent singleton that is not destroyed between scenes</summary>
    public class PersistentSingleton<T> : MonoBehaviour where T : Component {

        ///<summary>Internal writable instance</summary>
        protected static T _internalInstance;

        ///<summary>Is instance not null</summary>
        public static bool HasInstance => _internalInstance != null;

        ///<summary>Get instance without autocreating - null if not present in scene</summary>
        public static T TryGetInstance() => HasInstance ? _internalInstance : null;

        ///<summary>Current instance - may be null</summary>
        public static T Current => _internalInstance;

        ///<summary>Validated readonly instance - will auto create if null</summary>
        public static T Instance {
            get {
                if (_internalInstance == null) {
                    StartSingleton();
                }

                return _internalInstance;
            }
        }

        ///<summary>Will start singleton if not present</summary>
        public static void StartSingleton() {
            if (_internalInstance == null) {
                _internalInstance = FindFirstObjectByType<T>();
                if (_internalInstance == null) {
                    GameObject obj = new GameObject();
                    obj.name = $"{typeof(T).Name} - AutoCreated";
                    _internalInstance = obj.AddComponent<T>();
                    (_internalInstance as PersistentSingleton<T>).OnAutoCreate();
                }
                (_internalInstance as PersistentSingleton<T>).OnAwake();
                DontDestroyOnLoad(Instance.gameObject);
            }
        }

        ///<summary>Called if not present in scene and had to auto-create self</summary>
        protected virtual void OnAutoCreate() { }

        ///<summary>Not to be overriden - use OnAwake for Awake related logic</summary>
        protected void Awake() {
            InitialiseSingleton();
            OnAwake();
        }

        ///<summary>Called after InitialiseSingleton in Awake or on first lazy load</summary>
        protected virtual void OnAwake() { }

        ///<summary>Initialises singleton with self</summary>
        protected virtual void InitialiseSingleton() {
            if (!Application.isPlaying) {
                return;
            }

            if (_internalInstance == null) {
                _internalInstance = this as T;
                DontDestroyOnLoad(Instance.gameObject);
                enabled = true;
            } else {
                if (this != _internalInstance) {
                    Destroy(this.gameObject);
                }
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnBeginPlay() {
            _internalInstance = null;
        }
#endif
    }
}