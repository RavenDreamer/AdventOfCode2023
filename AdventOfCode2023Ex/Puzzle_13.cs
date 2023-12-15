using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_13
{
	public struct ReflectResult
	{
		public int linesBeyondReflection { get; set; }
		public bool isHorizontal { get; set; }

	}

	private static ReflectResult? DoReflection(List<string> vals, bool isReversed = false)
	{
		string target;

		// look from right
		target = vals[0];
		for (int i = vals.Count - 1; i > 0; i--) // ignore the last column, since it'll always match itself
		{
			if (vals[i].Equals(target))
			{
				// check whether the shape between 0 and i
				// are mirrors of one another.
				bool invalidMirror = false;
				var potentialCenterpoint = i / 2;
				for (int j = 0; j < potentialCenterpoint + 1; j++)
				{
					if (!vals[j].Equals(vals[i - j]))
					{
						invalidMirror = true;
					}
					// If we're looking at the same line, we're not reflected
					// between a column, we're reflected over a line, which is
					// not valid (even if it is reflection-y.
					if (j == i - j)
					{
						invalidMirror = true;
					}
				}
				// we found a mirror!
				// mirror is center on i / 2 -> i / 2 + 1
				if (invalidMirror == false)
				{
					// plus 1 because the results want the Count, not the index
					var temp = new ReflectResult() { isHorizontal = false, linesBeyondReflection = potentialCenterpoint + 1 };

					if (isReversed)
					{
						//we need to count the other direction, because we're reversed.
						temp.linesBeyondReflection = vals.Count - temp.linesBeyondReflection;
					}

					return temp;
				}
			}
		}

		// no dice
		return null;
	}

	public class CharMap
	{
		List<string> Rows = new();
		List<string> Cols = new();

		public int Width
		{
			get
			{
				return Cols.Count;
			}
		}

		public int Height
		{
			get
			{
				return Rows.Count;
			}
		}

		public CharMap(List<string> rows)
		{
			this.Rows = rows;
			this.Cols = TransposeRows(rows);
		}

		// we have a y length list of x-length rows
		// we want an output of x length list of y-length rows
		private List<string> TransposeRows(List<string> rows)
		{
			List<string> transposed = new();

			int height = rows.Count;
			int width = rows[0].Length;

			for (int i = 0; i < width; i++)
			{
				string rowWise = "";
				for (int j = 0; j < height; j++)
				{
					rowWise += rows[j][i];
				}

				transposed.Add(rowWise);
			}

			return transposed;
		}

		public CharMap CleanSmudge(int x, int y)
		{
			// returns a copy of this CharMap with the x,y symbol swapped
			var queryChar = Rows[y][x];
			char swapChar;
			if (queryChar == '#')
			{
				swapChar = '.';
			}
			else
			{
				swapChar = '#';
			}

			string newRow;
			if (x == Cols.Count)
			{
				newRow = Rows[y][..x] + swapChar;
			}
			else
			{
				newRow = Rows[y][..x] + swapChar + Rows[y][(x + 1)..];
			}

			var newRows = new List<string>(Rows);
			newRows[y] = newRow;

			return new CharMap(newRows);

		}

		internal ReflectResult FindReflection(ReflectResult antiReflection)
		{
			// try 4 times, rows and columns forward and backwards
			var workCols = new List<string>(Cols);

			var attemptVertLeft = DoReflection(workCols);

			if (attemptVertLeft != null)
			{
				if (((ReflectResult)attemptVertLeft).isHorizontal == antiReflection.isHorizontal &&
					((ReflectResult)attemptVertLeft).linesBeyondReflection == antiReflection.linesBeyondReflection)
				{
					// not the answer we're looking for. Keep searchin'.
				}
				else
				{
					return (ReflectResult)attemptVertLeft;
				}
			}

			workCols.Reverse();
			var attemptVerRight = DoReflection(workCols, true);

			if (attemptVerRight != null)
			{
				if (((ReflectResult)attemptVerRight).isHorizontal == antiReflection.isHorizontal &&
					((ReflectResult)attemptVerRight).linesBeyondReflection == antiReflection.linesBeyondReflection)
				{
					// not the answer we're looking for. Keep searchin'.
				}
				else
				{
					return (ReflectResult)attemptVerRight;
				}
			}

			var workRows = new List<string>(Rows);

			var attemptHorizTop = DoReflection(workRows);
			if (attemptHorizTop != null)
			{
				var temp = (ReflectResult)attemptHorizTop;
				temp.isHorizontal = true;

				if (temp.isHorizontal == antiReflection.isHorizontal &&
					temp.linesBeyondReflection == antiReflection.linesBeyondReflection)
				{
					// not the answer we're looking for. Keep searchin'.
				}
				else
				{
					return temp;
				}

			}

			workRows.Reverse();
			var attemptHorizBot = DoReflection(workRows, true);
			if (attemptHorizBot != null)
			{
				var temp = (ReflectResult)attemptHorizBot;
				temp.isHorizontal = true;
				if (temp.isHorizontal == antiReflection.isHorizontal &&
					temp.linesBeyondReflection == antiReflection.linesBeyondReflection)
				{
					// not the answer we're looking for. Keep searchin'.
				}
				else
				{
					return temp;
				}
			}

			//if we get here, we have a map with NO symmetry
			return new ReflectResult() { linesBeyondReflection = -1 };
		}



		// for a mirror to exist vertically, EITHER the first column has a
		// duplicate XOR the last column has a duplicate.
		internal ReflectResult FindReflection()
		{

			// try 4 times, rows and columns forward and backwards
			var workCols = new List<string>(Cols);

			var attemptVertLeft = DoReflection(workCols);

			if (attemptVertLeft != null) return (ReflectResult)attemptVertLeft;

			workCols.Reverse();
			var attemptVerRight = DoReflection(workCols, true);

			if (attemptVerRight != null) return (ReflectResult)attemptVerRight;

			var workRows = new List<string>(Rows);

			var attemptHorizTop = DoReflection(workRows);
			if (attemptHorizTop != null)
			{
				var temp = (ReflectResult)attemptHorizTop;
				temp.isHorizontal = true;
				return temp;
			}

			workRows.Reverse();
			var attemptHorizBot = DoReflection(workRows, true);
			if (attemptHorizBot != null)
			{
				var temp = (ReflectResult)attemptHorizBot;
				temp.isHorizontal = true;
				return temp;
			}

			//if we get here, we have a map with NO symmetry
			return new ReflectResult() { linesBeyondReflection = -1 };
		}


	}

	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_13.txt");
			string[] input = File.ReadAllLines(path);

			List<CharMap> charMaps = new();
			List<string> charMapInput = new();

			foreach (var s in input)
			{
				if (s.Length == 0 && charMapInput.Count != 0)
				{
					var temp = new CharMap(new List<string>(charMapInput));
					charMaps.Add(temp);

					charMapInput.Clear();
				}
				else
				{
					charMapInput.Add(s);
				}
			}
			if (charMapInput.Count != 0)
			{
				var temp = new CharMap(new List<string>(charMapInput));
				charMaps.Add(temp);

				charMapInput.Clear();
			}

			int resTotals = 0;

			foreach (var cm in charMaps)
			{
				ReflectResult res = cm.FindReflection();
				if (res.isHorizontal)
				{
					resTotals += 100 * res.linesBeyondReflection;
				}
				else
				{
					resTotals += res.linesBeyondReflection;
				}
			}

			return resTotals;
		}


	}

	internal class Part_2
	{
		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_13.txt");
			string[] input = File.ReadAllLines(path);

			List<CharMap> charMaps = new();
			List<string> charMapInput = new();

			foreach (var s in input)
			{
				if (s.Length == 0 && charMapInput.Count != 0)
				{
					var temp = new CharMap(new List<string>(charMapInput));
					charMaps.Add(temp);

					charMapInput.Clear();
				}
				else
				{
					charMapInput.Add(s);
				}
			}
			if (charMapInput.Count != 0)
			{
				var temp = new CharMap(new List<string>(charMapInput));
				charMaps.Add(temp);

				charMapInput.Clear();
			}

			int resTotals = 0;

			foreach (var cm in charMaps)
			{



				ReflectResult res = cm.FindReflection();
				ReflectResult smudgeFinal = new ReflectResult() { linesBeyondReflection = -1 };

				for (int x = 0; x < cm.Width; x++)
				{
					for (int y = 0; y < cm.Height; y++)
					{
						var smudgeRes = cm.CleanSmudge(x, y).FindReflection(res);

						// no reflection, skip
						if (smudgeRes.linesBeyondReflection == -1) continue;

						// same reflection, skip
						if (smudgeRes.linesBeyondReflection == res.linesBeyondReflection &&
							smudgeRes.isHorizontal == res.isHorizontal) continue;

						// different reflection, this is the one we want
						smudgeFinal = smudgeRes;
						goto Found;
					}
				}

			Found:
				if (smudgeFinal.linesBeyondReflection < 0)
				{
					throw new Exception("crud");
				}
				if (smudgeFinal.isHorizontal)
				{
					resTotals += 100 * smudgeFinal.linesBeyondReflection;
				}
				else
				{
					resTotals += smudgeFinal.linesBeyondReflection;
				}
			}

			return resTotals;
		}
	}
}
