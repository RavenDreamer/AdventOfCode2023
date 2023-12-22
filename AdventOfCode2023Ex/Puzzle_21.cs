using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_21
{
	internal struct Point
	{
		private readonly long x, y;

		public long X { get { return x; } }
		public long Y { get { return y; } }

		public Point(long x, long y)
		{
			this.x = x;
			this.y = y;
		}

		public override int GetHashCode()
		{
			return (int)(31 * x + 17 * y); // Or something like that
		}

		public override bool Equals(object obj)
		{
			return obj is Point point && Equals(point);
		}

		public bool Equals(Point p)
		{
			return x == p.x && y == p.y;
		}
	}

	internal struct MetaPoint
	{
		private readonly long x, y, metaX, metaY;

		public long X { get { return x; } }
		public long Y { get { return y; } }

		public long MetaX { get { return metaX; } }
		public long MetaY { get { return metaY; } }


		public MetaPoint(long x, long y, long metaX, long metaY)
		{
			this.x = x;
			this.y = y;
			this.metaX = metaX;
			this.metaY = metaY;
		}

		public Point GetMetaPoint()
		{
			return new Point(metaX, metaY);
		}

		public Point GetLocalPoint()
		{
			return new Point(x, y);
		}

		public override int GetHashCode()
		{
			return (int)(31 * x + 17 * y + 11 * metaX + 5 * metaY); // Or something like that
		}

		public override bool Equals(object obj)
		{
			return obj is MetaPoint point && Equals(point);
		}

		public bool Equals(MetaPoint p)
		{
			return x == p.x && y == p.y && metaX == p.metaX && metaY == p.metaY;
		}
	}


	internal class Part_1
	{
		const int STEP_COUNT_CONST = 64;

		static long Width { get; set; }
		static long Height { get; set; }

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_21.txt");
			string[] input = File.ReadAllLines(path);

			Width = input[0].Length;
			Height = input.Length;
			var rockGrid = new bool[Width, Height];
			Point startPos = new();

			HashSet<Point> walkPos = new();

			// map input to a grid
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var realY = Height - y - 1;
					switch (input[y][x])
					{
						case '#':
							rockGrid[x, realY] = true;
							break;
						case 'S':
							startPos = new Point(x, realY);
							// technically redundant
							rockGrid[x, realY] = false;
							break;
						default:
							rockGrid[x, realY] = false;
							break;
					}
				}
			}

			// start at startPos
			walkPos.Add(startPos);
			for (int i = 0; i < STEP_COUNT_CONST; i++)
			{
				HashSet<Point> nextWalkPos = new();
				foreach (Point p in walkPos)
				{
					List<Point> adjacentPoints = getAdjacentPositions(rockGrid, p);
					foreach (Point p2 in adjacentPoints)
					{
						nextWalkPos.Add(p2);
					}
				}

				walkPos = nextWalkPos;
			}


			return walkPos.Count;
		}

		private static List<Point> getAdjacentPositions(bool[,] rockGrid, Point p)
		{
			var leftPoint = new Point(p.X - 1, p.Y);
			var upPoint = new Point(p.X, p.Y + 1);
			var rightPoint = new Point(p.X + 1, p.Y);
			var downPoint = new Point(p.X, p.Y - 1);

			var outResults = new List<Point>();

			if (leftPoint.X >= 0 && rockGrid[leftPoint.X, leftPoint.Y] == false) outResults.Add(leftPoint);
			if (rightPoint.X < Width && rockGrid[rightPoint.X, rightPoint.Y] == false) outResults.Add(rightPoint);
			if (downPoint.Y >= 0 && rockGrid[downPoint.X, downPoint.Y] == false) outResults.Add(downPoint);
			if (upPoint.Y < Height && rockGrid[upPoint.X, upPoint.Y] == false) outResults.Add(upPoint);

			return outResults;
		}
	}

	internal class Part_2
	{

		public struct IntTuple
		{
			public int first { get; set; }
			public int second { get; set; }
		}

		const int STEP_COUNT_CONST = 327;

		static long Width { get; set; }
		static long Height { get; set; }

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_21.txt");
			string[] input = File.ReadAllLines(path);

			Width = input[0].Length;
			Height = input.Length;
			var rockGrid = new bool[Width, Height];
			MetaPoint startPos = new();

			HashSet<MetaPoint> walkPos = new();

			// map input to a grid
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var realY = Height - y - 1;
					switch (input[y][x])
					{
						case '#':
							rockGrid[x, realY] = true;
							break;
						case 'S':
							startPos = new MetaPoint(x, realY, 0, 0);
							// technically redundant
							rockGrid[x, realY] = false;
							break;
						default:
							rockGrid[x, realY] = false;
							break;
					}
				}
			}

			// pattern tracking
			IntTuple stepCount = new();


			// start at startPos
			walkPos.Add(startPos);
			for (int i = 0; i < STEP_COUNT_CONST; i++)
			{
				HashSet<MetaPoint> nextWalkPos = new();
				foreach (MetaPoint p in walkPos)
				{
					List<MetaPoint> adjacentPoints = getAdjacentPositions(rockGrid, p);
					foreach (MetaPoint p2 in adjacentPoints)
					{
						nextWalkPos.Add(p2);
					}
				}

				//int thisCount = nextWalkPos.ToList().Where(p => p.MetaX == -2 && p.MetaY == 0).Count();
				//if (stepCount.first == thisCount && stepCount.second == thisCount && thisCount != 0)
				//{
				//	// we're alternating in a way that each subsequent iteration will be the same constant value 
				//	// for this "block"
				//	Console.WriteLine("MetaPoint 0,0 constant " + thisCount + " at " + i);
				//}
				//else if (stepCount.second == thisCount && thisCount != 0)
				//{
				//	// we're alternating between two values. This mean our cycle started at "i-2"
				//	Console.WriteLine("MetaPoint 0,0 alternate cycle started at " + (i - 2) + " steps " + thisCount);
				//}
				//else
				//{
				//	stepCount.second = stepCount.first;
				//	stepCount.first = thisCount;
				//}


				walkPos = nextWalkPos;
			}

			//steps: 65(0) => 3730
			//steps: 196(131) => 33366
			//steps: 327(262) => 92548

			// Assume quadratic, solve the quadratic equation axx + bx + c, treating step 65 as X = 0
			// c = 3730
			// b = 14863
			// a = 14773
			// n = (steps - width/2) / width = 202300
			// 14773*202300*202300 + 14863*202300+ 3730


			return walkPos.Count;
		}

		private static List<MetaPoint> getAdjacentPositions(bool[,] rockGrid, MetaPoint p)
		{
			var leftPoint = new MetaPoint(p.X - 1, p.Y, p.MetaX, p.MetaY);
			var upPoint = new MetaPoint(p.X, p.Y + 1, p.MetaX, p.MetaY);
			var rightPoint = new MetaPoint(p.X + 1, p.Y, p.MetaX, p.MetaY);
			var downPoint = new MetaPoint(p.X, p.Y - 1, p.MetaX, p.MetaY);

			var outResults = new List<MetaPoint>();

			if (leftPoint.X == -1)
			{
				leftPoint = new MetaPoint(Width - 1, p.Y, p.MetaX - 1, p.MetaY);
			}
			if (rockGrid[leftPoint.X, leftPoint.Y] == false)
			{
				outResults.Add(leftPoint);
			}

			if (rightPoint.X == Width)
			{
				rightPoint = new MetaPoint(0, p.Y, p.MetaX + 1, p.MetaY);
			}
			if (rockGrid[rightPoint.X, rightPoint.Y] == false)
			{
				outResults.Add(rightPoint);
			}

			if (upPoint.Y == Height)
			{
				upPoint = new MetaPoint(p.X, 0, p.MetaX, p.MetaY + 1);
			}
			if (rockGrid[upPoint.X, upPoint.Y] == false)
			{
				outResults.Add(upPoint);
			}

			if (downPoint.Y == -1)
			{
				downPoint = new MetaPoint(p.X, Height - 1, p.MetaX, p.MetaY - 1);
			}
			if (rockGrid[downPoint.X, downPoint.Y] == false)
			{
				outResults.Add(downPoint);
			}

			return outResults;
		}
	}
}