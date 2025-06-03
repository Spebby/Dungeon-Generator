using CMPM146.Levels;
using TMPro;
using UnityEngine;


namespace CMPM146.UI {
    public class MenuSelectorController : MonoBehaviour {
        public TextMeshProUGUI label;
        public string level;
        public EnemySpawner spawner;

        public void SetLevel(string text) {
            level      = text;
            label.text = text;
        }

        public void StartLevel() {
            spawner.StartLevel(level);
        }
    }
}