using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_17
{
	public enum Direction
	{
		None = -1,
		Left = 0,
		Right = 1,
		Up = 2,
		Down = 3,
	}


	internal class Node
	{

		internal int X { get; set; }
		internal int Y { get; set; }

		internal Direction DirectionOfMovement { get; set; }
		internal int TimesMovedInThatDirection { get; set; }
		internal int CumulativeCost { get; set; }
		internal int ThisCost { get; set; }
		internal Node PriorNode { get; set; }
	}

	internal static int Height { get; set; }
	internal static int Width { get; set; }

	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\debug.txt");
			string[] input = File.ReadAllLines(path);

			Height = input.Length;
			Width = input[0].Length;
			var maxTimes = 3;
			var numDirections = 4;


			Node[,,,] gridSpace = new Node[Width, Height, maxTimes, numDirections];


			// map input to a grid
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					for (int t = 0; t < 3; t++)
					{
						foreach (Direction d in Enum.GetValues<Direction>())
						{
							if (d == Direction.None) continue;
							var realY = Height - y - 1;
							var cost = int.Parse(input[y][x].ToString());

							var tempNode = new Node()
							{
								X = x,
								Y = realY,
								ThisCost = cost,
								DirectionOfMovement = d,
								TimesMovedInThatDirection = t,
								CumulativeCost = -1,
							};

							gridSpace[x, realY, t, (int)d] = tempNode;
						}
					}
				}
			}

			// left is semi-arbitrary here -- it could also be up, for instance
			PriorityQueue<Node, int> seekerNodes = new();
			var startNode = gridSpace[0, Height - 1, 0, (int)Direction.Left];
			seekerNodes.Enqueue(gridSpace[0, Height - 1, 0, (int)Direction.Left], 1);
			// we want to consider all directions from our start
			startNode.DirectionOfMovement = Direction.None;
			startNode.CumulativeCost = 0;

			Node goalNode = null;

			while (seekerNodes.Count > 0)
			{
				// always grab the next best path
				// for the given direction
				var workingNode = seekerNodes.Dequeue();

				if (workingNode.X == Width - 1 && workingNode.Y == 0)
				{
					// we reached the goal
					goalNode = workingNode;
					break;
				}
				// get all adjacent Nodes
				////////////////////List<Node> adjacents = GetHeadings(workingNode, gridSpace);

				////////////////////foreach (var adjNode in adjacents)
				////////////////////{


				////////////////////	var prospectiveCost = workingNode.CumulativeCost + adjNode.ThisCost;

				////////////////////	var priorCumulativeCost = adjNode.CumulativeCost;

				////////////////////	// if prospectiveCost is lower than the current cumulativecost, update
				////////////////////	if (prospectiveCost < priorCumulativeCost || priorCumulativeCost == -1)
				////////////////////	{
				////////////////////		adjNode.PriorNode = workingNode;
				////////////////////		adjNode.CumulativeCost = prospectiveCost;
				////////////////////		seekerNodes.Enqueue(adjNode, prospectiveCost);

				////////////////////	}
				////////////////////	else
				////////////////////	{
				////////////////////		// this path isn't faster, so exit out
				////////////////////		continue;
				////////////////////	}
				////////////////////}
			}

			var priorNode = goalNode;


			while (priorNode.PriorNode != null)
			{
				Console.WriteLine(priorNode.X + "," + priorNode.Y);
				priorNode = priorNode.PriorNode;
			}

			return goalNode.CumulativeCost;
		}




	}

	private static List<List<Node>> GetHeadings(Node workingNode, Node[,,,] gridSpace)
	{
		// use the node to find "adjacent" nodes
		List<List<Node>> results = new();

		var validDirections = Enum.GetValues<Direction>().ToList();

		// can't go none
		validDirections.Remove(Direction.None);
		// can't go backwards
		validDirections.Remove(GetOpposite(workingNode.DirectionOfMovement));

		if (workingNode.TimesMovedInThatDirection == 0 && workingNode.DirectionOfMovement != Direction.None)
		{
			// needs to move a minimum of 4, so return all next nodes in sequence
			// (otherwise the costs get screwed up)

			List<Node> jumpList = new();

			var timesMoved = 1;

			var xOffset = GetDirectionXOffset(workingNode.DirectionOfMovement);
			var yOffset = GetDirectionYOffset(workingNode.DirectionOfMovement);

			if (yOffset == 0)
			{
				for (int x = 1; x < 4; x++)
				{
					var newX = x * GetDirectionXOffset(workingNode.DirectionOfMovement) + workingNode.X;
					var newY = workingNode.Y;
					// out of bounds, so no valid nodes
					if (newX < 0 || newX >= Puzzle_17.Width) return results; ;
					if (newY < 0 || newY >= Puzzle_17.Height) return results; ;

					jumpList.Add(gridSpace[newX, newY, timesMoved++, (int)workingNode.DirectionOfMovement]);
				}
			}
			if (xOffset == 0)
			{
				for (int y = 1; y < 4; y++)
				{
					var newX = workingNode.X;
					var newY = y * GetDirectionYOffset(workingNode.DirectionOfMovement) + workingNode.Y;
					// out of bounds, so no valid nodes
					if (newX < 0 || newX >= Puzzle_17.Width) return results; ;
					if (newY < 0 || newY >= Puzzle_17.Height) return results; ;

					jumpList.Add(gridSpace[newX, newY, timesMoved++, (int)workingNode.DirectionOfMovement]);
				}
			}


			// we're valid. Let's add it.
			results.Add(jumpList);
			return results;
		}

		if (workingNode.TimesMovedInThatDirection == 9)
		{
			// can't move more than 10 in the same direction
			validDirections.Remove(workingNode.DirectionOfMovement);
		}


		// for the remaining directions, we need to calculate X and Y offsets
		foreach (var direction in validDirections)
		{
			var timesMoved = workingNode.DirectionOfMovement == direction ? workingNode.TimesMovedInThatDirection + 1 : 0;
			var newX = GetDirectionXOffset(direction) + workingNode.X;
			var newY = GetDirectionYOffset(direction) + workingNode.Y;

			// out of bounds, no need to add
			if (newX < 0 || newX >= Puzzle_17.Width) continue;
			if (newY < 0 || newY >= Puzzle_17.Height) continue;

			// we're valid. Let's add it.
			// account for zero indexing on timesMoved key
			results.Add(new List<Node>() { gridSpace[newX, newY, timesMoved, (int)direction] });
		}

		return results;
	}

	private static int GetDirectionXOffset(Direction direction)
	{
		switch (direction)
		{
			case Direction.Left:
				return -1;
			case Direction.Right:
				return 1;
		}
		return 0;
	}

	private static int GetDirectionYOffset(Direction direction)
	{
		switch (direction)
		{
			case Direction.Down:
				return -1;
			case Direction.Up:
				return 1;
		}
		return 0;
	}

	private static Direction GetOpposite(Direction dir)
	{
		switch (dir)
		{
			case Direction.Left:
				return Direction.Right;
			case Direction.Right:
				return Direction.Left;
			case Direction.Up:
				return Direction.Down;
			case Direction.Down:
				return Direction.Up;
		}

		return Direction.None;
	}

	internal class Part_2
	{
		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_17.txt");
			string[] input = File.ReadAllLines(path);

			Height = input.Length;
			Width = input[0].Length;
			var maxTimes = 10;
			var numDirections = 4;


			Node[,,,] gridSpace = new Node[Width, Height, maxTimes, numDirections];


			// map input to a grid
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					for (int t = 0; t < 10; t++)
					{
						foreach (Direction d in Enum.GetValues<Direction>())
						{
							if (d == Direction.None) continue;
							var realY = Height - y - 1;
							var cost = int.Parse(input[y][x].ToString());

							var tempNode = new Node()
							{
								X = x,
								Y = realY,
								ThisCost = cost,
								DirectionOfMovement = d,
								TimesMovedInThatDirection = t,
								CumulativeCost = -1,
							};

							gridSpace[x, realY, t, (int)d] = tempNode;
						}
					}
				}
			}

			// left is semi-arbitrary here -- it could also be up, for instance
			PriorityQueue<Node, int> seekerNodes = new();
			var startNode = gridSpace[0, Height - 1, 0, (int)Direction.Left];
			seekerNodes.Enqueue(gridSpace[0, Height - 1, 0, (int)Direction.Left], 1);
			// we want to consider all directions from our start
			startNode.DirectionOfMovement = Direction.None;
			startNode.CumulativeCost = 0;

			Node goalNode = null;

			while (seekerNodes.Count > 0)
			{
				// always grab the next best path
				// for the given direction
				var workingNode = seekerNodes.Dequeue();

				if (workingNode.X == Width - 1 && workingNode.Y == 0 && workingNode.TimesMovedInThatDirection >= 3)
				{
					// we reached the goal
					goalNode = workingNode;
					break;
				}
				// get all adjacent Nodes
				List<List<Node>> adjacents = GetHeadings(workingNode, gridSpace);

				foreach (var adjList in adjacents)
				{
					if (adjList.Count == 0) continue;

					var last = adjList.Last();

					var prospectiveCost = workingNode.CumulativeCost + adjList.Sum(n => n.ThisCost);

					var priorCumulativeCost = last.CumulativeCost;

					// if prospectiveCost is lower than the current cumulativecost, update
					if (prospectiveCost < priorCumulativeCost || priorCumulativeCost == -1)
					{
						last.PriorNode = workingNode;
						last.CumulativeCost = prospectiveCost;
						seekerNodes.Enqueue(last, prospectiveCost);

					}
					else
					{
						// this path isn't faster, so exit out
						continue;
					}
				}
			}

			var priorNode = goalNode;


			while (priorNode.PriorNode != null)
			{
				Console.WriteLine(priorNode.X + "," + priorNode.Y);
				priorNode = priorNode.PriorNode;
			}

			return goalNode.CumulativeCost;
		}
	}
}