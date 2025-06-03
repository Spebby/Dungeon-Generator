using CMPM146.DamageSystem;
using UnityEngine;


namespace CMPM146.UI {
    public class HealthBar : MonoBehaviour {
        public GameObject slider;
        public Hittable HP;
        float _oldRatio;

        void Update() {
            if (HP == null) return;
            float ratio = HP.HP * 1.0f / HP.MaxHP;
            if (!(Mathf.Abs(_oldRatio - ratio) > 0.01f)) return;
            slider.transform.localScale    = new Vector3(ratio, 1, 1);
            slider.transform.localPosition = new Vector3(-(1 - ratio) / 2, 0, 0);
            _oldRatio                       = ratio;
        }

        public void SetHealth(Hittable hp) {
            HP = hp;
            float ratio = hp.HP * 1.0f / hp.MaxHP;

            slider.transform.localScale    = new Vector3(ratio, 1, 1);
            slider.transform.localPosition = new Vector3(-(1 - ratio) / 2, 0, 0);
            _oldRatio                       = ratio;
        }
    }
}