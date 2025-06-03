using CMPM146.Core;
using CMPM146.DamageSystem;
using UnityEngine;


namespace CMPM146.Enemies {
    public class EnemyAttack : EnemyAction {
        int _damage;
        public int AttackDamage => _damage;

        float _strengthFactor;
        public float StrengthFactor => _strengthFactor;

        protected override bool Perform(Transform target) {
            int amount = _damage;
            amount += Mathf.RoundToInt(Enemy.GetEffect("strength") * _strengthFactor);
            GameManager.Instance.Player.GetComponent<PlayerController>().HP
                       .Damage(new Damage(amount, Damage.Type.PHYSICAL));
            return true;
        }

        public EnemyAttack(float cooldown, float range, int damage, float strengthFactor) : base(cooldown, range) {
            _damage         = damage;
            _strengthFactor = strengthFactor;
        }
    }
}