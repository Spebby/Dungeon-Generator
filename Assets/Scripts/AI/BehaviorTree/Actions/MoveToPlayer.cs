using CMPM146.Core;
using CMPM146.Movement;
using UnityEngine;


namespace CMPM146.AI.BehaviorTree.Actions {
    public class MoveToPlayer : BehaviorTree {
        readonly float _arrivedDistance;

        public override Result Run() {
            Vector3 direction = GameManager.Instance.Player.transform.position - Agent.transform.position;
            if (direction.magnitude < _arrivedDistance) {
                Agent.GetComponent<Unit>().movement = new Vector2(0, 0);
                return Result.SUCCESS;
            }

            Agent.GetComponent<Unit>().movement = direction.normalized;
            return Result.IN_PROGRESS;
        }

        public MoveToPlayer(float arrivedDistance) : base() {
            _arrivedDistance = arrivedDistance;
        }

        public override BehaviorTree Copy() {
            return new MoveToPlayer(_arrivedDistance);
        }
    }
}