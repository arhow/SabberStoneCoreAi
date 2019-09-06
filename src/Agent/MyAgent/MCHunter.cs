using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.POGame;
using SabberStoneCoreAi.Agent;
using SabberStoneCore.Enums;
using System.Diagnostics;
using System;

namespace SabberStoneCoreAi.Agent.MyAgents.MC
{
	
	class Node
	{
		public double reward;
		public int nj;
		public int j;
		public int N;
		public Node parent;
		public Dictionary<PlayerTask, Node> childDict;
		public POGame.POGame currentPOGame;

		public Node(double reward,
						int nj,
						int j,
						int N,
						Node parent,
						Dictionary<PlayerTask, Node> childDict,
						SabberStoneCoreAi.POGame.POGame poGame)
		{
			this.reward = reward;
			this.nj = nj;
			this.j = j;
			this.N = N;
			this.parent = parent;
			this.childDict = childDict;
			this.currentPOGame = poGame;
		}

		public Node(SabberStoneCoreAi.POGame.POGame poGame)
		{
			this.reward = 0;
			this.nj = 0;
			this.j = 0;
			this.N = 0;
			this.parent = null;
			this.childDict = new Dictionary<PlayerTask, Node>();
			this.currentPOGame = poGame;
		}

		public Node()
		{
			this.reward = 0;
			this.nj = 0;
			this.j = 0;
			this.N = 0;
			this.parent = null;
			this.childDict = null;
			currentPOGame = null;
		}

		public Node GetChild(PlayerTask option)
		{
			Node child = null;
			this.childDict.TryGetValue(option, out child);
			return child;
		}

		public Node GetChild(int j)
		{
			foreach (KeyValuePair<PlayerTask, Node> item in this.childDict)
			{
				if (item.Value.j == j)
				{
					return item.Value;
				}
			}
			return null;
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

		public bool IsEndSearch()
		{
			return IsCurrentPlayerTurnEnd() || IsGameOver();
		}

		public Node AddChild(Node parent, PlayerTask option, int j, POGame.POGame simulatedPOGame)
		{
			Debug.Assert(simulatedPOGame != null);
			Debug.Assert(option != null);
			Node newChildNode = new Node(0, 0, j, 0, parent, new Dictionary<PlayerTask, Node>(), simulatedPOGame);
			parent.childDict.Add(option, newChildNode);
			return newChildNode;
		}

		public void UpdateReward(double reward)
		{
			this.nj++;
			this.reward += reward;
			this.parent.N += 1;
		}

		public int SumChildN()
		{
			int sum = 0;
			foreach (KeyValuePair<PlayerTask, Node> item in childDict)
			{
				sum += item.Value.nj;
			}
			return sum;
		}
	};

	class MCSimulator
	{
		public double ucb1Coef = 1.0f;
		public long oneOptionCalculateTime = 2000;
		public long oneTurnCalculateTime = 70000;
		public int rndSeed = 0;
		public int mcSearchDepth = 50;
		public Random rnd = null;
		public Node root = null;

		// for check
		public int realmaxSearchDepth = 0;
		public int maxnOnePlayOut = 0;
		public long longestOneOptionTime = 0;
		public long longestOneTurnTime = 0;
		public MCSimulator(SabberStoneCoreAi.POGame.POGame poGame, int randomSeed, Random rnd)
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

		}

		public Node Root(SabberStoneCoreAi.POGame.POGame poGame)
		{
			return new Node(poGame);
		}

		public double UCBValue(Node node)
		{
			Debug.Assert(node.parent != null);
			return UCBValue(node.reward, node.nj, node.parent.N, this.ucb1Coef);
		}

		public double UCBValue(double reward, int nj, int N, double c)
		{
			Debug.Assert(nj > 0);
			return reward / nj + c * (Math.Sqrt((2 * Math.Log(N)) / (nj)));
		}

		public PlayerTask GetBestOption(Node node)
		{
			// only remaind endturn move
			if (node.childDict.Count() == 0)
				return node.currentPOGame.CurrentPlayer.Options()[0];
			PlayerTask bestOption = node.childDict.First().Key;
			Node bestSimulatedNode = node.childDict.First().Value;
			double bestUCTValue = UCBValue(bestSimulatedNode);
			foreach (KeyValuePair<PlayerTask, Node> item in node.childDict)
			{
				double uctValue = UCBValue(item.Value);
				if (uctValue > bestUCTValue)
				{
					bestUCTValue = uctValue;
					bestSimulatedNode = item.Value;
					bestOption = item.Key;
				}
			}
			return bestOption;
		}

		public Node GetBestSimulatedNode(Node node)
		{
			// only remaind endturn move
			if (node.childDict.Count() == 0)
				return null;
			PlayerTask bestOption = node.childDict.First().Key;
			Node bestSimulatedNode = node.childDict.First().Value;
			double bestUCTValue = UCBValue(bestSimulatedNode);
			foreach (KeyValuePair<PlayerTask, Node> item in node.childDict)
			{
				double uctValue = UCBValue(item.Value);
				if (uctValue > bestUCTValue)
				{
					bestUCTValue = uctValue;
					bestSimulatedNode = item.Value;
					bestOption = item.Key;
				}
			}
			return bestSimulatedNode;
		}

