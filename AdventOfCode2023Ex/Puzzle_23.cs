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

	internal class GraphNode
	{
		internal Point2 Loc { get; set; }
		internal string Name { get; set; }

		internal List<Tuple<GraphNode, int>> Edges { get; set; } = new();

		internal void AddEdge(Tuple<GraphNode, int> par)
		{
			Edges.Add(par);
		}

		public GraphNode(string name, Point2 loc)
		{
			Name = name;
			Loc = loc;
		}

		public override string ToString()
		{
			return Name + ": " + Loc.X + ", " + Loc.Y;
		}

		internal int DistToNext(GraphNode next)
		{
			foreach (Tuple<GraphNode, int> par in Edges)
			{
				if (next == par.Item1)
				{
					return par.Item2;
				}
			}

			throw new Exception("Invalid node thingy");
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
							allNodes.Add(new DAGNode(new Point2(x, realY), "Node " + (allNodes.Count + 1)));
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
				if (node == endNode) continue;

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
			int longestPath = GetLongestPathToTarget(startNode, endNode);
			return longestPath - 1; // (since we moved the start up)

			// PART 2 BEGINS HERE
			// J/K, we're no longer a DAG. So butcher the DAG class back into regular ol' graphnodes.

			////List<GraphNode> nodes = new List<GraphNode>();
			////Dictionary<Point2, GraphNode> gnLookup = new();

			////GraphNode startGraphNode = null;
			////GraphNode endGraphNode = null;
			////foreach (DAGNode node in allNodes)
			////{
			////	var tempNode = new GraphNode(node.Name);

			////	if (node.Name == "Start Node")
			////	{
			////		startGraphNode = tempNode;
			////	}
			////	if (node.Name == "Goal Node")
			////	{
			////		endGraphNode = tempNode;
			////	}
			////	gnLookup[node.StartPos] = tempNode;
			////}

			////// all nodes exist, now add in directionality
			////foreach (DAGNode node in allNodes)
			////{
			////	var tempNode = gnLookup[node.StartPos];

			////	// end node has no outgoing edges
			////	if (node != endNode)
			////	{
			////		foreach (var e in node.Edges)
			////		{
			////			tempNode.AddEdge(new Tuple<GraphNode, int>(gnLookup[e.Key], e.Value));
			////		}
			////	}
			////	// start node has no incoming edges
			////	if (node != startNode)
			////	{
			////		foreach (var e in node.Edges)
			////		{
			////			gnLookup[e.Key].AddEdge(new Tuple<GraphNode, int>(tempNode, e.Value));
			////		}
			////	}
			////}


			////int longestPath = GetLongestPathToTarget(startGraphNode, endGraphNode, new HashSet<GraphNode>());
			////return longestPath; //longestPath;// - 1; // (since we moved the start up)

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

		private static int GetLongestPathToTarget(GraphNode startNode, GraphNode endNode, HashSet<GraphNode> visited)
		{
			if (startNode == endNode) return 0;

			if (visited.Contains(startNode)) return 0;
			visited.Add(startNode);


			int longestPath = 0;
			foreach (var pair in startNode.Edges)
			{
				var pathLength = pair.Item2;

				var prospectiveLength = pathLength + GetLongestPathToTarget(pair.Item1, endNode, new HashSet<GraphNode>(visited));

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
		static int Width { get; set; }
		static int Height { get; set; }

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_23.txt");
			string[] input = File.ReadAllLines(path);

			Width = input[0].Length;
			Height = input.Length;
			var pathGrid = new char[Width, Height];

			// map input to a list of GraphNodes
			List<GraphNode> allNodes = new();
			List<Point2> nodeLocs = new();

			// map input to a grid
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var realY = Height - y - 1;
					pathGrid[x, realY] = input[y][x];
				}
			}

			// replace 1st and last nodes with distinct chars
			pathGrid[1, Height - 1] = 'S';
			pathGrid[Width - 2, 0] = 'E';

			Dictionary<Point2, GraphNode> nodeLookup = new();

			GraphNode start = null;
			GraphNode end = null;

			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					var tempPoint = new Point2(x, y);
					switch (pathGrid[x, y])
					{
						case 'S':
							nodeLookup[tempPoint] = new GraphNode("Start Node", tempPoint);
							start = nodeLookup[tempPoint];
							break;
						case 'E':
							nodeLookup[tempPoint] = new GraphNode("Goal Node", tempPoint);
							end = nodeLookup[tempPoint];
							break;
						case '>':
						case 'v':
							nodeLookup[tempPoint] = new GraphNode("Node", tempPoint);
							break;
					}
				}
			}

			// calculate edges between our nodes
			foreach (var node in nodeLookup.Values)
			{

				var initialAdjPoints = GetAdjacentPoints(node.Loc, Width, Height);

				foreach (var adjPoint in initialAdjPoints)
				{
					HashSet<GraphNode> edgeCandidates = new();

					var visited = new HashSet<Point2>();
					visited.Add(node.Loc);

					// look along adjacent points until we find a non '.' coord
					int pathCost = 0;
					var adjPoints = new List<Point2>() { adjPoint };
					while (adjPoints.Count > 0)
					{
						var workAdj = adjPoints[0];
						// if we've been there, remove and continue;
						if (visited.Contains(adjPoints[0]))
						{
							adjPoints.RemoveAt(0);
							continue;
						}
						visited.Add(workAdj);
						// if we haven't been, check the character at that location
						switch (pathGrid[workAdj.X, workAdj.Y])
						{
							case '.':
								// valid square, add its adjacent points to adjPoints
								adjPoints.AddRange(GetAdjacentPoints(workAdj, Width, Height));
								adjPoints.RemoveAt(0);
								pathCost++;
								continue;
							case '#':
								// invalid square, remove and continue;
								adjPoints.RemoveAt(0);
								continue;
							default:
								// we found a >, v, S, or E
								edgeCandidates.Add(nodeLookup[new Point2(workAdj.X, workAdj.Y)]);
								adjPoints.RemoveAt(0);
								continue;
						}
					}


					// set edge length to visited.Length - 1 (because we added in the start node
					foreach (var ec in edgeCandidates)
					{
						node.AddEdge(new Tuple<GraphNode, int>(ec, pathCost));
					}
				}
			}

			// start from 19,3
			var allPaths = FindPathsToNode(start, end, new HashSet<GraphNode>());

			int longestPath = CalculatePathLengths(allPaths);

			return longestPath;
		}

		private static int CalculatePathLengths(List<List<GraphNode>> allPaths)
		{
			int longestPath = 0;
			foreach (var path in allPaths)
			{
				int pathLength = 0;
				for (int i = 0; i < path.Count - 1; i++)
				{
					var current = path[i];
					var next = path[i + 1];
					var distToNext = current.DistToNext(next);
					pathLength = pathLength + 1 + distToNext;
				}

				if (pathLength > longestPath)
				{
					// Console.WriteLine("Found new longest path:")
					longestPath = pathLength;
				}
			}

			return longestPath;
		}

		private static int GetLongestPathToTarget(GraphNode startNode, GraphNode endNode, HashSet<GraphNode> visited)
		{
			if (startNode == endNode) return 1;

			visited.Add(startNode);

			int longestPath = 0;
			foreach (var pair in startNode.Edges)
			{
				if (visited.Contains(pair.Item1)) continue;

				var pathLength = pair.Item2;

				var prospectiveLength = 1 + pathLength + GetLongestPathToTarget(pair.Item1, endNode, new HashSet<GraphNode>(visited));

				if (prospectiveLength > longestPath)
				{
					longestPath = prospectiveLength;
				}
			}

			return longestPath;
		}

		private static List<List<GraphNode>> FindPathsToNode(GraphNode startNode, GraphNode endNode, HashSet<GraphNode> visited)
		{
			List<List<GraphNode>> result = new List<List<GraphNode>>();

			if (startNode == endNode)
			{
				var goalPath = new List<GraphNode>();
				goalPath.Add(startNode);
				result.Add(goalPath);
				return result;
			}

			visited.Add(startNode);
			// recurse
			foreach (var pair in startNode.Edges)
			{
				if (visited.Contains(pair.Item1)) continue;

				// because of the way the graph is constructed, these edges are exclusive with each other.
				var cloneVisited = new HashSet<GraphNode>(visited);
				// so add in the "other" edges"
				foreach (var subPair in startNode.Edges)
				{
					if (subPair.Item1 == pair.Item1) continue;
					cloneVisited.Add(subPair.Item1);
				}

				var resultPaths = FindPathsToNode(pair.Item1, endNode, cloneVisited);
				foreach (var p in resultPaths)
				{
					p.Add(startNode);
					result.Add(p);
				}
			}

			return result;
		}

		private static List<Point2> GetAdjacentPoints(Point2 pos, int width, int height)
		{
			List<Point2> adjPoints = new List<Point2>()
			{
				 new Point2(pos.X+1, pos.Y),
				 new Point2(pos.X-1, pos.Y),
				 new Point2(pos.X, pos.Y+1),
				 new Point2(pos.X, pos.Y-1),
			};

			List<Point2> results = new();
			foreach (Point2 p in adjPoints)
			{
				// remove points that are out of bounds
				if (p.X < 0 || p.X >= Width || p.Y < 0 || p.Y >= Height)
				{
					// do not add to output
					continue;
				}
				results.Add(p);
			}

			return results;
		}
	}
}