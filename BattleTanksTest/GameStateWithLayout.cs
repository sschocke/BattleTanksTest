using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BattleTanksTest
{
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

        public List<ActionSet> GetLegalActions(int playerIdx)
        {
            List<ActionSet> result = new List<ActionSet>();

            Player player = this.players[playerIdx];
            if (player.units.Count < 1)
            {
                return result;
            }

            bool test;
            ActionSet actionSet;
            foreach (Action act1 in Enum.GetValues(typeof(Action)))
            {
                actionSet = new ActionSet(act1, Action.NONE);
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

                        actionSet = new ActionSet(act1, act2);
                        test = this.isPseudoLegal(playerIdx, actionSet);
                        if (test == false)
                        {
                            continue;
                        }

                        result.Add(actionSet);
                    }
                }
            }

            //result.Reverse();
            return result;
        }

        public bool isPseudoLegal(int playerIdx, ActionSet actions)
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

        private void moveBullets(int playerIdx)
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
        }

        private void applyPlayerMovement(int playerIdx, ActionSet actions)
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
                    case Action.FIRE:
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
                        if (anyUnitAt(newX, newY, tank) != null) blocked = true;

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
                        if (anyUnitAt(newX, newY, tank) != null) blocked = true;

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
                        if (anyUnitAt(newX, newY, tank) != null) blocked = true;

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
                        if (anyUnitAt(newX, newY, tank) != null) blocked = true;

                        if (!blocked) this.players[playerIdx].units[unitIdx].y++;
                        this.players[playerIdx].units[unitIdx].direction = Direction.DOWN;
                        break;
                }
            }
        }
        private void applyPlayerFire(int playerIdx, ActionSet actions)
        {
            for (int unitIdx = 0; unitIdx < this.players[playerIdx].units.Count; unitIdx++)
            {
                Unit sourceTank = this.players[playerIdx].units[unitIdx];

                switch (actions[unitIdx])
                {
                    case Action.FIRE:
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

        private void checkCollisions(bool bulletsOnly)
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
                    var bulletQry = from b in allBullets
                                    where b.x == bullet.x && b.y == bullet.y && b.id != bullet.id
                                    select b;
                    if (bulletQry.Count() > 0)
                    {
                        destroyBullets(bullet.x, bullet.y);
                    }

                    if (bulletsOnly == true) continue;

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
                    Unit baseHitByTank = anyUnitAt(this.players[playerIdx].playerBase.x, this.players[playerIdx].playerBase.y);
                    if (baseHitByTank != null)
                    {
                        this.players[playerIdx].playerBase = new Base(-1, -1);
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
            return anyUnitAt(x, y, null);
        }
        private Unit anyUnitAt(int x, int y, Unit excl)
        {
            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                foreach (Unit unit in this.players[playerIdx].units)
                {
                    if ((x >= unit.x - 2) && (x <= unit.x + 2) &&
                        (y >= unit.y - 2) && (y <= unit.y + 2) &&
                        ((excl == null) || (excl.id != unit.id)))
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
            this.moveBullets(0);
            this.checkCollisions(false);
            this.moveBullets(1);
            this.checkCollisions(false);

            this.moveBullets(0);
            this.checkCollisions(true);
            this.moveBullets(1);
            this.checkCollisions(true);

            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                ActionSet actions = new ActionSet(this.players[playerIdx].units.Count);
                for (int unitIdx = 0; unitIdx < this.players[playerIdx].units.Count; unitIdx++)
                {
                    actions[unitIdx] = this.players[playerIdx].units[unitIdx].action;
                }

                this.applyPlayerMovement(playerIdx, actions);
            }

            this.checkCollisions(false);

            for (int playerIdx = 0; playerIdx < 2; playerIdx++)
            {
                ActionSet actions = new ActionSet(this.players[playerIdx].units.Count);
                for (int unitIdx = 0; unitIdx < this.players[playerIdx].units.Count; unitIdx++)
                {
                    actions[unitIdx] = this.players[playerIdx].units[unitIdx].action;
                }

                this.applyPlayerFire(playerIdx, actions);
            }

            this.checkCollisions(false);
        }
        public bool IsPlayerWinning(int playerIdx)
        {
            int opponentIdx = (playerIdx + 1) % 2;
            bool isOpponentLosing = this.IsPlayerLosing(opponentIdx);
            if (isOpponentLosing == false) return false;

            bool amILosing = this.IsPlayerLosing(playerIdx);
            if (amILosing && isOpponentLosing) return false;

            return true;
        }
        public bool IsPlayerLosing(int playerIdx)
        {
            return ((this.players[playerIdx].playerBase.x == -1) && (this.players[playerIdx].playerBase.y == -1));
        }

        public virtual void ProcessEvents(Events events) { }
    }
}
