using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;

namespace _Project.Script.Generic
{
    public class MonoSingletonPunCallbacks<T>: MonoBehaviourPunCallbacks, IInitializable where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();
        private static bool _isApplicationQuit = false;

        [SerializeField] private bool isDontDestroyOnLoad = false;
        
        
        public bool IsInitialized { get; protected set; }
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_isApplicationQuit) return null;
                    if (_instance == null) _instance = FindAnyObjectByType<T>();
                    if (_instance != null) return _instance;
                
                    var obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                    return _instance;
                }
            }
        }
    
        protected virtual void Awake()
        {
            if (_instance == null) _instance = this as T;
            else DestroyImmediate(this);
        
            if (isDontDestroyOnLoad) DontDestroyOnLoad(this);
        }
    
        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuit = true;
        }

        
        public virtual void Initialize()
        {
            IsInitialized = true;
        }
    }
}
