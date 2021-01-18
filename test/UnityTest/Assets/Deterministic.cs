using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Box2DSharp.Testbed.Unity
{
    public class Deterministic : MonoBehaviour
    {
        public Text HashText;

        public Button ExitButton;

        public string hash = "testing...";

        public void Start()
        {
            ExitButton.onClick.AddListener(() => SceneManager.LoadScene("Init"));
            Task.Factory.StartNew(
                () =>
                {
                    var res = new DeterministicTester().TestTumbler(3000, 400);
                    hash = res.Hash;
                    var path = Path.Combine(Application.persistentDataPath, "TestTumbler.txt");
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    File.WriteAllText(path, res.Data);
                    Debug.Log($"Data write to {path}");
                });
        }

        private void Update()
        {
            HashText.text = hash;
        }
    }
}