using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

internal class Puzzle_3_1
{
	public static int schematicSize = 140;
	static HashSet<char> numChars = new HashSet<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_3.txt");
		string[] input = File.ReadAllLines(path);

		char[,] transposedData = new char[schematicSize, schematicSize];


		Dictionary<string, int> partDict = new Dictionary<string, int>();

		// transpose data for ease of use
		for (int y = 0; y < schematicSize; y++)
		{
			for (int x = 0; x < schematicSize; x++)
			{

				transposedData[x, schematicSize - y - 1] = input[y][x];
			}
		}

		// input data is either numbers or symbols
		// scan data for symbols; when we find a symbol look in the 8 adjacent squares for numbers. 
		// if present, that's a part number
		for (int y = 0; y < schematicSize; y++)
		{
			for (int x = 0; x < schematicSize; x++)
			{
				char target = transposedData[x, y];
				if (numChars.Contains(target) || target == '.')
				{
					//skip
					continue;
				}
				// we are a symbol, so look for surrounding part numbers
				List<KeyValuePair<string, int>> parts = FindPartsNearLocations(transposedData, x, y);
				foreach (var kvp in parts)
				{
					// this will overwrite duplicates with themselves, which effectively ignores them (which is what we want)
					partDict[kvp.Key] = kvp.Value;
				}
			}
		}

		return partDict.Values.Sum();
	}

	private static List<KeyValuePair<string, int>> FindPartsNearLocations(char[,] transposedData, int x, int y)
	{
		List<KeyValuePair<string, int>> output = new List<KeyValuePair<string, int>>();

		// Northwest
		if (x >= 1 && y < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x - 1, y + 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// North
		if (y < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x, y + 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// Northeast
		if (x < schematicSize - 1 && y < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x + 1, y + 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}

		// west
		if (x >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x - 1, y);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// East
		if (x < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x + 1, y);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}


		// Southwest
		if (x >= 1 && y >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x - 1, y - 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// North
		if (y >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x, y - 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// Northeast
		if (x < schematicSize - 1 && y >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x + 1, y - 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}

		return output;
	}

	private static KeyValuePair<string, int> FindPartsNearLocation(char[,] transposedData, int x, int y)
	{
		char testChar = transposedData[x, y];
		if (numChars.Contains(testChar))
		{
			// we have a part ID, look left until we find a non-number, then look right until we find a non-number
			// and take the chars in between those as the part ID

			// look left
			int leftOffset = 0;
			while (x - leftOffset - 1 >= 0)
			{
				char leadingInt = transposedData[x - leftOffset - 1, y];
				if (numChars.Contains(leadingInt))
				{
					leftOffset++;
				}
				else
				{
					break;
				}
			}

			// look right
			int rightOffset = 0;
			while (x + rightOffset + 1 < 140)
			{
				char leadingInt = transposedData[x + rightOffset + 1, y];
				if (numChars.Contains(leadingInt))
				{
					rightOffset++;
				}
				else
				{
					break;
				}
			}

			// generate part number from offsets
			string intString = "";
			for (int i = x - leftOffset; i <= x + rightOffset; i++)
			{
				intString += transposedData[i, y];
			}

			// partLoc
			string partLoc = (x - leftOffset) + "-" + (x + rightOffset) + "," + y;

			return new KeyValuePair<string, int>(partLoc, Int32.Parse(intString));
		}
		else
		{
			// the target cell wasn't a number, so return -1
			return new KeyValuePair<string, int>("none", -1);
		}
	}
}

internal class Puzzle_3_2
{
	public static int schematicSize = 140;
	static HashSet<char> numChars = new HashSet<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_3.txt");
		string[] input = File.ReadAllLines(path);

		char[,] transposedData = new char[schematicSize, schematicSize];


		int gearRatioSum = 0;

		// transpose data for ease of use
		for (int y = 0; y < schematicSize; y++)
		{
			for (int x = 0; x < schematicSize; x++)
			{

				transposedData[x, schematicSize - y - 1] = input[y][x];
			}
		}

		// input data is either numbers or symbols
		// scan data for a * symbol; when we find a symbol look in the 8 adjacent squares for numbers. 
		// if exactly 2 are present, it's a gear
		for (int y = 0; y < schematicSize; y++)
		{
			for (int x = 0; x < schematicSize; x++)
			{
				char target = transposedData[x, y];
				if (target != '*')
				{
					//skip
					continue;
				}
				// we are a gear, so look for surrounding part numbers
				List<KeyValuePair<string, int>> parts = FindPartsNearLocations(transposedData, x, y);

				// dedupe
				var uniqueParts = new HashSet<KeyValuePair<string, int>>(parts);

				if (uniqueParts.Count == 2)
				{
					// it's a gear; sum and return gear ratio
					var temp = uniqueParts.ToList();
					gearRatioSum += temp[0].Value * temp[1].Value;
				}
			}
		}

		return gearRatioSum;
	}

	private static List<KeyValuePair<string, int>> FindPartsNearLocations(char[,] transposedData, int x, int y)
	{
		List<KeyValuePair<string, int>> output = new List<KeyValuePair<string, int>>();

		// Northwest
		if (x >= 1 && y < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x - 1, y + 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// North
		if (y < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x, y + 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// Northeast
		if (x < schematicSize - 1 && y < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x + 1, y + 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}

		// west
		if (x >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x - 1, y);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// East
		if (x < schematicSize - 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x + 1, y);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}


		// Southwest
		if (x >= 1 && y >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x - 1, y - 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// North
		if (y >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x, y - 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}
		// Northeast
		if (x < schematicSize - 1 && y >= 1)
		{
			var potentialPart = FindPartsNearLocation(transposedData, x + 1, y - 1);
			if (potentialPart.Value != -1) { output.Add(potentialPart); }
		}

		return output;
	}

	private static KeyValuePair<string, int> FindPartsNearLocation(char[,] transposedData, int x, int y)
	{
		char testChar = transposedData[x, y];
		if (numChars.Contains(testChar))
		{
			// we have a part ID, look left until we find a non-number, then look right until we find a non-number
			// and take the chars in between those as the part ID

			// look left
			int leftOffset = 0;
			while (x - leftOffset - 1 >= 0)
			{
				char leadingInt = transposedData[x - leftOffset - 1, y];
				if (numChars.Contains(leadingInt))
				{
					leftOffset++;
				}
				else
				{
					break;
				}
			}

			// look right
			int rightOffset = 0;
			while (x + rightOffset + 1 < 140)
			{
				char leadingInt = transposedData[x + rightOffset + 1, y];
				if (numChars.Contains(leadingInt))
				{
					rightOffset++;
				}
				else
				{
					break;
				}
			}

			// generate part number from offsets
			string intString = "";
			for (int i = x - leftOffset; i <= x + rightOffset; i++)
			{
				intString += transposedData[i, y];
			}

			// partLoc
			string partLoc = (x - leftOffset) + "-" + (x + rightOffset) + "," + y;

			return new KeyValuePair<string, int>(partLoc, Int32.Parse(intString));
		}
		else
		{
			// the target cell wasn't a number, so return -1
			return new KeyValuePair<string, int>("none", -1);
		}
	}
}