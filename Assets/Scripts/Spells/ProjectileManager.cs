using System;
using CMPM146.Core;
using CMPM146.DamageSystem;
using CMPM146.Projectiles;
using UnityEngine;


namespace CMPM146.Spells {
    public class ProjectileManager : MonoBehaviour {
        public GameObject[] projectiles;

        void Start() {
            GameManager.Instance.ProjectileManager = this;
        }

        public void CreateProjectile(
            int which, string trajectory, Vector3 where, Vector3 direction, float speed,
            Action<Hittable, Vector3> onHit) {
            GameObject newProjectile =
                Instantiate(projectiles[which], where + direction.normalized * 1.1f,
                            Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
            newProjectile.GetComponent<ProjectileController>().Movement =  MakeMovement(trajectory, speed);
            newProjectile.GetComponent<ProjectileController>().OnHit    += onHit;
        }

        public void CreateProjectile(
            int which, string trajectory, Vector3 where, Vector3 direction, float speed,
            Action<Hittable, Vector3> onHit,
            float lifetime) {
            GameObject newProjectile =
                Instantiate(projectiles[which], where + direction.normalized * 1.1f,
                            Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
            newProjectile.GetComponent<ProjectileController>().Movement =  MakeMovement(trajectory, speed);
            newProjectile.GetComponent<ProjectileController>().OnHit    += onHit;
            newProjectile.GetComponent<ProjectileController>().SetLifetime(lifetime);
        }

        public ProjectileMovement MakeMovement(string movementType, float speed) {
            return movementType switch {
                "straight"  => new StraightProjectileMovement(speed),
                "homing"    => new HomingProjectileMovement(speed),
                "spiraling" => new SpiralingProjectileMovement(speed),
                _           => null
            };
        }
    }
}