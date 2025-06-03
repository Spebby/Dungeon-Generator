using CMPM146.DamageSystem;
using CMPM146.Movement;
using UnityEngine;


namespace CMPM146.Enemies {
    public class EnemyHeal : EnemyAction {
        int _amount;

        protected override bool Perform(Transform target) {
            EnemyController healee = target.GetComponent<EnemyController>();

            // some targets might have a debuff
            if (healee.GetEffect("noheal") > 0) return false;
            Enemy.HP.Damage(new Damage(1, Damage.Type.DARK));
            healee.HP.Heal(_amount);
            return true;
        }

        public EnemyHeal(float cooldown, float range, int amount) : base(cooldown, range) {
            _amount = amount;
        }
    }
}