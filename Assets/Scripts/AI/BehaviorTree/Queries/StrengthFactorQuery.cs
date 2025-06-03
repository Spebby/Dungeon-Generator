using CMPM146.Core;
using CMPM146.Enemies;
using CMPM146.Movement;
using UnityEngine;


namespace CMPM146.AI.BehaviorTree.Queries {
    public class StrengthFactorQuery : BehaviorTree {
        float _minStrengthFactor;

        public override Result Run() {
            GameObject target = GameManager.Instance.GetClosestOtherEnemy(Agent.gameObject);
            if (((EnemyAttack)target.GetComponent<EnemyController>().GetAction("attack")).StrengthFactor
             >= _minStrengthFactor) return Result.SUCCESS;
            return Result.FAILURE;
        }

        public StrengthFactorQuery(float minStrengthFactor) : base() {
            _minStrengthFactor = minStrengthFactor;
        }

        public override BehaviorTree Copy() {
            return new StrengthFactorQuery(_minStrengthFactor);
        }
    }
}