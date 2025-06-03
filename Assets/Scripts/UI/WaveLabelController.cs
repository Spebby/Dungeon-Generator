using CMPM146.Core;
using TMPro;
using UnityEngine;


namespace CMPM146.UI {
    public class WaveLabelController : MonoBehaviour {
        TextMeshProUGUI _tmp;

        void Start() {
            _tmp = GetComponent<TextMeshProUGUI>();
        }

        void Update() {
            if (GameManager.Instance.State == GameManager.GameState.INWAVE)
                _tmp.text = "Enemies left: " + GameManager.Instance.EnemyCount;
            if (GameManager.Instance.State == GameManager.GameState.COUNTDOWN)
                _tmp.text = "Starting in " + GameManager.Instance.Countdown;
        }
    }
}