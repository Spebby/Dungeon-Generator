using CMPM146.Core;
using CMPM146.Enemies;
using UnityEngine;


namespace CMPM146.AI.BehaviorTree.Actions {
    public class Heal : BehaviorTree {
        public override Result Run() {
            GameObject  target = GameManager.Instance.GetClosestOtherEnemy(Agent.gameObject);
            EnemyAction act    = Agent.GetAction("heal");
            if (act == null) return Result.FAILURE;

            bool success = act.Do(target.transform);
            return success ? Result.SUCCESS : Result.FAILURE;
        }

        public Heal() : base() { }

        public override BehaviorTree Copy() {
            return new Heal();
        }
    }
}