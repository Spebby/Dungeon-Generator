using UnityEngine;


namespace CMPM146.Projectiles {
    public class ProjectileMovement {
        public readonly float Speed;

        public ProjectileMovement(float speed) {
            Speed = speed;
        }

        public virtual void Movement(Transform transform) { }
    }
}