using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 방 관련 상태를 관리하는 클래스
    /// 상태 머신 패턴을 적용하여 복잡한 상태 전환을 관리
    /// </summary>
    public class RoomStateManager
    {
        public enum RoomOperationState
        {
            Idle,           // 대기 상태
            Creating,       // 방 생성 중
            Joining,        // 방 참가 중
            Joined,         // 방 참가 완료
            Leaving,        // 방 나가기 중
            Error           // 오류 상태
        }

        private RoomOperationState _currentState = RoomOperationState.Idle;
        private ClientState _lastNetworkState = ClientState.Disconnected;

        public RoomOperationState CurrentState => _currentState;
        public ClientState LastNetworkState => _lastNetworkState;

        /// <summary>
        /// 상태 변경
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        /// <param name="reason">상태 변경 이유</param>
        public void ChangeState(RoomOperationState newState, string reason = "")
        {
            if (_currentState == newState)
            {
                return;
            }

            var oldState = _currentState;
            _currentState = newState;

            Debug.Log($"[RoomStateManager] 상태 변경: {oldState} → {newState} ({reason})");
            OnStateChanged?.Invoke(oldState, newState, reason);
        }

        /// <summary>
        /// 네트워크 상태 업데이트
        /// </summary>
        /// <param name="newNetworkState">새로운 네트워크 상태</param>
        public void UpdateNetworkState(ClientState newNetworkState)
        {
            if (_lastNetworkState != newNetworkState)
            {
                var oldNetworkState = _lastNetworkState;
                _lastNetworkState = newNetworkState;

                Debug.Log($"[RoomStateManager] 네트워크 상태 변경: {oldNetworkState} → {newNetworkState}");
                OnNetworkStateChanged?.Invoke(oldNetworkState, newNetworkState);
            }
        }

        /// <summary>
        /// 방 생성 시작
        /// </summary>
        public void StartRoomCreation()
        {
            if (_currentState != RoomOperationState.Idle)
            {
                throw new InvalidOperationException($"방 생성은 {RoomOperationState.Idle} 상태에서만 가능합니다. 현재 상태: {_currentState}");
            }

            ChangeState(RoomOperationState.Creating, "방 생성 시작");
        }

        /// <summary>
        /// 방 생성 완료
        /// </summary>
        /// <param name="success">성공 여부</param>
        public void CompleteRoomCreation(bool success)
        {
            if (_currentState != RoomOperationState.Creating)
            {
                Debug.LogWarning($"[RoomStateManager] 방 생성 완료 호출이지만 현재 상태가 {RoomOperationState.Creating}가 아닙니다. 현재 상태: {_currentState}");
            }

            ChangeState(success ? RoomOperationState.Joined : RoomOperationState.Error,
                       success ? "방 생성 성공" : "방 생성 실패");
        }

        /// <summary>
        /// 방 참가 시작
        /// </summary>
        public void StartRoomJoining()
        {
            if (_currentState != RoomOperationState.Idle)
            {
                throw new InvalidOperationException($"방 참가는 {RoomOperationState.Idle} 상태에서만 가능합니다. 현재 상태: {_currentState}");
            }

            ChangeState(RoomOperationState.Joining, "방 참가 시작");
        }

        /// <summary>
        /// 방 참가 완료
        /// </summary>
        /// <param name="success">성공 여부</param>
        public void CompleteRoomJoining(bool success)
        {
            if (_currentState != RoomOperationState.Joining)
            {
                Debug.LogWarning($"[RoomStateManager] 방 참가 완료 호출이지만 현재 상태가 {RoomOperationState.Joining}가 아닙니다. 현재 상태: {_currentState}");
            }

            ChangeState(success ? RoomOperationState.Joined : RoomOperationState.Error,
                       success ? "방 참가 성공" : "방 참가 실패");
        }

        /// <summary>
        /// 방 나가기 시작
        /// </summary>
        public void StartRoomLeaving()
        {
            if (_currentState != RoomOperationState.Joined)
            {
                throw new InvalidOperationException($"방 나가기는 {RoomOperationState.Joined} 상태에서만 가능합니다. 현재 상태: {_currentState}");
            }

            ChangeState(RoomOperationState.Leaving, "방 나가기 시작");
        }

        /// <summary>
        /// 방 나가기 완료
        /// </summary>
        public void CompleteRoomLeaving()
        {
            if (_currentState != RoomOperationState.Leaving)
            {
                Debug.LogWarning($"[RoomStateManager] 방 나가기 완료 호출이지만 현재 상태가 {RoomOperationState.Leaving}가 아닙니다. 현재 상태: {_currentState}");
            }

            ChangeState(RoomOperationState.Idle, "방 나가기 완료");
        }

        /// <summary>
        /// 오류 상태로 전환
        /// </summary>
        /// <param name="errorMessage">오류 메시지</param>
        public void SetError(string errorMessage)
        {
            ChangeState(RoomOperationState.Error, $"오류: {errorMessage}");
        }

        /// <summary>
        /// 상태 초기화
        /// </summary>
        public void Reset()
        {
            ChangeState(RoomOperationState.Idle, "상태 초기화");
        }

        /// <summary>
        /// 현재 상태가 특정 상태인지 확인
        /// </summary>
        /// <param name="state">확인할 상태</param>
        /// <returns>일치 여부</returns>
        public bool IsInState(RoomOperationState state)
        {
            return _currentState == state;
        }

        /// <summary>
        /// 현재 상태가 안전한 상태인지 확인
        /// </summary>
        /// <returns>안전한 상태 여부</returns>
        public bool IsInSafeState()
        {
            return _currentState == RoomOperationState.Idle || _currentState == RoomOperationState.Joined;
        }

        /// <summary>
        /// 현재 상태가 작업 중인지 확인
        /// </summary>
        /// <returns>작업 중 여부</returns>
        public bool IsOperationInProgress()
        {
            return _currentState == RoomOperationState.Creating ||
                   _currentState == RoomOperationState.Joining ||
                   _currentState == RoomOperationState.Leaving;
        }

        /// <summary>
        /// 현재 상태가 오류 상태인지 확인
        /// </summary>
        /// <returns>오류 상태 여부</returns>
        public bool IsInErrorState()
        {
            return _currentState == RoomOperationState.Error;
        }

        /// <summary>
        /// 방에 참가되어 있는지 확인
        /// </summary>
        /// <returns>참가 여부</returns>
        public bool IsInRoom()
        {
            return _currentState == RoomOperationState.Joined && PhotonNetwork.InRoom;
        }

        /// <summary>
        /// 네트워크 상태가 안정적인지 확인
        /// </summary>
        /// <returns>안정적인 상태 여부</returns>
        public bool IsNetworkStable()
        {
            return _lastNetworkState == ClientState.ConnectedToMasterServer ||
                   _lastNetworkState == ClientState.JoinedLobby ||
                   _lastNetworkState == ClientState.Joined;
        }

        /// <summary>
        /// 상태 변경 이벤트
        /// </summary>
        public event Action<RoomOperationState, RoomOperationState, string> OnStateChanged;

        /// <summary>
        /// 네트워크 상태 변경 이벤트
        /// </summary>
        public event Action<ClientState, ClientState> OnNetworkStateChanged;
    }
}
