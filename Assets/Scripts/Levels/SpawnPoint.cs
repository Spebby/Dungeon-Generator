using UnityEngine;


namespace CMPM146.Levels {
    public class SpawnPoint : MonoBehaviour {
        public enum SpawnName {
            RED,
            GREEN,
            BONE
        }

        public SpawnName kind;
    }
}