using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace BattleTanksTest
{
    public enum Action { NONE, UP, DOWN, LEFT, RIGHT, FIRE };
    public enum Direction { NONE, UP, DOWN, LEFT, RIGHT };
    public enum State { FULL, EMPTY, OUT_OF_BOUNDS, NONE };

    public class Bullet
    {
        private static int nextBulletId;

        public Direction direction;
        public int id;
        public int x;
        public int y;

        public Bullet(int x, int y, Direction dir)
        {
            this.x = x;
            this.y = y;
            this.direction = dir;
            this.id = nextBulletId++;
        }

        public Bullet(Bullet source)
        {
            this.x = source.x;
            this.y = source.y;
            this.direction = source.direction;
            this.id = source.id;
        }
    }

    public class Base
    {
        public int x;
        public int y;

        public Base(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class BlockEvent
    {
        public Point point;
        public State newState;

        public BlockEvent(int x, int y, State state)
        {
            this.point = new Point(x, y);
            this.newState = state;
        }
    }

    public class UnitEvent
    {
        public int tick;
        public Bullet bullet;
        public Unit unit;
    }

    public class Events
    {
        public Queue<BlockEvent> blockEvents;
        public Queue<UnitEvent> unitEvents;

        public Events()
        {
            this.blockEvents = new Queue<BlockEvent>();
            this.unitEvents = new Queue<UnitEvent>();
        }

        public Events(Events source)
            :this()
        {
            foreach (BlockEvent ev in source.blockEvents)
            {
                this.blockEvents.Enqueue(ev);
            }
            foreach (UnitEvent ev in source.unitEvents)
            {
                this.unitEvents.Enqueue(ev);
            }
        }

        public void Clear()
        {
            this.blockEvents.Clear();
            this.unitEvents.Clear();
        }
    }

    public class Unit
    {
        private static int nextUnitId;

        public Action action;
        public Direction direction;
        public int id;
        public int x;
        public int y;

        public Unit(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.action = Action.NONE;
            this.direction = Direction.NONE;
            this.id = nextUnitId++;
        }

        public Unit(Unit source)
        {
            this.x = source.x;
            this.y = source.y;
            this.action = source.action;
            this.direction = source.direction;
            this.id = source.id;
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

        public Player(Player source)
            : this(source.name)
        {
            foreach (Bullet bullet in source.bullets)
            {
                this.bullets.Add(new Bullet(bullet));
            }
            foreach (Unit unit in source.units)
            {
                this.units.Add(new Unit(unit));
            }

            this.playerBase = new Base(source.playerBase.x, source.playerBase.y);
        }

        public Unit GetUnit(int unitID)
        {
            foreach (Unit unit in this.units)
            {
                if (unit.id == unitID)
                {
                    return unit;
                }
            }

            return null;
        }
    }

    public abstract class GameStateBase
    {
        public int currentTick;
        public Player[] players;

        protected GameStateBase()
        {
            this.currentTick = 0;
            this.players = new Player[2];
        }

        public GameStateBase(GameStateBase source)
        {
            this.currentTick = source.currentTick;
            this.players = new Player[2];
            this.players[0] = new Player(source.players[0]);
            this.players[1] = new Player(source.players[1]);
        }
    }

    public class GameState : GameStateBase
    {
        public Events events;

        protected GameState()
            :base()
        {
            this.events = new Events();
        }

        public GameState(GameState source)
            :base(source)
        {
            this.events = new Events(source.events);
        }
        public GameState(ServerGameState source, int playerIdx)
            :base(source)
        {
            this.events = new Events(source.playerEvents[playerIdx]);
            source.playerEvents[playerIdx].Clear();
        }
    }
}
