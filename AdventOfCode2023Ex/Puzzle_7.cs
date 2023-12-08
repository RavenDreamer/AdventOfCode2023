using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;




internal class Puzzle_7
{

	public enum PokerRank
	{
		fiveKind = 7,
		fourKind = 6,
		fullHouse = 5,
		threeKind = 4,
		twoPair = 3,
		onePair = 2,
		highCard = 1,
	}

	public static Dictionary<char, int> rankDict = new() {
			{ 'A', 13 },
			{ 'K', 12 },
			{ 'Q', 11 },
			{ 'J', 10 },
			{ 'T', 9 },
			{ '9', 8 },
			{ '8', 7 },
			{ '7', 6 },
			{ '6', 5 },
			{ '5', 4 },
			{ '4', 3 },
			{ '3', 2 },
			{ '2', 1 },
	};

	public static Dictionary<char, int> jokerRankDict = new() {
			{ 'A', 13 },
			{ 'K', 12 },
			{ 'Q', 11 },
			{ 'T', 9 },
			{ '9', 8 },
			{ '8', 7 },
			{ '7', 6 },
			{ '6', 5 },
			{ '5', 4 },
			{ '4', 3 },
			{ '3', 2 },
			{ '2', 1 },
			{ 'J', -1 },
	};



	public static Dictionary<char, int> PokerDict
	{
		get
		{
			return new() {
			{ 'A', 0 },
			{ 'K', 0 },
			{ 'Q', 0 },
			{ 'J', 0 },
			{ 'T', 0 },
			{ '9', 0 },
			{ '8', 0 },
			{ '7', 0 },
			{ '6', 0 },
			{ '5', 0 },
			{ '4', 0 },
			{ '3', 0 },
			{ '2', 0 },
		};
		}
	}

	public static PokerRank RankHand(string hand)
	{
		var dict = PokerDict;
		foreach (char c in hand)
		{
			dict[c] = dict[c] + 1;
		}

		var iter = dict.Values;

		// check for fiveKind
		int result;
		var defaultKVP = default(int);

		result = iter.FirstOrDefault(s => s == 5);
		if (!result.Equals(defaultKVP)) return PokerRank.fiveKind;

		result = iter.FirstOrDefault(s => s == 4);
		if (!result.Equals(defaultKVP)) return PokerRank.fourKind;

		// full house is 1 = 3 && 1 = 2
		if (iter.Where(s => s == 3).Count() == 1 && iter.Where(s => s >= 2).Count() == 2)
		{
			return PokerRank.fullHouse;
		}

		// check for threeKind
		result = iter.FirstOrDefault(s => s == 3);
		if (!result.Equals(defaultKVP)) return PokerRank.threeKind;

		// check for two pair
		if (iter.Where(s => s == 2).Count() == 2)
		{
			return PokerRank.twoPair;
		}

		// check for one pair
		if (iter.Where(s => s == 2).Count() == 1)
		{
			return PokerRank.onePair;
		}

		return PokerRank.highCard;
	}

	public static PokerRank RankJokerHand(string hand)
	{

		// five of a kind can't be upgraded
		// four of a kind with 1 joker upgrades to 5 of a kind
		// full house with either 2 or 3 jokers upgrades to 5 of a kind
		// three of a kind upgrades to 4kind with 1 joker, or 5kind with 2 jokers
		//	 (3 of a kind with 3 jokers doesn't upgrade because the jokers are the 3kind)
		// two pair upgardes to full house with 1 joker, or 4kind with 2 jokers
		// one pair upgrades to threekind with 1 joker, stays put with 2 jokers, and 5kind with 3 jokers
		// highcard upgrades to onepair with 1 joker, and stays put otherwise

		PokerRank baseRank = RankHand(hand);
		int jokerCount = hand.Count(s => s == 'J');

		switch (baseRank)
		{
			case PokerRank.fiveKind:
				return PokerRank.fiveKind;
			case PokerRank.fourKind:
				if (jokerCount == 1) return PokerRank.fiveKind;
				if (jokerCount == 0) return PokerRank.fourKind;
				if (jokerCount == 4) return PokerRank.fiveKind;
				break;
			case PokerRank.fullHouse:
				if (jokerCount == 2 || jokerCount == 3) return PokerRank.fiveKind;
				if (jokerCount == 0) return PokerRank.fullHouse;
				break;
			case PokerRank.threeKind:
				if (jokerCount == 1) return PokerRank.fourKind;
				if (jokerCount == 2) return PokerRank.fiveKind;
				if (jokerCount == 3) return PokerRank.fourKind;
				if (jokerCount == 0) return PokerRank.threeKind;
				break;
			case PokerRank.twoPair:
				if (jokerCount == 1) return PokerRank.fullHouse;
				if (jokerCount == 2) return PokerRank.fourKind;
				if (jokerCount == 0) return PokerRank.twoPair;
				break;
			case PokerRank.onePair:
				if (jokerCount == 1) return PokerRank.threeKind;
				if (jokerCount == 2) return PokerRank.threeKind;
				if (jokerCount == 0) return PokerRank.onePair;
				break;
			case PokerRank.highCard:
				if (jokerCount == 1) return PokerRank.onePair;
				if (jokerCount == 0) return PokerRank.highCard;
				break;
		}

		throw new Exception("Oops. You didn't account for this, did you!");

	}


