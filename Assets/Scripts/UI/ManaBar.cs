using CMPM146.Spells;
using UnityEngine;


namespace CMPM146.UI {
    public class ManaBar : MonoBehaviour {
        public GameObject slider;

        public SpellCaster Sc;

        float _oldPerc;

        void Update() {
            if (Sc == null) return;
            float ratio = Sc.Mana * 1.0f / Sc.MaxMana;
            if (!(Mathf.Abs(_oldPerc - ratio) > 0.01f)) return;
            slider.transform.localScale    = new Vector3(ratio, 1, 1);
            slider.transform.localPosition = new Vector3(-(1 - ratio) / 2, 0, 0);
            _oldPerc                       = ratio;
        }

        public void SetSpellCaster(SpellCaster sc) {
            Sc       = sc;
            _oldPerc = 0;
        }
    }
}