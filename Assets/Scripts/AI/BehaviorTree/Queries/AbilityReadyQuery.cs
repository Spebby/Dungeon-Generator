namespace CMPM146.AI.BehaviorTree.Queries {
    public class AbilityReadyQuery : BehaviorTree {
        string _ability;

        public override Result Run() {
            if (Agent.GetAction(_ability).Ready()) return Result.SUCCESS;
            return Result.FAILURE;
        }

        public AbilityReadyQuery(string ability) : base() {
            _ability = ability;
        }

        public override BehaviorTree Copy() {
            return new AbilityReadyQuery(_ability);
        }
    }
}