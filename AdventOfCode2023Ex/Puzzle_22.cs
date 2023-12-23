using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_22
{
	internal struct Point3
	{
		private readonly long x, y, z;

		public long X { get { return x; } }
		public long Y { get { return y; } }
		public long Z { get { return z; } }

		public Point3(long x, long y, long z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public override int GetHashCode()
		{
			return (int)(31 * x + 17 * y + 47 * z); // Or something like that
		}

		public override bool Equals(object obj)
		{
			return obj is Point3 point && Equals(point);
		}

		public bool Equals(Point3 p)
		{
			return x == p.x && y == p.y && z == p.z;
		}
	}

	internal class Brick
	{
		public Point3 StartPoint { get; private set; }
		public Point3 EndPoint { get; private set; }

		public string Name { get; private set; }

		public Brick(Point3 startPos, Point3 endPos, string name)
		{
			Name = name;

			// bricks only have length in 1 dimension. We want
			// start pos to be the "lower" value, and endPos to have the "higher"
			// value
			if (startPos.X < endPos.X)
			{
				StartPoint = startPos;
				EndPoint = endPos;
			}
			else if (startPos.X > endPos.X)
			{
				StartPoint = endPos;
				EndPoint = startPos;
			}
			else if (startPos.Y < endPos.Y)
			{
				StartPoint = startPos;
				EndPoint = endPos;
			}
			else if (startPos.Y > endPos.Y)
			{
				StartPoint = endPos;
				EndPoint = startPos;
			}
			else if (startPos.Z < endPos.Z)
			{
				StartPoint = startPos;
				EndPoint = endPos;
			}
			else if (startPos.Z > endPos.Z)
			{
				StartPoint = endPos;
				EndPoint = startPos;
			}
			else
			{
				// we're a 1x1x1 brick, so start and end are 
				// meaningless
				StartPoint = startPos;
				EndPoint = endPos;
			}
		}

		public long Bottom
		{
			get
			{
				return StartPoint.Z;
			}
		}

		public long Top
		{
			get
			{
				return EndPoint.Z;
			}
		}

		// this is "1 less" than the bricks are actually tall, so +1
		public long BrickHeight
		{
			get
			{
				return EndPoint.Z - StartPoint.Z + 1;
			}
		}

		internal void TranslateDown(long distDown)
		{
			StartPoint = new Point3(StartPoint.X, StartPoint.Y, StartPoint.Z - distDown);
			EndPoint = new Point3(EndPoint.X, EndPoint.Y, EndPoint.Z - distDown);
		}

		public bool ContainsPoint(Point3 intersection)
		{
			return (StartPoint.X <= intersection.X && intersection.X <= EndPoint.X &&
				StartPoint.Y <= intersection.Y && intersection.Y <= EndPoint.Y &&
				StartPoint.Z <= intersection.Z && intersection.Z <= EndPoint.Z);
		}

		public List<Brick> SupportedBy { get; set; } = new();
		public List<Brick> DependedUpon { get; set; } = new();
		internal void AddSupports(List<Brick> bricks)
		{
			SupportedBy.AddRange(bricks);
		}

		internal void AddDependent(Brick b)
		{
			DependedUpon.Add(b);
		}
	}

	internal class BrickSupport
	{
		public Brick Brick { get; set; }
		public List<string> SupportedBy { get; set; } = new();
		public List<string> SupportingBricks { get; set; } = new();

	}

	internal class Part_1
	{
		static long XMax { get; set; }
		static long YMax { get; set; }
		static long ZMax { get; set; }

		const long ZMin = 1;
		const long XMin = 0;
		const long YMin = 0;

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_22.txt");
			string[] input = File.ReadAllLines(path);

			int brickCount = 0;

			List<Brick> brickList = new List<Brick>();

			// map input to a list of bricsk
			foreach (string s in input)
			{
				// split on ~
				var point3s = s.Split('~');

				var startCoords = point3s[0].Split(',');
				var endCoords = point3s[1].Split(',');

				Point3 startPos = new Point3(int.Parse(startCoords[0]), int.Parse(startCoords[1]), int.Parse(startCoords[2]));
				Point3 endPos = new Point3(int.Parse(endCoords[0]), int.Parse(endCoords[1]), int.Parse(endCoords[2]));

				// update maximum extents
				if (startPos.X > XMax) XMax = startPos.X;
				if (startPos.Y > YMax) YMax = startPos.Y;
				if (startPos.Z > ZMax) ZMax = startPos.Z;

				if (endPos.X > XMax) XMax = endPos.X;
				if (endPos.Y > YMax) YMax = endPos.Y;
				if (endPos.Z > ZMax) ZMax = endPos.Z;

				brickList.Add(new Brick(startPos, endPos, "Brick #" + brickCount++));
			}

			//we want to sort ascending (from lowest Z to highest)
			brickList.Sort((b1, b2) => b1.Bottom.CompareTo(b2.Bottom));


			// because long, each cell is initially 0, which is what we want
			long[,] heightMap = new long[XMax + 1, YMax + 1];
			Dictionary<long, List<Brick>> brickTops = new Dictionary<long, List<Brick>>();

			foreach (Brick b in brickList)
			{
				// b is not a reference
				long distDown = long.MaxValue;

				// if Z is > 1, move the brick down as far as possible
				for (long x = b.StartPoint.X; x <= b.EndPoint.X; x++)
				{
					for (long y = b.StartPoint.Y; y <= b.EndPoint.Y; y++)
					{
						// we don't need to iterate over the Z points, because
						// the height updates are the same.

						// Calculate the difference between this brick-point and
						// the height. We want to find the minimum (aka, the first time
						// we would collide)
						long heightResult = b.Bottom - heightMap[x, y];
						if (heightResult < distDown)
						{
							distDown = heightResult;
						}
					}
				}

				// now we have the offset for how far down the brick "moves"
				// so update the height map for each of the brick-points

				for (long x = b.StartPoint.X; x <= b.EndPoint.X; x++)
				{
					for (long y = b.StartPoint.Y; y <= b.EndPoint.Y; y++)
					{
						// e.g. b.Bottom =  1, heightResult = 1 - 0, => 1
						// increase heightMap[x,y] by heightResult + brickHeight
						heightMap[x, y] = b.Bottom - distDown + b.BrickHeight;
					}
				}

				b.TranslateDown(distDown - 1);
				if (brickTops.ContainsKey(b.Top))
				{
					brickTops[b.Top].Add(b);
				}
				else
				{
					brickTops[b.Top] = new List<Brick>() { b };
				}
			}

			// we now have a list of translated bricks, and a map that allows us to find 
			// all bricks on that "level".

			// now start from the top so we can find all the bricks that support the brick we're querying
			brickList.Reverse();
			foreach (Brick b in brickList)
			{
				// ignore bricks on the bottom
				if (b.Bottom == 1) continue;

				HashSet<Brick> supportedBy = new();

				// get all bricks whose top is 1 level below us.
				var lowerBricks = brickTops[b.Bottom - 1];
				foreach (Brick lowerBrick in lowerBricks)
				{
					bool skipRest = false;
					for (long x = b.StartPoint.X; x <= b.EndPoint.X; x++)
					{
						for (long y = b.StartPoint.Y; y <= b.EndPoint.Y; y++)
						{
							// make a Point3 that corresponds to this part of the brick
							Point3 bPoint = new Point3(x, y, b.Bottom - 1);

							if (lowerBrick.ContainsPoint(bPoint))
							{
								supportedBy.Add(lowerBrick);

								lowerBrick.AddDependent(b);

								skipRest = true;
							}

							if (skipRest) break;
						}
						if (skipRest) break;
					}
				}

				b.AddSupports(supportedBy.ToList());
			}

			int disinteBrickCounter = 0;
			foreach (Brick b in brickList)
			{
				// a brick can be safely disintegrated iff
				// - it's "Dependent List" is empty OR
				// - each brick in its "Dependent List" has
				// at least 2 supports.
				if (b.DependedUpon.Count == 0)
				{
					disinteBrickCounter++;
					Console.WriteLine(b.Name + " can be disintegrated");
					continue;
				}

				bool isRedundant = true;
				foreach (Brick dep in b.DependedUpon)
				{
					if (dep.SupportedBy.Count == 1)
					{
						isRedundant = false;
					}
				}

				if (isRedundant)
				{
					disinteBrickCounter++;
					Console.WriteLine(b.Name + " can be disintegrated");
				}
			}

			return disinteBrickCounter;
		}


	}

	internal class Part_2
	{
		static long XMax { get; set; }
		static long YMax { get; set; }

		const long XMin = 0;
		const long YMin = 0;

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_22.txt");
			string[] input = File.ReadAllLines(path);

			int brickCount = 0;

			List<Brick> brickList = new List<Brick>();

			// map input to a list of bricsk
			foreach (string s in input)
			{
				// split on ~
				var point3s = s.Split('~');

				var startCoords = point3s[0].Split(',');
				var endCoords = point3s[1].Split(',');

				Point3 startPos = new Point3(int.Parse(startCoords[0]), int.Parse(startCoords[1]), int.Parse(startCoords[2]));
				Point3 endPos = new Point3(int.Parse(endCoords[0]), int.Parse(endCoords[1]), int.Parse(endCoords[2]));

				// update maximum extents
				if (startPos.X > XMax) XMax = startPos.X;
				if (startPos.Y > YMax) YMax = startPos.Y;

				if (endPos.X > XMax) XMax = endPos.X;
				if (endPos.Y > YMax) YMax = endPos.Y;

				brickList.Add(new Brick(startPos, endPos, "Brick #" + brickCount++));
			}

			//we want to sort ascending (from lowest Z to highest)
			brickList.Sort((b1, b2) => b1.Bottom.CompareTo(b2.Bottom));


			// because long, each cell is initially 0, which is what we want
			long[,] heightMap = new long[XMax + 1, YMax + 1];
			Dictionary<long, List<Brick>> brickTops = new Dictionary<long, List<Brick>>();

			foreach (Brick b in brickList)
			{
				// b is not a reference
				long distDown = long.MaxValue;

				// if Z is > 1, move the brick down as far as possible
				for (long x = b.StartPoint.X; x <= b.EndPoint.X; x++)
				{
					for (long y = b.StartPoint.Y; y <= b.EndPoint.Y; y++)
					{
						// we don't need to iterate over the Z points, because
						// the height updates are the same.

						// Calculate the difference between this brick-point and
						// the height. We want to find the minimum (aka, the first time
						// we would collide)
						long heightResult = b.Bottom - heightMap[x, y];
						if (heightResult < distDown)
						{
							distDown = heightResult;
						}
					}
				}

				// now we have the offset for how far down the brick "moves"
				// so update the height map for each of the brick-points

				for (long x = b.StartPoint.X; x <= b.EndPoint.X; x++)
				{
					for (long y = b.StartPoint.Y; y <= b.EndPoint.Y; y++)
					{
						// e.g. b.Bottom =  1, heightResult = 1 - 0, => 1
						// increase heightMap[x,y] by heightResult + brickHeight
						heightMap[x, y] = b.Bottom - distDown + b.BrickHeight;
					}
				}

				b.TranslateDown(distDown - 1);
				if (brickTops.ContainsKey(b.Top))
				{
					brickTops[b.Top].Add(b);
				}
				else
				{
					brickTops[b.Top] = new List<Brick>() { b };
				}
			}

			// we now have a list of translated bricks, and a map that allows us to find 
			// all bricks on that "level".

			// now start from the top so we can find all the bricks that support the brick we're querying
			brickList.Reverse();
			foreach (Brick b in brickList)
			{
				// ignore bricks on the bottom
				if (b.Bottom == 1) continue;

				HashSet<Brick> supportedBy = new();

				// get all bricks whose top is 1 level below us.
				var lowerBricks = brickTops[b.Bottom - 1];
				foreach (Brick lowerBrick in lowerBricks)
				{
					bool skipRest = false;
					for (long x = b.StartPoint.X; x <= b.EndPoint.X; x++)
					{
						for (long y = b.StartPoint.Y; y <= b.EndPoint.Y; y++)
						{
							// make a Point3 that corresponds to this part of the brick
							Point3 bPoint = new Point3(x, y, b.Bottom - 1);

							if (lowerBrick.ContainsPoint(bPoint))
							{
								supportedBy.Add(lowerBrick);

								lowerBrick.AddDependent(b);

								skipRest = true;
							}

							if (skipRest) break;
						}
						if (skipRest) break;
					}
				}

				b.AddSupports(supportedBy.ToList());
			}

			// calculate the # of bricks that would disintegrate in a 
			// chain reaction for each brick.

			// I think this could be used as some kind of partial cache, but 
			// I'm going to just try and brute force it, first.
			Dictionary<string, int> chainDisintegrateDict = new();

			foreach (Brick b in brickList)
			{
				// when a brick is disintegrated, it stops providing
				// support. When one of those bricks then has no supports,
				// it too disintegrates in a CHAIN REACTION.
				HashSet<string> disintegratedBricks = new();

				// start with the brick we're querying.
				disintegratedBricks.Add(b.Name);
				//int disintegrateCombob = 0;

				List<Brick> bricksToCheck = new();
				HashSet<Brick> nextRound = new(b.DependedUpon);

				while (bricksToCheck.Count > 0 || nextRound.Count > 0)
				{
					bricksToCheck = nextRound.ToList();
					nextRound = new();

					while (bricksToCheck.Count > 0)
					{
						Brick dependent = bricksToCheck[0];
						bricksToCheck.RemoveAt(0);

						bool isSupported = false;
						// if each "support" in this dependent is contained
						// by disintegrated bricks, then this brick is disintegrated
						foreach (Brick support in dependent.SupportedBy)
						{
							if (disintegratedBricks.Contains(support.Name) == false)
							{
								isSupported = true;
							}
						}
						if (!isSupported)
						{
							// this brick was disintegrated. Do we have to do some extra processing
							// here?
							disintegratedBricks.Add(dependent.Name);
							//disintegrateCombob++;
							foreach (Brick dd in dependent.DependedUpon)
							{
								nextRound.Add(dd);
							}
						}
					}
				}

				chainDisintegrateDict[b.Name] = disintegratedBricks.Count - 1;
			}

			return chainDisintegrateDict.Values.Sum();
		}


	}
}