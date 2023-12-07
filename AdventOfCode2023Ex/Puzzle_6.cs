using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

internal class Puzzle_6_1
{
	public static int Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_6.txt");
		string[] input = File.ReadAllLines(path);

		var times = input[0].Split(":")[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => Int32.Parse(s)).ToList();
		var distances = input[1].Split(":")[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => Int32.Parse(s)).ToList();

		int result = 1; // we're multiplying, so start with 1

		for (int i = 0; i < times.Count(); i++)
		{
			// Race formula:
			// Distance < (H)*(L-H) where 0 <= H <= L
			// We want to find H
			// D < HL - H^2
			// H^2 - LH + D > 0
			// So Quadratic formula (ax²+bx+c=0) should give us (-b±√(b²-4ac))/(2a) .:
			// -L +/- Sqrt(L^2 - 4(1)(-D)) / 2(1)
			// and all our values should be within that range!
			var l = times[i] * -1;
			var d = distances[i];

			// Floor PosL
			var posL = (
				-l + Math.Sqrt((l * l) - (4 * 1 * d)
								)
				) / 2;
			var intPosL = (int)Math.Floor(posL);
			// because we want to BEAT the record, not match it, if the floor did nothing
			// we need to subtract 1 more
			if (posL == intPosL)
			{
				intPosL--;
			}

			// Ceil NegL
			var negL = (
				-l - Math.Sqrt((l * l) - (4 * 1 * d)
								)
				) / 2;
			var intNegL = (int)Math.Ceiling(negL);
			// because we want to BEAT the record, not match it, if the Ceil did nothing
			// we need to add 1 more
			if (negL == intNegL)
			{
				intNegL++;
			}

			var diff = intPosL - intNegL + 1;


			result *= diff;
		}

		return result;
	}

}

internal class Puzzle_6_2
{


	public static long Execute()
	{
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_6.txt");
		string[] input = File.ReadAllLines(path);

		var times = Int64.Parse(input[0].Split(":")[1].Replace(" ", ""));
		var distances = Int64.Parse(input[1].Split(":")[1].Replace(" ", ""));

		// Race formula:
		// Distance < (H)*(L-H) where 0 <= H <= L
		// We want to find H
		// D < HL - H^2
		// H^2 - LH + D > 0
		// So Quadratic formula (ax²+bx+c=0) should give us (-b±√(b²-4ac))/(2a) .:
		// -L +/- Sqrt(L^2 - 4(1)(-D)) / 2(1)
		// and all our values should be within that range!
		var l = times * -1;
		var d = distances;

		// Floor PosL
		var posL = (
			-l + Math.Sqrt((l * l) - (4 * 1 * d)
							)
			) / 2;
		var intPosL = (int)Math.Floor(posL);
		// because we want to BEAT the record, not match it, if the floor did nothing
		// we need to subtract 1 more
		if (posL == intPosL)
		{
			intPosL--;
		}

		// Ceil NegL
		var negL = (
			-l - Math.Sqrt((l * l) - (4 * 1 * d)
							)
			) / 2;
		var intNegL = (int)Math.Ceiling(negL);
		// because we want to BEAT the record, not match it, if the Ceil did nothing
		// we need to add 1 more
		if (negL == intNegL)
		{
			intNegL++;
		}

		var diff = intPosL - intNegL + 1;


		return diff;
	}
}