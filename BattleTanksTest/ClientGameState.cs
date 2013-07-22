using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;

namespace BattleTanksTest
{
    public class ClientGameState : GameStateWithLayout
    {
        public int myPlayerIdx;

        public ClientGameState(State[,] layout, GameStateBase state, int myIdx)
            : base(state)
        {
            this.blocks = layout;
            this.myPlayerIdx = myIdx;
        }

        public ClientGameState(ClientGameState source)
            : base(source)
        {
            this.myPlayerIdx = source.myPlayerIdx;
        }

        public override void ProcessEvents(Events events)
        {
            foreach (BlockEvent ev in events.blockEvents)
            {
                Debug.WriteLine("Player " + (this.myPlayerIdx + 1).ToString() + " : Marking block at " + ev.point.ToString() + " as " + ev.newState.ToString());
                this.blocks[ev.point.X, ev.point.Y] = ev.newState;
            }

            events.Clear();
        }

        public Player Me { get { return this.players[this.myPlayerIdx]; } }
        public Player Opponent { get { return this.players[(this.myPlayerIdx + 1) % 2]; } }

        public bool Won { get { return this.IsPlayerWinning(myPlayerIdx); } }
        public bool Lost { get { return this.IsPlayerLosing(myPlayerIdx); } }

        public double Evaluate()
        {
            if (this.Won == true) return 10000;
            if (this.Lost == true) return -10000;

            double score = 0.0;

            score += (this.Me.units.Count * 20.0);
            score -= (this.Opponent.units.Count * 19.0);

            foreach(Unit tank in this.Me.units)
            {
                Size distToOpponentBase = Utils.DistanceBetween(tank, this.Opponent.playerBase);
                if (distToOpponentBase.Width == 0)
                {
                    score += 2.0;
                    if ((distToOpponentBase.Height < 0) && (tank.direction == Direction.DOWN)) score += 5.0;
                    if ((distToOpponentBase.Height > 0) && (tank.direction == Direction.UP)) score += 5.0;
                }
                else if (distToOpponentBase.Height == 0)
                {
                    score += 2.0;
                    if ((distToOpponentBase.Width < 0) && (tank.direction == Direction.RIGHT)) score += 5.0;
                    if ((distToOpponentBase.Width > 0) && (tank.direction == Direction.LEFT)) score += 5.0;
                }
                else
                {
                    double manhattanDist = Utils.ManhattanDistance(distToOpponentBase);

                    score -= (manhattanDist * 1.0);
                }
            }

            //score += (this.Me.bullets.Count * 0.5);

            return score;
        }
    }
}
