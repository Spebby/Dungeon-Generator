using CMPM146.AI.BehaviorTree;
using CMPM146.AI.BehaviorTree.Actions;
using CMPM146.Movement;


namespace CMPM146.AI {
    public class BehaviorBuilder {
        public static BehaviorTree.BehaviorTree MakeTree(EnemyController agent) {
            BehaviorTree.BehaviorTree result = null;
            if (agent.monster == "warlock") {
                result = new Sequence(new BehaviorTree.BehaviorTree[] {
                    new MoveToPlayer(agent.GetAction("attack").Range),
                    new Attack(),
                    new PermaBuff(),
                    new Heal(),
                    new Buff()
                });
            } else if (agent.monster == "zombie") {
                result = new Sequence(new BehaviorTree.BehaviorTree[] {
                    new MoveToPlayer(agent.GetAction("attack").Range),
                    new Attack()
                });
            } else {
                result = new Sequence(new BehaviorTree.BehaviorTree[] {
                    new MoveToPlayer(agent.GetAction("attack").Range),
                    new Attack()
                });
            }

            // do not change/remove: each node should be given a reference to the agent
            foreach (BehaviorTree.BehaviorTree n in result.AllNodes()) {
                n.SetAgent(agent);
            }

            return result;
        }
    }
}