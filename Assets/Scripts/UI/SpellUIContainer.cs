using CMPM146.Core;
using UnityEngine;


namespace CMPM146.UI {
    public class SpellUIContainer : MonoBehaviour {
        public GameObject[] spellUIs;
        public PlayerController player;

        void Start() {
            // we only have one spell (right now)
            spellUIs[0].SetActive(true);
            for (int i = 1; i < spellUIs.Length; ++i) {
                spellUIs[i].SetActive(false);
            }
        }
    }
}