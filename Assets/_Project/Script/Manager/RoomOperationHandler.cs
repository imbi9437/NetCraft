using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 방 관련 비동기 작업을 관리하는 핸들러
    /// UniTaskCompletionSource 관리와 작업 상태 추적 담당
    /// </summary>
    public class RoomOperationHandler
    {
        private UniTaskCompletionSource<bool> _roomOperationCompletionSource;
        private UniTaskCompletionSource<bool> _connectionCompletionSource;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _isCreatingRoom = false;
        private bool _isJoiningRoom = false;

        /// <summary>
        /// 방 생성 작업 시작
        /// </summary>
        public void StartRoomCreation()
        {
            if (_isCreatingRoom)
            {
                throw new InvalidOperationException("이미 방 생성 중입니다.");
            }

            _isCreatingRoom = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _roomOperationCompletionSource = new UniTaskCompletionSource<bool>();
        }

        /// <summary>
        /// 방 참가 작업 시작
        /// </summary>
        public void StartRoomJoining()
        {
            if (_isJoiningRoom)
            {
                throw new InvalidOperationException("이미 방 참가 중입니다.");
            }

            _isJoiningRoom = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _roomOperationCompletionSource = new UniTaskCompletionSource<bool>();
        }

        /// <summary>
        /// 연결 작업 시작
        /// </summary>
        public void StartConnection()
        {
            _connectionCompletionSource = new UniTaskCompletionSource<bool>();
        }

        /// <summary>
        /// 방 생성 완료 (성공)
        /// </summary>
        public void CompleteRoomCreation(bool success)
        {
            if (_roomOperationCompletionSource != null && !_roomOperationCompletionSource.Task.Status.IsCompleted())
            {
                _roomOperationCompletionSource.TrySetResult(success);
            }
        }

        /// <summary>
        /// 방 참가 완료 (성공)
        /// </summary>
        public void CompleteRoomJoining(bool success)
        {
            if (_roomOperationCompletionSource != null && !_roomOperationCompletionSource.Task.Status.IsCompleted())
            {
                _roomOperationCompletionSource.TrySetResult(success);
            }
        }

        /// <summary>
        /// 연결 완료
        /// </summary>
        public void CompleteConnection(bool success)
        {
            if (_connectionCompletionSource != null && !_connectionCompletionSource.Task.Status.IsCompleted())
            {
                _connectionCompletionSource.TrySetResult(success);
            }
        }

        /// <summary>
        /// 방 생성 작업 완료 및 정리
        /// </summary>
        public void FinishRoomCreation()
        {
            _isCreatingRoom = false;
            Cleanup();
        }

        /// <summary>
        /// 방 참가 작업 완료 및 정리
        /// </summary>
        public void FinishRoomJoining()
        {
            _isJoiningRoom = false;
            Cleanup();
        }

        /// <summary>
        /// 방 생성 작업 대기
        /// </summary>
        public async UniTask<bool> WaitForRoomCreationAsync()
        {
            if (_roomOperationCompletionSource == null)
            {
                Debug.LogError("[RoomOperationHandler] 방 생성 작업이 시작되지 않았습니다.");
                return false;
            }

            try
            {
                return await _roomOperationCompletionSource.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[RoomOperationHandler] 방 생성이 취소되었습니다.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoomOperationHandler] 방 생성 중 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 방 참가 작업 대기
        /// </summary>
        public async UniTask<bool> WaitForRoomJoiningAsync()
        {
            if (_roomOperationCompletionSource == null)
            {
                Debug.LogError("[RoomOperationHandler] 방 참가 작업이 시작되지 않았습니다.");
                return false;
            }

            try
            {
                return await _roomOperationCompletionSource.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[RoomOperationHandler] 방 참가가 취소되었습니다.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoomOperationHandler] 방 참가 중 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 연결 작업 대기
        /// </summary>
        public async UniTask<bool> WaitForConnectionAsync()
        {
            if (_connectionCompletionSource == null)
            {
                Debug.LogError("[RoomOperationHandler] 연결 작업이 시작되지 않았습니다.");
                return false;
            }

            try
            {
                return await _connectionCompletionSource.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[RoomOperationHandler] 연결이 취소되었습니다.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoomOperationHandler] 연결 중 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 취소 토큰 가져오기
        /// </summary>
        public CancellationToken GetCancellationToken()
        {
            return _cancellationTokenSource?.Token ?? CancellationToken.None;
        }

        /// <summary>
        /// 작업 상태 확인
        /// </summary>
        public bool IsCreatingRoom => _isCreatingRoom;
        public bool IsJoiningRoom => _isJoiningRoom;
        public bool IsOperationInProgress => _isCreatingRoom || _isJoiningRoom;

        /// <summary>
        /// 리소스 정리
        /// </summary>
        private void Cleanup()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _roomOperationCompletionSource = null;
            _connectionCompletionSource = null;
        }

        /// <summary>
        /// 완전 정리
        /// </summary>
        public void Dispose()
        {
            _isCreatingRoom = false;
            _isJoiningRoom = false;
            Cleanup();
        }
    }
}
