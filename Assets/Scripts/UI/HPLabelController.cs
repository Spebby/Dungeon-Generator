using CMPM146.Core;
using TMPro;
using UnityEngine;


namespace CMPM146.UI {
    public class HPLabelController : MonoBehaviour {
        TextMeshProUGUI _tmp;
        PlayerController _player;

        void Start() {
            _tmp = GetComponent<TextMeshProUGUI>();
        }

        void Update() {
            if (GameManager.Instance.State != GameManager.GameState.INWAVE) return;
            if (!_player) _player = GameManager.Instance.Player.GetComponent<PlayerController>();
            _tmp.text = "HP: " + _player.HP.HP + "/" + _player.HP.MaxHP + " (lowest: " + _player.HP.MinHP + ")";
        }
    }
}