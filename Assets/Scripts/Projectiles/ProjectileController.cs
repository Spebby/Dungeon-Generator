using System;
using System.Collections;
using CMPM146.Core;
using CMPM146.DamageSystem;
using CMPM146.Movement;
using UnityEngine;


namespace CMPM146.Projectiles {
    public class ProjectileController : MonoBehaviour {
        public float lifetime;
        public event Action<Hittable, Vector3> OnHit;
        public ProjectileMovement Movement;

        void Update() {
            Movement.Movement(transform);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.CompareTag("projectile")) return;
            if (collision.gameObject.CompareTag("unit")) {
                EnemyController ec = collision.gameObject.GetComponent<EnemyController>();
                if (ec) {
                    OnHit?.Invoke(ec.HP, transform.position);
                } else {
                    PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                    if (pc != null) OnHit?.Invoke(pc.HP, transform.position);
                }
            }

            Destroy(gameObject);
        }

        public void SetLifetime(float lifetime) {
            StartCoroutine(Expire(lifetime));
        }

        IEnumerator Expire(float lifetime) {
            yield return new WaitForSeconds(lifetime);
            Destroy(gameObject);
        }
    }
}