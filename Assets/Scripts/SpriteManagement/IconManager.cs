using UnityEngine;
using UnityEngine.UI;


namespace CMPM146.SpriteManagement {
    public class IconManager : MonoBehaviour {
        [SerializeField] protected Sprite[] sprites;

        public void PlaceSprite(int which, Image target) {
            target.sprite = sprites[which];
        }

        public Sprite Get(int index) => sprites[index];
        public int GetCount() => sprites.Length;
    }
}