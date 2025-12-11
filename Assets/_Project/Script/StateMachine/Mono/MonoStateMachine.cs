using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Script.StateMachine.Mono
{
    /// <summary>
    /// MonoBehaviour 기반 상태 패턴 최상위 객체 <br/>
    /// 상태 변경의 경우 int값 외에도 자식 객체에서 선언한 enum같은 값을 override를 통해 ChangeState 함수 확장하여 사용
    /// </summary>
    public abstract class MonoStateMachine : MonoBehaviour
    {
        private Dictionary<int, MonoState> _stateDic;
        private MonoState _prevState;
        private MonoState _currentState;

        protected virtual void Awake()
        {
            // 1단계: 상태 등록 먼저 수행
            RegisterStates();

            // 2단계: 상태 등록 완료 후 초기 상태로 전환
            InitializeDefaultState();
        }

        /// <summary>
        /// 상태들을 딕셔너리에 등록
        /// </summary>
        private void RegisterStates()
        {
            _stateDic = new Dictionary<int, MonoState>();
            var states = GetComponentsInChildren<MonoState>(true);

            foreach (var state in states)
            {
                _stateDic.TryAdd(state.index, state);
                state.Initialize(this);
                state.gameObject.SetActive(false);
                state.enabled = false;
            }
        }

        /// <summary>
        /// 초기 상태 설정
        /// </summary>
        private void InitializeDefaultState()
        {
            if (_stateDic.TryGetValue(0, out var state)) ChangeState(state.index);
            else
            {
                int index = _stateDic.Values.First().index;
                ChangeState(index);
            }
        }

        public void ChangeState(int index)
        {
            if (_currentState != null && _currentState.index == index) return;
            if (_stateDic.ContainsKey(index) == false) return;
            
            
            if (_currentState != null)
            {
                _currentState.gameObject.SetActive(false);
                _currentState.enabled = false; // 스크립트 컴포넌트 비활성화
            }

            _prevState = _currentState;
            _currentState = _stateDic[index];

            // 새 상태 활성화
            _currentState.gameObject.SetActive(true);
            _currentState.enabled = true; // 스크립트 컴포넌트 활성화
        }

        public void RollBackState()
        {
            if (_prevState == null) return;
                
            ChangeState(_prevState.index);
        }
    }
}
