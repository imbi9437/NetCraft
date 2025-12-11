using _Project.Script.VoiceChat;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime; // Player 정보를 사용하기 위해 추가

public class PlayerVolumeSlider : MonoBehaviour
{
    // Inspector 창에서 연결할 UI 요소들
    [SerializeField] private Text playerNameText;
    [SerializeField] private Slider volumeSlider;

    private int playerActorNumber; // 이 UI가 담당하는 플레이어의 고유 번호
    private VoiceUI voiceUI; // 메인 UI 매니저

    // VoiceUI가 이 함수를 호출하여 UI를 초기화합니다.
    public void Setup(Player player, VoiceUI uiManager, float initialVolume)
    {
        playerActorNumber = player.ActorNumber;
        // 플레이어 닉네임이 없으면 "Player (고유번호)" 형식으로 표시
        playerNameText.text = string.IsNullOrEmpty(player.NickName) ? $"Player {player.ActorNumber}" : player.NickName;
        voiceUI = uiManager;

        // 슬라이더 값 변경 이벤트를 함수에 연결
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.value = initialVolume;
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    // 슬라이더 값이 변경될 때 호출되는 함수
    private void OnSliderValueChanged(float value)
    {
        // 메인 VoiceUI 매니저에게 "이 플레이어의 볼륨을 이 값으로 변경해줘" 라고 알림
        if (voiceUI != null)
        {
            voiceUI.SetPlayerVolume(playerActorNumber, value);
        }
    }
}