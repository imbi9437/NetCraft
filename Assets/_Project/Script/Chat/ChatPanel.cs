using _Project.Script.Manager;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EVT = _Project.Script.EventStruct.PhotonChatEvents;


public class ChatPanel : MonoBehaviour
{
    private const float MovementValue = -164.48f;
    
    [Header("채팅 필수 패널 컴포넌트")]
    [SerializeField] private TextMeshProUGUI roomLabel;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;

    [SerializeField] private RectTransform messageContent;
    [SerializeField] private TextMeshProUGUI msgPrefab;
    
    [SerializeField] private Toggle minmaxToggle;
    [SerializeField] private GameObject downIconCover;
    [SerializeField] private GameObject upIconCover;
    [SerializeField] private RectTransform chatPanelRect;
    [SerializeField] private RectTransform topPanelRect;

    private Sequence sequence;
    
    private void Awake()
    {
        messageInput.onSubmit.AddListener(OnSendButtonClick);
        sendButton.onClick.AddListener(OnSendButtonClick);
        EventHub.Instance?.RegisterEvent<EVT.OnChatMsgReceivedEvent>(ReceiveChatMessage);
        
        minmaxToggle.onValueChanged.AddListener(MinmaxToggleClick);
    }

    private void OnDestroy()
    {
        messageInput.onSubmit.RemoveListener(OnSendButtonClick);
        sendButton.onClick.RemoveListener(OnSendButtonClick);
        EventHub.Instance?.UnregisterEvent<EVT.OnChatMsgReceivedEvent>(ReceiveChatMessage);
        
        minmaxToggle.onValueChanged.RemoveListener(MinmaxToggleClick);
    }

    #region 채팅 이벤트 함수

    
    private void OnSendButtonClick(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (message.ContainsFword())
        {
            var evt = new EVT.OnChatMsgReceivedEvent("","<color=red>경고","비속어가 포함되어 있습니다.</color>");
            ReceiveChatMessage(evt);
        }
        else
        {
            EventHub.Instance.RaiseEvent(new EVT.RequestSendChatMsgEvent(0, message));
        }
        messageInput.text = "";
        messageInput.ActivateInputField();
    }
    private void OnSendButtonClick() => OnSendButtonClick(messageInput.text);
    
    
    private void ReceiveChatMessage(EVT.OnChatMsgReceivedEvent evt)
    {
        TextMeshProUGUI messageEntry = Instantiate(msgPrefab, messageContent);
        string showMessage = $"{evt.senderName}: {evt.message}";
        messageEntry.text = showMessage;
    }

    
    #endregion

    
    // TODO : 병합 후 채팅창 토글 및 애니메이션 조정
    private void MinmaxToggleClick(bool isOn)
    {
        downIconCover.gameObject.SetActive(isOn == false);
        upIconCover.gameObject.SetActive(isOn);
        SetTop(isOn);
    }
    
    private void SetTop(bool isOn)
    {
        if (sequence.IsActive()) sequence.Kill();
        
        float offset = isOn ? MovementValue : 0;
        
        var topTween = topPanelRect.DOAnchorPosY(offset, 0.2f);
        var midSizeTween = chatPanelRect.DOSizeDelta(new Vector2(0, offset), 0.2f);
        var midPosTween = chatPanelRect.DOAnchorPosY(offset * 0.5f, 0.2f);

        sequence.Join(topTween);
        sequence.Join(midSizeTween);
        sequence.Join(midPosTween);
    }

}
