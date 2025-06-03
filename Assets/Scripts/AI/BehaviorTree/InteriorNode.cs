using System.Collections.Generic;


namespace CMPM146.AI.BehaviorTree {
    public class InteriorNode : BehaviorTree {
        protected readonly List<BehaviorTree> Children;
        protected int CurrentChild;

        public InteriorNode(IEnumerable<BehaviorTree> children) : base() {
            Children = new List<BehaviorTree>();
            Children.AddRange(children);
            CurrentChild = 0;
        }

        public List<BehaviorTree> CopyChildren() {
            List<BehaviorTree> newChildren = new();
            foreach (BehaviorTree c in Children) {
                newChildren.Add(c.Copy());
            }

            return newChildren;
        }

        public override IEnumerable<BehaviorTree> AllNodes() {
            yield return this;
            foreach (BehaviorTree c in Children) {
                foreach (BehaviorTree n in c.AllNodes()) {
                    yield return n;
                }
            }
        }
    }
}