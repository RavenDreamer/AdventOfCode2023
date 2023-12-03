using System;
using System.Reflection;
using System.Text.RegularExpressions;

internal class Puzzle_1_1
{
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_1.txt");
		string[] input = File.ReadAllLines(path);
		int sum = 0;

		foreach (string line in input)
		{
			// ignore alpha parts of the string
			string numberOnly = Regex.Replace(line, "[^0-9.]", "");

			// get first and last digit and concatenate (this may be same value)
			string calValue = numberOnly[0].ToString() + numberOnly[numberOnly.Length - 1].ToString();
			// convert to int
			int calInt = Int32.Parse(calValue);
			sum += calInt;
		}


		return sum;
	}
}

internal class Puzzle_1_2
{
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_1.txt");
		string[] input = File.ReadAllLines(path);
		int sum = 0;

		foreach (string line in input)
		{
			string left = getLeftNumber(line);
			string right = getRightNumber(line);

			// get first and last digit and concatenate (this may be same value)
			string calValue = left + right;
			// convert to int
			int calInt = Int32.Parse(calValue);
			sum += calInt;
		}

		return sum;
	}

	private static string getLeftNumber(string line)
	{
		for (int i = 0; i < line.Length; i++)
		{
			switch (line[i])
			{
				// for digits, we just return immediately
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return line[i].ToString();
				// for letters, we only care about 'o','t','f','s','e','n','z'
				case 'o':
					// one
					if (i + 2 < line.Length)
					{
						if (line[i + 1] == 'n' && line[i + 2] == 'e')
						{
							return "1";
						}
						// this letter isn't the start of a numeric representation
					}
					break;

				case 't':
					// two && three
					if (i + 2 < line.Length)
					{
						if (line[i + 1] == 'w' && line[i + 2] == 'o')
						{
							return "2";
						}
					}
					// 
					if (i + 4 < line.Length)
					{
						if (line[i + 1] == 'h' && line[i + 2] == 'r' && line[i + 3] == 'e' && line[i + 4] == 'e')
						{
							return "3";
						}
						// this letter isn't the start of a numeric representation
					}
					break;
				case 'f':
					// four && five
					if (i + 3 < line.Length)
					{
						if (line[i + 1] == 'o' && line[i + 2] == 'u' && line[i + 3] == 'r')
						{
							return "4";
						}

						if (line[i + 1] == 'i' && line[i + 2] == 'v' && line[i + 3] == 'e')
						{
							return "5";
						}
					}
					break;
				case 's':
					// six && seven
					if (i + 2 < line.Length)
					{
						if (line[i + 1] == 'i' && line[i + 2] == 'x')
						{
							return "6";
						}

					}
					if (i + 4 < line.Length)
					{
						if (line[i + 1] == 'e' && line[i + 2] == 'v' && line[i + 3] == 'e' && line[i + 4] == 'n')
						{
							return "7";
						}
						// this letter isn't the start of a numeric representation
					}
					break;
				case 'e':
					// eight
					if (i + 4 < line.Length)
					{
						if (line[i + 1] == 'i' && line[i + 2] == 'g' && line[i + 3] == 'h' && line[i + 4] == 't')
						{
							return "8";
						}
						// this letter isn't the start of a numeric representation
					}
					break;
				case 'n':
					// nine
					if (i + 3 < line.Length)
					{
						if (line[i + 1] == 'i' && line[i + 2] == 'n' && line[i + 3] == 'e')
						{
							return "9";
						}
					}
					break;
				case 'z':
					// "zero" is not part of input data
					break;


				default:
					// do nothing;
					break;

			}
		}

		throw new Exception("Line did not have a parsed number from left");
	}


	private static string getRightNumber(string line)
	{
		for (int i = line.Length - 1; i >= 0; i--)
		{
			switch (line[i])
			{
				// for digits, we just return immediately
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return line[i].ToString();
				// for letters, we only care about 'e','o','r','x','n','t',
				case 'e':
					// one, three, five, nine
					if (i - 2 >= 0)
					{
						if (line[i - 1] == 'n' && line[i - 2] == 'o')
						{
							return "1";
						}
						// this letter isn't the start of a numeric representation
					}
					if (i - 4 >= 0)
					{
						if (line[i - 1] == 'e' && line[i - 2] == 'r' && line[i - 3] == 'h' && line[i - 4] == 't')
						{
							return "3";
						}
						// this letter isn't the start of a numeric representation
					}
					if (i - 3 >= 0)
					{
						if (line[i - 1] == 'v' && line[i - 2] == 'i' && line[i - 3] == 'f')
						{
							return "5";
						}

						if (line[i - 1] == 'n' && line[i - 2] == 'i' && line[i - 3] == 'n')
						{
							return "9";
						}
					}
					break;

				case 'o':
					// two ("zero" is not part of the input data)
					if (i - 2 >= 0)
					{
						if (line[i - 1] == 'w' && line[i - 2] == 't')
						{
							return "2";
						}
					}

					break;
				case 'r':
					// four 
					if (i - 3 >= 0)
					{
						if (line[i - 1] == 'u' && line[i - 2] == 'o' && line[i - 3] == 'f')
						{
							return "4";
						}
					}
					break;
				case 'x':
					// six 
					if (i - 2 >= 0)
					{
						if (line[i - 1] == 'i' && line[i - 2] == 's')
						{
							return "6";
						}

					}
					break;
				case 'n':
					// seven
					if (i - 4 >= 0)
					{
						if (line[i - 1] == 'e' && line[i - 2] == 'v' && line[i - 3] == 'e' && line[i - 4] == 's')
						{
							return "7";
						}
						// this letter isn't the start of a numeric representation
					}
					break;
				case 't':
					// eight
					if (i - 4 >= 0)
					{
						if (line[i - 1] == 'h' && line[i - 2] == 'g' && line[i - 3] == 'i' && line[i - 4] == 'e')
						{
							return "8";
						}
						// this letter isn't the start of a numeric representation
					}
					break;

				default:
					// do nothing;
					break;

			}
		}

		throw new Exception("Line did not have a parsed number from left");
	}
}