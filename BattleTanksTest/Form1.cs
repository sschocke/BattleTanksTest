using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BattleTanksTest
{
    public partial class Form1 : Form
    {
        public Font gridFont;
        public GameState gameState;
        public Rectangle gridRect;

        public Form1()
        {
            InitializeComponent();
            gridFont = new Font("Arial", 10);

            string layout = File.ReadAllText("layout1.txt");
            gameState = new GameState(layout);
#if DEBUG
            this.MinimumSize = new Size((gameState.BoardWidth * 16) + 10, (gameState.BoardHeight * 16) + 30);
            this.Size = new Size((gameState.BoardWidth * 32) + 10, (gameState.BoardHeight * 32) + 30);
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
            }

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(gameState.currentTick.ToString(), gridFont, Brushes.Black, 10, 10, sf);
        }

        private static void drawUnit(Graphics g, int gridWidth, int gridHeight, Unit unit, Brush color)
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
            g.DrawRectangle(Pens.Black, unitGunRect);
            g.FillEllipse(color, unitHatchRect);
            g.DrawEllipse(Pens.Black, unitHatchRect);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            gameState.currentTick++;
            canvas.Invalidate();
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
    }
}
