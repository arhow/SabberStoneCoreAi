using System;
using System.Collections.Generic;
using System.Text;
using SabberStoneCore.Model;
using SabberStoneCore.Enums;

namespace SabberStoneCoreAi.Agent.MyAgents
{
    class MyDecks
    {
		private static MyDecks instance = null;

		List<List<Card>> Decks = null;// new List<List<Card>>();
		List<CardClass> Heros = null;

		private MyDecks(string path)
		{
			Decks = new List<List<Card>>();
			Heros = new List<CardClass>();
			if (path == null)
			{
				path = System.IO.Path.GetFullPath(@"..\..\..\")+"decks.csv";
				string[] decks = Log.ReadAllLines(path);
				foreach(string strDeck in decks){
					string[] cards = strDeck.Replace(", ", "xxxx").Split(',');
					List<Card> deck = new List<Card>();
					foreach (string strCard in cards)
					{
						deck.Add(Cards.FromName(strCard.Replace("xxxx", ", ").Replace("\"","")));
					}
					Decks.Add(deck);
				}

				for (int i = 0; i <5; i++)
				{
					Heros.Add(CardClass.DRUID);
				}
				for (int i = 0; i < 11; i++)
				{
					Heros.Add(CardClass.HUNTER);
				}
				for (int i = 0; i < 9; i++)
				{
					Heros.Add(CardClass.MAGE);
				}
				for (int i = 0; i < 11; i++)
				{
					Heros.Add(CardClass.PALADIN);
				}
				for (int i = 0; i < 12; i++)
				{
					Heros.Add(CardClass.PRIEST);
				}
				for (int i = 0; i < 6; i++)
				{
					Heros.Add(CardClass.ROGUE);
				}
				for (int i = 0; i < 5; i++)
				{
					Heros.Add(CardClass.SHAMAN);
				}
				for (int i = 0; i < 10; i++)
				{
					Heros.Add(CardClass.WARLOCK);
				}
				for (int i = 0; i < 8; i++)
				{
					Heros.Add(CardClass.WARRIOR);
				}
			}
		}

		public static MyDecks Instance(string path = null)
		{
			if (instance == null)
			{
				instance = new MyDecks(path);
			}
			return instance;
		}

		public List<List<Card>> GetMyDecks()
		{
			return Decks;
		}

		public int GetMyDecksCount()
		{
			return Decks.Count;
		}

		public List<CardClass> GetMyHeros()
		{
			return Heros;
		}

		public int GetMyHerosCount()
		{
			return Heros.Count;
		}


		public static List<Card> AggroPirateWarrior => new List<Card>()
		{
			Cards.FromName("Leeroy Jenkins"),//Cards.FromName("Sir Finley Mrrgglton"),
			Cards.FromName("Fiery War Axe"),
			Cards.FromName("Fiery War Axe"),
			Cards.FromName("Heroic Strike"),
			Cards.FromName("Heroic Strike"),
			Cards.FromName("N'Zoth's First Mate"),
			Cards.FromName("N'Zoth's First Mate"),
			Cards.FromName("Upgrade!"),
			Cards.FromName("Upgrade!"),
			Cards.FromName("Bloodsail Cultist"),
			Cards.FromName("Bloodsail Cultist"),
			Cards.FromName("Frothing Berserker"),
			Cards.FromName("Frothing Berserker"),
			Cards.FromName("Kor'kron Elite"),
			Cards.FromName("Kor'kron Elite"),
			Cards.FromName("Arcanite Reaper"),
			Cards.FromName("Arcanite Reaper"),
			Cards.FromName("Patches the Pirate"),
			Cards.FromName("Mortal Strike"),//Cards.FromName("Small-Time Buccaneer"),
			Cards.FromName("Mortal Strike"),//Cards.FromName("Small-Time Buccaneer"),
			Cards.FromName("Southsea Deckhand"),
			Cards.FromName("Southsea Deckhand"),
			Cards.FromName("Bloodsail Raider"),
			Cards.FromName("Bloodsail Raider"),
			Cards.FromName("Southsea Captain"),
			Cards.FromName("Southsea Captain"),
			Cards.FromName("Dread Corsair"),
			Cards.FromName("Dread Corsair"),
			Cards.FromName("Naga Corsair"),
			Cards.FromName("Naga Corsair"),
		};
	}
}

