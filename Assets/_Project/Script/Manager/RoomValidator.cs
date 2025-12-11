using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 방 관련 검증 로직을 담당하는 클래스
    /// 비밀번호 검증, 방 정보 유효성 검사 등
    /// </summary>
    public class RoomValidator
    {
        private readonly Dictionary<string, RoomInfo> _cachedRoomList;

        public RoomValidator(Dictionary<string, RoomInfo> cachedRoomList)
        {
            _cachedRoomList = cachedRoomList;
        }

        /// <summary> 방 생성 유효성 검사 </summary>
        public ValidationResult ValidateRoomCreation(string roomName, string password, bool isPublic, int maxPlayers)
        {
            if (string.IsNullOrEmpty(roomName)) return new ValidationResult(false, "방 이름을 입력해주세요.");
            if (maxPlayers < 0 || maxPlayers > 8) return new ValidationResult(false, "최대 인원은 2-8명 사이여야 합니다.");
            if (isPublic == false && string.IsNullOrEmpty(password)) return new ValidationResult(false, "비밀번호를 입력해 주세요.");

            return new ValidationResult(true, "유효한 방 정보입니다.");
        }

        /// <summary> 방 참가 유효성 검사 </summary>
        public ValidationResult ValidateRoomJoin(string roomName, string password)
        {
            if (string.IsNullOrEmpty(roomName)) return new ValidationResult(false, "방 이름을 입력해주세요.");
            if (!_cachedRoomList.ContainsKey(roomName)) return new ValidationResult(false, "존재하지 않는 방입니다.");
            
            var roomInfo = _cachedRoomList[roomName];
            bool isPublic = roomInfo.CustomProperties.ContainsKey("roomType") && roomInfo.CustomProperties["roomType"].ToString() == "public";
            string roomPassWord = roomInfo.CustomProperties.ContainsKey("password") ? roomInfo.CustomProperties["password"].ToString() : "";
            
            if (roomInfo.PlayerCount >= roomInfo.MaxPlayers) return new ValidationResult(false, "방이 가득 찼습니다.");
            if (isPublic == false && password != roomPassWord) return new ValidationResult(false, "비밀번호가 일치하지 않습니다.");
            
            return new ValidationResult(true, "방 참가 가능합니다.");
        }
    }

    /// <summary> 검증 결과를 담는 클래스 </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }
}
