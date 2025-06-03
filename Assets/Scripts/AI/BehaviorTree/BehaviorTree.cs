using System.Collections.Generic;
using CMPM146.Movement;


namespace CMPM146.AI.BehaviorTree {
    public class BehaviorTree {
        public enum Result {
            SUCCESS,
            FAILURE,
            IN_PROGRESS
        };

        public EnemyController Agent;

        public virtual Result Run() {
            return Result.SUCCESS;
        }

        public BehaviorTree() { }

        public void SetAgent(EnemyController agent) {
            Agent = agent;
        }

        public virtual IEnumerable<BehaviorTree> AllNodes() {
            yield return this;
        }

        public virtual BehaviorTree Copy() {
            return null;
        }
    }
}