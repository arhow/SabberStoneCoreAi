using System;
using System.Collections.Generic;
using System.Text;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCoreAi.POGame;
using SabberStoneCoreAi.Agent.ExampleAgents;
using SabberStoneCoreAi.Agent.MyAgents;
using SabberStoneCoreAi.Agent;


namespace SabberStoneCoreAi.Score.MyScore
{

	internal class MyProgram
	{
		public static Random random = new Random();

		public static double ucb1Coef = (random.Next(20000) - 10000) / 100;
		public static int mcSearchDepth = Convert.ToInt32(Next(50.0, 20));
		public static int mcOpSearchDepth = Convert.ToInt32(Next(10.0, 5));
		public static int maxOpOnePlayOutTimes = Convert.ToInt32(Next(1000.0, 200));
		public static int maxOnePlayOutTimes = Convert.ToInt32(Next(8000.0, 2000));
		public static int[,] BoardCoef = new int[10,21];
		public static int[] MinionsCoef = new int[11];
		public static int[] OpMinionsCoef = new int[11];
		public static int dimension = 237;

		//public static string coefString = "1790.9975756918814,1994.9976318602733,2127.208922821077,43171.567770567555,41885.693324509484,55523.09017947657,5165.593152228012,-43358.02808646331,-33992.4083718597,4637.840286146347,53310.432887171126,43231.394806314805,-15181.123685158374,-3407.6286429034926,37293.16825021208,38836.20554789508,-34016.791173946796,-55472.73517126283,-30872.790179602798,16445.81702724511,-57628.04731107251,40379.12230768994,-52721.92027554938,47231.74630119117,-37803.825672763356,-54402.34055400615,-18927.895350520077,63313.1381903792,6471.998345380817,-15072.407333689205,-32043.521985033996,26800.58741229645,-21765.547943938145,11504.208191288295,-35687.02635421215,-17896.73459632613,-3689.093302237832,14494.29810345675,-12470.844394140491,-10453.882070841708,61987.24865359794,-5704.753007140451,-49279.48293271378,-3759.5362726839658,10878.710846876493,22700.88918690534,-3881.8424871613547,10847.991286843846,-7393.743028213114,-48885.21262274838,4075.197866274808,-10082.296265324187,-29235.493981616768,-41726.841880310494,1116.9799473411138,51506.86795949979,-26888.80904978714,-18798.63676292455,11739.374145039614,-6361.083077923746,14252.914794624741,-15819.77908573917,2848.80150928354,-56772.131564846226,9348.270847138354,-10488.121066868229,-16030.35074857431,-52575.709377304294,-34461.817685589725,4893.118394472223,-30721.124399968063,-11372.17726680511,494.529428430716,4615.686816520605,-11098.29753441498,-43912.05286738606,17667.334000983374,-11914.545683515224,-12965.811188888747,-23849.67217721639,46960.81742176546,23097.5766443435,-7961.017493648805,-37742.423082497735,-119.9992629842189,41582.5832271681,-17912.217756901147,34413.3952030719,16323.869574523607,-2907.4057779873783,-53726.24833924506,-11430.21589684494,-23727.759112491694,-45882.25896435499,43639.98280857256,-639.4787003000761,-31707.47769919286,18967.31704257555,-1822.9330644576298,55063.94074262861,1155.9593078663345,-11050.782115191174,-23523.974085819682,-22032.70181918667,34267.930746749116,2736.054330054789,59142.605224262225,32924.627991518726,15743.678240227495,-50792.252704845945,-17545.72746782768,-651.0498566263672,61868.90832603058,17319.713175385627,55334.67277694448,60649.43730186039,24121.98184512851,-52744.05957316915,37356.416033559406,14226.767955366267,-58616.40647746577,23401.345704897245,12003.551772426052,21273.433344960416,38674.65454216997,55791.848555791345,4760.52337557197,-2105.475983684992,34814.59670392663,-12432.7619279016,-54755.567776080454,28729.471037627227,4721.03816204502,43308.53763645869,-45387.28176604787,26590.392398471326,-6503.097322372595,-57614.65341399073,11777.738395765471,4545.056087725063,38740.676627959525,-41200.8782087845,-9303.300747238934,47968.30427321979,-43900.73303185766,25852.263735463788,31815.757111761228,-16245.919020688048,56531.0421494298,60846.7757615954,15757.890796953008,-56352.47727307953,55456.068926429994,-9712.241619971412,-16417.90012513719,-3494.6040151486436,402.0678922249099,-22606.48132776971,-6920.029560114959,-55634.44177780339,-44586.52006487464,36995.3045748755,-23249.810149181812,-58786.59913523546,-20012.212164718083,-5647.241545013577,-4331.861416903816,-17872.340300508396,-17381.945674982784,-7286.752650152284,593.3544176177172,55563.408115501465,26531.151528534323,28677.277014267373,9627.582262891608,62187.16146401258,-9298.764813518914,24494.267838933727,23972.43647464404,-6759.6581564188355,-55752.804584493286,41780.96112577777,-51200.55799371882,-19662.61074914702,10893.60822290875,-55792.30035059868,13395.80318023039,-10250.168074741063,5878.726453227072,55748.38199090707,57887.59275023274,-38933.148571984246,40156.9912002944,686.7866402261092,-56665.36044425287,-16881.829282293038,17997.934760499127,-24457.9697421939,-36024.670925606595,9128.791421374983,6834.229445537045,-31643.86926063844,-24436.002552746755,-35319.59666121367,-23716.093135505915,-29730.05043199626,-17030.985924447912,-8975.734167973356,-33964.951126503634,10641.750235860789,-12731.068819362623,2930.4811281094526,29078.77296237427,62086.991236416565,46936.12826967302,37104.05920467316,38551.67481792985,8453.411559646293,10666.527014550737,40075.564669173975,31161.57593801263,42941.134369544976,62630.93002735708,61847.73155294983,60334.35396968278,29329.713273347876,-41865.64495676631,-58821.011878548685,-30121.506797744798,-29124.12090056565,-24242.407546591327,-46904.418226636415,-43806.81364211775,-7431.370653555152,-19602.250465089634,-5472.906917842009,-19302.764445640394";
		public static string coefString = "8.18139009651699,30,28,10000,1320,10000,706,-7191,-6525,-2430,4434,8829,-2228,1195,8066,6626,-4660,-10000,-8647,7975,-10000,8461,-10000,9808,-8232,-10000,-3203,10000,1349,-2492,-6844,8409,5821,-1488,-9711,-3754,1726,5786,-566,6990,10000,-4239,-8694,-5123,4896,4403,-325,4545,-3436,-4497,2641,-8769,-10000,-6406,150,10000,-2633,-4883,2050,28,2691,-2553,3400,-10000,947,-376,-4479,-10000,-3580,4691,-6017,-2266,-3051,6136,1597,-10000,6700,-2353,-4171,-5343,6417,4173,-748,-6245,2626,9891,-2507,5855,-7639,-10000,-10000,-3346,3918,-6614,10000,2421,-4732,10000,-9826,10000,4144,3644,-3857,-1888,997,111,10000,2242,5646,-10000,-5568,860,10000,5240,9522,10000,5726,-9943,3986,4611,-10000,8616,-4447,-3409,7384,10000,3739,3429,8025,-3979,-9289,5025,-187,4890,-7164,4252,-10000,-9880,6609,6477,7313,-4367,-8576,10000,-3120,1642,7168,-2794,10000,10000,5249,-10000,10000,-6068,-5724,-2757,10000,-5902,490,-10000,-9679,5042,-6682,-9895,-4594,-1221,568,-8285,-3569,-2599,-5964,6877,8940,-5063,1979,10000,1763,5085,3136,139,-10000,10000,-10000,-6198,-1293,-10000,-3054,-6314,3602,8955,10000,-966,5218,-2989,-10000,-439,5252,-1455,-9641,-8558,425,-4989,-3104,-3128,-4922,-2993,-6042,2408,-6216,8626,-2,-4720,-4265,10000,7951,9118,9993,1176,8866,8789,2402,7178,10000,10000,10000,5817,-6575,-10000,-4691,-4613,-5378,-7961,-6289,-1567,-5677,-638,-1105";

