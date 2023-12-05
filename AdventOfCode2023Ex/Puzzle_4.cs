using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

internal class Puzzle_4_1
{
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_4.txt");
		string[] input = File.ReadAllLines(path);

		int result = 0;

		// example line:
		// Card   1: 99 65 21  4 72 20 77 98 27 70 | 34 84 74 18 41 45 72  2  1 75 52 47 50 93 25 10 79 87 42 69  8 12 54 96 92
		foreach (string line in input)
		{
			HashSet<string> winningNumbers = new HashSet<string>();
			int winnerCount = 0;

			// discard the first bit
			var numbers = line.Substring(10).Split("|");
			// numbers[0] is the winning numbers
			var winners = numbers[0].Split(" ");
			foreach (string s in winners)
			{
				if (string.IsNullOrWhiteSpace(s)) continue;

				winningNumbers.Add(s);
			}
			// numbers[1] is the numbers you have
			var held = numbers[1].Split(" ");
			foreach (string s in held)
			{
				if (winningNumbers.Contains(s))
				{
					winnerCount++;
				}
			}
			// points per card = 2^(n-1)
			double pointsWon = Math.Pow(2, winnerCount - 1);
			result += (int)pointsWon;
		}

		return result;
	}

}

internal class Puzzle_4_2
{
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_4.txt");
		string[] input = File.ReadAllLines(path);

		Dictionary<int, string> scratchCardLookup = new Dictionary<int, string>();

		Dictionary<int, int> cardPointLookup = new Dictionary<int, int>();



		// example line:
		// Card   1: 99 65 21  4 72 20 77 98 27 70 | 34 84 74 18 41 45 72  2  1 75 52 47 50 93 25 10 79 87 42 69  8 12 54 96 92

		// reverse the input, since we want to start with the end of the list (which will never score more than itself)
		var reversedInput = input.Reverse();
		foreach (string line in reversedInput)
		{
			// discard the first bit
			var bifurcated = line.Split(":");

			// bifurcated[0] contains the card number
			// bifurcated[1] is the  numbers
			int cardNumber = Int32.Parse(bifurcated[0].Substring(5));

			// each card starts at 1 value (itself)
			int cardPointValue = 1;
			// see if it has any winners
			var numbers = bifurcated[1].Split("|");

			HashSet<string> winningNumbers = new HashSet<string>();
			int winnerCount = 0;

			// numbers[0] is the winningNumbers
			foreach (string s in numbers[0].Split(" "))
			{
				if (string.IsNullOrWhiteSpace(s)) continue;

				winningNumbers.Add(s);
			}
			// numbers[1] is the numbers you have
			var held = numbers[1].Split(" ");
			foreach (string s in held)
			{
				if (winningNumbers.Contains(s))
				{
					winnerCount++;
				}
			}

			for (int i = 1; i <= winnerCount; i++)
			{
				// for each winner, look up the pointValue of the won card
				// which is 0 if we don't have a value
				if (cardPointLookup.TryGetValue(cardNumber + i, out int foundValue))
				{
					cardPointValue += foundValue;
				}
				else
				{
					// we didn't have a value, because we went off the edge of the
					// dict
				}
			}
			cardPointLookup[cardNumber] = cardPointValue;
		}


		return cardPointLookup.Values.Sum();
	}
}