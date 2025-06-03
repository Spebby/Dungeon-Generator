using System;
using CMPM146.Core;
using UnityEngine;


namespace CMPM146.DamageSystem {
    public class Hittable {
        public enum Team {
            PLAYER,
            MONSTERS
        }

        // ReSharper disable once InconsistentNaming
        public Team team;

        public int HP;
        public int MaxHP;
        public int MinHP;

        public readonly GameObject Owner;

        public void Damage(Damage damage) {
            EventBus.Instance.DoDamage(Owner.transform.position, damage, this);
            HP -= damage.Amount;
            if (HP < MinHP) MinHP = HP;
            if (HP <= 0) {
                HP = 0;
                OnDeath();
            }
        }

        public void Heal(int amount) {
            // no resurrection
            if (HP <= 0) return;

            // no overhealing
            if (MaxHP - HP < amount) amount = MaxHP - HP;
            if (amount == 0) return;
            EventBus.Instance.DoHeal(Owner.transform.position, amount, this);
            HP += amount;
        }

        public event Action OnDeath;

        public Hittable(int hp, Team team, GameObject owner) {
            HP        = hp;
            MaxHP     = hp;
            MinHP     = hp;
            this.team = team;
            Owner     = owner;
        }

        public void SetMaxHP(int maxHP) {
            float ratio = HP * 1.0f / MaxHP;
            MaxHP = maxHP;
            MinHP = maxHP;
            HP    = Mathf.RoundToInt(ratio * maxHP);
        }
    }
}