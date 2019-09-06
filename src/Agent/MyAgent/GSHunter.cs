using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.POGame;
using SabberStoneCoreAi.Agent;
using SabberStoneCore.Enums;
using System.Diagnostics;
using System;

namespace SabberStoneCoreAi.Agent.MyAgents
{


	class GSSimulator
	{

		public int calculateTime = 1000;
		public int maxDepth = 5;
		public int maxnNode = 20;

		public GSSimulator()
		{

		}

		public PlayerTask Simulate(SabberStoneCoreAi.POGame.POGame game)
		{
			List<PlayerTask> options = game.CurrentPlayer.Options();
			PlayerTask bestOption = null;
			double bestReward = Double.MinValue;
			double reward = Double.MinValue;

			foreach (PlayerTask option in options)
			{
				if (option.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					// evalute the current game
					// maybe do nothing is the best(directly end turn)
					reward = EvaluateNode(game);
				}
				else
				{
					POGame.POGame simulatedGame = Simulate(game, option);
					// simulate the option
					reward = Search(simulatedGame, maxDepth, maxnNode);
				}

				if (reward > bestReward)
				{
					bestReward = reward;
					bestOption = option;
				}
			}

			Debug.Assert(bestOption != null);
			return bestOption;
		}

		public POGame.POGame Simulate(SabberStoneCoreAi.POGame.POGame game, PlayerTask option)
		{
			LinkedList<PlayerTask> options = new LinkedList<PlayerTask>();
			options.AddLast(option);
			Dictionary<PlayerTask, SabberStoneCoreAi.POGame.POGame> dict = game.Simulate(options.ToList<PlayerTask>());
			SabberStoneCoreAi.POGame.POGame simulatedPOGame = null;
			dict.TryGetValue(option, out simulatedPOGame);
			return simulatedPOGame;
		}

		public double Search(SabberStoneCoreAi.POGame.POGame game, int depth, int nNode)
		{
			List<PlayerTask> options = game.CurrentPlayer.Options();

			if (depth <= 0 || nNode <= 0)
			{
				return EvaluateNode(game);
			}
			double maxReward = Double.MinValue;
			double reward = Double.MinValue;

			foreach (PlayerTask option in options)
			{
				
				if (option.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					reward = EvaluateNode(game);
				}
				else
				{
					POGame.POGame simulatedGame = Simulate(game, option);
					nNode -= 1;
					reward = Search(simulatedGame, depth - 1, nNode);
				}
				if (reward > maxReward)
					maxReward = reward;
				
			}
			return maxReward;
		}


		public double EvaluateNode(POGame.POGame game)
		{
			SabberStoneCoreAi.Score.AggroScore aggroScore = new SabberStoneCoreAi.Score.AggroScore();
			aggroScore.Controller = game.CurrentPlayer;
			return aggroScore.Rate();
		}
	};



	class GSHunter : AbstractAgent
	{
		private Random Rnd = new Random();

		public override void InitializeAgent()
		{
		}

		public override void FinalizeAgent()
		{
			//Nothing to do here
		}

		public override void FinalizeGame()
		{
			//Nothing to do here
		}

		public override PlayerTask GetMove(SabberStoneCoreAi.POGame.POGame poGame)
		{
			int rndSeed = 0;
			long startMillisecond = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			GSSimulator simulator = new GSSimulator();
			PlayerTask bestOption = null;
			try
			{
				bestOption = simulator.Simulate(poGame);
			}
			catch (Exception e)
			{
				bestOption = getBestMove(poGame);
			}
			
			return bestOption;
		}

		public override void InitializeGame()
		{
			//Nothing to do here
		}

		// second plan
		public PlayerTask getBestMove(SabberStoneCoreAi.POGame.POGame poGame)
		{
			List<PlayerTask> options = poGame.CurrentPlayer.Options();
			LinkedList<PlayerTask> minionAttacks = new LinkedList<PlayerTask>();
			foreach (PlayerTask task in options)
			{
				if (task.PlayerTaskType == PlayerTaskType.MINION_ATTACK && task.Target == poGame.CurrentOpponent.Hero)
				{
					minionAttacks.AddLast(task);
				}
			}
			if (minionAttacks.Count > 0)
				return minionAttacks.First.Value;

			PlayerTask summonMinion = null;
			foreach (PlayerTask task in options)
			{
				if (task.PlayerTaskType == PlayerTaskType.PLAY_CARD)
				{
					summonMinion = task;
				}
			}
			if (summonMinion != null)
				return summonMinion;

			else
				return options[0];
		}

	}
}

