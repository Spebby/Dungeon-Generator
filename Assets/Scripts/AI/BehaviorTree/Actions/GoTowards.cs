using CMPM146.Movement;
using UnityEngine;


namespace CMPM146.AI.BehaviorTree.Actions {
    public class GoTowards : BehaviorTree {
        Transform _target;
        float _arrivedDistance;
        float _distance;
        bool _inProgress;
        Vector3 _startPoint;

        public override Result Run() {
            if (!_inProgress) {
                _inProgress = true;
                _startPoint = Agent.transform.position;
            }

            Vector3 direction = _target.position - Agent.transform.position;
            if (direction.magnitude < _arrivedDistance ||
                (Agent.transform.position - _startPoint).magnitude >= _distance) {
                Agent.GetComponent<Unit>().movement = new Vector2(0, 0);
                _inProgress                         = false;
                return Result.SUCCESS;
            }

            Agent.GetComponent<Unit>().movement = direction.normalized;
            return Result.IN_PROGRESS;
        }

        public GoTowards(Transform target, float distance, float arrivedDistance) : base() {
            _target          = target;
            _arrivedDistance = arrivedDistance;
            _distance        = distance;
            _inProgress      = false;
        }

        public override BehaviorTree Copy() {
            return new GoTowards(_target, _distance, _arrivedDistance);
        }
    }
}