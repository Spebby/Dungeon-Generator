using CMPM146.Core;
using CMPM146.Spells;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace CMPM146.UI {
    public class SpellUI : MonoBehaviour {
        public GameObject icon;
        public RectTransform cooldown;
        public TextMeshProUGUI manacost;
        public TextMeshProUGUI damage;
        public GameObject highlight;
        public Spell Spell;
        float _lastTextUpdate;
        const float UPDATE_DELAY = 1;
        public GameObject dropbutton;

        void Start() {
            _lastTextUpdate = 0;
        }

        public void SetSpell(Spell spell) {
            Spell = spell;
            GameManager.Instance.SpellIconManager.PlaceSprite(spell.GetIcon(), icon.GetComponent<Image>());
        }

        void Update() {
            if (Spell == null) return;
            if (Time.time > _lastTextUpdate + UPDATE_DELAY) {
                manacost.text   = Spell.GetManaCost().ToString();
                damage.text     = Spell.GetDamage().ToString();
                _lastTextUpdate = Time.time;
            }

            float sinceLast = Time.time - Spell.LastCast;
            float ratio     = sinceLast > Spell.GetCooldown() ? 0 : 1 - sinceLast / Spell.GetCooldown();

            cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48 * ratio);
        }
    }
}