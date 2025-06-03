using CMPM146.Core;


namespace CMPM146.SpriteManagement {
    public class EnemySpriteManager : IconManager {
        void Start() {
            GameManager.Instance.EnemySpriteManager = this;
        }
    }
}