using System.Collections;
using CMPM146.DamageSystem;
using UnityEngine;


namespace CMPM146.Spells {
    public class SpellCaster {
        public int Mana;
        public readonly int MaxMana;
        public readonly int ManaReg;
        public readonly Hittable.Team Team;
        public readonly Spell Spell;

        public IEnumerator ManaRegeneration() {
            while (true) {
                Mana += ManaReg;
                Mana =  Mathf.Min(Mana, MaxMana);
                yield return new WaitForSeconds(1);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        public SpellCaster(int mana, int manaReg, Hittable.Team team) {
            Mana    = mana;
            MaxMana = mana;
            ManaReg = manaReg;
            Team    = team;
            Spell   = new SpellBuilder().Build(this);
        }

        public IEnumerator Cast(Vector3 where, Vector3 target) {
            if (Mana < Spell.GetManaCost() || !Spell.IsReady()) yield break;
            Mana -= Spell.GetManaCost();
            yield return Spell.Cast(where, target, Team);
        }
    }
}