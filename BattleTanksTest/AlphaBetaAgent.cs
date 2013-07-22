using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BattleTanksTest
{
    public class AlphaBetaAgent : IAgent
    {
        private AIDebugWindow debugWindow;
        private long nodesSearched;
        private long cutoffs;
        private int searchDepth;
        private Stopwatch searchDuration;
        private Stack<ActionSet> moveStack;

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

        public AlphaBetaAgent(AIDebugWindow debug)
        {
            this.debugWindow = debug;
            this.searchDepth = 2;
            this.searchDuration = new Stopwatch();
            this.moveStack = new Stack<ActionSet>();
        }

        public ActionSet GetAction(ClientGameState myState)
        {
            this.nodesSearched = 0;
            this.cutoffs = 0;
            this.moveStack.Clear();
            this.searchDuration.Restart();
            double alpha = double.MinValue;
            double beta = double.MaxValue;
            Result best = this.getNode(-1, 0, myState, alpha, beta);
            searchDuration.Stop();
            this.debugWindow.AddLog("Depth " + this.searchDepth + " searched " + this.nodesSearched + " nodes(" + this.cutoffs + " cutoffs) in " + searchDuration.ElapsedMilliseconds + "ms");
            return best.Actions;
        }

        private Result getNode(int curDepth, int agentIdx, ClientGameState curState, double alpha, double beta)
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

            if (me.bullets.Count < 1 || curDepth > 15)
            {
#if DEBUG
                if ((curDepth >= this.searchDepth) || (curState.Won == true) || (curState.Lost == true))
#else
            if ((curDepth == this.searchDepth) || (curState.Won == true) || (curState.Lost == true) || (searchDuration.ElapsedMilliseconds > 2800))
#endif
                {
                    double score = curState.Evaluate();
                    //Program.abDebug.WriteLine(String.Join(",", moveStack) + " - " + curDepth.ToString() + ":" + score.ToString());
                    ActionSet actions = new ActionSet(me.units.Count);
                    for (int idx = 0; idx < me.units.Count; idx++)
                    {
                        actions[idx] = Action.NONE;
                    }

                    return new Result(score, actions);
                }
            }

            Result result;
            if (agentIdx == 0)
            {
                result = this.getMax(curDepth, agentIdx, curState, alpha, beta);
            }
            else
            {
                result = this.getMin(curDepth, agentIdx, curState, alpha, beta);
            }

            return result;
        }

        private Result getMin(int curDepth, int agentIdx, ClientGameState curState, double alpha, double beta)
        {
            List<ActionSet> legalActions;
            double min = Double.MaxValue;
            ActionSet bestActions = new ActionSet(curState.Opponent.units.Count);
            int opponentIdx = (curState.myPlayerIdx + 1) % 2;
            if (curDepth < this.searchDepth)
            {
                legalActions = curState.GetLegalActions(opponentIdx);
            }
            else
            {
                legalActions = new List<ActionSet>();
                legalActions.Add(bestActions);
            }
            if (legalActions.Count < 1)
            {
                return new Result(1000, bestActions);
            }

            foreach (ActionSet action in legalActions)
            {
                //Program.abDebug.Write("[" + action.ToString() + "]");
                ClientGameState successor = new ClientGameState(curState);
                for (int unitIdx = 0; unitIdx < successor.Opponent.units.Count; unitIdx++)
                {
                    successor.Opponent.units[unitIdx].action = action[unitIdx];
                }
                successor.UpdateGameState();
                int nextAgentIdx = (agentIdx + 1) % 2;
                moveStack.Push(action);
                Result result = this.getNode(curDepth, nextAgentIdx, successor, alpha, beta);
                if (result.Score < min)
                {
                    min = result.Score;
                    //Program.abDebug.WriteLine("Min at " + curDepth + " : " + min);
                    action.CopyTo(bestActions);
                    beta = min;
                }
                moveStack.Pop();
                if (min < alpha)
                {
                    //Program.abDebug.WriteLine("Alpha Cutoff");
                    this.cutoffs++;
                    break;
                }
            }

            return new Result(min, bestActions);
        }

        private Result getMax(int curDepth, int agentIdx, ClientGameState curState, double alpha, double beta)
        {
            List<ActionSet> legalActions;
            double max = Double.MinValue;
            ActionSet bestActions = new ActionSet(curState.Me.units.Count);
            if (curDepth < this.searchDepth)
            {
                legalActions = curState.GetLegalActions(curState.myPlayerIdx);
            }
            else
            {
                legalActions = new List<ActionSet>();
                legalActions.Add(bestActions);
            }
            if (legalActions.Count < 1)
            {
                return new Result(-1000, bestActions);
            }

            foreach (ActionSet action in legalActions)
            {
                //Program.abDebug.Write("[" + action.ToString() + "]");
                ClientGameState successor = new ClientGameState(curState);
                for (int unitIdx = 0; unitIdx < successor.Me.units.Count; unitIdx++)
                {
                    successor.Me.units[unitIdx].action = action[unitIdx];
                }
                int nextAgentIdx = (agentIdx + 1) % 2;

                moveStack.Push(action);
                Result result = this.getNode(curDepth, nextAgentIdx, successor, alpha, beta);
                if (curDepth == 0)
                {
                    debugWindow.AddLog(action.ToString() + " : " + result.Score);
                }
                if (result.Score > max)
                {
                    max = result.Score;
                    //Program.abDebug.WriteLine("Max at " + curDepth + " : " + max);
                    action.CopyTo(bestActions);
                    alpha = max;
                    if (curDepth == 0)
                    {
                        for( int idx=0; idx<curState.Me.units.Count; idx++)
                        {
                            Program.MainForm.SetAction(curState.myPlayerIdx, curState.Me.units[idx].id, bestActions[idx]);
                        }
                    }
                }
                moveStack.Pop();
                if (max > beta)
                {
                    //Program.abDebug.WriteLine("Beta Cutoff");
                    this.cutoffs++;
                    break;
                }
            }

            return new Result(max, bestActions);
        }
    }
}
