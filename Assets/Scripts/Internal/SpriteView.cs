using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace CMPM146.Internal {
    public class SpriteView : MonoBehaviour {
        public TextMeshProUGUI label;

        public Image image;

        public void Apply(string label, Sprite sprite) {
            this.label.text = label;
            image.sprite    = sprite;
        }
    }
}