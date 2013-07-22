using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BattleTanksTest
{
    public class MiniMaxAgent : IAgent
    {
        private AIDebugWindow debugWindow;
        private long nodesSearched;
        private int searchDepth;

        private struct Result
        {
            public double Score;
            public ActionSet Actions;

            public Result(double score, ActionSet actions)
            {
                this.Score = score;
                this.Actions = actions;
            }
        }

        public MiniMaxAgent(AIDebugWindow debug)
        {
            this.debugWindow = debug;
            this.searchDepth = 2;
        }

        public ActionSet GetAction(ClientGameState myState)
        {
            Stopwatch searchDuration = new Stopwatch();

            this.nodesSearched = 0;
            searchDuration.Start();
            Result best = this.getNode(-1, 0, myState);
            searchDuration.Stop();
            this.debugWindow.AddLog("Depth " + this.searchDepth + " searched " + this.nodesSearched + " nodes in " + searchDuration.ElapsedMilliseconds + "ms");
            return best.Actions;
        }

        private Result getNode(int curDepth, int agentIdx, ClientGameState curState)
        {
            Player me = curState.Me;
            Player opponent = curState.Opponent;
            
            this.nodesSearched++;
            if (agentIdx == 0) {
                curDepth++;
                if (nodesSearched % 1000 == 0)
                {
                    this.debugWindow.AddLog("Searched " + this.nodesSearched + " nodes...");
                }
            } else {
                me = curState.Opponent;
                opponent = curState.Me;
            }


            if ((curDepth == this.searchDepth) || (curState.Won == true) || (curState.Lost == true))
            {
                double score = curState.Evaluate();
                ActionSet actions = new ActionSet(me.units.Count);
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
            ActionSet bestActions = new ActionSet(curState.Me.units.Count);
            int opponentIdx = (curState.myPlayerIdx + 1) % 2;
            List<ActionSet> legalActions = curState.GetLegalActions(opponentIdx);

            foreach (ActionSet action in legalActions)
            {
                ClientGameState successor = new ClientGameState(curState);
                for (int unitIdx = 0; unitIdx < successor.Opponent.units.Count; unitIdx++)
                {
                    successor.Opponent.units[unitIdx].action = action[unitIdx];
                }
                successor.UpdateGameState();
                int nextAgentIdx = (agentIdx + 1) % 2;

                Result result = this.getNode(curDepth, nextAgentIdx, successor);
                if (result.Score < min)
                {
                    min = result.Score;
                    action.CopyTo(bestActions);
                }
            }

            return new Result(min, bestActions);
        }

        private Result getMax(int curDepth, int agentIdx, ClientGameState curState)
        {
            double max = Double.MinValue;
            ActionSet bestActions = new ActionSet(curState.Me.units.Count);
            List<ActionSet> legalActions = curState.GetLegalActions(curState.myPlayerIdx);

            foreach (ActionSet action in legalActions)
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
                    action.CopyTo(bestActions);
                }
            }

            return new Result(max, bestActions);
        }
    }
}
