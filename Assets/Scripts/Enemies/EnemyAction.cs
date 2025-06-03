using CMPM146.Movement;
using UnityEngine;


namespace CMPM146.Enemies {
    public class EnemyAction {
        public float LastUse;
        public readonly float Cooldown;
        public readonly float Range;

        public EnemyController Enemy;

        public bool Ready() {
            return LastUse + Cooldown < Time.time;
        }

        public bool CanDo(Transform target) {
            return Ready() && InRange(target);
        }

        public bool InRange(Transform target) {
            return (target.position - Enemy.transform.position).magnitude <= Range;
        }

        public bool Do(Transform target) {
            if (!CanDo(target)) return false;
            LastUse = Time.time;
            return Perform(target);
        }

        protected virtual bool Perform(Transform target) {
            return false;
        }

        public EnemyAction(float cooldown, float range) {
            Cooldown = cooldown;
            Range    = range;
        }
    }
}