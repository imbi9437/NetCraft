using System;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Script.UI.Main.Element
{
    public class RoomItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private Image roomTypeIcon;
        [SerializeField] private Button joinButton;
        
        [SerializeField] private Sprite[] roomTypeIcons;
        
        
        private RoomInfo roomInfo;
        private Action<RoomInfo> onRoomClick;
        
        public void UpdateRoomItem(RoomInfo room, Action<RoomInfo> onRoomClick = null)
        {
            roomInfo = room;
            this.onRoomClick = onRoomClick;
            
            roomNameText.text = room.Name;
            playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
            
            bool isPublic = (bool)room.CustomProperties["roomType"];
            roomTypeIcon.sprite = roomTypeIcons[isPublic ? 0 : 1];
            
            if (joinButton != null)
                joinButton.onClick.AddListener(OnJoinButtonClick);
        }

        private void OnJoinButtonClick() => onRoomClick?.Invoke(roomInfo);
    }
}
