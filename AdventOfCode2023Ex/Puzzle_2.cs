using System;
using System.Reflection;
using System.Text.RegularExpressions;

internal class Puzzle_2_1
{
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_2.txt");
		string[] input = File.ReadAllLines(path);

		int possibleIDsums = 0;

		// example input line: 
		//
		// Game 76: 4 green, 1 red, 3 blue; 7 blue, 3 green, 3 red; 4 blue, 2 red, 3 green; 4 blue, 1 green
		// [Game] %d : X1 <y1>, X2 <y2>...;

		foreach (string line in input)
		{
			// split the input line into the "Game X" and the value-color bits
			var stringBits = line.Split(":");

			// remove "Game" from the first bit
			int gameNum = int.Parse(stringBits[0].Substring(4));

			// for the other stringbit, split on `;` and then `,`
			Dictionary<string, int> gameMaxDict = new() { { "red", 0 }, { "green", 0 }, { "blue", 0 } };
			var gameBits = stringBits[1].Split(";");
			foreach (string game in gameBits)
			{
				var components = game.Split(",");
				// and finally split on [space] and parse
				foreach (string kvp in components)
				{
					var data = kvp.Trim().Split(" ");
					int value = Int32.Parse(data[0]); ;
					string key = data[1];
					// [0] is the value, [1] is the key
					// if [0] is greater than the current max, set it to the
					// new max
					if (gameMaxDict[key] < value)
					{
						gameMaxDict[key] = value;
					}
				}
			}
			// test the dict against our possibility values
			if (gameMaxDict["red"] <= 12 &&
				gameMaxDict["blue"] <= 14 &&
				gameMaxDict["green"] <= 13)
			{
				// game was possible for the given configuration. Output
				possibleIDsums += gameNum;
			}
		}

		return possibleIDsums;
	}
}

internal class Puzzle_2_2
{
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_2.txt");
		string[] input = File.ReadAllLines(path);

		int powerSums = 0;

		// example input line: 
		//
		// Game 76: 4 green, 1 red, 3 blue; 7 blue, 3 green, 3 red; 4 blue, 2 red, 3 green; 4 blue, 1 green
		// [Game] %d : X1 <y1>, X2 <y2>...;

		foreach (string line in input)
		{
			// split the input line into the "Game X" and the value-color bits
			var stringBits = line.Split(":");

			// remove "Game" from the first bit
			int gameNum = int.Parse(stringBits[0].Substring(4));

			// for the other stringbit, split on `;` and then `,`
			Dictionary<string, int> gameMaxDict = new() { { "red", 0 }, { "green", 0 }, { "blue", 0 } };
			var rounds = stringBits[1].Split(";");
			foreach (string round in rounds)
			{
				Dictionary<string, int> roundMaxDict = new() { { "red", 0 }, { "green", 0 }, { "blue", 0 } };
				var sourceValues = round.Split(",");
				// and finally split on [space] and parse
				foreach (string kvp in sourceValues)
				{
					var data = kvp.Trim().Split(" ");
					int value = Int32.Parse(data[0]); ;
					string key = data[1];
					// [0] is the value, [1] is the key
					// if [0] is greater than the current max, set it to the
					// new max
					if (roundMaxDict[key] < value)
					{
						roundMaxDict[key] = value;
					}
				}

				// test against the overall gameMaxDict
				if (gameMaxDict["red"] < roundMaxDict["red"])
				{
					gameMaxDict["red"] = roundMaxDict["red"];
				}
				if (gameMaxDict["blue"] < roundMaxDict["blue"])
				{
					gameMaxDict["blue"] = roundMaxDict["blue"];
				}
				if (gameMaxDict["green"] < roundMaxDict["green"])
				{
					gameMaxDict["green"] = roundMaxDict["green"];
				}
			}
			// The power of a set of cubes is equal to the numbers of red, green, and blue cubes multiplied together.
			int powerVal = gameMaxDict["red"] * gameMaxDict["blue"] * gameMaxDict["green"];
			powerSums += powerVal;
		}

		return powerSums;
	}
}