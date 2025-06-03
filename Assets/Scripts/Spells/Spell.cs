using System.Collections;
using CMPM146.Core;
using CMPM146.DamageSystem;
using UnityEngine;


namespace CMPM146.Spells {
    public class Spell {
        public static float Duration() {
            return 1.5f + GameManager.Instance.CurrentTime() / 240;
        }

        public static float Speed() {
            return 10f;
        }

        public static int ProjectileDamage() {
            return GameManager.Instance.CurrentTime() > GameManager.Instance.WinTime() / 2 ? 35 : 25;
        }

        public float LastCast;
        public SpellCaster Owner;
        public Hittable.Team Team;

        public Spell(SpellCaster owner) {
            Owner = owner;
        }

        public string GetName() {
            return "Bolt";
        }

        public int GetManaCost() {
            return 10;
        }

        public int GetDamage() {
            return ProjectileDamage();
        }

        public float GetCooldown() {
            return 1.5f;
        }

        public virtual int GetIcon() {
            return 0;
        }

        public bool IsReady() {
            return LastCast + GetCooldown() < Time.time;
        }

        public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team) {
            if (!IsReady()) yield break;
            Team     = team;
            LastCast = Time.time;
            GameManager.Instance.ProjectileManager.CreateProjectile(0, "homing", where, target - where, Speed(), OnHit,
                                                                    Duration());
            yield return new WaitForEndOfFrame();
        }

        void OnHit(Hittable other, Vector3 impact) {
            if (other.team != Team) other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }
    }
}