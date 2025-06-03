using CMPM146.Core;


namespace CMPM146.SpriteManagement {
    public class SpellIconManager : IconManager {
        void Start() {
            GameManager.Instance.SpellIconManager = this;
        }
    }
}