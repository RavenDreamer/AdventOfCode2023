using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_11
{

	internal struct Position
	{
		public int X { get; set; }
		public int Y { get; set; }
		public Position()
		{
			X = 0;
			Y = 0;
		}
	}

	internal class Galaxy
	{
		public int ID { get; set; }
		public Position Position { get; set; }

		public string GetPairName(Galaxy other)
		{
			if (this.ID < other.ID)
			{
				return this.ID + "," + other.ID;
			}
			else
			{
				return other.ID + "," + this.ID;
			}

		}
	}

	internal class Part_1
	{
		const long DIST_MULT = 1000000;

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_11.txt");
			string[] input = File.ReadAllLines(path);

			HashSet<int> wideColumns = new();
			HashSet<int> tallRows = new();
			List<Galaxy> galaxies = new();

			int width = input[0].Length;
			int height = input.Length;


			for (int x = 0; x < width; x++)
			{
				var yEmpty = true;

				for (int y = 0; y < height; y++)
				{
					var charQuery = input[y][x];

					if (charQuery == '#')
					{
						yEmpty = false;

						var galaxy = new Galaxy();
						galaxy.ID = galaxies.Count;
						galaxy.Position = new Position() { X = x, Y = height - 1 - y };

						galaxies.Add(galaxy);
					}
				}

				if (yEmpty == true)
				{
					wideColumns.Add(x);
				}
			}

			// this isn't optimally efficient, but we need to look for tallRows so repeat w/o
			// adding galaxies this time
			for (int y = 0; y < height; y++)
			{
				var xEmpty = true;

				for (int x = 0; x < width; x++)
				{
					var charQuery = input[y][x];

					if (charQuery == '#')
					{
						xEmpty = false;
					}
				}

				if (xEmpty == true)
				{
					tallRows.Add(height - 1 - y);
				}
			}

			List<List<long>> pathDist = new();
			HashSet<string> addedPairs = new();
			foreach (var galaxy in galaxies)
			{
				List<long> shortestPaths = new();
				foreach (var subGalaxy in galaxies)
				{
					if (galaxy.ID == subGalaxy.ID) continue;
					if (addedPairs.Contains(galaxy.GetPairName(subGalaxy)))
					{
						// already handled this pairing
						continue;
					}

					// manhatten distance as a base
					long manDist = Math.Abs(galaxy.Position.X - subGalaxy.Position.X) + Math.Abs(galaxy.Position.Y - subGalaxy.Position.Y);

					// any tall rows (y values) between the two get a bonus
					// any wide columns (x values) between the two get a bonus
					long bonusDist = 0;
					var lowerXBound = galaxy.Position.X < subGalaxy.Position.X ? galaxy.Position.X : subGalaxy.Position.X;
					var upperXBound = galaxy.Position.X > subGalaxy.Position.X ? galaxy.Position.X : subGalaxy.Position.X;
					foreach (int coord in wideColumns)
					{
						if (coord >= lowerXBound && upperXBound >= coord)
						{
							bonusDist++;
						}
					}

					var lowerYBound = galaxy.Position.Y < subGalaxy.Position.Y ? galaxy.Position.Y : subGalaxy.Position.Y;
					var upperYBound = galaxy.Position.Y > subGalaxy.Position.Y ? galaxy.Position.Y : subGalaxy.Position.Y;
					foreach (int coord in tallRows)
					{
						if (coord >= lowerYBound && upperYBound >= coord)
						{
							bonusDist++;
						}
					}

					shortestPaths.Add(manDist + (bonusDist * (DIST_MULT - 1)));
					addedPairs.Add(galaxy.GetPairName(subGalaxy));
				}
				pathDist.Add(shortestPaths);
			}

			return pathDist.SelectMany(i => i).Sum(); ;
		}
	}

	internal class Part_2
	{
		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_10.txt");
			string[] input = File.ReadAllLines(path);


			return 0;
		}


	}

}
