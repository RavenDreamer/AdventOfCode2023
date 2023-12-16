using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_15
{

	static internal int HASH(string input)
	{
		int currentValue = 0;
		foreach (char c in input)
		{
			var asciiVal = (int)c;
			currentValue += asciiVal;
			currentValue *= 17;
			currentValue = currentValue % 256;
		}

		return currentValue;
	}

	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_15.txt");
			string[] input = File.ReadAllLines(path);

			int rollingSum = 0;
			var steps = input[0].Split(",");
			foreach (string s in steps)
			{
				rollingSum += HASH(s);
			}
			return rollingSum;
		}


	}

	internal class Part_2
	{
		internal struct Lens
		{
			internal string Label { get; set; }
			internal int FocalLength { get; set; }
		}

		internal class FocalBox
		{
			Dictionary<string, int> focalIndices = new();
			List<Lens> lenses = new List<Lens>();

			internal int GetFocusingPower(int boxID)
			{
				int rollingSum = 0;
				for (int i = 0; i < lenses.Count; i++)
				{
					rollingSum += (boxID + 1) * (i + 1) * lenses[i].FocalLength;
				}

				return rollingSum;
			}

			internal void AddLens(Lens l)
			{
				// if there is already a lens int his box with the same label,
				// replace the old lens with the new lens.
				if (focalIndices.TryGetValue(l.Label, out int index))
				{
					// we found an existing lens with this label, so
					// replace in-place
					lenses[index] = l;
				}
				else
				{
					// no existing lens, ad the lens at the end of the list
					lenses.Add(l);
					focalIndices.Add(l.Label, lenses.Count - 1);
				}
			}

			internal void RemoveLens(string lensLabel)
			{
				if (focalIndices.TryGetValue(lensLabel, out int index))
				{
					// remove the lens in question, then regenerate the indices
					lenses.RemoveAt(index);

					focalIndices.Clear();
					for (int i = 0; i < lenses.Count; i++)
					{
						var lclLens = lenses[i];
						focalIndices[lclLens.Label] = i;
					}
				}
				else
				{
					//trying to remove a lens that doesn't exist -- nothing to do
				}
			}
		}


		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_15.txt");
			string[] input = File.ReadAllLines(path);


			FocalBox[] HASHMAP = new FocalBox[256];
			// initialize values
			for (int i = 0; i < 256; i++)
			{
				HASHMAP[i] = new FocalBox();
			}


			int rollingSum = 0;
			var steps = input[0].Split(",");
			foreach (string s in steps)
			{

				//s has either a '=' or a '-' in it
				if (s.Contains("-"))
				{
					//remove the lens with the label, if its in the box
					// the label is everything but the last character (which is a '-')
					var label = s[..^1];
					var boxNumber = HASH(label);
					HASHMAP[boxNumber].RemoveLens(label);
				}
				else
				{
					//split string into label and focalLength
					var splits = s.Split("=");
					var label = splits[0];
					var focalLength = splits[1];
					var boxNumber = HASH(label);

					Lens workingLens = new Lens() { Label = label, FocalLength = int.Parse(focalLength) };
					HASHMAP[boxNumber].AddLens(workingLens);
				}
			}

			var focusPower = 0;
			for (int i = 0; i < 256; i++)
			{
				focusPower += HASHMAP[i].GetFocusingPower(i);
			}

			return focusPower;
		}

	}

}
