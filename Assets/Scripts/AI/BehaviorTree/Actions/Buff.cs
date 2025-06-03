using CMPM146.Core;
using CMPM146.Enemies;
using UnityEngine;


namespace CMPM146.AI.BehaviorTree.Actions {
    public class Buff : BehaviorTree {
        public override Result Run() {
            GameObject  target = GameManager.Instance.GetClosestOtherEnemy(Agent.gameObject);
            EnemyAction act    = Agent.GetAction("buff");
            if (act == null) return Result.FAILURE;

            bool success = act.Do(target.transform);
            return success ? Result.SUCCESS : Result.FAILURE;
        }

        public Buff() : base() { }

        public override BehaviorTree Copy() {
            return new Buff();
        }
    }
}