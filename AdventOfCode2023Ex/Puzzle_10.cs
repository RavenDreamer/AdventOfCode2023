using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_10
{

	public enum pipeDirection
	{
		westNorth,
		westEast,
		westSouth,

		northEast,
		northSouth,

		eastSouth,

		none,
	}

	public enum cardinalDirection
	{
		north = 1,
		west = 2,
		east = 4,
		south = 8,
	}

	public const int S_CONST = (int)cardinalDirection.north + (int)cardinalDirection.west;
	public const char S_CHAR = 'J';

	public static Dictionary<char, int> pipeDirections = new Dictionary<char, int>()
	{
		{'-', (int)cardinalDirection.west + (int)cardinalDirection.east },
		{'|',(int)cardinalDirection.north + (int)cardinalDirection.south },
		{'L',(int)cardinalDirection.north + (int)cardinalDirection.east },
		{'J',(int)cardinalDirection.west + (int)cardinalDirection.north  },
		{'7',(int)cardinalDirection.west + (int)cardinalDirection.south  },
		{'F',(int)cardinalDirection.south + (int)cardinalDirection.east  },
		{'S',S_CONST },
		{'.',0 },
	};

	internal struct MazeHeading
	{
		public int xVal { get; set; }
		public int yVal { get; set; }
		public cardinalDirection dir { get; set; }

		public string PrintCoords()
		{
			return xVal + "," + yVal;
		}

		internal void UpdateHeading(cardinalDirection cardinalDirection)
		{
			// heading dir is the opposite of what we entered on.
			// if we're going west, we're coming from the east, etc.

			// west == -x
			// east == +x
			// north == +y
			// south == -y
			switch (cardinalDirection)
			{
				case cardinalDirection.west:
					dir = cardinalDirection.east;
					xVal--;
					return;
				case cardinalDirection.east:
					dir = cardinalDirection.west;
					xVal++;
					return;
				case cardinalDirection.north:
					dir = cardinalDirection.south;
					yVal++;
					return;
				case cardinalDirection.south:
					dir = cardinalDirection.north;
					yVal--;
					return;
			}
		}


		// The offset used depends on both the shape and the heading.
		// If we're a |, we want to check east if we're heading north
		// and west if we're heading south.
		//
		// Dir here is the direction we're COMING FROM, not going to, so
		// it's backwards from what is intuitive - but it's faster to fix
		// it here than in the struct, I think...
		internal List<Tuple<int, int>> GetRightHandOffset(char pipeType)
		{
			List<Tuple<int, int>> outList = new();

			if (pipeType == 'S') pipeType = S_CHAR;

			switch (pipeType)
			{
				case '-':
					if (dir == cardinalDirection.west)
					{
						// right hand side is "down"
						outList.Add(new Tuple<int, int>(0, -1));
					}
					else
					{
						// right hand side is "up"
						outList.Add(new Tuple<int, int>(0, 1));
					}
					break;
				case '|':
					if (dir == cardinalDirection.south)
					{
						// right hand side is "east"
						outList.Add(new Tuple<int, int>(1, 0));
					}
					else
					{
						// right hand side is "west"
						outList.Add(new Tuple<int, int>(-1, 0));
					}
					break;
				case 'L':
					if (dir == cardinalDirection.north)
					{
						// right hand side is "left", "left+down", and "down"
						outList.Add(new Tuple<int, int>(-1, 0));
						outList.Add(new Tuple<int, int>(-1, -1));
						outList.Add(new Tuple<int, int>(0, -1));
					}
					else
					{
						// right hand side is "up+right"
						// which is handled by adjacent pieces
					}
					break;

				case 'J':
					if (dir == cardinalDirection.north)
					{
						// right hand side is "left+up"
						// which is handled by adjacent pieces
					}
					else
					{
						// right hand side is "down", "down+right", and "right"
						outList.Add(new Tuple<int, int>(0, -1));
						outList.Add(new Tuple<int, int>(1, -1));
						outList.Add(new Tuple<int, int>(1, 0));
					}
					break;
				case '7':
					if (dir == cardinalDirection.west)
					{
						// right hand side is "left+down"
						// which is handled by adjacent pieces
					}
					else
					{
						// right hand side is "east", "up+east", and "up"
						outList.Add(new Tuple<int, int>(1, 0));
						outList.Add(new Tuple<int, int>(1, 1));
						outList.Add(new Tuple<int, int>(0, 1));
					}
					break;
				case 'F':
					if (dir == cardinalDirection.south)
					{
						// right hand side is "right+down"
						// which is handled by adjacent pieces
					}
					else
					{
						// right hand side is "west", "up+west", and "up"
						outList.Add(new Tuple<int, int>(-1, 0));
						outList.Add(new Tuple<int, int>(-1, 1));
						outList.Add(new Tuple<int, int>(0, 1));
					}
					break;
				default:
					throw new Exception("how'd we get here?");
			}

			return outList;
		}
	}

	internal class MazeNode
	{
		//internal int xCoord { get; set; }
		//internal int yCoord { get; set; }
		internal char pipeLayout { get; set; }
		internal int myDirection { get; set; }

		public MazeNode(char input)
		{
			myDirection = pipeDirections[input];
			pipeLayout = input;
		}

		/// <summary>
		/// If you come into this pipe traveling in direction X, you leave it going 
		/// in direction Y
		/// </summary>
		/// <param name="x"></param>
		/// <returns>y</returns>
		public cardinalDirection TravelThrough(cardinalDirection input)
		{
			// myDirection - input == output (independent of direction)
			return (cardinalDirection)(myDirection - (int)input);

		}
		/// <summary>
		/// decomposes this node from its conjoined pipe directions
		/// to the constituent cardinal directions
		/// </summary>
		/// <returns></returns>
		public List<cardinalDirection> GetDirections()
		{
			List<cardinalDirection> outList = new();

			foreach (cardinalDirection dir in Enum.GetValues<cardinalDirection>())
			{
				var temp = myDirection - dir;
				// if this is another valid value, we have one of the 
				// constituent directions
				if (Enum.IsDefined<cardinalDirection>(temp))
				{
					outList.Add(dir);
					outList.Add(temp);
					return outList;
				}
			}

			throw new Exception("Unknown direction enum stuff");
		}
	}

	internal class Part_1
	{
		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_10.txt");
			string[] input = File.ReadAllLines(path);

			var mazeNodes = new MazeNode[input.Length, input.Length];

			int startX = -1;
			int startY = -1;
			MazeNode startNode = null;

			// we have a square grid, and our first lines are 0,lenght-1, not 0,0'
			for (int x = 0; x < input.Length; x++)
			{
				for (int y = 0; y < input.Length; y++)
				{
					var node = new MazeNode(input[y][x]);
					mazeNodes[x, input.Length - 1 - y] = node;

					if (node.pipeLayout == 'S')
					{
						startNode = node;
						startX = x;
						startY = input.Length - 1 - y;
					}
				}
			}
			//HashSet<string> visitedNodes = new() { startX+","+startY};

			var dirs = startNode.GetDirections();
			string startCoords = startX + "," + startY;
			// alpha is the forward looking traversal
			MazeHeading alphaHeading = new MazeHeading() { dir = dirs[0], xVal = startX, yVal = startY };

			// omega is the backwards looking traversal
			MazeHeading omegaHeading = new MazeHeading() { dir = dirs[1], xVal = startX, yVal = startY };

			int stepCount = 0;

			while (alphaHeading.PrintCoords() != omegaHeading.PrintCoords() || alphaHeading.PrintCoords() == startCoords)
			{
				// move in alpha direction
				alphaHeading.UpdateHeading(mazeNodes[alphaHeading.xVal, alphaHeading.yVal].TravelThrough(alphaHeading.dir));

				// move in omegaDirection
				omegaHeading.UpdateHeading(mazeNodes[omegaHeading.xVal, omegaHeading.yVal].TravelThrough(omegaHeading.dir));

				stepCount++;
			}

			return stepCount;
		}
	}

	internal class Part_2
	{
		public static int width;
		public static int height;
		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_10.txt");
			string[] input = File.ReadAllLines(path);

			var mazeNodes = new MazeNode[input[0].Length, input.Length];

			int startX = -1;
			int startY = -1;
			MazeNode startNode = null;

			// we have a square grid, and our first lines are 0,lenght-1, not 0,0'
			for (int x = 0; x < input[0].Length; x++)
			{
				for (int y = 0; y < input.Length; y++)
				{
					var node = new MazeNode(input[y][x]);
					mazeNodes[x, input.Length - 1 - y] = node;

					if (node.pipeLayout == 'S')
					{
						startNode = node;
						startX = x;
						startY = input.Length - 1 - y;
					}
				}
			}

			width = input[0].Length;
			height = input.Length;
			//HashSet<string> visitedNodes = new() { startX+","+startY};

			var dirs = startNode.GetDirections();
			string startCoords = startX + "," + startY;
			// only need one direction
			MazeHeading omegaHeading = new MazeHeading() { dir = dirs[1], xVal = startX, yVal = startY };

			int stepCount = 0;
			int bestX = int.MaxValue;
			int bestY = 0;

			HashSet<Tuple<int, int>> pathHash = new();

			// We need to traverse the pipe once to know which pipes are actually part of the loop.
			// (this is why we can't simply pick the top, left-most 'F' char -- there's no guarantee it's
			// part of the shape!)
			while (omegaHeading.PrintCoords() != startCoords || stepCount == 0)
			{
				var node = mazeNodes[omegaHeading.xVal, omegaHeading.yVal];
				pathHash.Add(new Tuple<int, int>(omegaHeading.xVal, omegaHeading.yVal));

				//check for 'F'
				if (node.pipeLayout == 'F' || node.pipeLayout == 'S' && S_CHAR == 'F')
				{
					// we want to update x + y, if new x is less than bestX
					// and we want to update y if nex x == bestX but newY > bestY
					if (omegaHeading.xVal < bestX)
					{
						bestX = omegaHeading.xVal;
						bestY = omegaHeading.yVal;
					}
					else if (omegaHeading.xVal == bestX)
					{
						if (omegaHeading.yVal > bestY)
						{
							bestY = omegaHeading.yVal;
						}
					}
				}

				// move in omegaDirection
				var traverseDir = node.TravelThrough(omegaHeading.dir);
				omegaHeading.UpdateHeading(traverseDir);
				stepCount++;
			}

			// we now have what is guaranteed to be a convex angle (the bestX,bestY coords of 'F').
			// Travel along the shape in a clockwise rotation, scanning the "right-hand side"
			// for '.' chars. Add the coords of those '.' chars to a hashSet, then 
			// flood-fill from those '.' to look for larger enclosed areas.

			// Assumption: the pipes are always simple (no holes) so the "right-hand side" will encompass the totality
			// of the enclosed area

			// while traveling in a clockwise direction, track areas on the "right-hand" to our direction of motion. These
			// are the blocks that are candidate

			var clockwiseHeading = new MazeHeading() { dir = cardinalDirection.south, xVal = bestX, yVal = bestY };
			startCoords = bestX + "," + bestY;

			List<Tuple<int, int>> potentialCoords = new();
			HashSet<Tuple<int, int>> floodedCoords = new();
			stepCount = 0;

			while (clockwiseHeading.PrintCoords() != startCoords || stepCount == 0)
			{
				var node = mazeNodes[clockwiseHeading.xVal, clockwiseHeading.yVal];

				// check for '.'
				// The offset used depends on both the shape and the heading.
				// If we're a |, we want to check east if we're heading north
				// and west if we're heading south.

				var coordOffsets = clockwiseHeading.GetRightHandOffset(node.pipeLayout);

				foreach (var coord in coordOffsets)
				{
					var netCoord = new Tuple<int, int>(coord.Item1 + clockwiseHeading.xVal, coord.Item2 + clockwiseHeading.yVal);
					if (pathHash.Contains(netCoord) != true)
					{
						// we found some enclosed ground!
						potentialCoords.Add(new Tuple<int, int>(
							coord.Item1 + clockwiseHeading.xVal, coord.Item2 + clockwiseHeading.yVal
							)
						);
					}
				}


				// move in clockwiseDirection
				var traverseDir = node.TravelThrough(clockwiseHeading.dir);
				clockwiseHeading.UpdateHeading(traverseDir);
				stepCount++;
			}

			// Now all that's left is to floodfill the groundCoords for adjacent ground (that must
			// be enclosed, since all the ground we have already is enclosed)
			while (potentialCoords.Count > 0)
			{
				var workingCoords = potentialCoords.First();
				// I really realyl really should have just imported
				// my Point class...
				List<Tuple<int, int>> offsetCoords = GetOffsets(workingCoords);
				foreach (var coord in offsetCoords)
				{
					if (floodedCoords.Contains(coord)) continue;
					// add this if it's not in pathHash
					if (pathHash.Contains(coord) != true)
					{
						potentialCoords.Add(coord);
					}
				}

				// add the workingCoords to the floodedCoords
				floodedCoords.Add(workingCoords);
				// and remove it from potentialCoords.
				potentialCoords.Remove(workingCoords);
			}

			return floodedCoords.Count;
		}

		private static List<Tuple<int, int>> GetOffsets(Tuple<int, int> workingCoords)
		{
			// generate 8 directions around the xCord,yCord of workingCoord

			List<Tuple<int, int>> offsets = new List<Tuple<int, int>>();
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					offsets.Add(new Tuple<int, int>(workingCoords.Item1 + i, workingCoords.Item2 + j));
				}
			}
			// remove all offsets where negative or greater than width / height
			return offsets.Where(n => n.Item1 >= 0 && n.Item1 < width &&
									  n.Item2 >= 0 && n.Item2 < height).ToList();
		}
	}

}
