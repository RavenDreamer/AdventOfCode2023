using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_9
{
	internal const int ROW_LENGTH = 21;
	internal class OASISRow
	{
		internal int[] dataRow = new int[ROW_LENGTH];
		internal List<List<int>> successorRows;

		public OASISRow(string input)
		{
			var nums = input.Split(" ");
			for (int i = 0; i < ROW_LENGTH; i++)
			{
				dataRow[i] = Convert.ToInt32(nums[i]);
			}
			successorRows = new List<List<int>>() { new List<int>(dataRow) };
		}

		public void GenerateSuccessors()
		{
			var workingRow = new List<int>();
			workingRow.AddRange(dataRow);
			while (workingRow.Any(n => n != 0))
			{
				var nextRow = new List<int>(workingRow.Count - 1);
				for (int i = 0; i < workingRow.Count - 1; i++)
				{
					nextRow.Add(workingRow[i + 1] - workingRow[i]);
				}
				successorRows.Add(nextRow);
				workingRow = nextRow;
			}
		}

		public void ExtrapolateValue()
		{
			// add a zero to the end of the last row
			successorRows.Last().Add(0);

			// minus 2, since we already did the first one
			for (int i = successorRows.Count - 2; i >= 0; i--)
			{
				// A - B == X   =>   B + X == A
				// X == child row value
				// A == extrapolated value
				// B == last "true" value of row
				int extrapolated = successorRows[i].Last() + successorRows[i + 1].Last();
				successorRows[i].Add(extrapolated);
			}
		}

		public void ExtrapolateFormerValue()
		{
			// add a zero to the start of the last row
			// because they're all 0, adding it to the end is the same
			// as adding it to the start.
			successorRows.Last().Add(0);

			// minus 2, since we already did the first one
			for (int i = successorRows.Count - 2; i >= 0; i--)
			{
				// A + X == B   =>   B - X == A
				// X == child row value
				// A == extrapolated value
				// B == last "true" value of row
				int extrapolated = successorRows[i].First() - successorRows[i + 1].First();
				successorRows[i].Insert(0, extrapolated);
			}
		}

		public int GetLatest()
		{
			return successorRows[0].Last();
		}

		public int GetPrehistory()
		{
			return successorRows[0][0];
		}
	}

	internal class Part_1
	{
		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_9.txt");
			string[] input = File.ReadAllLines(path);
			List<OASISRow> rows = new();

			int rollingSum = 0;

			foreach (string s in input)
			{
				var temp = new OASISRow(s);
				temp.GenerateSuccessors();
				temp.ExtrapolateValue();
				rows.Add(temp);
				rollingSum += temp.GetLatest();
			}

			return rollingSum;
		}
	}

	internal class Part_2
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_9.txt");
			string[] input = File.ReadAllLines(path);
			List<OASISRow> rows = new();

			int rollingSum = 0;

			foreach (string s in input)
			{
				var temp = new OASISRow(s);
				temp.GenerateSuccessors();
				temp.ExtrapolateFormerValue();
				rows.Add(temp);
				rollingSum += temp.GetPrehistory();
			}

			return rollingSum;
		}
	}
}

