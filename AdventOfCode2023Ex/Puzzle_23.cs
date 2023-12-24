using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_23
{
	internal struct Point2
	{
		private readonly int x, y;

		public int X { get { return x; } }
		public int Y { get { return y; } }

		public Point2(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public override int GetHashCode()
		{
			return 31 * x + 17 * y; // Or something like that
		}

		public override bool Equals(object obj)
		{
			return obj is Point2 point && Equals(point);
		}

		public bool Equals(Point2 p)
		{
			return x == p.x && y == p.y;
		}
	}

	internal class DAGNode
	{
		// The Point2 is the endPosition
		// which we can use to look up other DAGs
		public Dictionary<Point2, int> Edges { get; set; } = new();
		public Point2 StartPos { get; set; }

		public string Name { get; set; }

		public DAGNode(Point2 start, string name)
		{
			StartPos = start;
			Name = name;
		}

		public void AddEdge(Point2 edgeEndPos, int edgeDist)
		{
			Edges[edgeEndPos] = edgeDist;
		}
	}

	internal class Edge
	{
		public Point2 StartPos { get; set; }
		public Point2 EndPos { get; set; }
		public int Length { get; set; }

		public Edge(Point2 start, Point2 end, int length)
		{
			StartPos = start;
			EndPos = end;
			Length = length;
		}
	}

	internal class Part_1
	{
		static int Width { get; set; }
		static int Height { get; set; }

		static Dictionary<Point2, DAGNode> nodeLookup = new();

		static Dictionary<Point2, HashSet<Edge>> edgeLookup = new();

		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\debug.txt");
			string[] input = File.ReadAllLines(path);

			Width = input[0].Length;
			Height = input.Length;
			var pathGrid = new char[Width, Height];

			// map input to a list of DAGNodes
			List<DAGNode> allNodes = new();


			// map input to a grid
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var realY = Height - y - 1;
					pathGrid[x, realY] = input[y][x];
					switch (input[y][x])
					{
						case '^':
						case '>':
						case '<':
						case 'v':
							// create a new DAG node that starts from here
							allNodes.Add(new DAGNode(new Point2(x, realY), "Node " + allNodes.Count + 1));
							break;
					}
				}
			}
			// create a pseudo DAGNode that starts at <1,Height>
			var startNode = new DAGNode(new Point2(1, Height), "Start Node");
			allNodes.Add(startNode);

			// create a pseudo DAGNode that starts at <Width-2, 0>
			var endNode = new DAGNode(new Point2(Width - 2, 0), "Goal Node");
			allNodes.Add(endNode);

			// traverse the grid, finding edges for each DAG node
			foreach (DAGNode node in allNodes)
			{
				HashSet<Point2> traversedPoints = new();
				Point2 lastPos = node.StartPos;

				bool isTraversing = true;
				while (isTraversing)
				{
					var validNeighbors = GetAdjacentPositions(lastPos, traversedPoints, pathGrid);
					switch (validNeighbors.Count)
					{
						case 0:
							// if we have ZERO options, we're at the goal node
							node.AddEdge(new Point2(Width - 2, 0), traversedPoints.Count);
							isTraversing = false;
							break;
						case 1:
							var tempPoint = validNeighbors[0];
							// only 1 option, so go down that path
							traversedPoints.Add(tempPoint);
							lastPos = tempPoint;

							// if the char at this position is V or >, that's the end of the edge
							if (pathGrid[tempPoint.X, tempPoint.Y] == 'v' || pathGrid[tempPoint.X, tempPoint.Y] == '>')
							{
								node.AddEdge(tempPoint, traversedPoints.Count);
								isTraversing = false;
							}
							break;
						case 2:
							// we are at a branch path, so add both points
							// (input data only has > and v so only a maximum of 2 options
							foreach (Point2 subP in validNeighbors)
							{
								node.AddEdge(subP, traversedPoints.Count + 1);
							}
							isTraversing = false;
							break;
					}
				}

				nodeLookup[node.StartPos] = node;
			}

			// PART 1 SOLUTION HERE
			// int longestPath = GetLongestPathToTarget(startNode, endNode);
			// return longestPath -1 // (since we moved the start up)

			// PART 2 BEGINS HERE
			// J/K, we're no longer a DAG. So butcher the DAG class back into regular ol' graphnodes.

			Dictionary<Point2, DAGNode> graphNodeDict = new();
			foreach (DAGNode node in allNodes)
			{
				graphNodeDict[node.StartPos] = new DAGNode(node.StartPos, "Node " + graphNodeDict.Count);
			}

			// now that it's initialized, add in the bidirectionality
			foreach (DAGNode node in allNodes)
			{
				// duplicate edges
				foreach (var e in node.Edges)
				{
					graphNodeDict[node.StartPos].AddEdge(e.Key, e.Value);
				}

				// set up reverse edges
				foreach (var e in node.Edges)
				{
					graphNodeDict[e.Key].AddEdge(node.StartPos, e.Value);
				}
			}

			int longestPath = GetPart2LongestPath(graphNodeDict[new Point2(1, Height)], graphNodeDict[new Point2(0, Width - 2)], graphNodeDict, new HashSet<Point2>());
			return longestPath;// - 1; // (since we moved the start up)

			//////List<Edge> allEdges = new();
			//////Edge startEdge;
			//////foreach (var node in allNodes)
			//////{
			//////	foreach (var pair in node.Edges)
			//////	{
			//////		allEdges.Add(new Edge(node.StartPos, pair.Key, pair.Value));
			//////	}
			//////}


			//////// make a dictionary to get all edges for a given point.
			//////edgeLookup = new();
			//////foreach (var edge in allEdges)
			//////{
			//////	if (edgeLookup.ContainsKey(edge.StartPos))
			//////	{
			//////		edgeLookup[edge.StartPos].Add(edge);
			//////	}
			//////	else
			//////	{
			//////		edgeLookup[edge.StartPos] = new();
			//////		edgeLookup[edge.StartPos].Add(edge);
			//////	}

			//////	if (edgeLookup.ContainsKey(edge.EndPos))
			//////	{
			//////		edgeLookup[edge.EndPos].Add(edge);
			//////	}
			//////	else
			//////	{
			//////		edgeLookup[edge.EndPos] = new();
			//////		edgeLookup[edge.EndPos].Add(edge);
			//////	}
			//////}

			//////Point2 startPos = new Point2(1, Height);
			//////Point2 endPos = new Point2(Width - 2, 0);
			//////var visited = new HashSet<Point2>();

			//////int longLength = GetLongestAllDirectionalPathToTarget(startPos, endPos, 0, visited);


			// return longLength;
		}

		private static int GetLongestAllDirectionalPathToTarget(Point2 startPos, Point2 goalPos, int cumulativeLength, HashSet<Point2> visitedNodes)
		{
			if (startPos.Equals(goalPos)) return cumulativeLength;

			// abort out if this would cause a cycle;
			if (visitedNodes.Contains(startPos)) return 0;

			visitedNodes.Add(startPos);
			var relevantEdges = edgeLookup[startPos];

			int longestPath = 0;
			foreach (var edge in relevantEdges)
			{
				// for each edge, duplicate the visitedNodes list and
				// recurse
				HashSet<Point2> recurseVisited = new HashSet<Point2>(visitedNodes);

				// use whichever point isn't equal to startPos
				if (edge.StartPos.Equals(startPos))
				{
					// use endPos
					var prospectiveLength = GetLongestAllDirectionalPathToTarget(edge.EndPos, goalPos, cumulativeLength + edge.Length, recurseVisited);
					if (prospectiveLength > longestPath)
					{
						longestPath = prospectiveLength;
					}
				}
				else
				{
					// use startPos
					var prospectiveLength = GetLongestAllDirectionalPathToTarget(edge.StartPos, goalPos, cumulativeLength + edge.Length, recurseVisited);
					if (prospectiveLength > longestPath)
					{
						longestPath = prospectiveLength;
					}
				}
			}

			return longestPath;
		}

		private static int GetLongestPathToTarget(DAGNode startNode, DAGNode endNode)
		{
			if (startNode == endNode) return 0;

			int longestPath = 0;
			foreach (var pair in startNode.Edges)
			{
				var pathLength = pair.Value;

				var prospectiveLength = pathLength + GetLongestPathToTarget(nodeLookup[pair.Key], endNode);

				if (prospectiveLength > longestPath)
				{
					longestPath = prospectiveLength;
				}
			}

			return longestPath;
		}

		private static int GetPart2LongestPath(DAGNode startNode, DAGNode endNode, Dictionary<Point2, DAGNode> graphNodes, HashSet<Point2> visitedNodes)
		{
			if (startNode == endNode) return 0;

			visitedNodes.Add(startNode.StartPos);

			var clonedVisited = new HashSet<Point2>(visitedNodes);

			int longestPath = 0;
			foreach (var pair in startNode.Edges)
			{
				var pathLength = pair.Value;

				var prospectiveLength = pathLength + GetPart2LongestPath(graphNodes[pair.Key], endNode, graphNodes, clonedVisited);

				if (prospectiveLength > longestPath)
				{
					longestPath = prospectiveLength;
				}
			}

			return longestPath;
		}

		private static List<Point2> GetAdjacentPositions(Point2 pos, HashSet<Point2> priorPos, char[,] pathGrid)
		{
			if (pos.Y == Height || pathGrid[pos.X, pos.Y] == 'v')
			{
				// only allow down, always valid
				return new List<Point2>() { new Point2(pos.X, pos.Y - 1) };
			}

			if (pathGrid[pos.X, pos.Y] == '>')
			{
				// only allow right, always valid
				return new List<Point2>() { new Point2(pos.X + 1, pos.Y) };
			}

			// gets valid positions for the next step from this position
			List<Point2> adjPoints = new List<Point2>()
			{
				 new Point2(pos.X+1, pos.Y),
				 new Point2(pos.X-1, pos.Y),
				 new Point2(pos.X, pos.Y+1),
				 new Point2(pos.X, pos.Y-1),
			};

			List<Point2> outPoints = new();

			foreach (Point2 p in adjPoints)
			{
				// remove points that are out of bounds
				if (p.X < 0 || p.X >= Width || p.Y < 0 || p.Y >= Height)
				{
					// do not add to output
					continue;
				}
				// remove points that are in the prior path
				if (priorPos.Contains(p))
				{
					// do not add to output
					continue;
				}
				// remove points that go off the '.' path
				if (pathGrid[p.X, p.Y] == '#')
				{
					// forest, so invalid tile
					// do not add to output
					continue;
				}
				// if moving left, don't allow >
				if (p.X < pos.X && pathGrid[p.X, p.Y] == '>')
				{
					continue;
				}

				// if moving right, don't allow <
				if (p.X > pos.X && pathGrid[p.X, p.Y] == '<')
				{
					continue;
				}

				// if moving up, don't allow v
				if (p.Y > pos.Y && pathGrid[p.X, p.Y] == 'v')
				{
					continue;
				}

				// if movind down, don't allow ^
				if (p.Y < pos.Y && pathGrid[p.X, p.Y] == '^')
				{
					continue;
				}

				outPoints.Add(p);
			}

			return outPoints;
		}
	}

	internal class Part_2
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_22.txt");
			string[] input = File.ReadAllLines(path);

			return 0;
		}


	}
}