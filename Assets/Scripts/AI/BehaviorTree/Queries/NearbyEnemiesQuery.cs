using System.Collections.Generic;
using CMPM146.Core;
using UnityEngine;


namespace CMPM146.AI.BehaviorTree.Queries {
    public class NearbyEnemiesQuery : BehaviorTree {
        int _count;
        float _distance;

        public override Result Run() {
            List<GameObject> nearby = GameManager.Instance.GetEnemiesInRange(Agent.transform.position, _distance);
            if (nearby.Count >= _count) return Result.SUCCESS;
            return Result.FAILURE;
        }

        public NearbyEnemiesQuery(int count, float distance) : base() {
            _count    = count;
            _distance = distance;
        }

        public override BehaviorTree Copy() {
            return new NearbyEnemiesQuery(_count, _distance);
        }
    }
}