using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BattleTanksTest
{
    public static class Utils
    {
        public static Size DistanceBetween(int x1, int y1, int x2, int y2)
        {
            return new Size(x1 - x2, y1 - y2);
        }
        public static Size DistanceBetween(Unit tank, Base playerBase)
        {
            return DistanceBetween(tank.x, tank.y, playerBase.x, playerBase.y);
        }
        public static Size DistanceBetween(Unit tank1, Unit tank2)
        {
            return DistanceBetween(tank1.x, tank1.y, tank2.x, tank2.y);
        }
        public static Size DistanceBetween(Unit tank, int x, int y)
        {
            return DistanceBetween(tank.x, tank.y, x, y);
        }
        public static Size DistanceBetween(Unit tank, Point pt)
        {
            return DistanceBetween(tank.x, tank.y, pt.X, pt.Y);
        }
        public static Size DistanceBetween(Base playerBase, int x, int y)
        {
            return DistanceBetween(playerBase.x, playerBase.y, x, y);
        }

        public static double ManhattanDistance(Size dist)
        {
            double mDist = Math.Sqrt((double)(dist.Width * dist.Width) + (double)(dist.Height * dist.Height));

            return mDist;
        }
    }
}
