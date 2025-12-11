using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace _Project.Script.VoiceChat
{
    public class SoundSettingsManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        public AudioMixer mainMixer;

        [Header("UI Sliders")]
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider uiVolumeSlider;

        public static SoundSettingsManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("SoundSettingsManager 초기화 완료.");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            if (uiVolumeSlider != null) uiVolumeSlider.onValueChanged.AddListener(SetUiVolume);

            LoadVolumeSettings();
        }

        void LoadVolumeSettings()
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            float uiVol = PlayerPrefs.GetFloat("UIVolume", 0.75f);

            if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol;
            SetMusicVolume(musicVol);

            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol;
            SetSfxVolume(sfxVol);

            if (uiVolumeSlider != null) uiVolumeSlider.value = uiVol;
            SetUiVolume(uiVol);

            Debug.Log($"저장된 볼륨 설정 불러오기 완료: Music({musicVol}), SFX({sfxVol}), UI({uiVol})");
        }

        public void SetMusicVolume(float volume)
        {
            float dbVolume = volume > 0.001f ? Mathf.Log10(volume) * 20 : -80f;
            mainMixer.SetFloat("MusicVolume", dbVolume);
            PlayerPrefs.SetFloat("MusicVolume", volume);
            Debug.Log($"배경음악 볼륨 설정: {volume} (dB: {dbVolume})");
        }

        public void SetSfxVolume(float volume)
        {
            float dbVolume = volume > 0.001f ? Mathf.Log10(volume) * 20 : -80f;
            mainMixer.SetFloat("SFXVolume", dbVolume);
            PlayerPrefs.SetFloat("SFXVolume", volume);
            Debug.Log($"효과음 볼륨 설정: {volume} (dB: {dbVolume})");
        }

        public void SetUiVolume(float volume)
        {
            float dbVolume = volume > 0.001f ? Mathf.Log10(volume) * 20 : -80f;
            mainMixer.SetFloat("UIVolume", dbVolume);
            PlayerPrefs.SetFloat("UIVolume", volume);
            Debug.Log($"UI 볼륨 설정: {volume} (dB: {dbVolume})");
        }
    }
}