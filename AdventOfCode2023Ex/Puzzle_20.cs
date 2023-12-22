using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_20
{
	public enum Pulse
	{
		lowPulse,
		highPulse,
	}

	internal interface IModule
	{
		internal void ReceivePulse(IModule source, Pulse p);
		internal void AddDestination(IModule destination);

		internal string GetName();
	}

	internal struct PulseOperation
	{
		public Pulse Pulse { get; set; }
		public IModule Source { get; set; }
		public IModule Destination { get; set; }
	}

	internal static Queue<PulseOperation> pulseOperations = new();

	internal class FlipFlopModule : IModule
	{
		bool isOff { get; set; } = true;

		List<IModule> Destinations { get; set; } = new();

		void IModule.ReceivePulse(IModule _, Pulse p)
		{
			if (p == Pulse.highPulse) return;

			// flip "offness" on a low pulse
			isOff = !isOff;

			// we went from On->Off
			if (isOff)
			{
				foreach (IModule m in Destinations)
				{
					pulseOperations.Enqueue(new PulseOperation() { Destination = m, Pulse = Pulse.lowPulse, Source = this });
				}
			}
			// we went from Off->On
			else
			{
				foreach (IModule m in Destinations)
				{
					pulseOperations.Enqueue(new PulseOperation() { Destination = m, Pulse = Pulse.highPulse, Source = this });
				}
			}
		}

		void IModule.AddDestination(IModule destination)
		{
			Destinations.Add(destination);
		}

		string IModule.GetName()
		{
			return Name;
		}

		public string Name { get; private set; }
		public FlipFlopModule(string name)
		{
			Name = name;
		}
	}

	internal class ConjunctionModule : IModule
	{
		List<IModule> Destinations { get; set; } = new();

		Dictionary<IModule, Pulse> pulseMemory = new Dictionary<IModule, Pulse>();

		internal void AddSource(IModule source)
		{
			pulseMemory[source] = Pulse.lowPulse;
		}

		string IModule.GetName()
		{
			return Name;
		}

		// repeat pulse to all listeners
		void IModule.ReceivePulse(IModule source, Pulse p)
		{
			// update the memory
			pulseMemory[source] = p;

			// check if every value in pulseMemory is HighPulse
			if (pulseMemory.Values.All(s => s == Pulse.highPulse))
			{
				// everything is high, send a low pulse
				foreach (IModule m in Destinations)
				{
					pulseOperations.Enqueue(new PulseOperation() { Destination = m, Pulse = Pulse.lowPulse, Source = this });
				}
			}
			else
			{
				// else, send a high pulse
				foreach (IModule m in Destinations)
				{
					pulseOperations.Enqueue(new PulseOperation() { Destination = m, Pulse = Pulse.highPulse, Source = this });
				}
			}
		}

		void IModule.AddDestination(IModule destination)
		{
			Destinations.Add(destination);
		}
		public string Name { get; private set; }
		public ConjunctionModule(string name)
		{
			Name = name;
		}


	}

	internal class BroadcastModule : IModule
	{
		List<IModule> Destinations { get; set; } = new();

		string IModule.GetName()
		{
			return Name;
		}

		// repeat pulse to all listeners
		void IModule.ReceivePulse(IModule _, Pulse p)
		{
			foreach (IModule m in Destinations)
			{
				pulseOperations.Enqueue(new PulseOperation() { Destination = m, Pulse = p, Source = this });
			}
		}

		void IModule.AddDestination(IModule destination)
		{
			Destinations.Add(destination);
		}

		public string Name { get; private set; }

		public BroadcastModule(string name)
		{
			Name = name;
		}
	}


	internal class OutputModule : IModule
	{
		string IModule.GetName()
		{
			return Name;
		}

		void IModule.ReceivePulse(IModule _, Pulse p)
		{
			return;
		}

		void IModule.AddDestination(IModule _)
		{
			return;
		}

		public string Name { get; private set; }
		public OutputModule(string name)
		{
			Name = name;
		}
	}



	internal class Part_1
	{
		const int BUTTON_PUSHES_CONST = 1000;

		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_20.txt");
			string[] input = File.ReadAllLines(path);

			Dictionary<string, IModule> modules = new();
			Dictionary<string, List<string>> moduleDestinations = new();

			foreach (string s in input)
			{
				// split on " -> "
				var stringBits = s.Split(" -> ");
				// grab the name and create the appropriate broadcaster
				var name = stringBits[0];
				var destinationModules = stringBits[1].Split(", ");
				if (name == "broadcaster")
				{
					// we have the broadcaster module
					// grab its destinations

					modules["broadcaster"] = new BroadcastModule("broadcaster");
					moduleDestinations["broadcaster"] = new List<string>(destinationModules);
				}
				else
				{
					// look at the first character to determine if we have a FlipFlop or a Conjunction
					if (name[0] == '%')
					{
						// Flip-Flop
						modules[name[1..]] = new FlipFlopModule(name[1..]);
						moduleDestinations[name[1..]] = new List<string>(destinationModules);
					}
					else if (name[0] == '&')
					{
						// Conjunction
						modules[name[1..]] = new ConjunctionModule(name[1..]);
						moduleDestinations[name[1..]] = new List<string>(destinationModules);
					}
				}
			}

			List<Tuple<string, IModule>> outputModules = new();

			// now that all the modules have been instantiated, we can hook them up
			foreach (var pair in modules)
			{
				var destinations = moduleDestinations[pair.Key];

				foreach (var dest in destinations)
				{
					// empty output modules are a thing
					if (modules.ContainsKey(dest) == false)
					{
						var output = new OutputModule(dest);
						outputModules.Add(new Tuple<string, IModule>(dest, output));
						pair.Value.AddDestination(output);
						continue;
					}

					pair.Value.AddDestination(modules[dest]);

					if (modules[dest] is ConjunctionModule)
					{
						(modules[dest] as ConjunctionModule).AddSource(pair.Value);
					}
				}
			}

			// I don't know if this is strictly necessary
			foreach (var tuple in outputModules)
			{
				modules[tuple.Item1] = tuple.Item2;
			}

			long lowPulseCount = 0;
			long highPulseCount = 0;

			for (int i = 0; i < BUTTON_PUSHES_CONST; i++)
			{
				pulseOperations.Enqueue(new PulseOperation() { Destination = modules["broadcaster"], Pulse = Pulse.lowPulse });

				while (pulseOperations.Count > 0)
				{
					var op = pulseOperations.Dequeue();

					op.Destination.ReceivePulse(op.Source, op.Pulse);

					if (op.Pulse == Pulse.lowPulse)
					{
						lowPulseCount++;
					}
					else
					{
						highPulseCount++;
					}
				}
			}

			return lowPulseCount * highPulseCount;
		}

	}

	internal class Part_2
	{


		public static long Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_19.txt");
			string[] input = File.ReadAllLines(path);


			return 0;
		}
	}
}