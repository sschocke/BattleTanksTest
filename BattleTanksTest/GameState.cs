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

    public class GameStateWithLayout : GameStateBase
    {
        public delegate void DestroyedWall(int x, int y);

        public State[,] blocks;

        protected GameStateWithLayout()
            : base()
        { }

        protected GameStateWithLayout(GameStateBase state)
            : base(state)
        { }

        protected GameStateWithLayout(GameStateWithLayout source)
            : base(source)
        {
            this.blocks = new State[source.BoardWidth, source.BoardHeight];
            for (int x = 0; x < source.BoardWidth; x++)
            {
                for (int y = 0; y < source.BoardHeight; y++)
                {
                    this.blocks[x, y] = source.blocks[x, y];
                }
            }
        }

        protected GameStateWithLayout(string layout)
            : base()
        { }

        protected void fillBoard(string layout)
        {
            string[] rows = layout.Split('\n');
            int width = rows[0].Trim().Length;
            int height = rows.Length;

            this.blocks = new State[width, height];

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

        public event DestroyedWall onWallDestroyed;

        public List<Action[]> GetLegalActions(int playerIdx)
        {
            List<Action[]> result = new List<Action[]>();

            Player player = this.players[playerIdx];
            if (player.units.Count < 1)
            {
                return result;
            }

            bool test;
            Action[] actionSet;
            foreach (Action act1 in Enum.GetValues(typeof(Action)))
            {
                actionSet = new Action[] { act1, Action.NONE };
                test = this.isPseudoLegal(playerIdx, actionSet);
                if (test == false)
                {
                    continue;
                }
                result.Add(actionSet);

                if (player.units.Count > 1)
                {
                    foreach (Action act2 in Enum.GetValues(typeof(Action)))
                    {
                        if (act2 == Action.NONE)
                        {
                            continue;
                        }

                        actionSet = new Action[] { act1, act2 };
                        test = this.isPseudoLegal(playerIdx, actionSet);
                        if (test == false)
                        {
                            continue;
                        }

                        result.Add(actionSet);
                    }
                }
            }

            return result;
        }

        public bool isPseudoLegal(int playerIdx, Action[] actions)
        {
            int newX, newY;

            for (int unitIdx = 0; unitIdx < this.players[playerIdx].units.Count; unitIdx++)
            {
                Unit tank = this.players[playerIdx].units[unitIdx];

                switch (actions[unitIdx])
                {
                    case Action.NONE:
                    case Action.FIRE:
                        break;
                    case Action.LEFT:
                        newX = tank.x - 1;
                        newY = tank.y;

                        if (newX < 2) return false;
                        for (int uy = newY - 2; uy <= newY + 2; uy++)
                        {
                            if ((this.blocks[newX - 2, uy] != State.EMPTY) && (tank.direction == Direction.LEFT)) return false;
                        }
                        break;
                    case Action.RIGHT:
                        newX = tank.x + 1;
                        newY = tank.y;

                        if (newX >= this.BoardWidth - 2) return false;
                        for (int uy = newY - 2; uy <= newY + 2; uy++)
                        {
                            if ((this.blocks[newX + 2, uy] != State.EMPTY) && (tank.direction == Direction.RIGHT)) return false;
                        }
                        break;
                    case Action.UP:
                        newX = tank.x;
                        newY = tank.y - 1;

                        if (newY < 2) return false;
                        for (int ux = newX - 2; ux <= newX + 2; ux++)
                        {
                            if ((this.blocks[ux, newY - 2] != State.EMPTY) && (tank.direction == Direction.UP)) return false;
                        }
                        break;
                    case Action.DOWN:
                        newX = tank.x;
                        newY = tank.y + 1;

                        if (newY >= this.BoardHeight - 2) return false;
                        for (int ux = newX - 2; ux <= newX + 2; ux++)
                        {
                            if ((this.blocks[ux, newY + 2] != State.EMPTY) && (tank.direction == Direction.DOWN)) return false;
                        }
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        private void moveBullets()
        {
            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                List<Bullet> bullets = this.players[playerIdx].bullets;
                foreach (Bullet bullet in bullets)
                {
                    switch (bullet.direction)
                    {
                        case Direction.UP:
                            bullet.y--;
                            break;
                        case Direction.DOWN:
                            bullet.y++;
                            break;
                        case Direction.LEFT:
                            bullet.x--;
                            break;
                        case Direction.RIGHT:
                            bullet.x++;
                            break;
                    }
                }

                checkCollisions();
            }
        }

        private void applyPlayerActions(int playerIdx, Action[] actions)
        {
            int newX, newY;
            bool blocked = false;

            //bool test = isPseudoLegal(playerIdx, actions);
            for (int unitIdx = 0; unitIdx < this.players[playerIdx].units.Count; unitIdx++)
            {
                Unit tank = this.players[playerIdx].units[unitIdx];
                blocked = false;

                switch (actions[unitIdx])
                {
                    case Action.NONE:
                        break;
                    case Action.LEFT:
                        newX = tank.x - 1;
                        newY = tank.y;

                        if (newX < 2) throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move LEFT(Left Border Reached)");
                        for (int uy = newY - 2; uy <= newY + 2; uy++)
                        {
                            if (this.blocks[newX - 2, uy] != State.EMPTY)
                            {
                                if (tank.direction == Direction.LEFT)
                                {
                                    throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move LEFT(Wall Hit and Already facing Left)");
                                }
                                blocked = true;
                            }
                        }
                        if (!blocked) this.players[playerIdx].units[unitIdx].x--;
                        this.players[playerIdx].units[unitIdx].direction = Direction.LEFT;
                        break;
                    case Action.RIGHT:
                        newX = tank.x + 1;
                        newY = tank.y;

                        if (newX >= this.BoardWidth - 2) throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move RIGHT(Right Border Reached)");
                        for (int uy = newY - 2; uy <= newY + 2; uy++)
                        {
                            if (this.blocks[newX + 2, uy] != State.EMPTY)
                            {
                                if (tank.direction == Direction.RIGHT)
                                {
                                    throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move RIGHT(Wall Hit and Already facing Right)");
                                }
                                blocked = true;
                            }
                        }
                        if (!blocked) this.players[playerIdx].units[unitIdx].x++;
                        this.players[playerIdx].units[unitIdx].direction = Direction.RIGHT;
                        break;
                    case Action.UP:
                        newX = tank.x;
                        newY = tank.y - 1;

                        if (newY < 2) throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move UP(Top Border Reached)");
                        for (int ux = newX - 2; ux <= newX + 2; ux++)
                        {
                            if (this.blocks[ux, newY - 2] != State.EMPTY)
                            {
                                if (tank.direction == Direction.UP)
                                {
                                    throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move UP(Wall Hit and Already facing Up)");
                                }
                                blocked = true;
                            }
                        }
                        if (!blocked) this.players[playerIdx].units[unitIdx].y--;
                        this.players[playerIdx].units[unitIdx].direction = Direction.UP;
                        break;
                    case Action.DOWN:
                        newX = tank.x;
                        newY = tank.y + 1;

                        if (newY >= this.BoardHeight - 2) throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move DOWN(Bottom Border Reached)");
                        for (int ux = newX - 2; ux <= newX + 2; ux++)
                        {
                            if (this.blocks[ux, newY + 2] != State.EMPTY)
                            {
                                if (tank.direction == Direction.DOWN)
                                {
                                    throw new InvalidOperationException("Player " + playerIdx + " Tank " + unitIdx + " : Illegal Move DOWN(Wall Hit and Already facing Down)");
                                }
                                blocked = true;
                            }
                        }
                        if (!blocked) this.players[playerIdx].units[unitIdx].y++;
                        this.players[playerIdx].units[unitIdx].direction = Direction.DOWN;
                        break;
                    case Action.FIRE:
                        Unit sourceTank = this.players[playerIdx].units[unitIdx];
                        switch (sourceTank.direction)
                        {
                            case Direction.UP:
                                this.players[playerIdx].bullets.Add(new Bullet(sourceTank.x, sourceTank.y - 3, Direction.UP));
                                break;
                            case Direction.DOWN:
                                this.players[playerIdx].bullets.Add(new Bullet(sourceTank.x, sourceTank.y + 3, Direction.DOWN));
                                break;
                            case Direction.LEFT:
                                this.players[playerIdx].bullets.Add(new Bullet(sourceTank.x - 3, sourceTank.y, Direction.LEFT));
                                break;
                            case Direction.RIGHT:
                                this.players[playerIdx].bullets.Add(new Bullet(sourceTank.x + 3, sourceTank.y, Direction.RIGHT));
                                break;
                        }
                        break;
                }
            }
        }

        private void checkCollisions()
        {
            List<Bullet> allBullets = new List<Bullet>();
            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                List<Bullet> bullets = this.players[playerIdx].bullets;
                allBullets.AddRange(bullets);
            }

            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                Bullet[] bullets = this.players[playerIdx].bullets.ToArray();
                foreach (Bullet bullet in bullets)
                {
                    if (bullet.x < 0 || bullet.x >= this.BoardWidth || bullet.y < 0 || bullet.y >= this.BoardHeight)
                    {
                        destroyBullets(bullet.x, bullet.y);
                        allBullets.Remove(bullet);
                        continue;
                    }
                    if (this.blocks[bullet.x, bullet.y] == State.FULL)
                    {
                        destroyWall(bullet);
                        destroyBullets(bullet.x, bullet.y);
                        allBullets.Remove(bullet);
                    }
                    Unit unitHit = anyUnitAt(bullet.x, bullet.y);
                    if (unitHit != null)
                    {
                        destroyUnit(unitHit);
                        destroyBullets(bullet.x, bullet.y);
                        allBullets.Remove(bullet);
                    }
                    int baseHit = anyBaseAt(bullet.x, bullet.y);
                    if (baseHit >= 0)
                    {
                        this.players[baseHit].playerBase = new Base(-1, -1);
                    }
                    var bulletQry = from b in allBullets
                                    where b.x == bullet.x && b.y == bullet.y && b.id != bullet.id
                                    select b;
                    if (bulletQry.Count() > 0)
                    {
                        destroyBullets(bullet.x, bullet.y);
                    }
                }
            }
        }

        private int anyBaseAt(int x, int y)
        {
            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                if (this.players[playerIdx].playerBase.x == x && this.players[playerIdx].playerBase.y == y)
                {
                    return playerIdx;
                }
            }

            return -1;
        }

        private void destroyUnit(Unit unit)
        {
            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                this.players[playerIdx].units.Remove(unit);
            }
        }

        private Unit anyUnitAt(int x, int y)
        {
            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                foreach (Unit unit in this.players[playerIdx].units)
                {
                    if ((x >= unit.x - 2) && (x <= unit.x + 2) &&
                        (y >= unit.y - 2) && (y <= unit.y + 2))
                    {
                        return unit;
                    }
                }
            }

            return null;
        }

        private void destroyWall(Bullet bullet)
        {
            Point[] points = new Point[5];
            points[0] = new Point(bullet.x, bullet.y);
            switch (bullet.direction)
            {
                case Direction.UP:
                case Direction.DOWN:
                    points[1] = new Point(bullet.x - 2, bullet.y);
                    points[2] = new Point(bullet.x - 1, bullet.y);
                    points[3] = new Point(bullet.x + 1, bullet.y);
                    points[4] = new Point(bullet.x + 2, bullet.y);
                    break;
                case Direction.LEFT:
                case Direction.RIGHT:
                    points[1] = new Point(bullet.x, bullet.y - 2);
                    points[2] = new Point(bullet.x, bullet.y - 1);
                    points[3] = new Point(bullet.x, bullet.y + 1);
                    points[4] = new Point(bullet.x, bullet.y + 2);
                    break;
            }

            foreach (Point pt in points)
            {
                if (this.blocks[pt.X, pt.Y] == State.FULL)
                {
                    this.blocks[pt.X, pt.Y] = State.EMPTY;
                    if (this.onWallDestroyed != null)
                    {
                        this.onWallDestroyed(pt.X, pt.Y);
                    }
                }
            }
        }

        private void destroyBullets(int x, int y)
        {
            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                Bullet[] bullets = this.players[playerIdx].bullets.ToArray();
                foreach (Bullet bullet in bullets)
                {
                    if (bullet.x == x && bullet.y == y)
                    {
                        this.players[playerIdx].bullets.Remove(bullet);
                    }
                }
            }
        }

        public void UpdateGameState()
        {
            this.moveBullets();

            this.moveBullets();

            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                Action[] actions = new Action[this.players[playerIdx].units.Count];
                for (int unitIdx = 0; unitIdx < this.players[playerIdx].units.Count; unitIdx++)
                {
                    actions[unitIdx] = this.players[playerIdx].units[unitIdx].action;
                }

                this.applyPlayerActions(playerIdx, actions);
            }

            this.checkCollisions();
        }

        public virtual void ProcessEvents(Events events) { }
    }

    public class ClientGameState : GameStateWithLayout
    {
        public int myPlayerIdx;

        public ClientGameState(State[,] layout, GameStateBase state, int myIdx)
            :base(state)
        {
            this.blocks = layout;
            this.myPlayerIdx = myIdx;
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
    }
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
    }
}
