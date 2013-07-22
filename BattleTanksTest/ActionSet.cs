using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleTanksTest
{
    public class ActionSet
    {
        private Action[] actions;

        public ActionSet(int size)
        {
            this.actions = new Action[size];
        }
        public ActionSet(Action act1)
            : this(1)
        {
            this.actions[0] = act1;
        }
        public ActionSet(Action act1, Action act2)
            : this(2)
        {
            this.actions[0] = act1;
            this.actions[1] = act2;
        }

        public Action this[int idx]
        {
            get { return this.actions[idx]; }
            set { this.actions[idx] = value; }
        }

        public override string ToString()
        {
            string actionList = string.Join(",", this.actions);

            return "{" + actionList + "}";
        }

        public void CopyTo(ActionSet target)
        {
            for (int idx = 0; idx < this.actions.Length; idx++)
            {
                if (idx >= target.actions.Length) break;

                target.actions[idx] = this.actions[idx];
            }
        }
    }
}
