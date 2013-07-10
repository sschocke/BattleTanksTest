using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BattleTanksTest
{
    public enum Action { NONE, UP, DOWN, LEFT, RIGHT, FIRE };
    public enum Direction { NONE, UP, DOWN, LEFT, RIGHT };
    public enum State { FULL, EMPTY, OUT_OF_BOUNDS, NONE };

    public struct Bullet
    {
        public Direction direction;
        public int id;
        public int x;
        public int y;
    }

    public struct Base
    {
        public int x;
        public int y;

        public Base(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Unit
    {
        public Action action;
        public Direction direction;
        public int x;
        public int y;

        public Unit(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.action = Action.NONE;
            this.direction = Direction.NONE;
        }
    }

    public class Player
    {
        public string name;
        public Base playerBase;
        public List<Bullet> bullets;
        public List<Unit> units;

        public Player(string playerName)
        {
            this.name = playerName;
            this.bullets = new List<Bullet>();
            this.units = new List<Unit>();
        }
    }

    public class GameState
    {
        public int currentTick;
        public State[,] blocks;
        public Player[] players;

        public GameState(string layout)
        {
            this.currentTick = 0;
            this.players = new Player[2];
            this.players[0]= new Player("Player 1");
            this.players[1]= new Player("Player 2");

            string[] rows = layout.Split('\n');
            int width = rows[0].Trim().Length;
            int height = rows.Length;

            this.blocks = new State[width, height];
            fillBoard(rows);
            setupTanks();
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

        private void fillBoard(string[] rows)
        {
            int y = 0;
            foreach (string row in rows)
            {
                int x = 0;
                foreach (char c in row.Trim())
                {
                    blocks[x, y] = State.EMPTY;
                    switch (c)
                    {
                        case '#':
                            blocks[x, y] = State.FULL;
                            break;
                        case '$':
                            players[0].playerBase = new Base(x, y);
                            break;
                        case '%':
                            players[1].playerBase = new Base(x, y);
                            break;
                        case 'X':
                            players[0].units.Add(new Unit(x, y));
                            break;
                        case 'Y':
                            players[1].units.Add(new Unit(x, y));
                            break;
                    }
                    x++;
                }
                y++;
            }
        }

        public int BoardWidth { get { return blocks.GetLength(0); } }
        public int BoardHeight { get { return blocks.GetLength(1); } }
    }
}