		public PlayerTask Simulate(long startMillisecond, long accumulativeMillisecond)
		{
			// only remaind endturn option
			// return endturn option
			if (root.IsCurrentPlayerTurnEnd() || root.IsGameOver())
				return root.currentPOGame.CurrentPlayer.Options()[0];
			int nOnePlayout = 0;
			bool isStop = false;
			while (!isStop &&
				DateTimeOffset.Now.ToUnixTimeMilliseconds() - startMillisecond < this.oneOptionCalculateTime &&
				DateTimeOffset.Now.ToUnixTimeMilliseconds() - accumulativeMillisecond < this.oneTurnCalculateTime)
			{
				OnePlayOut(startMillisecond, accumulativeMillisecond);
				nOnePlayout++;
			}

			PlayerTask bestOption = GetBestOption(root);
			if (nOnePlayout > maxnOnePlayOut) maxnOnePlayOut = nOnePlayout;
			Debug.Assert(bestOption != null);
			return bestOption;
		}
 
		public int OnePlayOut(long startMillisecond, long accumulativeMillisecond)
		{
			Node currentNode =  root;
			int step = 0;
			LinkedList<int> optionList = null;
			Node leafNode = MonteCarloSearch(currentNode, startMillisecond, out optionList, step);
			double reward = EvaluateNode(leafNode);
			UpdateReward(leafNode, reward);
			return 0;
		}

		public Node MonteCarloSearch(Node startNode, long startMillisecond, out LinkedList<int> optionList, int step)
		{

			Node currentNode = startNode;
			optionList = new LinkedList<int>();
			int nmcsearch = step;
			
			while (currentNode.currentPOGame.CurrentPlayer.Id == startNode.currentPOGame.CurrentPlayer.Id &&
				DateTimeOffset.Now.ToUnixTimeMilliseconds() - startMillisecond < this.oneOptionCalculateTime &&
				nmcsearch < mcSearchDepth)
			{
				// random select a option
				List<PlayerTask> optionsList = currentNode.currentPOGame.CurrentPlayer.Options();
				int nCurrentPlayerOptions = optionsList.Count();
				int rndOptionNo = rnd.Next(nCurrentPlayerOptions);
				PlayerTask rndOption = optionsList[rndOptionNo];

				//Debug.Assert(pogameAfterBestMove != null);
				Node tmpNode = currentNode.GetChild(rndOptionNo);

				while (nCurrentPlayerOptions > 1 &&
					tmpNode != null &&
					rndOption.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					rndOptionNo = rnd.Next(nCurrentPlayerOptions);
					rndOption = optionsList[rndOptionNo];

					//Debug.Assert(pogameAfterBestMove != null);
					tmpNode = currentNode.GetChild(rndOptionNo);
				}

				// save rndoptionNo which is Node j value
				optionList.AddLast(rndOptionNo);

				// if j child is existed
				if (tmpNode == null)
				{
					// simulate the option
					LinkedList<PlayerTask> options = new LinkedList<PlayerTask>();
					options.AddLast(rndOption);
					Dictionary<PlayerTask, POGame.POGame> dict = currentNode.currentPOGame.Simulate(options.ToList<PlayerTask>());
					POGame.POGame simulatedPOGame = null;
					dict.TryGetValue(rndOption, out simulatedPOGame);

					// some time simulate option returned POGame is null
					if (simulatedPOGame != null)
					{
						currentNode = currentNode.AddChild(currentNode, rndOption, rndOptionNo, simulatedPOGame);
					}
					else
					{
						break;
					}
				}
				// if not existed
				else
				{
					currentNode = tmpNode;
				}
				nmcsearch++;

			}
			return currentNode;
		}

		public double EvaluateNode(Node currentNode)
		{
			//Score.MyScore.MyAggroScore aggroScore = new Score.MyScore.MyAggroScore(this.BoardCoef, this.MinionsCoef, this.OpMinionsCoef);
			Score.AggroScore aggroScore = new Score.AggroScore();
			if (currentNode.currentPOGame.CurrentPlayer.Name == root.currentPOGame.CurrentPlayer.Name)
				aggroScore.Controller = currentNode.currentPOGame.CurrentPlayer;
			else
				aggroScore.Controller = currentNode.currentPOGame.CurrentOpponent;
			return aggroScore.Rate();
		}

		public void UpdateReward(Node leafNode, double reward)
		{
			Node currentNode = leafNode;
			while (currentNode.parent != null)
			{
				currentNode.UpdateReward(reward);
				Debug.Assert(currentNode.parent.N == currentNode.parent.SumChildN());
				currentNode = currentNode.parent;
			}
			return;
		}

	};

	class MCHunter : AbstractAgent
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
			long startOptionMillisecond = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			if (startTurnMillsecond == 0)
			{
				startTurnMillsecond = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			}

			MCSimulator simulator = new MCSimulator(poGame, rndSeed, Rnd);
			PlayerTask bestOption = null;
			try
			{
				bestOption = simulator.Simulate(startOptionMillisecond, startTurnMillsecond);
			}
			catch (Exception e)
			{
				bestOption = getBestMove(poGame);
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
