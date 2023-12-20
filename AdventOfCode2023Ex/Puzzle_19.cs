using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_19
{


	internal class Rule
	{
		internal string ResultWorkflow { get; set; }
		internal string Operator { get; set; } = "=";

		internal string Parameter { get; set; }
		internal int Value { get; set; }
	}

	internal class Workflow
	{
		internal string Name { get; set; }
		internal List<Rule> Ruleset { get; set; } = new List<Rule>();

		internal string ProcessPart(Dictionary<string, int> partAttr)
		{
			string result = "";
			foreach (Rule r in Ruleset)
			{
				switch (r.Operator)
				{
					case "<":
						if (partAttr[r.Parameter] < r.Value)
						{
							result = r.ResultWorkflow;
							return result;
						}
						break;
					case ">":
						if (partAttr[r.Parameter] > r.Value)
						{
							result = r.ResultWorkflow;
							return result; ;
						}
						break;
					default:
						// the fallthrough case if none of the other
						// rules pass
						result = r.ResultWorkflow;
						return result;
				}
			}
			// shouldn't be reachable
			return result;
		}

		internal List<Tuple<string, AttrRange>> GenerateDownstreamRanges(AttrRange range)
		{
			var results = new List<Tuple<string, AttrRange>>();
			AttrRange workingRange = range.Clone();


			foreach (var r in Ruleset)
			{
				// attempt to fit the range to the rule
				switch (r.Operator)
				{
					// Parameter max value must be < r.Value
					case "<":
						//
						if (workingRange.Values[r.Parameter].Max < r.Value)
						{
							// we're entirely contained already, nothing to do
						}
						else
						{
							// generate two ranges, one where Max == r.Value - 1 
							// and one where Min = r.Value [workingRange]
							var recursiveChild = workingRange.Clone();
							workingRange.Values[r.Parameter].Min = r.Value;
							recursiveChild.Values[r.Parameter].Max = r.Value - 1;
							//workingRange continues on to the next Rule
							// recursiveChild gets added to the results
							results.Add(new Tuple<string, AttrRange>(r.ResultWorkflow, recursiveChild));
						}
						break;
					case ">":
						//
						if (workingRange.Values[r.Parameter].Min > r.Value)
						{
							// we're entirely contained already, nothing to do
						}
						else
						{
							// generate two ranges, one where Min == r.Value + 1 
							// and one where Max = r.Value [workingRange]
							var recursiveChild = workingRange.Clone();
							workingRange.Values[r.Parameter].Max = r.Value;
							recursiveChild.Values[r.Parameter].Min = r.Value + 1;
							//workingRange continues on to the next Rule
							// recursiveChild gets added to the results
							results.Add(new Tuple<string, AttrRange>(r.ResultWorkflow, recursiveChild));
						}

						break;
					default:
						// the fallthrough case if none of the other
						// update the ruleset to check, but 
						// keep the range the same
						var cloneRange = workingRange.Clone();
						// add the new range to R's next workflow
						results.Add(new Tuple<string, AttrRange>(r.ResultWorkflow, cloneRange));
						break;
				}
			}

			return results;
		}
	}

	internal class NumRange
	{
		public long Min { get; set; }
		public long Max { get; set; }
	}

	internal class AttrRange
	{
		internal Dictionary<string, NumRange> Values { get; } = new Dictionary<string, NumRange>();

		public AttrRange()
		{
			Values["x"] = new NumRange() { Min = 1, Max = 4000 };
			Values["m"] = new NumRange() { Min = 1, Max = 4000 };
			Values["a"] = new NumRange() { Min = 1, Max = 4000 };
			Values["s"] = new NumRange() { Min = 1, Max = 4000 };
		}

		internal AttrRange Clone()
		{
			var clonse = new AttrRange() { };

			clonse.Values["x"] = new NumRange() { Min = this.Values["x"].Min, Max = this.Values["x"].Max };
			clonse.Values["m"] = new NumRange() { Min = this.Values["m"].Min, Max = this.Values["m"].Max };
			clonse.Values["a"] = new NumRange() { Min = this.Values["a"].Min, Max = this.Values["a"].Max };
			clonse.Values["s"] = new NumRange() { Min = this.Values["s"].Min, Max = this.Values["s"].Max };

			return clonse;
		}

		internal long CalculateDistinct()
		{
			return (Values["x"].Max - Values["x"].Min + 1) *
				(Values["m"].Max - Values["m"].Min + 1) *
				(Values["a"].Max - Values["a"].Min + 1) *
				(Values["s"].Max - Values["s"].Min + 1);
		}
	}

	internal class Part_1
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_19.txt");
			string[] input = File.ReadAllLines(path);

			Dictionary<string, Workflow> workDict = new();
			List<Dictionary<string, int>> partList = new();

			int i = 0;
			// Workflows
			while (input[i] != "")
			{
				// the name of the workflow is the characters before the {
				var nameLimit = input[i].IndexOf('{');
				var name = input[i].Substring(0, nameLimit);
				var allRules = input[i].Substring(nameLimit + 1, input[i].Length - 2 - nameLimit);
				// rules are comma separated
				List<Rule> rules = new();
				foreach (var rs in allRules.Split(","))
				{
					//split on the colon, if present
					var splitbits = rs.Split(":");
					if (splitbits.Length > 1)
					{
						//e.g. s>537:gd
						rules.Add(new Rule()
						{
							Parameter = splitbits[0].Substring(0, 1),
							Operator = splitbits[0].Substring(1, 1),
							Value = int.Parse(splitbits[0].Substring(2)),
							ResultWorkflow = splitbits[1],
						});
					}
					else
					{
						// just the resultcase
						rules.Add(new Rule() { ResultWorkflow = rs });
					}
				}
				Workflow wkFlow = new Workflow()
				{
					Name = name,
					Ruleset = rules,
				};
				// I guess name doesn't need to be stored on the workflow, probably
				workDict.Add(name, wkFlow);

				i++;
			}
			// skip a line
			i++;
			// parts
			for (int j = i; j < input.Length; j++)
			{
				// removes the {...} 
				var part = input[j].Substring(1, input[j].Length - 2);
				var attrSplit = part.Split(",");
				var partAttrs = new Dictionary<string, int>();
				partAttrs["x"] = int.Parse(attrSplit[0].Substring(2));
				partAttrs["m"] = int.Parse(attrSplit[1].Substring(2));
				partAttrs["a"] = int.Parse(attrSplit[2].Substring(2));
				partAttrs["s"] = int.Parse(attrSplit[3].Substring(2));

				partList.Add(partAttrs);
			}

			// process parts
			List<Dictionary<string, int>> acceptedParts = new();
			foreach (var part in partList)
			{
				string currentWorkflow = "in";
				while (currentWorkflow != "A" && currentWorkflow != "R")
				{
					currentWorkflow = workDict[currentWorkflow].ProcessPart(part);
				}

				if (currentWorkflow == "A")
				{
					acceptedParts.Add(part);
				}
			}

			long atrSum = acceptedParts.Sum(s => s["x"] + s["m"] + s["a"] + s["s"]);

			return atrSum;
		}

	}

	internal class Part_2
	{


		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_19.txt");
			string[] input = File.ReadAllLines(path);

			Dictionary<string, Workflow> workDict = new();
			List<Dictionary<string, int>> partList = new();

			int i = 0;
			// Workflows
			while (input[i] != "")
			{
				// the name of the workflow is the characters before the {
				var nameLimit = input[i].IndexOf('{');
				var name = input[i].Substring(0, nameLimit);
				var allRules = input[i].Substring(nameLimit + 1, input[i].Length - 2 - nameLimit);
				// rules are comma separated
				List<Rule> rules = new();
				foreach (var rs in allRules.Split(","))
				{
					//split on the colon, if present
					var splitbits = rs.Split(":");
					if (splitbits.Length > 1)
					{
						//e.g. s>537:gd
						rules.Add(new Rule()
						{
							Parameter = splitbits[0].Substring(0, 1),
							Operator = splitbits[0].Substring(1, 1),
							Value = int.Parse(splitbits[0].Substring(2)),
							ResultWorkflow = splitbits[1],
						});
					}
					else
					{
						// just the resultcase
						rules.Add(new Rule() { ResultWorkflow = rs });
					}
				}
				Workflow wkFlow = new Workflow()
				{
					Name = name,
					Ruleset = rules,
				};
				// I guess name doesn't need to be stored on the workflow, probably
				workDict.Add(name, wkFlow);

				i++;
			}

			const int RATING_MAX = 4000;
			const int RATING_MIN = 1;

			// for each workflow, build partial ranges
			// e.g. in{s<1351:px,qqz}
			// read the first rule and recurse two new partial ranges:
			// s < 1351 -> px and s < 1351 -> qqz

			List<Tuple<string, AttrRange>> activeList = new();
			List<AttrRange> allowedRanges = new();

			activeList.Add(new Tuple<string, AttrRange>("in", new AttrRange()));
			while (activeList.Count > 0)
			{
				Workflow wkFlow = workDict[activeList[0].Item1];
				List<Tuple<string, AttrRange>> modRanges = wkFlow.GenerateDownstreamRanges(activeList[0].Item2);

				activeList.RemoveAt(0);
				foreach (var mod in modRanges)
				{
					if (mod.Item1 == "A")
					{
						allowedRanges.Add(mod.Item2);
					}
					else if (mod.Item1 == "R")
					{
						// do nothing, this is a dead-end
					}
					else
					{
						// add to list and recurse
						activeList.Add(mod);
					}
				}
			}

			long possibilities = allowedRanges.Sum(r => r.CalculateDistinct());

			return possibilities;
		}
	}
}