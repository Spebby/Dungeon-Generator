using CMPM146.AI.BehaviorTree;
using CMPM146.AI.BehaviorTree.Actions;
using CMPM146.Movement;


namespace CMPM146.AI {
    public class BehaviorBuilder {
        public static BehaviorTree.BehaviorTree MakeTree(EnemyController agent) {
            BehaviorTree.BehaviorTree result = agent.monster switch {
                "warlock" => new Sequence(new BehaviorTree.BehaviorTree[] {
                    new MoveToPlayer(agent.GetAction("attack").Range), new Attack(), new PermaBuff(), new Heal(),
                    new Buff()
                }),
                "zombie" => new Sequence(new BehaviorTree.BehaviorTree[] {
                    new MoveToPlayer(agent.GetAction("attack").Range), new Attack()
                }),
                _ => new Sequence(new BehaviorTree.BehaviorTree[] {
                    new MoveToPlayer(agent.GetAction("attack").Range), new Attack()
                })
            };

            // do not change/remove: each node should be given a reference to the agent
            foreach (BehaviorTree.BehaviorTree n in result.AllNodes()) {
                n.SetAgent(agent);
            }

            return result;
        }
    }
}