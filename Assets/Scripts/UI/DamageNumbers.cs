using CMPM146.Core;
using CMPM146.DamageSystem;
using UnityEngine;


namespace CMPM146.UI {
    public class DamageNumbers : MonoBehaviour {
        public GameObject DamageNumber;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            EventBus.Instance.OnDamage += OnDamage;
            EventBus.Instance.OnHeal   += OnHeal;
        }

        // Update is called once per frame
        void Update() { }

        void OnDamage(Vector3 where, Damage dmg, Hittable target) {
            GameObject newDmgNr = Instantiate(DamageNumber, where, Quaternion.identity);
            Vector3    dmgPos   = where + new Vector3(0, 0, -2);
            if (target.team == Hittable.Team.MONSTERS) {
                newDmgNr.GetComponent<AnimateDamage>().Setup(dmg.Amount.ToString(), dmgPos,
                                                             dmgPos + new Vector3(0, 3, 0), 10, 2, Color.blue,
                                                             Color.magenta, 1.5f);
            } else {
                newDmgNr.GetComponent<AnimateDamage>().Setup(dmg.Amount.ToString(), dmgPos,
                                                             dmgPos + new Vector3(0, 3, 0), 12, 4, Color.red,
                                                             Color.white, 1.5f);
            }
        }

        void OnHeal(Vector3 where, int amount, Hittable target) {
            GameObject newDmgNr = Instantiate(DamageNumber, where, Quaternion.identity);
            Vector3    dmgPos   = where + new Vector3(0, 0, -2);
            if (target.team == Hittable.Team.MONSTERS) {
                newDmgNr.GetComponent<AnimateDamage>().Setup(amount.ToString(), dmgPos, dmgPos + new Vector3(0, 3, 0),
                                                             12, 4, Color.green, Color.magenta, 1.5f);
            } else {
                newDmgNr.GetComponent<AnimateDamage>().Setup(amount.ToString(), dmgPos, dmgPos + new Vector3(0, 3, 0),
                                                             10, 2, Color.green, Color.white, 1.5f);
            }
        }
    }
}