using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_8
{
	internal class Node
	{
		internal string NodeName { get; set; }
		internal Node? LeftNode { get; set; }
		internal Node? RightNode { get; set; }

		public Node(string name)
		{
			this.NodeName = name;
		}
	}

	internal class Puzzle_8_1
	{
		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_8.txt");
			string[] input = File.ReadAllLines(path);

			Dictionary<string, Node> nodes = new Dictionary<string, Node>();

			// first address is on line 3 (index 2)
			for (int i = 2; i < input.Length; i++)
			{
				// start by going through the input and adding each
				// distinct node to our dict for fast lookup.
				var nodeName = input[i].Substring(0, 3);
				nodes.Add(nodeName, new Node(nodeName));
			}

			// second, set Left and Right Nodes from the dict
			for (int i = 2; i < input.Length; i++)
			{
				// start by going through the input and adding each
				// distinct node to our dict for fast lookup.
				var nodeName = input[i].Substring(0, 3);
				var leftNode = input[i].Substring(7, 3);
				var rightNode = input[i].Substring(12, 3);

				var temp = nodes[nodeName];
				temp.LeftNode = nodes[leftNode];
				temp.RightNode = nodes[rightNode];
			}

			// now iterate over the instructions
			var workingNode = nodes["AAA"];
			var instructionIndex = 0;
			var instructionCount = 0;

			var instructions = input[0];
			var maxInstructions = instructions.Length;

			while (workingNode.NodeName != "ZZZ")
			{
				// read next instruction
				var current = instructions[instructionIndex];
				if (current == 'L')
				{
					workingNode = workingNode.LeftNode;
				}
				else if (current == 'R')
				{
					workingNode = workingNode.RightNode;
				}

				instructionIndex++;
				instructionCount++;

				if (instructionIndex == maxInstructions)
				{
					instructionIndex = 0;
				}
			}


			return instructionCount;
		}
	}

	internal class Puzzle_8_2
	{

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_8.txt");
			string[] input = File.ReadAllLines(path);


			Dictionary<string, Node> nodes = new Dictionary<string, Node>();

			// first address is on line 3 (index 2)
			for (int i = 2; i < input.Length; i++)
			{
				// start by going through the input and adding each
				// distinct node to our dict for fast lookup.
				var nodeName = input[i].Substring(0, 3);
				nodes.Add(nodeName, new Node(nodeName));
			}

			// second, set Left and Right Nodes from the dict
			for (int i = 2; i < input.Length; i++)
			{
				// start by going through the input and adding each
				// distinct node to our dict for fast lookup.
				var nodeName = input[i].Substring(0, 3);
				var leftNode = input[i].Substring(7, 3);
				var rightNode = input[i].Substring(12, 3);

				var temp = nodes[nodeName];
				temp.LeftNode = nodes[leftNode];
				temp.RightNode = nodes[rightNode];
			}

			// now iterate over the instructions
			var workingKeys = nodes.Keys.Where(s => s[2] == 'A');
			var workingNodes = new List<Node>();
			foreach (var key in workingKeys)
			{
				workingNodes.Add(nodes[key]);
			}

			var instructionIndex = 0;
			var instructionCount = 0;

			var instructions = input[0];
			var maxInstructions = instructions.Length;

			List<long> cycleList = new();

			// find the minimum # for each starting path
			for (int i = 0; i < workingNodes.Count; i++)
			{
				var node = workingNodes[i];
				while (!node.NodeName.EndsWith('Z'))
				{
					// read next instruction
					var current = instructions[instructionIndex];
					if (current == 'L')
					{
						node = node.LeftNode;
					}
					else if (current == 'R')
					{
						node = node.RightNode;
					}

					instructionIndex++;
					instructionCount++;

					if (instructionIndex == maxInstructions)
					{
						instructionIndex = 0;
					}
				}
				cycleList.Add(instructionCount);
				instructionCount = 0;
				instructionIndex = 0;
			}

			return LCM(cycleList.ToArray());

		}

		// Aggregate means that lcm(a,b,c) == lcm(a,(lcm(b,c))
		static long LCM(long[] numbers)
		{
			return numbers.Aggregate(lcm);
		}
		static long lcm(long a, long b)
		{
			return Math.Abs(a * b) / GCD(a, b);
		}
		static long GCD(long a, long b)
		{
			return b == 0 ? a : GCD(b, a % b);
		}
	}
}

