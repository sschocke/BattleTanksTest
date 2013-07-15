using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace BattleTanksTest
{
    public partial class Form1 : Form
    {
        public Font gridFont;
        public ServerGameState gameState;
        public Rectangle gridRect;

        public AIDebugWindow p1Debug;
        public AIDebugWindow p2Debug;

        public Form1()
        {
            InitializeComponent();
            gridFont = new Font("Arial", 10);

            string layout = File.ReadAllText("layout1.txt");
            gameState = new ServerGameState(layout);
#if DEBUG
            this.MinimumSize = new Size((gameState.BoardWidth * 16) + 10, (gameState.BoardHeight * 16) + 30);
            this.Size = new Size((gameState.BoardWidth * 32) + 10, (gameState.BoardHeight * 32) + 30);
            this.gameTimer.Interval = 2000;
#else
            this.MinimumSize = new Size((gameState.BoardWidth * 8) + 10, (gameState.BoardHeight * 8) + 30);
            this.Size = new Size((gameState.BoardWidth * 8) + 10, (gameState.BoardHeight * 8) + 30);
#endif
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            int gridWidth = (canvas.Width - 10) / gameState.BoardWidth;
            int gridHeight = (canvas.Height - 25) / gameState.BoardHeight;

            gridRect = new Rectangle(5, 20, gridWidth, gridHeight);
            for (int y = 0; y < gameState.BoardHeight; y++)
            {
                gridRect.X = 5;
                for (int x = 0; x < gameState.BoardWidth; x++)
                {
                    if (gameState.blocks[x, y] == State.FULL)
                    {
                        e.Graphics.FillRectangle(Brushes.DarkGray, gridRect);
                    }
                    if ((gameState.players[0].playerBase.x == x) &&
                        (gameState.players[0].playerBase.y == y))
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, gridRect);
                    }
                    if ((gameState.players[1].playerBase.x == x) &&
                        (gameState.players[1].playerBase.y == y))
                    {
                        e.Graphics.FillRectangle(Brushes.Red, gridRect);
                    }
                    e.Graphics.DrawRectangle(Pens.Black, gridRect);
                    gridRect.Offset(gridWidth, 0);
                }
                gridRect.Offset(0, gridHeight);
            }

            for (int p = 0; p < gameState.players.Length; p++)
            {
                foreach (Unit unit in gameState.players[p].units)
                {
                    drawUnit(e.Graphics, gridWidth, gridHeight, unit, (p == 0 ? Brushes.DeepSkyBlue : Brushes.OrangeRed));
                }
                foreach (Bullet bullet in gameState.players[p].bullets)
                {
                    drawBullet(e.Graphics, gridWidth, gridHeight, bullet, (p == 0 ? Brushes.Blue : Brushes.Red));
                }
            }

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(gameState.currentTick.ToString(), gridFont, Brushes.Black, 10, 10, sf);
        }
        public static void drawUnit(Graphics g, int gridWidth, int gridHeight, Unit unit, Brush color)
        {
            int unitX = 5 + (unit.x * gridWidth) + (int)(gridWidth * 0.5);
            int unitY = 20 + (unit.y * gridHeight) + (int)(gridHeight * 0.5);
            Rectangle unitBaseRect = new Rectangle(unitX - (int)(gridWidth * 2.4), unitY - (int)(gridHeight * 2.4), (int)(gridWidth * 4.8), (int)(gridHeight * 4.8));
            Rectangle unitGunRect = new Rectangle(unitX - (int)(gridWidth * 0.2), unitY - (int)(gridHeight * 0.2), (int)(gridWidth * 0.4), (int)(gridHeight * 0.4));
            Rectangle unitHatchRect = new Rectangle(unitX - (int)(gridWidth * 0.4), unitY - (int)(gridHeight * 0.4), (int)(gridWidth * 0.8), (int)(gridHeight * 0.8));
            switch(unit.direction)
            {
                case Direction.UP:
                    unitGunRect.Y -= (int)(gridHeight * 1.8);
                    unitGunRect.Height = (int)(gridHeight * 2);
                    break;
                case Direction.DOWN:
                    unitGunRect.Height = (int)(gridHeight * 2);
                    break;
                case Direction.LEFT:
                    unitGunRect.X -= (int)(gridWidth * 1.8);
                    unitGunRect.Width = (int)(gridWidth * 2);
                    break;
                case Direction.RIGHT:
                    unitGunRect.Width = (int)(gridWidth * 2);
                    break;
            }
            g.FillRectangle(color, unitBaseRect);
            g.FillRectangle(Brushes.Black, unitGunRect);
            g.FillEllipse(color, unitHatchRect);
            g.DrawEllipse(Pens.Black, unitHatchRect);
        }
        public static void drawBullet(Graphics g, int gridWidth, int gridHeight, Bullet bullet, Brush color)
        {
            int bulletX = 5 + (bullet.x * gridWidth) + (int)(gridWidth * 0.5);
            int bulletY = 20 + (bullet.y * gridHeight) + (int)(gridHeight * 0.5);
            Rectangle bulletBaseRect = new Rectangle(bulletX - (int)(gridWidth * 0.4), bulletY - (int)(gridHeight * 0.4), (int)(gridWidth * 0.8), (int)(gridHeight * 0.8));
            g.FillEllipse(color, bulletBaseRect);
            g.DrawEllipse(Pens.Orange, bulletBaseRect);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (gameState.Running == false)
            {
                canvas.Invalidate();
                return;
            }

            List<Action[]> legalActions = gameState.GetLegalActions(0);
            gameState.currentTick++;
            gameState.UpdateGameState();
            canvas.Invalidate();
        }

        private void canvas_DoubleClick(object sender, EventArgs e)
        {
#if DEBUG
            p1Debug = new AIDebugWindow("Player 1", 0);
            p2Debug = new AIDebugWindow("Player 2", 1);
            p1Debug.Show(this);
            p2Debug.Show(this);
            p1Debug.Height = this.Height / 2;
            p2Debug.Height = this.Height / 2;
            p1Debug.Width = (this.Width / 2) + 300;
            p2Debug.Width = (this.Width / 2) + 300;
            p1Debug.Top = this.Top;
            p2Debug.Top = this.Top;
            p1Debug.Left = this.Left - p1Debug.Width;
            p2Debug.Left = this.Right;
#endif

            player1Worker.RunWorkerAsync();
            Thread.Sleep(100);
            player2Worker.RunWorkerAsync();

            this.Activate();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            gameTimer.Start();
        }
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            gameTimer.Stop();
        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            canvas.Invalidate();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (player1Worker.IsBusy)
            {
                player1Worker.CancelAsync();
            }
            if (player2Worker.IsBusy)
            {
                player2Worker.CancelAsync();
            }
        }

        // Game API
        public State[,] Login(string name)
        {
            State[,] returnVal = new State[gameState.BoardWidth, gameState.BoardHeight];
            for (int x = 0; x < gameState.BoardWidth; x++)
            {
                for (int y = 0; y < gameState.BoardHeight; y++)
                {
                    returnVal[x, y] = gameState.blocks[x, y];
                }
            }

            gameState.loginCount++;
            return returnVal;
        }
        public GameState GetStatus(int playerIdx)
        {
            GameState retval = new GameState(Program.MainForm.gameState, playerIdx);
            return retval;
        }
        public void SetAction(int playerIdx, int unitID, Action action)
        {
            Player player = Program.MainForm.gameState.players[playerIdx];
            Unit unit = player.GetUnit(unitID);
            if (unit != null)
            {
                unit.action = action;
            }
        }

        private void player1Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Random rnd = new Random(0);
            State[,] layout = Program.MainForm.Login("Player 1");
            GameState initialState = Program.MainForm.GetStatus(0);

            ClientGameState myState = new ClientGameState(layout, initialState, 0);
            while (player1Worker.CancellationPending == false)
            {
#if DEBUG
                p1Debug.UpdateState(myState);
#endif
                GameState curState = Program.MainForm.GetStatus(0);
                if (curState.events.blockEvents.Count > 0)
                {
                    p1Debug.AddLog("Player 1 received " + curState.events.blockEvents.Count + " unseen block events");
                }
                if (curState.currentTick > myState.currentTick)
                {
                    myState = new ClientGameState(layout, curState, 0);
                    myState.ProcessEvents(curState.events);
                }

                BruteForceAgent(myState);
                Thread.Sleep(100);
            }
        }
        private void player2Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Random rnd = new Random(0);
            State[,] layout = Program.MainForm.Login("Player 2");
            GameState initialState = Program.MainForm.GetStatus(1);

            ClientGameState myState = new ClientGameState(layout, initialState, 1);

            while (player2Worker.CancellationPending == false)
            {
#if DEBUG
                p2Debug.UpdateState(myState);
#endif
                GameState curState = Program.MainForm.GetStatus(1);
                if (curState.events.blockEvents.Count > 0)
                {
                    p2Debug.AddLog("Player 2 received " + curState.events.blockEvents.Count + " unseen block events");
                }
                if (curState.currentTick > myState.currentTick)
                {
                    myState = new ClientGameState(layout, curState, 1);
                    myState.ProcessEvents(curState.events);
                }

                RandomAgent(rnd, myState);
                Thread.Sleep(100);
            }
        }

        private static void RandomAgent(Random rnd, ClientGameState myState)
        {
            List<Action[]> legalActions = myState.GetLegalActions(myState.myPlayerIdx);
            if (legalActions.Count < 1)
            {
                return;
            }

            int pickActions = rnd.Next(legalActions.Count);
            Action[] picked = legalActions[pickActions];
            for (int idx = 0; idx < myState.players[myState.myPlayerIdx].units.Count; idx++)
            {
                Program.MainForm.SetAction(myState.myPlayerIdx, myState.players[myState.myPlayerIdx].units[idx].id, picked[idx]);
            }
        }

        private static void BruteForceAgent(ClientGameState myState)
        {
            int closestTankIdx = -1;
            Size closestTankDist = new Size(myState.BoardWidth,myState.BoardHeight);

            List<Action[]> legalActions = myState.GetLegalActions(myState.myPlayerIdx);
            if (legalActions.Count < 1)
            {
                return;
            }

            Player me = myState.Me;
            Player enemy = myState.Opponent;

            for (int idx = 0; idx < me.units.Count; idx++)
            {
                Unit tank = me.units[idx];
                Size distToEnemyBase = Utils.DistanceBetween(tank, enemy.playerBase);
                if (distToEnemyBase.Width + distToEnemyBase.Height < closestTankDist.Width + closestTankDist.Height)
                {
                    closestTankDist = distToEnemyBase;
                    closestTankIdx = idx;
                }
            }

            Unit closestTank = me.units[closestTankIdx];
            Action desiredAction = Action.NONE;
            if (closestTankDist.Height == 0)
            {
                //Vertically aligned with enemy base, so check if we are facing the right direction
                if (closestTankDist.Width < 0 && closestTank.direction != Direction.RIGHT)
                {
                    desiredAction = Action.RIGHT;
                }
                else if (closestTankDist.Width > 0 && closestTank.direction != Direction.LEFT)
                {
                    desiredAction = Action.LEFT;
                }
                else
                {
                    // We are aligned and facing the right direction, so start shooting
                    desiredAction = Action.FIRE;
                }
            }
            else if (closestTankDist.Width == 0)
            {
                //Horizontally aligned with enemy base, so check if we are facing the right direction
                if (closestTankDist.Height < 0 && closestTank.direction != Direction.DOWN)
                {
                    desiredAction = Action.DOWN;
                }
                else if (closestTankDist.Height > 0 && closestTank.direction != Direction.UP)
                {
                    desiredAction = Action.UP;
                }
                else
                {
                    // We are aligned and facing the right direction, so start shooting
                    desiredAction = Action.FIRE;
                }
            }
            else
            {
                // We still need to get in position... let's see if we can
                if (Math.Abs(closestTankDist.Width) <= Math.Abs(closestTankDist.Height))
                {
                    if (closestTankDist.Width < 0)
                    {
                        desiredAction = Action.RIGHT;
                    }
                    else {
                        desiredAction = Action.LEFT;
                    }
                }
                else
                {
                    if (closestTankDist.Height < 0)
                    {
                        desiredAction = Action.DOWN;
                    }
                    else
                    {
                        desiredAction = Action.UP;
                    }
                }
            }

            foreach (Action[] actionSet in legalActions)
            {
                if (actionSet[closestTankIdx] == desiredAction)
                {
                    Program.MainForm.SetAction(myState.myPlayerIdx, closestTank.id, actionSet[closestTankIdx]);
                }
            }
        }
    }
}
