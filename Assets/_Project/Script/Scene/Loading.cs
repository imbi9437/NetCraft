using System;
using _Project.Script.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Script.Scene
{
    public class Loading : MonoBehaviour
    {
        [SerializeField] private float loadingTime = 5f;
        [SerializeField] private Slider loadingBar;

        private void Awake()
        {
            loadingBar.value = 0;
        }

        private void Start()
        {
            SceneController.Instance.LoadTargetScene(loadingTime);
        }

        private void Update()
        {
            loadingBar.value += Time.deltaTime / loadingTime;
            loadingBar.value = Mathf.Clamp(loadingBar.value, 0, 1);
        }
    }
}
