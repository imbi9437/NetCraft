using System;
using System.Collections.Generic;
using _Project.Script.Generic;
using _Project.Script.Interface;
using UnityEngine;

namespace _Project.Script.Manager
{
    [DefaultExecutionOrder(-100)]   //스크립트 초기화 순서 조절 어트리뷰트
    public class EventHub : MonoSingleton<EventHub>
    {
        private readonly Dictionary<Type, Delegate> _eventDict = new Dictionary<Type, Delegate>();

        /// <summary>
        /// IEvent 인터페이스를 상속받는 구조체를 사용하는 함수들을 등록
        /// </summary>
        /// <param name="callback">등록할 함수</param>
        /// <typeparam name="T">IEvent를 구현하는 구조체</typeparam>
        public void RegisterEvent<T>(Action<T> callback) where T : struct, IEvent
        {
            if (_eventDict.TryGetValue(typeof(T), out var handler) == false)
                _eventDict.Add(typeof(T), callback);
            else
                _eventDict[typeof(T)] = (Action<T>)handler + callback;
        }

        /// <summary>
        /// IEvent 인터페이스를 상속받는 구조체를 사용하는 함수들을 등록 해제
        /// </summary>
        /// <param name="callback">등록할 함수</param>
        /// <typeparam name="T">IEvent를 구현하는 구조체</typeparam>    
        public void UnregisterEvent<T>(Action<T> callback) where T : struct, IEvent
        {
            if (_eventDict.TryGetValue(typeof(T), out var handler) == false) return;

            var cur = (Action<T>)handler - callback;

            if (cur == null) _eventDict.Remove(typeof(T));
            else _eventDict[typeof(T)] = cur;
        }


        /// <summary>
        /// 구독되어있는 T 타입의 이벤트들을 모두 수행
        /// </summary>
        /// <param name="eventArgs">매개변수로 사용할 구조체</param>
        /// <typeparam name="T">IEvent를 구현하는 구조체</typeparam>
        public void RaiseEvent<T>(T eventArgs) where T : struct, IEvent
        {
            if (_eventDict.TryGetValue(typeof(T), out var handler) == false) return;
            ((Action<T>)handler)?.Invoke(eventArgs);
        }
    }
}