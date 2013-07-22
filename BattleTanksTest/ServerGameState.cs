using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BattleTanksTest
{
    public class ServerGameState : GameStateWithLayout
    {
        public Events[] playerEvents;

        public ServerGameState(string layout)
            : base(layout)
        {
            this.players[0] = new Player("Player 1");
            this.players[1] = new Player("Player 2");

            fillBoard(layout);

            setupTanks();

            this.playerEvents = new Events[2];
            this.playerEvents[0] = new Events();
            this.playerEvents[1] = new Events();

            this.onWallDestroyed += new DestroyedWall(ServerGameState_onWallDestroyed);
        }

        void ServerGameState_onWallDestroyed(int x, int y)
        {
            BlockEvent ev = new BlockEvent(x, y, State.EMPTY);
            AddBlockEvent(ev);
        }

        public void AddBlockEvent(BlockEvent ev)
        {
            this.playerEvents[0].blockEvents.Enqueue(ev);
            this.playerEvents[1].blockEvents.Enqueue(ev);
        }
        public void AddUnitEvent(UnitEvent ev)
        {
            this.playerEvents[0].unitEvents.Enqueue(ev);
            this.playerEvents[1].unitEvents.Enqueue(ev);
        }

        private void setupTanks()
        {
            for (int p = 0; p < players.Length; p++)
            {
                for (int idx = 0; idx < players[p].units.Count; idx++)
                {
                    Unit tank = players[p].units[idx];
                    Size dist = Utils.DistanceBetween(tank, players[p].playerBase);
                    if (dist.Height < 0)
                    {
                        tank.direction = Direction.UP;
                    }
                    else
                    {
                        tank.direction = Direction.DOWN;
                    }
                }
            }
        }

        public int loginCount;
        public bool Running { get { return loginCount >= 2; } }
        public bool GameOver { get { return (IsPlayerLosing(0) || IsPlayerLosing(1)); } }
    }
}
