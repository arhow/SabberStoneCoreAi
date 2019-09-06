using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.POGame;
using SabberStoneCoreAi.Agent;
using SabberStoneCore.Enums;
using System.Diagnostics;
using System;
using System.Text;
using SabberStoneCoreAi.Score.MyScore;

namespace SabberStoneCoreAi.Agent.MyAgents
{
	
	class UCTNode
	{
		public double reward;
		public int nj;
		public int j;
		public int N;
		public UCTNode parent;
		public Dictionary<int, UCTNode> childDict;
		public POGame.POGame currentPOGame;
		public PlayerTask option;

		public UCTNode(double reward,
			int nj,
			int j,
			int N,
			UCTNode parent,
			PlayerTask option,
			SabberStoneCoreAi.POGame.POGame poGame,
			Dictionary<int, UCTNode> childDict)
		{
			this.reward = reward;
			this.nj = nj;
			this.j = j;
			this.N = N;
			this.parent = parent;
			this.currentPOGame = poGame;
			this.option = option;
			this.childDict = childDict;
		}

		public UCTNode(SabberStoneCoreAi.POGame.POGame poGame)
		{
			this.reward = 0;
			this.nj = 0;
			this.j = 0;
			this.N = 0;
			this.parent = null;
			this.currentPOGame = poGame;
			this.option = null;
			this.childDict = new Dictionary<int, UCTNode>();
		}

		public UCTNode GetChild(int j)
		{
			UCTNode child = null;
			this.childDict.TryGetValue(j, out child);
			return child;
		}

		public bool IsCurrentPlayerTurnEnd()
		{
			// only remain CONCEDE, END_TURN
			return currentPOGame.CurrentPlayer.Options().Count() == 1 &&
				currentPOGame.CurrentPlayer.Options()[0].PlayerTaskType == PlayerTaskType.END_TURN;
		}

		public bool IsGameOver()
		{
			return currentPOGame.State == State.COMPLETE;
		}

		public UCTNode AddChild(UCTNode parent, PlayerTask option, int j, POGame.POGame simulatedPOGame)
		{
			Debug.Assert(simulatedPOGame != null);
			Debug.Assert(option != null);
			UCTNode newChildNode = new UCTNode(0, 0, j, 0, parent, option, simulatedPOGame, new Dictionary<int, UCTNode>());
			parent.childDict.Add(j, newChildNode);
			return newChildNode;
		}

		public void UpdateReward(double reward)
		{
			this.nj++;
			this.reward += reward;
			this.parent.N += 1;
		}

		public string FullPrint()
		{
			return currentPOGame.FullPrint();
		}

		public string FullPathPrint()
		{
			var str = new StringBuilder();
			UCTNode currentNode = this;
			do
			{
				str.AppendLine(PlayerTaskPrint(currentNode.option));
				currentNode = currentNode.parent;
			} while (currentNode != null);
			return str.ToString();
		}

		public string PlayerTaskPrint(PlayerTask option)
		{
			var str = new StringBuilder();
			if (option != null)
				str.Append($"[{option.Source}][{option.PlayerTaskType}][{option.Target}]");
			return str.ToString();
		}
	};

	class UCTSimulator
	{
		public long maxOptionCalculateTime = 5000;
		public long maxTurnCalculateTime = 70000;
		public int rndSeed = 0;
		public int checkIntegralTree = 2000;

		public double ucb1Coef = 1.0f;
		public int mcSearchDepth = 50;
		public int mcOpSearchDepth = 10;
		public int maxOpOnePlayOutTimes = 1000;
		public int maxOnePlayOutTimes = 8000;

		public Random rnd = null;
		public UCTNode root = null;
		//CHOOSE, CONCEDE, END_TURN, HERO_ATTACK, HERO_POWER, MINION_ATTACK, PLAY_CARD
		List<PlayerTaskType> availableOptionTypes = new List<PlayerTaskType>();
		List<PlayerTaskType> availableOpOptionTypes = new List<PlayerTaskType>();

		public int[,] BoardCoef = new int[10,21];
		public int[] MinionsCoef = new int[11];
		public int[] OpMinionsCoef = new int[11];


