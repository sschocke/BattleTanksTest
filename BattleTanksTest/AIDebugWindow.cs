using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BattleTanksTest
{
    public partial class AIDebugWindow : Form
    {
        private int myPlayerIdx;
        private ClientGameState state = null;
        private Rectangle gridRect;

        public AIDebugWindow(string playerName, int playerIdx)
        {
            InitializeComponent();

            this.Text = "Debug Window - " + playerName;
            this.myPlayerIdx = playerIdx;
            this.addLogDel = new AddLogDelegate(this.AddLog);
        }

        public void UpdateState(ClientGameState newState)
        {
            this.state = newState;
            canvas.Invalidate();
        }
        private delegate void AddLogDelegate(string text);

        private AddLogDelegate addLogDel;
        public void AddLog(string text)
        {
            if (this.txtLog.InvokeRequired)
            {
                this.txtLog.Invoke(this.addLogDel, new object[] { text });
            }
            else
            {
                this.txtLog.AppendText(text + Environment.NewLine);
            }
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (this.state == null)
            {
                return;
            }

            int gridWidth = (canvas.Width - 10) / this.state.BoardWidth;
            int gridHeight = (canvas.Height - 10) / this.state.BoardHeight;

            gridRect = new Rectangle(5, 5, gridWidth, gridHeight);
            for (int y = 0; y < this.state.BoardHeight; y++)
            {
                gridRect.X = 5;
                for (int x = 0; x < this.state.BoardWidth; x++)
                {
                    if (this.state.blocks[x, y] == State.FULL)
                    {
                        e.Graphics.FillRectangle(Brushes.DarkGray, gridRect);
                    }
                    if ((this.state.players[0].playerBase.x == x) &&
                        (this.state.players[0].playerBase.y == y))
                    {
                        e.Graphics.FillRectangle((0 == myPlayerIdx ? Brushes.Green : Brushes.Red), gridRect);
                    }
                    if ((this.state.players[1].playerBase.x == x) &&
                        (this.state.players[1].playerBase.y == y))
                    {
                        e.Graphics.FillRectangle((1 == myPlayerIdx ? Brushes.Green : Brushes.Red), gridRect);
                    }
                    e.Graphics.DrawRectangle(Pens.Black, gridRect);
                    gridRect.Offset(gridWidth, 0);
                }
                gridRect.Offset(0, gridHeight);
            }
            for (int p = 0; p < this.state.players.Length; p++)
            {
                foreach (Unit unit in this.state.players[p].units)
                {
                    drawUnit(e.Graphics, gridWidth, gridHeight, unit, (p == myPlayerIdx ? Brushes.Green : Brushes.Red));
                }
                foreach (Bullet bullet in this.state.players[p].bullets)
                {
                    drawBullet(e.Graphics, gridWidth, gridHeight, bullet, (p == myPlayerIdx ? Brushes.Green : Brushes.Red));
                }
            }
        }

        public static void drawUnit(Graphics g, int gridWidth, int gridHeight, Unit unit, Brush color)
        {
            int unitX = 5 + (unit.x * gridWidth) + (int)(gridWidth * 0.5);
            int unitY = 5 + (unit.y * gridHeight) + (int)(gridHeight * 0.5);
            Rectangle unitBaseRect = new Rectangle(unitX - (int)(gridWidth * 2.4), unitY - (int)(gridHeight * 2.4), (int)(gridWidth * 4.8), (int)(gridHeight * 4.8));
            Rectangle unitGunRect = new Rectangle(unitX - (int)(gridWidth * 0.2), unitY - (int)(gridHeight * 0.2), (int)(gridWidth * 0.4), (int)(gridHeight * 0.4));
            Rectangle unitHatchRect = new Rectangle(unitX - (int)(gridWidth * 0.4), unitY - (int)(gridHeight * 0.4), (int)(gridWidth * 0.8), (int)(gridHeight * 0.8));
            switch (unit.direction)
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
            int bulletY = 5 + (bullet.y * gridHeight) + (int)(gridHeight * 0.5);
            Rectangle bulletBaseRect = new Rectangle(bulletX - (int)(gridWidth * 0.4), bulletY - (int)(gridHeight * 0.4), (int)(gridWidth * 0.8), (int)(gridHeight * 0.8));
            g.FillEllipse(color, bulletBaseRect);
        }

        private void AIDebugWindow_SizeChanged(object sender, EventArgs e)
        {
            canvas.Invalidate();
        }
    }
}
