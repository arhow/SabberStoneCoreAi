using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Model.Entities;

namespace SabberStoneCoreAi.Score.MyScore
{
    class MyAggroScore : SabberStoneCoreAi.Score.Score
	{
		public int RemainingMana => Controller.RemainingMana;
		public int UsedMana => Controller.UsedMana;
		public int Armer => Controller.Hero.Armor;
		public int OpArmer => Controller.Opponent.Hero.Armor;
		public int BaseMana => Controller.BaseMana;
		public int Turn => Controller.Game.Turn;
		public int BoardZoneCnt => BoardZone.Count;
		public int OpBoardZoneCnt => OpBoardZone.Count;

		public int[,] BoardCoef = new int[10,21];
		public int[] MinionsCoef = new int[11];
		public int[] OpMinionsCoef = new int[11];

		public const int RemainingManaCoef = 0;
		public const int UsedManaCoef = 1;
		public const int ArmerCoef = 2;
		public const int OpArmerCoef = 3;
		public const int BoardZoneCntCoef = 4;
		public const int OpBoardZoneCntCoef = 5;
		public const int HeroHpCoef = 6;
		public const int OpHeroHpCoef = 7;
		public const int HeroAtkCoef = 8;
		public const int OpHeroAtkCoef = 9;
		public const int HandTotCostCoef = 10;
		public const int HandCntCoef = 11;
		public const int OpHandCntCoef = 12;
		public const int DeckCntCoef = 13;
		public const int OpDeckCntCoef = 14;
		public const int MinionTotAtkCoef = 15;
		public const int OpMinionTotAtkCoef = 16;
		public const int MinionTotHealthCoef = 17;
		public const int OpMinionTotHealthCoef = 18;
		public const int MinionTotHealthTauntCoef = 19;
		public const int OpMinionTotHealthTauntCoef = 20;

		public MyAggroScore(int[,] BoardCoef, int[] MinionsCoef, int[] OpMinionsCoef)
		{
			this.BoardCoef = BoardCoef;
			this.MinionsCoef = MinionsCoef;
			this.OpMinionsCoef = OpMinionsCoef;
		}

		public override int Rate()
		{
			if (OpHeroHp < 1)
				return Int32.MaxValue;

			if (HeroHp < 1)
				return Int32.MinValue;

			int result = 0;
			int index = Turn;
			if (index > 9) index = 9;

			result += BoardCoef[index,RemainingManaCoef] * RemainingMana;
			result += BoardCoef[index,UsedManaCoef] * UsedMana;
			result += BoardCoef[index,ArmerCoef] * Armer;
			result += BoardCoef[index,OpArmerCoef] * OpArmer;
			result += BoardCoef[index,BoardZoneCntCoef] * BoardZoneCnt;
			result += BoardCoef[index,OpBoardZoneCntCoef] * OpBoardZoneCnt;
			result += BoardCoef[index,HeroHpCoef] * HeroHp;
			result += BoardCoef[index,OpHeroHpCoef] * OpHeroHp;
			result += BoardCoef[index,HeroAtkCoef] * HeroAtk;
			result += BoardCoef[index,OpHeroAtkCoef] * OpHeroAtk;
			result += BoardCoef[index,HandTotCostCoef] * HandTotCost;
			result += BoardCoef[index,HandCntCoef] * HandCnt;
			result += BoardCoef[index,OpHandCntCoef] * OpHandCnt;
			result += BoardCoef[index,DeckCntCoef] * DeckCnt;
			result += BoardCoef[index,OpDeckCntCoef] * OpDeckCnt;
			result += BoardCoef[index,MinionTotAtkCoef] * MinionTotAtk;
			result += BoardCoef[index,OpMinionTotAtkCoef] * OpMinionTotAtk;
			result += BoardCoef[index,MinionTotHealthCoef] * MinionTotHealth;
			result += BoardCoef[index,OpMinionTotHealthCoef] * OpMinionTotHealth;
			result += BoardCoef[index,MinionTotHealthTauntCoef] * MinionTotHealthTaunt;
			result += BoardCoef[index,OpMinionTotHealthTauntCoef] * OpMinionTotHealthTaunt;

			foreach(Minion m in BoardZone.GetAll())
			{
				if (m != null)
				{
					int cost = m.Cost;
					if (cost > 10) cost = 10;
					result += MinionsCoef[cost];
				}
			}

			foreach (Minion m in OpBoardZone.GetAll())
			{
				if (m != null)
				{
					int cost = m.Cost;
					if (cost > 10) cost = 10;
					result += OpMinionsCoef[cost];
				}
			}

			return result;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
