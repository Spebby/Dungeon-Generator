using CMPM146.Core;
using CMPM146.Enemies;


namespace CMPM146.AI.BehaviorTree.Actions {
    public class Attack : BehaviorTree {
        public override Result Run() {
            EnemyAction act = Agent.GetAction("attack");
            if (act == null) return Result.FAILURE;

            bool success = act.Do(GameManager.Instance.Player.transform);
            return success ? Result.SUCCESS : Result.FAILURE;
        }

        public Attack() : base() { }

        public override BehaviorTree Copy() {
            return new Attack();
        }
    }
}