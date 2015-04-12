using System;
using System.Linq;
using System.Windows;

namespace ChatWheel
{
    internal static class Utils
    {
        public static Point Multiply(this Point point, double val)
        {
            return new Point(point.X*val, point.Y*val);
        }

        public static double Distance2D(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static Point[] GetEquidistantPointsOnCircle(int pointsAmount, double circleRadius,
            double initialOffset = 0)
        {
            var equidistantPoints = new Point[pointsAmount];
            double x, y, current = initialOffset;
            var radialStep = ((Math.PI*2)/pointsAmount);
            for (var i = 0; i < pointsAmount; i++)
            {
                x = Math.Sin(current)*circleRadius;
                y = Math.Cos(current)*circleRadius*-1; //inverting the Y so that 0 is at top
                equidistantPoints[i] = new Point((int) x, (int) y);
                current += radialStep;
            }
            return equidistantPoints;
        }

        //TODO: not mine, refactor
        public static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            var angleRad = (Math.PI/180.0)*(angle - 90);

            var x = radius*Math.Cos(angleRad);
            var y = radius*Math.Sin(angleRad);

            return new Point(x, y);
        }

        public static double FindAngleBetweenPoints(Point p1, Point center, Point p2)
        {
            return (Math.Atan2(p1.X - center.X, p1.Y - center.Y) -
                    Math.Atan2(p2.X - center.X, p2.Y - center.Y))*(180/Math.PI);
        }

        public static bool IsInPolygon(Point[] poly, Point point)
        {
            var coef = poly.Skip(1).Select((p, i) =>
                (point.Y - poly[i].Y)*(p.X - poly[i].X)
                - (point.X - poly[i].X)*(p.Y - poly[i].Y))
                .ToList();

            if (coef.Any(p => p == 0))
                return true;

            for (var i = 1; i < coef.Count(); i++)
            {
                if (coef[i]*coef[i - 1] < 0)
                    return false;
            }
            return true;
        }

        public static bool IsAngleBetween(double tested, double angleStart, double angleEnd)
        {
            var tempAngle = ((angleEnd - angleStart)%360 + 360)%360;
            if (tempAngle >= 180)
            {
                tempAngle = angleStart;
                angleStart = angleEnd;
                angleEnd = tempAngle;
            }

            if (angleStart <= angleEnd)
            {
                return tested >= angleStart && tested <= angleEnd;
            }
            return tested >= angleStart || tested <= angleEnd;
        }
    }
}