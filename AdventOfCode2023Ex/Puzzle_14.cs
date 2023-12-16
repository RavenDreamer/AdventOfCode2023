using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_14
{
	public class SquareRock
	{
		public int XPos { get; set; }
		public int YPos { get; set; }

		// number of rocks resting against this strut
		public int BlockedRocks { get; set; }

		public int CalculatePlatformStress()
		{
			int rollingSum = 0;

			for (int i = 1; i <= BlockedRocks; i++)
			{
				rollingSum += (YPos - i);
			}

			return rollingSum;
		}
	}

	public class Strut
	{
		public int ID { get; set; }
		public int XPos { get; set; }
		public int YPos { get; set; }

		// number of rocks resting against this strut
		public int BlockedRocks { get; set; }

		public int CalculatePlatformStress()
		{
			int rollingSum = 0;

			for (int i = 0; i < BlockedRocks; i++)
			{
				rollingSum += (YPos - i);
			}

			return rollingSum;
		}

		public override string ToString()
		{
			return ID + "-" + XPos + "," + YPos + ":" + BlockedRocks;
		}
	}

	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_14.txt");
			string[] input = File.ReadAllLines(path);

			int height = input.Length;
			int width = input[0].Length;
			List<SquareRock> finalRocks = new();

			int heightConstant = height + 1;

			for (int x = 0; x < width; x++)
			{
				// metaphorical starting rock on the top northern edge
				SquareRock workingRock = new SquareRock() { YPos = heightConstant, XPos = x };
				for (int y = 0; y < height; y++)
				{
					// actual y = height - y
					// THIS NEEDS TO BE 1 indexed for the weight

					//if the character is a #, we start a new rock
					switch (input[y][x])
					{
						case '#':
							// save the current workingRock to the list and start a new one
							finalRocks.Add(workingRock);
							workingRock = new SquareRock() { XPos = x, YPos = height - y };
							break;
						case 'O':
							workingRock.BlockedRocks++;
							break;
						case '.':
							// nothing to do
							break;
						default:
							throw new Exception("malformed input");
					}
				}

				if (workingRock.BlockedRocks > 0)
				{
					finalRocks.Add(workingRock);
				}
			}

			return finalRocks.Sum(r => r.CalculatePlatformStress());
		}


	}

	internal class Part_2
	{
		//static int Height { get; set; }
		//static int Width { get; set; }

		static Dictionary<string, char[,]> tiltDict = new();
		static bool foundCache = false;
		static string tiltDir = "N";

		public static long Execute()
		{

			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_14.txt");
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

			List<Strut> struts = null;
			long firstCacheHit = -1;
			long secondCacheHit = -1;


			for (long i = 0; i < 120; i++)
			{
				////// print Gridspace for debugging
				//for (int y = gridSpace.GetLength(1) - 1; y >= 0; y--)
				//{

				//	string gridLine = "";

				//	for (int x = 0; x < gridSpace.GetLength(0); x++)
				//	{
				//		gridLine += gridSpace[x, y];

				//	}
				//	Console.WriteLine(gridLine);
				//}

				// tilt North
				tiltDir = "N";
				struts = Tilt(gridSpace);

				// find the answer
				var test = struts.Sum(r => r.CalculatePlatformStress());
				Console.WriteLine("answer:" + test);



				// Convert from struts to westward Grid
				gridSpace = RecreateRotateClockwise(struts, width, height);

				if (foundCache)
				{
					// if we found the north Tilt in the cache, that means we're in a cycle
					if (firstCacheHit == -1)
					{
						firstCacheHit = i;
						// clear the cache so we know exactly how long the cycle takes to repeat
						tiltDict.Clear();
						// and then re-add the one we just calculated to the cache
						string key = struts.Aggregate("", (a, b) => a + b.ToString() + "||" + tiltDir);
						tiltDict.Add(key, gridSpace);

					}
					else if (secondCacheHit == -1)
					{
						secondCacheHit = i;
						long cycleLength = secondCacheHit - firstCacheHit;

						// pattern = 
						// firstCacheHit + (iterCount - firstCacheHit) MOD cycleLength = the "equivalent" value?
						var modHalf = (1000000000 - firstCacheHit) % cycleLength;
						Console.WriteLine(firstCacheHit + "+" + modHalf);
					}
				}


				// tilt west
				tiltDir = "W";
				struts = Tilt(gridSpace);

				// find the answer
				test = struts.Sum(r => r.CalculatePlatformStress());
				if (test == 64)
				{
					Console.WriteLine("answer at:" + i);
				}

				// Convert from struts to southward Grid
				gridSpace = RecreateRotateClockwise(struts, height, width);

				// tilt south
				tiltDir = "S";
				struts = Tilt(gridSpace);

				// find the answer
				test = struts.Sum(r => r.CalculatePlatformStress());
				if (test == 64)
				{
					Console.WriteLine("answer at:" + i);
				}

				// Convert from struts to eastward Grid
				gridSpace = RecreateRotateClockwise(struts, width, height);

				// tilt east
				tiltDir = "E";
				struts = Tilt(gridSpace);

				// find the answer
				test = struts.Sum(r => r.CalculatePlatformStress());
				if (test == 64)
				{
					Console.WriteLine("answer at:" + i);
				}

				// Convert from struts back to northward Grid
				gridSpace = RecreateRotateClockwise(struts, height, width);

				int stress = CalculateStressFromGrid(gridSpace);
				Console.WriteLine("Stress: " + stress);
				if (stress == 64)
				{
					Console.WriteLine("HA~!");
				}

				long zz = struts.Sum(r => r.CalculatePlatformStress());
			}

			// need to tilt North 1 last time
			tiltDir = "N";
			struts = Tilt(gridSpace);

			//RecreateRotateClockwise(struts, width, height);

			return struts.Sum(r => r.CalculatePlatformStress());
		}

		private static int CalculateStressFromGrid(char[,] gridSpace)
		{
			int stress = 0;
			for (int y = gridSpace.GetLength(1) - 1; y >= 0; y--)
			{



				for (int x = 0; x < gridSpace.GetLength(0); x++)
				{
					if (gridSpace[x, y] == 'O')
					{
						stress += y + 1;
					}

				}
				//Console.WriteLine(gridLine);
			}
			return stress;
		}

		private static char[,] RecreateRotateClockwise(List<Strut> struts, int width, int height)
		{
			foundCache = false;
			// check the cache first.
			string key = struts.Aggregate("", (a, b) => a + b.ToString() + "||" + tiltDir);
			if (tiltDict.TryGetValue(key, out var tilt))
			{
				foundCache = true;
				return tilt;
			}

			// first, make a new grid
			char[,] workingGrid = new char[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					//initialize values
					workingGrid[x, y] = '.';
				}
			}

			// second, recreate the grid from the struts
			foreach (var s in struts)
			{
				if (s.YPos < height)
				{
					workingGrid[s.XPos, s.YPos] = '#';
				}
				for (int i = 1; i <= s.BlockedRocks; i++)
				{
					workingGrid[s.XPos, s.YPos - i] = 'O';
				}
			}
			//Console.WriteLine("");
			//Console.WriteLine("###");
			//Console.WriteLine("");

			//for (int y = workingGrid.GetLength(1) - 1; y >= 0; y--)
			//{

			//	string gridLine = "";

			//	for (int x = 0; x < workingGrid.GetLength(0); x++)
			//	{
			//		gridLine += workingGrid[x, y];

			//	}
			//	Console.WriteLine(gridLine);
			//}


			char[,] rotated = RotateGrid(width, height, workingGrid);
			tiltDict.Add(key, rotated);


			//Console.WriteLine("");
			//Console.WriteLine("###");
			//Console.WriteLine("");

			//for (int y = rotated.GetLength(1) - 1; y >= 0; y--)
			//{

			//	string gridLine = "";

			//	for (int x = 0; x < rotated.GetLength(0); x++)
			//	{
			//		gridLine += rotated[x, y];

			//	}
			//	Console.WriteLine(gridLine);
			//}

			return rotated;
		}

		private static T[,] RotateGrid<T>(int width, int height, T[,] workingGrid)
		{
			// now rotate the grid 90* clockwise into a new grid
			T[,] tiltedGrid = new T[height, width];
			for (int x = 0; x < width; x++)
			{
				var yPrime = width - 1 - x;
				for (int y = 0; y < height; y++)
				{
					tiltedGrid[y, yPrime] = workingGrid[x, y];
				}
			}

			return tiltedGrid;
		}

		private static List<Strut> Tilt(char[,] gridSpace)
		{
			int localWidth = gridSpace.GetLength(1);
			int localHeight = gridSpace.GetLength(0);

			int IDCounter = 0;
			List<Strut> finalRocks = new();

			for (int x = 0; x < localWidth; x++)
			{
				// metaphorical starting rock on the top northern edge
				Strut workingStrut = new Strut() { ID = IDCounter++, YPos = localHeight, XPos = x };
				for (int y = localHeight - 1; y >= 0; y--)
				{
					//if the character is a #, we start a new rock
					switch (gridSpace[x, y])
					{
						case '#':
							// save the current workingRock to the list and start a new one

							finalRocks.Add(workingStrut);

							workingStrut = new Strut() { ID = IDCounter++, XPos = x, YPos = y };
							break;
						case 'O':
							workingStrut.BlockedRocks++;
							break;
						case '.':
							// nothing to do
							break;
						default:
							throw new Exception("malformed input");
					}
				}


				finalRocks.Add(workingStrut);

			}

			return finalRocks;
		}
	}
}