	//public static PokerRank CompareHands(string handOne, string handTwo)

	public class PokerHand : Comparer<PokerHand>
	{
		public string HandValue { get; set; }
		public PokerRank HandRank { get; set; }

		public PokerHand(string hand)
		{
			HandValue = hand;
			HandRank = RankHand(hand);
		}


		public override int Compare(PokerHand? x, PokerHand? y)
		{
			// if the HandRank isn't the same, the higher rank wins
			if (x.HandRank != y.HandRank)
			{
				return x.HandRank.CompareTo(y.HandRank);
			}
			// Otherwise, the first different character wins
			for (int i = 0; i < 5; i++)
			{
				if (x.HandValue[i] != y.HandValue[i])
				{
					return rankDict[x.HandValue[i]].CompareTo(rankDict[y.HandValue[i]]);
				}
			}

			// the hands are identical
			return 0;
		}
	}

	public class JokerHand : Comparer<JokerHand>
	{
		public string HandValue { get; set; }
		public PokerRank HandRank { get; set; }

		public JokerHand(string hand)
		{
			HandValue = hand;
			HandRank = RankJokerHand(hand);
		}

		public override int Compare(JokerHand? x, JokerHand? y)
		{
			// if the HandRank isn't the same, the higher rank wins
			if (x.HandRank != y.HandRank)
			{
				return x.HandRank.CompareTo(y.HandRank);
			}
			// Otherwise, the first different character wins
			for (int i = 0; i < 5; i++)
			{
				if (x.HandValue[i] != y.HandValue[i])
				{
					return jokerRankDict[x.HandValue[i]].CompareTo(jokerRankDict[y.HandValue[i]]);
				}
			}

			// the hands are identical
			return 0;
		}
	}

	internal class Puzzle_7_1
	{
		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_7.txt");
			string[] input = File.ReadAllLines(path);

			List<KeyValuePair<PokerHand, int>> bids = new();

			foreach (string line in input)
			{
				var bits = line.Split(" ");
				// turn bits[0] into a PokerHand
				// and save bits[1] as the bid
				bids.Add(new KeyValuePair<PokerHand, int>(new PokerHand(bits[0]), Int32.Parse(bits[1])));
			}

			// sort the KVPs by their PokerHand
			bids.Sort((a, b) => a.Key.Compare(a.Key, b.Key));

			// multiply by rank and add
			int result = 0;
			var bidRank = 1;
			foreach (var pair in bids)
			{
				result += (pair.Value * bidRank);
				bidRank++;
			}

			return result;
		}
	}

	internal class Puzzle_7_2
	{

		public static int Execute()
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Puzzle_7.txt");
			string[] input = File.ReadAllLines(path);

			List<KeyValuePair<JokerHand, int>> bids = new();

			foreach (string line in input)
			{
				var bits = line.Split(" ");
				// turn bits[0] into a PokerHand
				// and save bits[1] as the bid
				bids.Add(new KeyValuePair<JokerHand, int>(new JokerHand(bits[0]), Int32.Parse(bits[1])));
			}

			// sort the KVPs by their PokerHand
			bids.Sort((a, b) => a.Key.Compare(a.Key, b.Key));

			// multiply by rank and add
			int result = 0;
			var bidRank = 1;
			foreach (var pair in bids)
			{
				result += (pair.Value * bidRank);
				bidRank++;
			}

			return result;
		}
	}
}

