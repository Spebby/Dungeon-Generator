using CMPM146.Core;
using CMPM146.Enemies;
using UnityEngine;


namespace CMPM146.AI.BehaviorTree.Actions {
    public class PermaBuff : BehaviorTree {
        public override Result Run() {
            GameObject  target = GameManager.Instance.GetClosestOtherEnemy(Agent.gameObject);
            EnemyAction act    = Agent.GetAction("permabuff");
            if (act == null) return Result.FAILURE;

            bool success = act.Do(target.transform);
            return success ? Result.SUCCESS : Result.FAILURE;
        }

        public PermaBuff() : base() { }

        public override BehaviorTree Copy() {
            return new PermaBuff();
        }
    }
}