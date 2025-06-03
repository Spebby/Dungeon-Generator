using System.Collections;
using CMPM146.Movement;
using UnityEngine;


namespace CMPM146.Enemies {
    public class EnemyBuff : EnemyAction {
        int _amount;
        int _duration;

        protected override bool Perform(Transform target) {
            EnemyController healee = target.GetComponent<EnemyController>();

            healee.AddEffect("strength", _amount);
            if (_duration > 0) healee.StartCoroutine(Expire(healee));

            return true;
        }

        public IEnumerator Expire(EnemyController healee) {
            yield return new WaitForSeconds(_duration);
            healee.AddEffect("strength", -_amount);
        }

        public EnemyBuff(float cooldown, float range, int amount, int duration) : base(cooldown, range) {
            _amount   = amount;
            _duration = duration;
        }

        public EnemyBuff(float cooldown, float range, int amount) : base(cooldown, range) {
            _amount   = amount;
            _duration = -1;
        }
    }
}