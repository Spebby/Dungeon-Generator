using System.Collections.Generic;


namespace CMPM146.AI.BehaviorTree {
    public class Sequence : InteriorNode {
        public override Result Run() {
            if (CurrentChild >= Children.Count) {
                CurrentChild = 0;
                return Result.SUCCESS;
            }

            Result res = Children[CurrentChild].Run();
            if (res == Result.FAILURE) {
                CurrentChild = 0;
                return Result.FAILURE;
            }

            if (res == Result.SUCCESS) CurrentChild++;
            return Result.IN_PROGRESS;
        }

        public Sequence(IEnumerable<BehaviorTree> children) : base(children) { }

        public override BehaviorTree Copy() {
            return new Sequence(CopyChildren());
        }
    }
}