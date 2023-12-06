using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

internal class Puzzle_5_1
{
	public struct TripleDouble
	{
		public double destinationRangeStart { get; set; }
		public double sourceRangeStart { get; set; }
		public double rangeLength { get; set; }

		public TripleDouble(double dest, double source, double range)
		{
			this.destinationRangeStart = dest;
			this.sourceRangeStart = source;
			this.rangeLength = range;
		}

		// A TripleDouble maps a range if sourceRangeStart <= source < sourceRangeStart+rangeLength
		public double MapsRange(double sourceVal)
		{
			if (sourceRangeStart + rangeLength > sourceVal && sourceRangeStart <= sourceVal)
			{
				//find the offset and add it to the destinationRangeStart
				return (sourceVal - sourceRangeStart) + destinationRangeStart;

			}
			// there was no valid mapping in this TripleDouble
			return -1;

		}

		// 

	}

	public static double Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_5.txt");
		string[] input = File.ReadAllLines(path);

		int result = 0;

		// seed -> soil -> fertilizer -> water -> light -> temperature -> humidity -> location

		// Line 0 has the seeds
		List<string> seeds = new List<string>(input[0].Substring(7).Split(" "));
		List<List<TripleDouble>> maps = new List<List<TripleDouble>>();
		maps.Add(new List<TripleDouble>()); // seed-soil
		maps.Add(new List<TripleDouble>()); // soil-fertilizer
		maps.Add(new List<TripleDouble>()); // fertilizer-water
		maps.Add(new List<TripleDouble>()); // water-light
		maps.Add(new List<TripleDouble>()); // light-temperature
		maps.Add(new List<TripleDouble>()); // temperature-humidity
		maps.Add(new List<TripleDouble>()); // humididty-location

		// Line 3 has the first seed-to-soil map
		int mapsOffset = 0;
		for (int i = 3; i < input.Length; i++)
		{
			string queryLine = input[i];

			// skip spacer lines
			if (queryLine.Length == 0) continue;

			if (queryLine.Contains(":"))
			{
				// we're at the start of the next map
				mapsOffset++;
				continue;
			}
			// we are in a mapping line
			var splitStrings = queryLine.Split(" ");

			maps[mapsOffset].Add(
				new TripleDouble(
					Double.Parse(splitStrings[0]),
					Double.Parse(splitStrings[1]),
					Double.Parse(splitStrings[2])
				)
			);
		}

		double smallestLoc = double.MaxValue;
		double smallestLocSeed = 0;

		// do 7 times, one for each mapping
		foreach (string seed in seeds)
		{
			double seedVal = Double.Parse(seed);
			double workingVal = seedVal;
			for (int m = 0; m < 7; m++)
			{
				foreach (TripleDouble td in maps[m])
				{
					double temp = td.MapsRange(workingVal);
					if (temp != -1)
					{
						workingVal = temp;
						break;
					}
				}
				// if every range returned -1, then
				// workingVal = workingVal, which is what we wanted
			}
			if (workingVal < smallestLoc)
			{
				smallestLoc = workingVal;
				smallestLocSeed = seedVal;
			}
		}

		return smallestLoc;
	}

}

internal class Puzzle_5_2
{
	public struct TripleDouble
	{
		public double DestinationRangeStart { get; set; }
		public double SourceRangeStart { get; set; }
		public double RangeLength { get; set; }

		public TripleDouble(double dest, double source, double range)
		{
			this.DestinationRangeStart = dest;
			this.SourceRangeStart = source;
			this.RangeLength = range;
		}

		// A TripleDouble maps a range if sourceRangeStart <= source < sourceRangeStart+rangeLength
		public double MapsRange(double sourceVal)
		{
			if (SourceRangeStart + RangeLength > sourceVal && SourceRangeStart <= sourceVal)
			{
				//find the offset and add it to the destinationRangeStart
				return (sourceVal - SourceRangeStart) + DestinationRangeStart;

			}
			// there was no valid mapping in this TripleDouble
			return -1;

		}

		// A TripleDouble reverse maps a range if destinationRangeStart <= source < destinationRangeStart+rangeLength
		//
		public double ReverseMapsRange(double sourceVal)
		{
			if (DestinationRangeStart + RangeLength > sourceVal && DestinationRangeStart <= sourceVal)
			{
				//find the offset and add it to the sourceRangeStart
				return (sourceVal - DestinationRangeStart) + SourceRangeStart;

			}
			// there was no valid mapping in this TripleDouble
			return -1;
		}

		// 

	}

	public struct DoubleDouble
	{
		public double Start { get; set; }
		public double Range { get; set; }

		public DoubleDouble(double start, double range)
		{
			Start = start;
			Range = range;
		}

		public bool ContainsVal(double sourceVal)
		{
			return (sourceVal >= Start) && (sourceVal - Start < Range);
		}
	}

	public static double Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_5.txt");
		string[] input = File.ReadAllLines(path);

		// seed -> soil -> fertilizer -> water -> light -> temperature -> humidity -> location

		// Line 0 has the seeds
		List<string> seeds = new(input[0].Substring(7).Split(" "));
		List<DoubleDouble> seedsActual = new();
		for (int j = 0; j < seeds.Count - 1; j += 2)
		{
			seedsActual.Add(new DoubleDouble(double.Parse(seeds[j]), double.Parse(seeds[j + 1])));
		}


		List<List<TripleDouble>> maps = new List<List<TripleDouble>>();
		maps.Add(new List<TripleDouble>()); // seed-soil
		maps.Add(new List<TripleDouble>()); // soil-fertilizer
		maps.Add(new List<TripleDouble>()); // fertilizer-water
		maps.Add(new List<TripleDouble>()); // water-light
		maps.Add(new List<TripleDouble>()); // light-temperature
		maps.Add(new List<TripleDouble>()); // temperature-humidity
		maps.Add(new List<TripleDouble>()); // humididty-location

		// Line 3 has the first seed-to-soil map
		int mapsOffset = 0;
		for (int j = 3; j < input.Length; j++)
		{
			string queryLine = input[j];

			// skip spacer lines
			if (queryLine.Length == 0) continue;

			if (queryLine.Contains(":"))
			{
				// we're at the start of the next map
				mapsOffset++;
				continue;
			}
			// we are in a mapping line
			var splitStrings = queryLine.Split(" ");

			maps[mapsOffset].Add(
				new TripleDouble(
					Double.Parse(splitStrings[0]),
					Double.Parse(splitStrings[1]),
					Double.Parse(splitStrings[2])
				)
			);
		}
		// I guess we technically have to start from 1 and go up without bound because
		// the maps don't have upper limits...

		// since we're going in reverse, go in reverse
		maps.Reverse();

		// do 7 times, one for each mapping
		double i = 0;
		while (i < double.MaxValue) // ...these aren't really doubles, are they? Bigints?
		{
			// do 7 times, one for each mapping
			double workingVal = i;
			for (int m = 0; m < 7; m++)
			{
				foreach (TripleDouble td in maps[m])
				{
					double temp = td.ReverseMapsRange(workingVal);
					if (temp != -1)
					{
						workingVal = temp;
						break;
					}
				}
				// if every range returned -1, then
				// workingVal = workingVal, which is what we wanted
			}

			// we have our reverse-mapped seed, check if it's in the list of seeds
			foreach (var dd in seedsActual)
			{
				if (dd.ContainsVal(workingVal))
				{
					return i;
				}
			}
			// didn't find it;
			// onto the next one!~
			i++;
		}

		throw new Exception("Uhoh");
	}
}