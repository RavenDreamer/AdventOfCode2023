using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_18
{
	public enum Direction
	{
		None = -1,
		Left = 0,
		Right = 1,
		Up = 2,
		Down = 3,

	}

	internal class Pair
	{
		public long X { get; set; }
		public long Y { get; set; }

		public Pair(long x, long y)
		{
			X = x;
			Y = y;
		}
	}

	internal class Edge
	{

		internal long StartX { get; set; }
		internal long StartY { get; set; }

		internal long EndX { get; set; }
		internal long EndY { get; set; }
		internal Direction Direction { get; set; }
		internal Edge PriorEdge { get; set; }
		internal Edge NextEdge { get; set; }

		internal string EdgeColor { get; set; } = "";

		public long GetDistance()
		{
			var xDiff = EndX - StartX;
			if (xDiff < 0) xDiff *= -1;
			var yDiff = EndY - StartY;
			if (yDiff < 0) yDiff *= -1;

			// this is an artefact of how edgeOrphans 
			// are calculated
			// just need to subtract 1, where doesn't matter
			if (Direction == Direction.None) xDiff--;

			return xDiff + yDiff;
		}

		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			var other = obj as Edge;

			return this.StartX == other.StartX &&
				this.StartY == other.StartY &&
				this.EndX == other.EndX &&
				this.EndY == other.EndY &&
				this.Direction == other.Direction;
		}

	}


	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\debug.txt");
			string[] input = File.ReadAllLines(path);

			List<Edge> edgeList = new();
			Edge priorEdge = null;
			// start at 0,0
			int rollingX = 0;
			int rollingY = 0;
			foreach (var s in input)
			{
				var initialX = rollingX;
				var initialY = rollingY;

				// each line has 3 parts, a Direction, a distance, and a Hex code
				var stringBits = s.Split(' ');

				var edgeDir = GetDirectionFromChar(stringBits[0]);
				var dist = int.Parse(stringBits[1]);
				switch (edgeDir)
				{
					case Direction.Left:
						rollingX += -dist;
						break;
					case Direction.Right:
						rollingX += dist;
						break;
					case Direction.Up:
						rollingY += dist;
						break;
					case Direction.Down:
						rollingY += -dist;
						break;
				}

				var newEdge = new Edge()
				{
					Direction = edgeDir,
					StartX = initialX,
					StartY = initialY,
					EndX = rollingX,
					EndY = rollingY,
					PriorEdge = priorEdge,
				};
				edgeList.Add(newEdge);
				priorEdge = newEdge;
			}

			// assign last Edge as the priorEdge to the first Edge
			edgeList[0].PriorEdge = priorEdge;

			int xOffset = 0;
			int yOffset = 0;

			List<Pair> verts = new();
			// set the forward looking edges too
			foreach (Edge edge in edgeList)
			{
				edge.PriorEdge.NextEdge = edge;

				var isConvex = IsConvexAngle(edge.Direction, edge.PriorEdge.Direction);

				if (isConvex)
				{
					// top left
					if (edge.Direction == Direction.Right)
					{
						xOffset = 0;
						yOffset = 0;
					}
					//bottom right
					if (edge.Direction == Direction.Left)
					{
						xOffset = 1;
						yOffset = -1;
					}
					// top right
					if (edge.Direction == Direction.Down)
					{
						xOffset = 1;
						yOffset = 0;
					}
					// bottom right
					if (edge.Direction == Direction.Up)
					{
						xOffset = 0;
						yOffset = -1;
					}
				}
				else
				{
					// top right
					if (edge.Direction == Direction.Right)
					{
						xOffset = 1;
						yOffset = 0;
					}
					// bottom left
					if (edge.Direction == Direction.Left)
					{
						xOffset = 0;
						yOffset = -1;
					}
					// bottom right
					if (edge.Direction == Direction.Down)
					{
						xOffset = 1;
						yOffset = -1;
					}
					// top left
					if (edge.Direction == Direction.Up)
					{
						xOffset = 0;
						yOffset = 0;
					}
				}

				var nextVert = new Pair(edge.StartX + xOffset, edge.StartY + yOffset);
				verts.Add(nextVert);
			}

			// Greene's theorem
			long cumulativeArea = 0;
			for (int i = 0; i < verts.Count - 1; i++)
			{
				var j = (i + 1) % verts.Count;
				cumulativeArea = cumulativeArea + (verts[i].X * verts[j].Y);
				cumulativeArea = cumulativeArea - (verts[i].Y * verts[j].X);
			}

			return cumulativeArea / 2;
		}

		private static List<Edge> GenerateSuccessor(List<Edge> edgeList, List<Edge> orphans)
		{
			// #######
			// #.....#
			// ###...#
			// ..#####
			var list = GetDebug();
			var output = new List<Edge>();

			Edge priorEdgePrime = null;
			for (int i = 0; i < list.Count; i++)
			{
				var workingEdge = list[i];
				// generate an interior edge
				var workingEdgePrime = GetInteriorEdge(workingEdge);

				if (IsConvexAngle(workingEdge.Direction, workingEdge.PriorEdge.Direction))
				{
					// if the previous corner was convex, start is one shorter (1 along the direction)
					switch (workingEdgePrime.Direction)
					{
						case Direction.Right:
							workingEdgePrime.StartX++;
							break;
						case Direction.Left:
							workingEdgePrime.StartX--;
							break;
						case Direction.Up:
							workingEdgePrime.StartY++;
							break;
						case Direction.Down:
							workingEdgePrime.StartY--;
							break;
					}
				}
				else
				{
					// if the previous corner was concave, start is one longer (1 opposite the direction)
					switch (workingEdgePrime.Direction)
					{
						case Direction.Right:
							workingEdgePrime.StartX--;
							break;
						case Direction.Left:
							workingEdgePrime.StartX++;
							break;
						case Direction.Up:
							workingEdgePrime.StartY--;
							break;
						case Direction.Down:
							workingEdgePrime.StartY++;
							break;
					}
				}
				// repeat for the other endpoint
				if (IsConvexAngle(workingEdge.NextEdge.Direction, workingEdge.Direction))
				{
					// if the next corner was convex, end is one shorter (1 opposite the direction)
					switch (workingEdgePrime.Direction)
					{
						case Direction.Right:
							workingEdgePrime.EndX--;
							break;
						case Direction.Left:
							workingEdgePrime.EndX++;
							break;
						case Direction.Up:
							workingEdgePrime.EndY--;
							break;
						case Direction.Down:
							workingEdgePrime.EndY++;
							break;
					}
				}
				else
				{
					// if the next corner was concave, end is one longer (1 along the direction)
					switch (workingEdgePrime.Direction)
					{
						case Direction.Right:
							workingEdgePrime.EndX++;
							break;
						case Direction.Left:
							workingEdgePrime.EndX--;
							break;
						case Direction.Up:
							workingEdgePrime.EndY++;
							break;
						case Direction.Down:
							workingEdgePrime.EndY--;
							break;
					}
				}

				workingEdgePrime.PriorEdge = priorEdgePrime;
				priorEdgePrime = workingEdgePrime;

				output.Add(workingEdgePrime);
			}

			// assign last Edge as the priorEdge to the first Edge
			output[0].PriorEdge = priorEdgePrime;

			// set the forward looking edges too
			foreach (Edge edge in output)
			{
				edge.PriorEdge.NextEdge = edge;
			}

			int currentIndex = 0;
			int maxIndex = output.Count;
			// now that we have the successor list we need to remove points (start & end are the same)
			// we do this by fusing the prior and next edges to one another
			while (currentIndex < maxIndex)
			{
				Edge edge = output[currentIndex];

				// this is a point, and we need to remove it
				if (edge.GetDistance() == 0)
				{
					output.RemoveAt(currentIndex);
					currentIndex--;
					maxIndex--;

					edge.PriorEdge.NextEdge = edge.NextEdge;
					edge.NextEdge.PriorEdge = edge.PriorEdge;

					// now we need to trim the edges to split the orphan
					// where we have overlapping opposite directions

					// form two new Edges. The continuous perimeter
					// edge (which starts from PriorEdge.Start and goes to
					// NextEdge.End in the direction of NextEdge) and the orphan (which starts from NextEdge.Start
					// and goes to PriorEdge.Start). ((PriorEdge.End and NextEdge.Start 
					// should be the same point))
					var replacementEdge = new Edge()
					{
						PriorEdge = edge.PriorEdge.PriorEdge,
						NextEdge = edge.NextEdge.NextEdge,
						Direction = edge.NextEdge.Direction,
						StartX = edge.PriorEdge.StartX,
						StartY = edge.PriorEdge.StartY,
						EndX = edge.NextEdge.EndX,
						EndY = edge.NextEdge.EndY,
					};
					var orphanEdge = new Edge()
					{
						Direction = Direction.None,
						StartX = edge.NextEdge.StartX,
						StartY = edge.NextEdge.StartY,
						EndX = edge.PriorEdge.StartX,
						EndY = edge.PriorEdge.StartY,
					};
					// we decremented after removing the point, so this
					// replaces point.PrevEdge
					output[currentIndex] = replacementEdge;
					output.Remove(edge.NextEdge);

					currentIndex--;
					maxIndex--;

					orphans.Add(orphanEdge);
				}

				currentIndex++;
			}

			return output;
		}

		private static bool IsConvexAngle(Direction current, Direction previous)
		{
			switch (current)
			{
				case Direction.Right:
					return previous == Direction.Up;
				case Direction.Left:
					return previous == Direction.Down;
				case Direction.Up:
					return previous == Direction.Left;
				case Direction.Down:
					return previous == Direction.Right;
				default:
					throw new Exception("Impossible to get here");
			}
		}


		private static Edge GetInteriorEdge(Edge workingEdge)
		{
			switch (workingEdge.Direction)
			{
				case Direction.Right:
					// y - 1
					return new Edge()
					{
						Direction = Direction.Right,
						StartX = workingEdge.StartX,
						StartY = workingEdge.StartY - 1,
						EndX = workingEdge.EndX,
						EndY = workingEdge.EndY - 1,
					};

				case Direction.Left:
					// y + 1
					return new Edge()
					{
						Direction = Direction.Left,
						StartX = workingEdge.StartX,
						StartY = workingEdge.StartY + 1,
						EndX = workingEdge.EndX,
						EndY = workingEdge.EndY + 1,
					};
				case Direction.Up:
					// x + 1
					return new Edge()
					{
						Direction = Direction.Up,
						StartX = workingEdge.StartX + 1,
						StartY = workingEdge.StartY,
						EndX = workingEdge.EndX + 1,
						EndY = workingEdge.EndY,
					};
				case Direction.Down:
					// x - 1
					return new Edge()
					{
						Direction = Direction.Down,
						StartX = workingEdge.StartX - 1,
						StartY = workingEdge.StartY,
						EndX = workingEdge.EndX - 1,
						EndY = workingEdge.EndY,
					};
				default:
					// nothing to improve
					return null;
			}
		}

		private static List<Edge> GetDebug()
		{
			// #######
			// #.....#
			// ###...#
			// ..#####
			List<Edge> debugList = new();
			debugList.Add(new Edge()
			{
				Direction = Direction.Right,
				StartX = 0,
				StartY = 3,
				EndX = 6,
				EndY = 3,
			});
			debugList.Add(new Edge()
			{
				Direction = Direction.Down,
				StartX = 6,
				StartY = 3,
				EndX = 6,
				EndY = 0,
			});
			debugList.Add(new Edge()
			{
				Direction = Direction.Left,
				StartX = 6,
				StartY = 0,
				EndX = 2,
				EndY = 0,
			});
			debugList.Add(new Edge()
			{
				Direction = Direction.Up,
				StartX = 2,
				StartY = 0,
				EndX = 2,
				EndY = 1,
			});
			debugList.Add(new Edge()
			{
				Direction = Direction.Left,
				StartX = 2,
				StartY = 1,
				EndX = 0,
				EndY = 1,
			});
			debugList.Add(new Edge()
			{
				Direction = Direction.Up,
				StartX = 0,
				StartY = 1,
				EndX = 0,
				EndY = 3,
			});
			Edge priorEdge = null;
			foreach (Edge e in debugList)
			{
				e.PriorEdge = priorEdge;
				priorEdge = e;
			}
			debugList[0].PriorEdge = priorEdge;

			// set the forward looking edges too
			foreach (Edge edge in debugList)
			{
				edge.PriorEdge.NextEdge = edge;
			}

			return debugList;
		}

		private static Direction GetDirectionFromChar(string v)
		{
			switch (v)
			{
				case "L":
					return Direction.Left;
				case "R":
					return Direction.Right;
				case "U":
					return Direction.Up;
				case "D":
					return Direction.Down;
				default:
					throw new Exception("can't get here");
			}
		}
	}

	internal class Part_2
	{
		private static bool IsConvexAngle(Direction current, Direction previous)
		{
			switch (current)
			{
				case Direction.Right:
					return previous == Direction.Up;
				case Direction.Left:
					return previous == Direction.Down;
				case Direction.Up:
					return previous == Direction.Left;
				case Direction.Down:
					return previous == Direction.Right;
				default:
					throw new Exception("Impossible to get here");
			}
		}

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_18.txt");
			string[] input = File.ReadAllLines(path);

			List<Edge> edgeList = new();
			Edge priorEdge = null;
			// start at 0,0
			long rollingX = 0;
			long rollingY = 0;
			foreach (var s in input)
			{
				var initialX = rollingX;
				var initialY = rollingY;

				// each line has 3 parts, a Direction, a distance, and a Hex code
				var stringBits = s.Split(' ');
				// for part 2, we only care about the hex code

				var rawHex = stringBits[2];
				// trim parens
				rawHex = rawHex.Trim('(');
				rawHex = rawHex.Trim(')');

				// parse the first 5 as a distance
				int hexDistance = int.Parse(rawHex.Substring(1, 5), System.Globalization.NumberStyles.HexNumber);

				// parse the last 1 as a direction
				Direction edgeDir;
				switch (rawHex[^1])
				{
					case '0':
						edgeDir = Direction.Right;
						break;
					case '1':
						edgeDir = Direction.Down;
						break;
					case '2':
						edgeDir = Direction.Left;
						break;
					case '3':
						edgeDir = Direction.Up;
						break;
					default:
						throw new Exception("invalid input");
				}

				switch (edgeDir)
				{
					case Direction.Left:
						rollingX += -hexDistance;
						break;
					case Direction.Right:
						rollingX += hexDistance;
						break;
					case Direction.Up:
						rollingY += hexDistance;
						break;
					case Direction.Down:
						rollingY += -hexDistance;
						break;
				}

				var newEdge = new Edge()
				{
					Direction = edgeDir,
					EdgeColor = stringBits[2],
					StartX = initialX,
					StartY = initialY,
					EndX = rollingX,
					EndY = rollingY,
					PriorEdge = priorEdge,
				};
				edgeList.Add(newEdge);
				priorEdge = newEdge;
			}

			// assign last Edge as the priorEdge to the first Edge
			edgeList[0].PriorEdge = priorEdge;

			int xOffset = 0;
			int yOffset = 0;

			List<Pair> verts = new();
			// set the forward looking edges too
			foreach (Edge edge in edgeList)
			{
				edge.PriorEdge.NextEdge = edge;

				var isConvex = IsConvexAngle(edge.Direction, edge.PriorEdge.Direction);

				if (isConvex)
				{
					// top left
					if (edge.Direction == Direction.Right)
					{
						xOffset = 0;
						yOffset = 0;
					}
					//bottom right
					if (edge.Direction == Direction.Left)
					{
						xOffset = 1;
						yOffset = -1;
					}
					// top right
					if (edge.Direction == Direction.Down)
					{
						xOffset = 1;
						yOffset = 0;
					}
					// bottom right
					if (edge.Direction == Direction.Up)
					{
						xOffset = 0;
						yOffset = -1;
					}
				}
				else
				{
					// top right
					if (edge.Direction == Direction.Right)
					{
						xOffset = 1;
						yOffset = 0;
					}
					// bottom left
					if (edge.Direction == Direction.Left)
					{
						xOffset = 0;
						yOffset = -1;
					}
					// bottom right
					if (edge.Direction == Direction.Down)
					{
						xOffset = 1;
						yOffset = -1;
					}
					// top left
					if (edge.Direction == Direction.Up)
					{
						xOffset = 0;
						yOffset = 0;
					}
				}

				var nextVert = new Pair(edge.StartX + xOffset, edge.StartY + yOffset);
				verts.Add(nextVert);
			}

			// Greene's theorem
			long cumulativeArea = 0;
			for (int i = 0; i < verts.Count - 1; i++)
			{
				var j = (i + 1) % verts.Count;
				cumulativeArea = cumulativeArea + (verts[i].X * verts[j].Y);
				cumulativeArea = cumulativeArea - (verts[i].Y * verts[j].X);
			}

			return cumulativeArea / 2;
		}
	}
}