		public static void SetCoef(double[] X)
		{
			int index = 0;

			ucb1Coef = X[index++];
			mcSearchDepth = (int)X[index++];
			mcOpSearchDepth = (int)X[index++];
			maxOpOnePlayOutTimes = (int)X[index++];
			maxOnePlayOutTimes = (int)X[index++];
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 21; j++)
				{
					BoardCoef[i, j] = (int)X[index++];
				}
			}

			for (int i = 0; i < 11; i++)
			{
				MinionsCoef[i] = (int)X[index++];
			}

			for (int i = 0; i < 11; i++)
			{
				OpMinionsCoef[i] = (int)X[index++];
			}
		}

		public static double[] String2Coef(string coefString)
		{
			string[] coefs = coefString.Split(',');
			double[] X = new double[dimension];
			int index = 0;
			foreach (string strCoef in coefs)
			{
				X[index++] = Convert.ToDouble(strCoef);
			}
			SetCoef(X);

			return X;
		}


		// check the best normal deck
		public static double Benchmark2(double[] X)
		{


			SetCoef(X);

			List<List<Card>> decks = MyDecks.Instance().GetMyDecks();
			List<CardClass> heros = MyDecks.Instance().GetMyHeros();



			for(int i = 40; i < 60; i++)
			{
				for (int j = 0; j < decks.Count; j++)
				{
					//todo: rename to Main
					GameConfig gameConfig = new GameConfig
					{
						StartPlayer = 1,
						Player1HeroClass = heros[i],
						Player2HeroClass = heros[j],
						FillDecks = false,
						Logging = false,
						Player1Deck = decks[i],
						Player2Deck = decks[j]
					};

					//Console.WriteLine("Setup POGameHandler");
					AbstractAgent player1 = new UCTHunter();
					AbstractAgent player2 = new UCTHunter();
					var gameHandler = new POGameHandler(gameConfig, player1, player2, debug: false);

					//Console.WriteLine("PlayGame");
					gameHandler.PlayGames(3);
					GameStats gameStats = gameHandler.getGameStats();

					gameStats.printResults();

					double winRate = (double)gameStats.PlayerA_Wins / (double)gameStats.GamesPlayed;

					Log.Instance("coef3.txt").Append(heros[i].ToString() + " " +i.ToString()+":" + heros[j].ToString() + " " + winRate.ToString());

					//double expectedMin = 1;

					//return Math.Pow(winRate - expectedMin, 2);
				}
			}


			return 0.0;
		}


		public static double Benchmark(double[] X)
		{

			// create random coef run 10 times and get the win rate
			SetCoef(X);

			//todo: rename to Main
			GameConfig gameConfig = new GameConfig
			{
				StartPlayer = 1,
				Player1HeroClass = CardClass.MAGE,
				Player2HeroClass = CardClass.MAGE,
				FillDecks = true,
				Logging = false,
			};

			//Console.WriteLine("Setup POGameHandler");
			AbstractAgent player1 = new FaceHunter();
			AbstractAgent player2 = new UCTHunter();
			var gameHandler = new POGameHandler(gameConfig, player1, player2, debug: false);

			//Console.WriteLine("PlayGame");
			gameHandler.PlayGames(20);
			GameStats gameStats = gameHandler.getGameStats();

			gameStats.printResults();
			//Console.WriteLine("Setup gameConfig");

			double winRate = (double)gameStats.PlayerB_Wins / (double)gameStats.GamesPlayed;
			var str = new StringBuilder();

			Log.Instance("coef.txt").Append(FullPrintCoef(winRate));

			double expectedMin = 1;

			return Math.Pow(winRate - expectedMin, 2);
		}

		private static void Main(string[] args)
		{

			double[] X = String2Coef(coefString);

			Benchmark2(X);

			//double[] minCoef = new double[dimension];
			//double[] maxCoef = new double[dimension];

			//int minCoefIndex = 0;
			//int maxCoefIndex = 0;

			//minCoef[minCoefIndex++] = -100;
			//minCoef[minCoefIndex++] = 10;
			//minCoef[minCoefIndex++] = 10;
			//minCoef[minCoefIndex++] = 200;
			//minCoef[minCoefIndex++] = 200;

			//maxCoef[maxCoefIndex++] = 100;
			//maxCoef[maxCoefIndex++] = 100;
			//maxCoef[maxCoefIndex++] = 100;
			//maxCoef[maxCoefIndex++] = 10000;
			//maxCoef[maxCoefIndex++] = 10000;

			//for(int i = 0; i < 10; i++)
			//{
			//	for (int j =0; j < 21;j++)
			//	{
			//		minCoef[minCoefIndex++] = -10000;
			//		maxCoef[maxCoefIndex++] = 10000;
			//	}
			//}

			//for (int i = 0; i < 11; i++)
			//{
			//	minCoef[minCoefIndex++] = 0;
			//	maxCoef[maxCoefIndex++] = 10000;
			//}

			//for (int i = 0; i < 11; i++)
			//{
			//	minCoef[minCoefIndex++] = -10000;
			//	maxCoef[maxCoefIndex++] = 0;
			//}

			// PSO.Run(Benchmark, 10, dimension, minCoef, maxCoef, 100, warmStart:true);


		}

		public static string FullPrintCoef(double winRate)
		{
			//double ucb1Coef = (random.Next(20000) - 10000) / 100;
			//int mcSearchDepth = Convert.ToInt32(Next(50.0, 20));
			//int mcOpSearchDepth = Convert.ToInt32(Next(10.0, 5));
			//int maxOpOnePlayOutTimes = Convert.ToInt32(Next(1000.0, 200));
			//int maxOnePlayOutTimes = Convert.ToInt32(Next(8000.0, 2000));
			//int[][] BoardCoef = new int[10][];
			//int[] MinionsCoef = new int[11];
			//int[] OpMinionsCoef = new int[11];
			var str = new StringBuilder();
			str.Append(ucb1Coef.ToString());
			str.Append(',');
			str.Append(mcSearchDepth.ToString());
			str.Append(',');
			str.Append(mcOpSearchDepth.ToString());
			str.Append(',');
			str.Append(maxOpOnePlayOutTimes.ToString());
			str.Append(',');
			str.Append(maxOnePlayOutTimes.ToString());
			str.Append(',');

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 21; j++)
				{
					str.Append(BoardCoef[i,j].ToString());
					str.Append(',');
				}
			}

			for (int i = 0; i < 11; i++)
			{
				str.Append(MinionsCoef[i].ToString());
				str.Append(',');
			}

			for (int i = 0; i < 11; i++)
			{
				str.Append(OpMinionsCoef[i].ToString());
				str.Append(',');
			}
			str.Append(winRate.ToString());

			return str.ToString();

		}


		public static double Next(double mu = 0.0, double sigma = 1.0, bool getCos = true)
		{
			

			if (getCos)
			{
				double rand = 0.0;
				while ((rand = random.NextDouble()) == 0.0) ;
				double rand2 = random.NextDouble();
				double normrand = Math.Sqrt(-2.0 * Math.Log(rand)) * Math.Cos(2.0 * Math.PI * rand2);
				normrand = normrand * sigma + mu;
				return normrand;
			}
			else
			{
				double rand;
				while ((rand = random.NextDouble()) == 0.0) ;
				double rand2 = random.NextDouble();
				double normrand = Math.Sqrt(-2.0 * Math.Log(rand)) * Math.Sin(2.0 * Math.PI * rand2);
				normrand = normrand * sigma + mu;
				return normrand;
			}
		}
	}
}
