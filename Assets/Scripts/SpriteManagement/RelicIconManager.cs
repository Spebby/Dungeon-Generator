using CMPM146.Core;


namespace CMPM146.SpriteManagement {
    public class RelicIconManager : IconManager {
        void Start() {
            GameManager.Instance.RelicIconManager = this;
        }
    }
}