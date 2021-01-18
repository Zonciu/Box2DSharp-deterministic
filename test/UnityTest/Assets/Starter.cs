using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Box2DSharp.Testbed.Unity
{
    public class Starter : MonoBehaviour
    {
        public Button TestbedButton;

        public Button DeterministicButton;

        public void Start()
        {
            TestbedButton.onClick.AddListener(() => SceneManager.LoadScene("Test"));
            DeterministicButton.onClick.AddListener(() => SceneManager.LoadScene("Deterministic"));
        }
    }
}