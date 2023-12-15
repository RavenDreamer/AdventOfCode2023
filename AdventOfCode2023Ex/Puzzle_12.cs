using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_12
{

	public enum springStatus
	{
		unknown,
		working,
		broken,
	}

	private static Dictionary<string, long> resultDict = new Dictionary<string, long>();

	private static long CalculateNonogram(string record, List<int> groupList)
	{
		// if there are no possible arrangements return 0, otherwise, return the number
		// of possible arrangements.

		// start by finding the earliest contiguous block

		long possibilities = 0;

		var firstTarget = groupList[0];



		// extra '.'s at the front of the string don't affect the possibilities and can be
		// removed to expedite processing. Ditto for those on the end of the string.
		var workingRecord = record.Trim('.');

		// convert this to a unique name
		var name = workingRecord + "||" + groupList.Aggregate("", (current, i) => current + (i + ","));


		if (resultDict.TryGetValue(name, out long validConfigurations))
		{
			return validConfigurations;
		}

		int sumTarget = 0;
		for (int i = 0; i < workingRecord.Length; i++)
		{

			switch (workingRecord[i])
			{
				case '.':
					if (sumTarget == firstTarget)
					{
						// we have hit a break recurse with substring(i+1) and groupList(1..)
						// because of our trimming, we know we can't be the last character, so the
						// substring is always in bounds.
						if (groupList.Count == 1)
						{
							// we've hit maximum recursion

							// if the rest of the string doesn't contain any #s, we're golden
							// otherwise we're invalid
							var restOfString = workingRecord[(i + 1)..];
							if (restOfString.Contains('#'))
							{
								resultDict[name] = 0;
								return 0;
							}
							resultDict[name] = 1;
							return 1;
						}
						else
						{
							var cloneGroup = new List<int>(groupList);
							cloneGroup.RemoveAt(0);
							possibilities += CalculateNonogram(workingRecord[(i + 1)..], cloneGroup);

							return possibilities;
						}

					}
					else if (sumTarget < firstTarget)
					{
						// we are an invalid configuration. Return 0.
						resultDict[name] = 0;
						return 0;
					}
					else
					{
						throw new Exception("How'd sumTarget get > firstTarget?!");
					}
				case '#':
					sumTarget++;
					if (sumTarget > firstTarget)
					{
						resultDict[name] = 0;
						return 0;
					}
					continue;
				case '?':
					// replace '?' with both # and . see which is valid
					// start of the range is inclusive						end of the range is exclusive

					string plusSubstring = "";
					string minusSubstring = "";

					if (i + 1 < workingRecord.Length)
					{
						//we have string to recurse
						plusSubstring = workingRecord[(i + 1)..];
						minusSubstring = workingRecord[(i + 1)..];
					}

					string validFork = workingRecord[..i] + '#' + plusSubstring;
					string invalidFork = workingRecord[..i] + '.' + minusSubstring;

					// convert this to a unique name
					var validName = validFork + "||" + groupList.Aggregate("", (current, i) => current + (i + ","));
					var invalidName = invalidFork + "||" + groupList.Aggregate("", (current, i) => current + (i + ","));


					var validRes = CalculateNonogram(validFork, groupList);
					var invalidRes = CalculateNonogram(invalidFork, groupList);

					resultDict[validName] = validRes;
					resultDict[invalidName] = invalidRes;

					// we've calculated all possible variations so there's
					// nothing left to do.
					return validRes + invalidRes;
			}

		}
		// we read through the entire string and haven't exited out yet
		if (sumTarget == firstTarget)
		{
			if (groupList.Count == 1)
			{
				// we've hit maximum recursion
				resultDict[name] = 1;
				return 1;
			}
			else
			{
				// we have more contiguous groups, but
				// we're out of characters.
				// This is an invalid state
				resultDict[name] = 0;
				return 0;
			}
		}
		else
		{
			// we couldn't reach the target
			// This is an invalid state;
			resultDict[name] = 0;
			return 0;
		}
	}

	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_12.txt");
			string[] input = File.ReadAllLines(path);

			long arrangements = 0;

			foreach (var s in input)
			{
				var lineBits = s.Split(" ");
				string record = lineBits[0];
				List<int> groupList = lineBits[1].Split(",").Select(i => int.Parse(i)).ToList();

				arrangements = arrangements + CalculateNonogram(record, groupList);
			}

			return arrangements;
		}


	}

	internal class Part_2
	{
		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_12.txt");
			string[] input = File.ReadAllLines(path);

			long arrangements = 0;

			foreach (var s in input)
			{
				var lineBits = s.Split(" ");
				string record = lineBits[0] + "?" + lineBits[0] + "?" + lineBits[0] + "?" + lineBits[0] + "?" + lineBits[0];
				List<int> groupList = lineBits[1].Split(",").Select(i => int.Parse(i)).ToList();

				List<int> mult5Grouplist = new();
				mult5Grouplist.AddRange(groupList);
				mult5Grouplist.AddRange(groupList);
				mult5Grouplist.AddRange(groupList);
				mult5Grouplist.AddRange(groupList);
				mult5Grouplist.AddRange(groupList);

				arrangements = arrangements + CalculateNonogram(record, mult5Grouplist);
			}
			return arrangements;
		}
	}
}
