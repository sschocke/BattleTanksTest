using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleTanksTest
{
    public class MiniMaxAgent
    {
        private AIDebugWindow debugWindow;
        private long nodesSearched;
        private int searchDepth;

        private struct Result
        {
            public double Score;
            public Action[] Actions;

            public Result(double score, Action[] actions)
            {
                this.Score = score;
                this.Actions = actions;
            }
        }

        public MiniMaxAgent(AIDebugWindow debug)
        {
            this.debugWindow = debug;
            this.searchDepth = 1;
        }

        public Action[] GetAction(ClientGameState myState)
        {
            this.nodesSearched = 0;
            Result best = this.getNode(-1, 0, myState);

            return best.Actions;
        }

        private Result getNode(int curDepth, int agentIdx, ClientGameState curState)
        {
            Player me = curState.Me;
            Player opponent = curState.Opponent;
            
            this.nodesSearched++;
            if (agentIdx == 0) {
                curDepth++;
            } else {
                me = curState.Opponent;
                opponent = curState.Me;
            }


            if ((curDepth == this.searchDepth) || (curState.Won == true) || (curState.Lost == true))
            {
                double score = curState.Evaluate();
                Action[] actions = new Action[me.units.Count];
                for (int idx = 0; idx < me.units.Count; idx++)
                {
                    actions[idx] = Action.NONE;
                }

                return new Result(score, actions);
            }

            Result result;
            if (agentIdx == 0)
            {
                result = this.getMax(curDepth, agentIdx, curState);
            }
            else
            {
                result = this.getMin(curDepth, agentIdx, curState);
            }

            return result;
        }

        private Result getMin(int curDepth, int agentIdx, ClientGameState curState)
        {
            double min = Double.MaxValue;
            Action[] bestActions = new Action[curState.Me.units.Count];
            int opponentIdx = (curState.myPlayerIdx + 1) % 2;
            List<Action[]> legalActions = curState.GetLegalActions(opponentIdx);

            foreach (Action[] action in legalActions)
            {
                ClientGameState successor = new ClientGameState(curState);
                for (int unitIdx = 0; unitIdx < successor.Opponent.units.Count; unitIdx++)
                {
                    successor.Opponent.units[unitIdx].action = action[unitIdx];
                }
                int nextAgentIdx = (agentIdx + 1) % 2;

                Result result = this.getNode(curDepth, nextAgentIdx, successor);
                if (result.Score < min)
                {
                    min = result.Score;
                    result.Actions.CopyTo(bestActions, 0);
                }
            }

            return new Result(min, bestActions);
        }

        private Result getMax(int curDepth, int agentIdx, ClientGameState curState)
        {
            double max = Double.MinValue;
            Action[] bestActions = new Action[curState.Me.units.Count];
            List<Action[]> legalActions = curState.GetLegalActions(curState.myPlayerIdx);

            foreach (Action[] action in legalActions)
            {
                ClientGameState successor = new ClientGameState(curState);
                for (int unitIdx = 0; unitIdx < successor.Me.units.Count; unitIdx++)
                {
                    successor.Me.units[unitIdx].action = action[unitIdx];
                }
                int nextAgentIdx = (agentIdx + 1) % 2;

                Result result = this.getNode(curDepth, nextAgentIdx, successor);
                if (result.Score > max)
                {
                    max = result.Score;
                    result.Actions.CopyTo(bestActions, 0);
                }
            }

            return new Result(max, bestActions);
        }
    }
}