		// for check
		private bool _debug_ = false;
		public int maxRealMCSearchTimes = 0;
		public int maxRealOnePlayOutTimes = 0;
		public long longestOptionCalcTime = 0;
		public long longestTurnCalcTime = 0;

		public List<PlayerTask> list = new List<PlayerTask>();


		public UCTSimulator(SabberStoneCoreAi.POGame.POGame poGame, int randomSeed, Random rnd,
			double ucb1Coef, int mcSearchDepth, int mcOpSearchDepth, int maxOpOnePlayOutTimes, int maxOnePlayOutTimes,
			int[,] BoardCoef, int[] MinionsCoef, int[] OpMinionsCoef)
		{
			this.root = Root(poGame);
			this.rndSeed = randomSeed;
			if (rnd != null)
			{
				this.rnd = rnd;
			}
			else
			{
				this.rnd = new Random(this.rndSeed);
			}

			this.ucb1Coef = ucb1Coef;
			this.mcSearchDepth = mcSearchDepth;
			this.mcOpSearchDepth = mcOpSearchDepth;
			this.maxOpOnePlayOutTimes = maxOpOnePlayOutTimes;
			this.maxOnePlayOutTimes = maxOnePlayOutTimes;

			this.BoardCoef = BoardCoef;
			this.MinionsCoef = MinionsCoef;
			this.OpMinionsCoef = OpMinionsCoef;

			availableOptionTypes.Add(PlayerTaskType.CHOOSE);
			availableOptionTypes.Add(PlayerTaskType.HERO_ATTACK);
			availableOptionTypes.Add(PlayerTaskType.HERO_POWER);
			availableOptionTypes.Add(PlayerTaskType.MINION_ATTACK);
			availableOptionTypes.Add(PlayerTaskType.PLAY_CARD);

			availableOpOptionTypes.Add(PlayerTaskType.HERO_ATTACK);
			availableOpOptionTypes.Add(PlayerTaskType.HERO_POWER);
			availableOpOptionTypes.Add(PlayerTaskType.MINION_ATTACK);

		}

		public UCTNode Root(SabberStoneCoreAi.POGame.POGame poGame)
		{
			return new UCTNode(poGame);
		}

		public double UCBValue(UCTNode node)
		{
			Debug.Assert(node.parent != null);
			Debug.Assert(node != null);
			return UCBValue(node.reward, node.nj, node.parent.N, ucb1Coef);
		}

		public double UCBValue(double reward, int nj, int N, double c)
		{
			Debug.Assert(nj > 0);
			return reward / nj + c * (Math.Sqrt((2 * Math.Log(N)) / nj));
		}

		public PlayerTask GetBestOption(UCTNode node, double rootUCTValue)
		{
			PlayerTask bestOption = GetEndTurnOption(node);
			UCTNode bestSimulatedNode = null;
			double bestUCTValue = Double.MinValue;
			if (bestOption != null)
			{
				bestSimulatedNode = root;
				bestUCTValue = rootUCTValue;
			}
			foreach (KeyValuePair<int, UCTNode> item in node.childDict)
			{
				double uctValue = UCBValue(item.Value);
				if (uctValue > bestUCTValue)
				{
					bestUCTValue = uctValue;
					bestSimulatedNode = item.Value;
					bestOption = item.Value.option;
				}
			}
			Debug.Assert(bestUCTValue != Double.MinValue);
			return bestOption;
		}

		public UCTNode GetBestChildUCTNode(UCTNode node)
		{
			PlayerTask bestOption = null;
			UCTNode bestSimulatedNode = null;
			double bestUCTValue = Double.MinValue;
			foreach (KeyValuePair<int, UCTNode> item in node.childDict)
			{
				double uctValue = UCBValue(item.Value);
				if (uctValue > bestUCTValue)
				{
					bestUCTValue = uctValue;
					bestSimulatedNode = item.Value;
					bestOption = item.Value.option;
				}
			}
			return bestSimulatedNode;
		}

