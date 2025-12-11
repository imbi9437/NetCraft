using System;
using System.Collections.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Script.UI.Main
{
    public enum MainMenuPanelType
    {
        Login,
        Create,
        Load,
        Lobby,
        Settings
    }

    public class MainUIManager : MonoBehaviour
    {
        private readonly Dictionary<MainMenuPanelType, MainPanel> _panels = new();

        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private bool hasWorld = false;
        private bool hasConnect = false;
        
        private void Awake()
        {
            var panels = GetComponentsInChildren<MainPanel>(true);

            foreach (var panel in panels)
            {
                _panels.TryAdd(panel.UIType, panel);
                panel.Initialize();
            }

            SubscribeButtonEvent();
            RegisterEvent();
        }

        private void Start()
        {
            if (FirebaseManager.Instance.IsLoggedIn == false) ShowPanel(MainMenuPanelType.Login);

            EventHub.Instance?.RaiseEvent(new PunEvents.RequestServerConnectionEvent());
            EventHub.Instance?.RaiseEvent(new DataEvents.RequestWorldDataExistEvent());
            EventHub.Instance?.RaiseEvent(new RequestPlaySoundEvent()
            {
                id = "BGM_Main",
                spatialBlend = 1f,
                loop = true,
                mixerGroupName = "MusicVolume"
            });
        }

        //test
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                HidePanel(MainMenuPanelType.Login);
            }
        }

        private void OnDestroy()
        {
            UnSubscribeButtonEvent();
            UnRegisterEvent();
        }

        #region 패널 컨트롤 함수

        private void ShowPanel(MainMenuPanelType type)
        {
            if (_panels.TryGetValue(type, out MainPanel panel) == false) return;

            panel.Show();
        }
        private void HidePanel(MainMenuPanelType type)
        {
            if (_panels.TryGetValue(type, out MainPanel panel) == false) return;

            panel.Hide();
        }

        #endregion


        #region 버튼 이벤트 래퍼


        private void SubscribeButtonEvent()
        {
            newGameButton.onClick.AddListener(() => ShowPanel(MainMenuPanelType.Create));
            loadGameButton.onClick.AddListener(() => ShowPanel(MainMenuPanelType.Load));
            joinGameButton.onClick.AddListener(() => ShowPanel(MainMenuPanelType.Lobby));
            settingsButton.onClick.AddListener(() => ShowPanel(MainMenuPanelType.Settings));

            exitButton.onClick.AddListener(Application.Quit);

            newGameButton.interactable = false;
            loadGameButton.interactable = false;
        }
        private void UnSubscribeButtonEvent()
        {
            newGameButton.onClick.RemoveAllListeners();
            loadGameButton.onClick.RemoveAllListeners();
            joinGameButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();

            exitButton.onClick.RemoveListener(Application.Quit);
        }


        #endregion


        #region EventHub 핸들러

        private void RegisterEvent()
        {
            EventHub.Instance?.RegisterEvent<PunEvents.OnConnectEvent>(OnConnected);
            EventHub.Instance?.RegisterEvent<PunEvents.SendServerConnectionEvent>(OnConnected);
            
            EventHub.Instance?.RegisterEvent<PunEvents.OnConnectEvent>(OnConnectedForLoad);
            EventHub.Instance?.RegisterEvent<PunEvents.SendServerConnectionEvent>(OnConnectedForLoad);
            
            EventHub.Instance?.RegisterEvent<DataEvents.SendWorldDataExistEvent>(ExistWorldData);
            EventHub.Instance?.RegisterEvent<DataEvents.CompleteInitUserDataEvent>(RequestWorldDataExist);
        }
        private void UnRegisterEvent()
        {
            EventHub.Instance?.UnregisterEvent<PunEvents.OnConnectEvent>(OnConnected);
            EventHub.Instance?.UnregisterEvent<PunEvents.SendServerConnectionEvent>(OnConnected);
            
            EventHub.Instance?.UnregisterEvent<PunEvents.OnConnectEvent>(OnConnectedForLoad);
            EventHub.Instance?.UnregisterEvent<PunEvents.SendServerConnectionEvent>(OnConnectedForLoad);
            
            EventHub.Instance?.UnregisterEvent<DataEvents.SendWorldDataExistEvent>(ExistWorldData);
            EventHub.Instance?.UnregisterEvent<DataEvents.CompleteInitUserDataEvent>(RequestWorldDataExist);
        }

        #endregion

        #region Event Rapper Functions
        
        private void OnConnected(PunEvents.OnConnectEvent evt) => SetNewButtonInteract(true);
        private void OnConnected(PunEvents.SendServerConnectionEvent evt) => SetNewButtonInteract(evt.isConnected);
        
        
        private void OnConnectedForLoad(PunEvents.OnConnectEvent evt)
        {
            hasConnect = true;
            SetLoadButtonInteract();
        }
        private void OnConnectedForLoad(PunEvents.SendServerConnectionEvent evt)
        {
            hasConnect = evt.isConnected;
            SetLoadButtonInteract();
        }

        
        private void RequestWorldDataExist(DataEvents.CompleteInitUserDataEvent evt) =>
            EventHub.Instance?.RaiseEvent(new DataEvents.RequestWorldDataExistEvent());
        private void ExistWorldData(DataEvents.SendWorldDataExistEvent evt)
        {
            hasWorld = evt.isExist;
            SetLoadButtonInteract();
        }

        #endregion

        private void SetNewButtonInteract(bool isOn) => newGameButton.interactable = isOn;
        private void SetLoadButtonInteract() => loadGameButton.interactable = hasConnect && hasWorld;

    }
}
