using CMPM146.Core;
using UnityEngine;


namespace CMPM146.UI {
    public class RelicUIManager : MonoBehaviour {
        public GameObject relicUIPrefab;
        public PlayerController player;

        void Start() {
            //EventBus.Instance.OnRelicPickup += OnRelicPickup;
        }

        void Update() { }

        /*public void OnRelicPickup(Relic r)
        {
            // make a new Relic UI representation
            GameObject rui = Instantiate(relicUIPrefab, transform);
            rui.transform.localPosition = new Vector3(-450 + 40 * (player.relics.Count - 1), 0, 0);
            RelicUI ruic = rui.GetComponent<RelicUI>();
            ruic.player = player;
            ruic.index = player.relics.Count - 1;

        }*/
    }
}