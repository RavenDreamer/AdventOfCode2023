using System;
using System.Collections.Generic;
using System.Dynamic;
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

	public const int S_CONST = (int)cardinalDirection.south + (int)cardinalDirection.east;

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

		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\debug.txt");
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

}
