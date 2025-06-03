using CMPM146.Core;


namespace CMPM146.SpriteManagement {
    public class PlayerSpriteManager : IconManager {
        void Start() {
            GameManager.Instance.PlayerSpriteManager = this;
        }
    }
}