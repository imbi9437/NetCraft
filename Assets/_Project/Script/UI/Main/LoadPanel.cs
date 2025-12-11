using System;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Script.UI.Main
{
    public class LoadPanel : MainPanel
    {
        public override MainMenuPanelType UIType => MainMenuPanelType.Load;
        
        [Header("방 이름 & 비밀번호")]
        [SerializeField] private TMP_InputField worldNameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Toggle worldNameChangeToggle;

        [Header("최대 인원 설정")]
        [SerializeField] private Button maxPlayerDownButton;
        [SerializeField] private Button maxPlayerUpButton;
        [SerializeField] private TMP_Text maxPlayerText;
        
        [Header("방 공개 여부")]
        [SerializeField] private Toggle publicRoomToggle;
        [SerializeField] private Toggle privateRoomToggle;

        [Header("패널 UI")]
        [SerializeField] private Button createButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private TMP_Text logText;

        private bool isPublicWorld = true;
        private int maxPlayers = 4;
        
        private void OnEnable()
        {
            EnableRoomInfoUI();
            EnableMaxPlayerUI();
            EnablePanelElement();
            EnableRoomTypeUI();
            
            EventHub.Instance?.RegisterEvent<PunEvents.OnCreateRoomFailedEvent>(FailedCreateRoom);
        }

        private void OnDisable()
        {
            DisableRoomInfoUI();
            DisableMaxPlayerUI();
            DisablePanelElement();
            DisableRoomTypeUI();
            
            EventHub.Instance?.UnregisterEvent<PunEvents.OnCreateRoomFailedEvent>(FailedCreateRoom);
        }


        #region UI 초기화 함수


        private void EnableRoomInfoUI()
        {
            worldNameInput.interactable = false;
            passwordInput.interactable = false;
            worldNameChangeToggle.isOn = false;

            string worldName = DataManager.Instance.GetWorldName();
            worldNameInput.SetTextWithoutNotify(worldName);
            passwordInput.SetTextWithoutNotify("");
            
            worldNameChangeToggle.onValueChanged.AddListener(isOn => worldNameInput.interactable = isOn);
        }
        private void DisableRoomInfoUI()
        {
            worldNameChangeToggle.onValueChanged.RemoveAllListeners();
        }
        

        private void EnableMaxPlayerUI()
        {
            maxPlayerUpButton.onClick.AddListener(() => MaxPlayerButtonClickAnimate(maxPlayerUpButton));
            maxPlayerUpButton.onClick.AddListener(() => ChangeMaxPlayers(true));
            
            maxPlayerDownButton.onClick.AddListener(() => MaxPlayerButtonClickAnimate(maxPlayerDownButton));
            maxPlayerDownButton.onClick.AddListener(() => ChangeMaxPlayers(false));

            maxPlayers = DataManager.Instance.GetWorldMaxPlayers();
            maxPlayerText.text = $"{maxPlayers} 명";
        }
        private void DisableMaxPlayerUI()
        {
            maxPlayerUpButton.onClick.RemoveAllListeners();
            maxPlayerDownButton.onClick.RemoveAllListeners();
        }


        private void EnableRoomTypeUI()
        {
            isPublicWorld = true;
            publicRoomToggle.isOn = isPublicWorld;
            privateRoomToggle.isOn = !isPublicWorld;
            
            publicRoomToggle.onValueChanged.AddListener(ChangeRoomType);
        }
        private void DisableRoomTypeUI()
        {
            publicRoomToggle.onValueChanged.RemoveListener(ChangeRoomType);
        }
        
        
        private void EnablePanelElement()
        {
            logText.text = "";
            createButton.interactable = true;
            
            createButton.onClick.AddListener(CreateButtonClick);
            closeButton.onClick.AddListener(Hide);
        }
        private void DisablePanelElement()
        {
            createButton.onClick.RemoveListener(CreateButtonClick);
            closeButton.onClick.RemoveListener(Hide);
        }
        

        #endregion


        #region UI 이벤트 함수

        
        private void MaxPlayerButtonClickAnimate(Button button)
        {
            button.DOKill();
            button.transform.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
        private void ChangeMaxPlayers(bool isUp)
        {
            maxPlayers = isUp ? maxPlayers + 1 : maxPlayers - 1;
            if (maxPlayers > MultiPlayManager.MaxPlayersLimit) maxPlayers = 1;

            maxPlayerText.text = $"{maxPlayers} 명";
        }
        
        
        private void ChangeRoomType(bool isPublic)
        {
            isPublicWorld = isPublic;

            passwordInput.interactable = isPublicWorld == false;
            passwordInput.SetTextWithoutNotify("");
        }

        
        private void CreateButtonClick()
        {
            createButton.interactable = false;
            
            var evt = new DataEvents.RequestLoadWorldEvent()
            {
                worldName = worldNameInput.text,
                password = passwordInput.text,
                isPublic = isPublicWorld,
                maxPlayers = maxPlayers
            };
            
            EventHub.Instance.RaiseEvent(evt);
        }

        
        #endregion

        #region EventHub 함수

        
        private void FailedCreateRoom(PunEvents.OnCreateRoomFailedEvent evt)
        {
            createButton.interactable = true;
            logText.text = $"<color=red>생성 실패\n코드 : {evt.returnCode}\n메세지 : {evt.message}</color>";
        }

        
        #endregion
    }
}