		public UCTNode GetWorstChildUCTNode(UCTNode node)
		{
			PlayerTask worstOption = null;
			UCTNode worstNode = null;
			double worstUCTValue = Double.MaxValue;
			foreach (KeyValuePair<int, UCTNode> item in node.childDict)
			{
				double uctValue = UCBValue(item.Value);
				if (uctValue < worstUCTValue)
				{
					worstUCTValue = uctValue;
					worstNode = item.Value;
					worstOption = item.Value.option;
				}
			}
			return worstNode;
		}

		public PlayerTask Simulate(UCTNode currentNode, long accumulativeMillisecond)
		{
			if (root.IsCurrentPlayerTurnEnd())
				return root.currentPOGame.CurrentPlayer.Options()[0];
			if (root.IsGameOver())
				return root.currentPOGame.CurrentPlayer.Options()[0];

			double reward = 0;
			int onePlayoutTimes = 0;
			bool isStop = false;
			long startMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

			if (_debug_) Log.Instance().Append(currentNode.FullPrint());

			while (!isStop &&
				onePlayoutTimes < maxOnePlayOutTimes &&
				DateTimeOffset.Now.ToUnixTimeMilliseconds() - startMilliseconds < this.maxOptionCalculateTime &&
				DateTimeOffset.Now.ToUnixTimeMilliseconds() - accumulativeMillisecond < this.maxTurnCalculateTime)
			{
				UCTNode currentPlayerLeafNode = OnePlayOut(currentNode, availableOptionTypes);

				if (_debug_) Log.Instance().Append(currentPlayerLeafNode.FullPathPrint());
				if (_debug_) Log.Instance().Append(currentPlayerLeafNode.FullPrint());

				reward = EvaluateUCTNode(currentPlayerLeafNode, maxTurnCalculateTime);

				if (_debug_) Log.Instance().Append("reward : " + reward.ToString());

				UpdateReward(currentPlayerLeafNode, reward);

				onePlayoutTimes++;
			}
			reward = EvaluateUCTNode(root, maxTurnCalculateTime);
			PlayerTask bestOption = GetBestOption(root,reward);
			if (onePlayoutTimes > maxRealOnePlayOutTimes) maxRealOnePlayOutTimes = onePlayoutTimes;
			Debug.Assert(bestOption != null);
			return bestOption;
		}

		public double EvaluateUCTNode(UCTNode currentPlayerLeafNode, long accumulativeMillisecond)
		{
			double reward = Double.MinValue;

			UCTNode opRootNode = SimulateEndTurn(currentPlayerLeafNode);

			if( opRootNode == null) // choice turn no endturn option
			{

			}
			else
			{
				// if no endturn option ,there is a choice option
					UCTNode bestOpNode = SimulateOppenont(opRootNode, accumulativeMillisecond);
				if (bestOpNode == null) // oppent no avaliable option to simulate
					reward = EvaluateGameCurrentPlayer(currentPlayerLeafNode.currentPOGame);
				else // oppent have avaliable option to simulate
					reward = UCBValue(bestOpNode);
			}


			return reward;
		}
 
		public UCTNode OnePlayOut(UCTNode currentNode, List<PlayerTaskType> availableOptionTypes)
		{
			//UCTNode currentNode =  root;
			int step = 0;
			Debug.Assert(currentNode.currentPOGame.CurrentPlayer.Options().Count() > 0);
			bool isStop = false;
			while (!isStop &&
				!currentNode.IsGameOver() &&
				AvailableOptions(currentNode, availableOptionTypes).Count() == currentNode.childDict.Count())
			{
				UCTNode bestChildNode = GetBestChildUCTNode(currentNode);
				//Debug.Assert(bestChildNode != null);

				if (bestChildNode == null)
				{
					isStop = true;
				}
				else
				{
					currentNode = bestChildNode;
					//step++;
				}
			}
			//LinkedList<int> optionList = null;
			UCTNode leafNode = MonteCarloSearch(currentNode, availableOptionTypes, step, mcSearchDepth);
			if (step > maxRealMCSearchTimes) maxRealMCSearchTimes = step;
			return leafNode;
		}

