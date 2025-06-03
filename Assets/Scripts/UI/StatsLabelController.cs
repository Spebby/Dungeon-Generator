using CMPM146.Core;
using CMPM146.Spells;
using TMPro;
using UnityEngine;


namespace CMPM146.UI {
    public class StatsLabelController : MonoBehaviour {
        TextMeshProUGUI _tmp;

        float _lastUpdate;

        void Start() {
            _tmp        = GetComponent<TextMeshProUGUI>();
            _lastUpdate = 0;
        }

        static string F(float v) {
            return v.ToString("0.00");
        }

        void Update() {
            if (!(_lastUpdate + 1f < Time.time)) return;
            _lastUpdate = Time.time;
            if (GameManager.Instance.State == GameManager.GameState.INWAVE) {
                string stats =
                    "\nPlayer Spell Stats\nLifetime: "
                  + F(Spell.Duration())
                  + "\nSpeed: "
                  + Spell.Speed()
                  + "\nDamage: "
                  + Spell.ProjectileDamage();
                _tmp.text = stats;
            } else
                _tmp.text = "";
        }
    }
}