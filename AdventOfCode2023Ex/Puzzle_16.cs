using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_16
{
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down,
		None,
	}

	internal struct Heading
	{
		internal int X { get; set; }
		internal int Y { get; set; }

		internal Direction Direction { get; set; }

		public override string ToString()
		{
			return X + "," + Y + ":" + Direction;
		}
	}



	static List<Heading> UpdateHeading(Heading h, Char target)
	{
		List<Heading> results = new List<Heading>();
		Heading offset;
		switch (target)
		{
			case '.':
				// go straight
				offset = GetHeadingOffset(h.Direction);
				h.X = h.X + offset.X;
				h.Y = h.Y + offset.Y;
				// direction remains the same
				results.Add(h);
				break;
			case '|':
				// if perpendicular, add two new Headings
				if (h.Direction == Direction.Left || h.Direction == Direction.Right)
				{
					// up first
					offset = GetHeadingOffset(Direction.Up);
					var upHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Up };
					results.Add(upHeading);
					// down
					offset = GetHeadingOffset(Direction.Down);
					var downHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Down };
					results.Add(downHeading);
				}
				else
				{
					//we pass through it w/o incident
					// go straight
					offset = GetHeadingOffset(h.Direction);
					h.X = h.X + offset.X;
					h.Y = h.Y + offset.Y;
					// direction remains the same
					results.Add(h);
				}
				break;
			case '-':
				// if perpendicular, add two new Headings
				if (h.Direction == Direction.Up || h.Direction == Direction.Down)
				{
					// left first
					offset = GetHeadingOffset(Direction.Left);
					var leftHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Left };
					results.Add(leftHeading);

					// right
					offset = GetHeadingOffset(Direction.Right);
					var rightHead = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Right };
					results.Add(rightHead);
				}
				else
				{
					//we pass through it w/o incident
					// go straight
					offset = GetHeadingOffset(h.Direction);
					h.X = h.X + offset.X;
					h.Y = h.Y + offset.Y;
					// direction remains the same
					results.Add(h);
				}
				break;
			case '\\':
				// right -> down
				// left -> up
				// up -> left
				// down -> right
				// if we are up or right, we are now right or up
				switch (h.Direction)
				{
					case Direction.Left:
						// go Up
						offset = GetHeadingOffset(Direction.Up);
						var downHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Up };
						results.Add(downHeading);
						break;
					case Direction.Right:
						// go Down
						offset = GetHeadingOffset(Direction.Down);
						var rightHead = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Down };
						results.Add(rightHead);
						break;
					case Direction.Up:
						// go Left
						offset = GetHeadingOffset(Direction.Left);
						var upHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Left };
						results.Add(upHeading);
						break;
					case Direction.Down:
						// go Right
						offset = GetHeadingOffset(Direction.Right);
						var leftHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Right };
						results.Add(leftHeading);
						break;
				}

				break;
			case '/':
				switch (h.Direction)
				{
					// right -> up
					// down -> left
					// up -> right
					// left -> down
					// if we are down or right, we are now right or down
					case Direction.Right:
						// go up
						offset = GetHeadingOffset(Direction.Up);
						var downHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Up };
						results.Add(downHeading);
						break;
					case Direction.Left:
						// go Down
						offset = GetHeadingOffset(Direction.Down);
						var rightHead = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Down };
						results.Add(rightHead);
						break;
					case Direction.Down:
						// go Left
						offset = GetHeadingOffset(Direction.Left);
						var upHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Left };
						results.Add(upHeading);
						break;
					case Direction.Up:
						// go Right
						offset = GetHeadingOffset(Direction.Right);
						var leftHeading = new Heading() { X = h.X + offset.X, Y = h.Y + offset.Y, Direction = Direction.Right };
						results.Add(leftHeading);
						break;
				}
				break;
			default:
				throw new Exception("unexpected input");
		}

		return results;
	}

	static Heading UnitLeft = new Heading() { Direction = Direction.None, X = -1, Y = 0 };
	static Heading UnitRight = new Heading() { Direction = Direction.None, X = 1, Y = 0 };
	static Heading UnitUp = new Heading() { Direction = Direction.None, X = 0, Y = 1 };
	static Heading UnitDown = new Heading() { Direction = Direction.None, X = 0, Y = -1 };

	private static Heading GetHeadingOffset(Direction direction)
	{
		switch (direction)
		{
			case Direction.Left:
				return UnitLeft;
			case Direction.Right:
				return UnitRight;
			case Direction.Up:
				return UnitUp;
			case Direction.Down:
				return UnitDown;
			default:
				throw new Exception("unexpected input");
		}
	}

	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_16.txt");
			string[] input = File.ReadAllLines(path);

			var height = input.Length;
			var width = input[0].Length;

			char[,] gridSpace = new char[width, height];


			// map input to a grid
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var realY = height - y - 1;
					gridSpace[x, realY] = input[y][x];
				}
			}

			HashSet<string> beamHistory = new();
			var activeBeams = new List<Heading>() { new Heading() { X = 0, Y = height - 1, Direction = Direction.Right } };


			while (activeBeams.Count > 0)
			{
				var workingBeam = activeBeams.First();
				// check beamHistory if we've been in this position + direction before
				if (beamHistory.Contains(workingBeam.ToString()) == false)
				{
					// we haven't been here before. Find the next tile(s)
					var nextHeadings = UpdateHeading(workingBeam, gridSpace[workingBeam.X, workingBeam.Y]);
					foreach (Heading h in nextHeadings)
					{
						// if we go off the grid, discard them
						if (h.X < 0 || h.X == width || h.Y < 0 || h.Y == height)
						{
							// do nothing
						}
						else
						{
							// add the new child-beams to the activeBeams list
							activeBeams.Add(h);
						}
					}

					beamHistory.Add(workingBeam.ToString());
				}
				else
				{
					// we already mapped this beam. nothing to do.
				}

				// remove from list
				activeBeams.Remove(workingBeam);
			}
			// Beam history has extra data counts (since each direction is a unique count
			// but isn't an energized count. So dedupe
			HashSet<string> dedupedHistory = new();
			foreach (var beam in beamHistory)
			{
				dedupedHistory.Add(beam.Split(":")[0]);
			}

			return dedupedHistory.Count;
		}


	}

	internal class Part_2
	{
		static int Width;
		static int Height;
		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_16.txt");
			string[] input = File.ReadAllLines(path);


			Height = input.Length;
			Width = input[0].Length;

			char[,] gridSpace = new char[Width, Height];


			// map input to a grid
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var realY = Height - y - 1;
					gridSpace[x, realY] = input[y][x];
				}
			}

			// This is the brute force method. Code is not set up to determine how many tiles each laser energizes
			// but that'd be a good way to do this. (identify unique lasers by initial heading)
			List<Heading> allStarts = new();
			for (int x = 0; x < Width; x++)
			{
				allStarts.Add(new Heading() { X = x, Y = Height - 1, Direction = Direction.Down });
				allStarts.Add(new Heading() { X = x, Y = 0, Direction = Direction.Up });
			}

			for (int y = 0; y < Height; y++)
			{
				allStarts.Add(new Heading() { X = 0, Y = y, Direction = Direction.Right });
				allStarts.Add(new Heading() { X = Width - 1, Y = y, Direction = Direction.Left });
			}

			int maxEnergized = 0;

			foreach (Heading h in allStarts)
			{
				int energized = ExecuteLasering(h, gridSpace);
				if (maxEnergized < energized) maxEnergized = energized;
			}

			return maxEnergized;

		}

		public static int ExecuteLasering(Heading startHeading, char[,] gridSpace)
		{
			HashSet<string> beamHistory = new();
			var activeBeams = new List<Heading>() { startHeading };

			while (activeBeams.Count > 0)
			{
				var workingBeam = activeBeams.First();
				// check beamHistory if we've been in this position + direction before
				if (beamHistory.Contains(workingBeam.ToString()) == false)
				{
					// we haven't been here before. Find the next tile(s)
					var nextHeadings = UpdateHeading(workingBeam, gridSpace[workingBeam.X, workingBeam.Y]);
					foreach (Heading h in nextHeadings)
					{
						// if we go off the grid, discard them
						if (h.X < 0 || h.X == Width || h.Y < 0 || h.Y == Height)
						{
							// do nothing
						}
						else
						{
							// add the new child-beams to the activeBeams list
							activeBeams.Add(h);
						}
					}

					beamHistory.Add(workingBeam.ToString());
				}
				else
				{
					// we already mapped this beam. nothing to do.
				}

				// remove from list
				activeBeams.Remove(workingBeam);
			}
			// Beam history has extra data counts (since each direction is a unique count
			// but isn't an energized count. So dedupe
			HashSet<string> dedupedHistory = new();
			foreach (var beam in beamHistory)
			{
				dedupedHistory.Add(beam.Split(":")[0]);
			}

			return dedupedHistory.Count;
		}
	}

}