		public List<PlayerTask> AvailableOptions(UCTNode currentNode, List<PlayerTaskType> availableOptionTypes, bool playCards = true)
		{
			if (currentNode == null)
			{
				Log.Instance().Append(currentNode.FullPathPrint());
				Log.Instance().Append(currentNode.FullPrint());
			}
			if (currentNode.currentPOGame == null)
			{
				Log.Instance().Append(currentNode.FullPathPrint());
				Log.Instance().Append(currentNode.FullPrint());
			}

			List<PlayerTask> options = currentNode.currentPOGame.CurrentPlayer.Options(playCards);
			List<PlayerTask> availableOptions = new List<PlayerTask>();
			foreach (PlayerTask option in options)
			{
				if (availableOptionTypes.Contains(option.PlayerTaskType))
				{
					availableOptions.Add(option);
				}
			}
			return availableOptions;
		}

		public PlayerTask GetEndTurnOption(UCTNode currentNode, bool playCards = false)
		{
			List<PlayerTask> optionsList = currentNode.currentPOGame.CurrentPlayer.Options(playCards);
			foreach (PlayerTask option in optionsList)
			{
				if (option.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					return option;
				}
			}
			return null;
		}

		public PlayerTask GetEndTurnOption(POGame.POGame game, bool playCards = false)
		{
			List<PlayerTask> optionsList = game.CurrentPlayer.Options(playCards);
			foreach (PlayerTask option in optionsList)
			{
				if (option.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					return option;
				}
			}
			return null;
		}

		public UCTNode MonteCarloSearch(UCTNode startNode, List<PlayerTaskType> availableOptionTypes, int step, int depth)
		{
			//GetEndTurnOption(currentNode.currentPOGame) == null
			UCTNode currentNode = startNode;
			List<PlayerTask> optionsList = AvailableOptions(currentNode, availableOptionTypes);
			bool isStop = false;
			while (!isStop &&
				step < depth &&
				optionsList.Count() > 0 &&
				!currentNode.IsGameOver() 
				|| GetEndTurnOption(currentNode) == null
				)
			{

				int rndOptionNo = rnd.Next(optionsList.Count());
				PlayerTask rndOption = optionsList[rndOptionNo];
				UCTNode tmpNode = currentNode.GetChild(rndOptionNo);

				//  if not existed 
				if (tmpNode == null)
				{
					// simulate the option
					POGame.POGame simulatedPOGame = SimulateOption(currentNode.currentPOGame, rndOption);
					// some time simulate option returned POGame is null
					if (simulatedPOGame == null)
					{
						isStop = true;
						Console.WriteLine(currentNode.FullPathPrint());
						Console.WriteLine(currentNode.FullPrint());
						
					}

					currentNode = currentNode.AddChild(currentNode, rndOption, rndOptionNo, simulatedPOGame);
				}
				//if j child is existed
				else
				{
					currentNode = tmpNode;
				}
				step++;

				// random select a option
				optionsList = AvailableOptions(currentNode, availableOptionTypes);

			}
			return currentNode;
		}

		public double EvaluateGameCurrentPlayer(POGame.POGame game)
		{
			Score.MyScore.MyAggroScore aggroScore = new Score.MyScore.MyAggroScore(this.BoardCoef, this.MinionsCoef, this.OpMinionsCoef);
			//Score.AggroScore aggroScore = new Score.AggroScore();
			aggroScore.Controller = game.CurrentPlayer;
			return aggroScore.Rate();
		}

		public double EvaluateGameCurrentOpponent(POGame.POGame game)
		{
			Score.MyScore.MyAggroScore aggroScore = new Score.MyScore.MyAggroScore(this.BoardCoef, this.MinionsCoef, this.OpMinionsCoef);
			//Score.AggroScore aggroScore = new Score.AggroScore();
			aggroScore.Controller = game.CurrentOpponent;
			return aggroScore.Rate();
		}

