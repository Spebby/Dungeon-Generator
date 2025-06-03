using CMPM146.Core;
using TMPro;
using UnityEngine;


namespace CMPM146.UI {
    public class TimeLeftController : MonoBehaviour {
        TextMeshProUGUI _tmp;

        void Start() {
            _tmp = GetComponent<TextMeshProUGUI>();
        }

        void Update() {
            if (GameManager.Instance.State == GameManager.GameState.INWAVE) {
                _tmp.text = "Time left: "
                          + Mathf.RoundToInt(GameManager.Instance.WinTime() - GameManager.Instance.CurrentTime())
                          + "s";
            }
        }
    }
}