using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


namespace CMPM146.UI {
    public class AnimateDamage : MonoBehaviour {
        [FormerlySerializedAs("color_i")] public Color colorI;
        [FormerlySerializedAs("color_f")] public Color colorF;
        public Vector3 initialPosition, finalPosition; //position to drift to, relative to the gameObject's local origin
        public float fadeDuration;
        float _fadeStartTime;
        [FormerlySerializedAs("font_i")] public int fontI;
        [FormerlySerializedAs("font_f")] public int fontF;
        public string dmg;
        float _timeoffset;

        public void Setup(
            string dmg, Vector3 initialPosition, Vector3 finalPosition, int fontI, int fontF, Color colorI,
            Color colorF, float duration) {
            this.dmg             = dmg;
            this.fontI           = fontI;
            this.fontF           = fontF;
            this.initialPosition = initialPosition;
            this.finalPosition   = finalPosition;
            this.colorI          = colorI;
            this.colorF          = colorF;
            fadeDuration         = duration;
        }

        void Start() {
            _fadeStartTime                = Time.time;
            GetComponent<TMP_Text>().text = dmg;
            _timeoffset                   = Random.value;
        }

        void Update() {
            float progress = (Time.time - _fadeStartTime) / fadeDuration;
            if (progress <= 1) {
                transform.position =
                    Vector3.Lerp(initialPosition, finalPosition, progress)
                  + new Vector3(Mathf.Sin((Time.time + _timeoffset) * 7) / 4, 0, 0);
                GetComponent<TMP_Text>().fontSize = Mathf.RoundToInt(progress * fontF + (1 - progress) * fontI);
                GetComponent<TMP_Text>().overrideColorTags = true;
                GetComponent<TMP_Text>().color = Color.Lerp(colorI, colorF, progress);
                GetComponent<TMP_Text>().fontMaterial.color = Color.Lerp(colorI, colorF, progress);
                _timeoffset += Random.value / 25;
            } else Destroy(gameObject);
        }
    }
}