		public POGame.POGame SimulateOption(POGame.POGame game, PlayerTask option)
		{

			Dictionary<PlayerTask, POGame.POGame> dict = null;
			LinkedList<PlayerTask> options = null;
			POGame.POGame simulatedPOGame = null;

			options = new LinkedList<PlayerTask>();
			options.AddLast(option);
			try
			{
				dict = game.Simulate(options.ToList<PlayerTask>());
			}
			catch (Exception e)
			{
				return null;
			}
			dict.TryGetValue(option, out simulatedPOGame);
			return simulatedPOGame;
		}

		public UCTNode SimulateEndTurn(UCTNode currentNode)
		{
			PlayerTask endTurnOption = GetEndTurnOption(currentNode);
			POGame.POGame simulatedPOGame = null;
			if (endTurnOption == null) return null;
			simulatedPOGame = SimulateOption(currentNode.currentPOGame, endTurnOption);
			//if (endTurnOption == null)
			//{
			//	List<PlayerTask> options = currentNode.currentPOGame.CurrentPlayer.Options();
			//	int rndOptionNo = rnd.Next(options.Count());
			//	PlayerTask rndOption = options[rndOptionNo];
			//	POGame.POGame tmpGame = SimulateOption(currentNode.currentPOGame, rndOption);
			//	endTurnOption = GetEndTurnOption(tmpGame);

			//	simulatedPOGame = SimulateOption(tmpGame, endTurnOption);
			//}
			//else
			//{

			//}
			UCTNode opRootNode = new UCTNode(0, 0, 0, 0, null, endTurnOption, simulatedPOGame, new Dictionary<int, UCTNode>());
			return opRootNode;
		}

		public UCTNode SimulateOppenont(UCTNode currentNode, long accumulativeMillisecond)
		{
			UCTNode opRootNode = currentNode;
			bool isStop = false;
			int opOnePlayoutTimes = 0;
			if (_debug_) Log.Instance().Append("simulateOppenont start");
			while (!isStop &&
			opOnePlayoutTimes < maxOpOnePlayOutTimes &&
			DateTimeOffset.Now.ToUnixTimeMilliseconds() - accumulativeMillisecond < maxTurnCalculateTime)
			{
				UCTNode opLeafNode = OnePlayOut(currentNode, availableOpOptionTypes);

				if (_debug_) Log.Instance().Append(opLeafNode.FullPathPrint());
				if (_debug_) Log.Instance().Append(opLeafNode.FullPrint());

				double reward = EvaluateGameCurrentOpponent(opLeafNode.currentPOGame);

				if (_debug_) Log.Instance().Append("reward" + reward.ToString());

				UpdateReward(opLeafNode, reward);
				opOnePlayoutTimes++;
			}
			if (_debug_) Log.Instance().Append("simulateOppenont end");
			return GetWorstChildUCTNode(opRootNode); 
		}

		public void UpdateReward(UCTNode leafNode, double reward)
		{
			UCTNode currentNode = leafNode;
			while (currentNode.parent != null)
			{
				currentNode.UpdateReward(reward);
				//Debug.Assert(currentNode.parent.N == currentNode.parent.SumChildN());
				currentNode = currentNode.parent;
			}
			return;
		}

	};

	class UCTHunter : AbstractAgent
	{
		private Random Rnd = new Random();
		long startTurnMillsecond = 0;

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
			if (startTurnMillsecond == 0)
			{
				startTurnMillsecond = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			}
			
			UCTSimulator simulator = new UCTSimulator(poGame, rndSeed, Rnd , MyProgram.ucb1Coef,
				MyProgram.mcSearchDepth, MyProgram.mcOpSearchDepth, MyProgram.maxOpOnePlayOutTimes, MyProgram.maxOnePlayOutTimes,
				MyProgram.BoardCoef, MyProgram.MinionsCoef, MyProgram.OpMinionsCoef);
			PlayerTask bestOption = null;
			try
			{
				bestOption = simulator.Simulate(simulator.root, startTurnMillsecond);
			}
			catch (Exception e)
			{
				bestOption = getBestMove(poGame);
				Log.Instance().Append(e.ToString());
			}
			if (bestOption.PlayerTaskType == PlayerTaskType.END_TURN)
			{
				startTurnMillsecond = 0;
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
