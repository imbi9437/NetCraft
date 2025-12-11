using System;
using System.Collections.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Generic;
using _Project.Script.Interface;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Script.Manager
{
    [DefaultExecutionOrder(-50)]
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private float minInitializeTime = 2f;
        
        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            InitializeManagers().Forget();
        }

        private async UniTask InitializeManagers()
        {
            try
            {
                List<UniTask> tasks = new List<UniTask>();
                var managers = GetComponents<IInitializable>();
                
                foreach (var manager in managers)
                {
                    if (ReferenceEquals(this, manager)) continue;
                    manager.Initialize();
                    UniTask task = UniTask.WaitUntil(() => manager.IsInitialized, cancellationToken: destroyCancellationToken);
                    tasks.Add(task);
                }
                
                UniTask waitTask = UniTask.Delay(TimeSpan.FromSeconds(minInitializeTime), cancellationToken: destroyCancellationToken);
                tasks.Add(waitTask);
                
                await UniTask.WhenAll(tasks);
                
                EventHub.Instance.RaiseEvent(new ManagerInitCompleteEvent());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
