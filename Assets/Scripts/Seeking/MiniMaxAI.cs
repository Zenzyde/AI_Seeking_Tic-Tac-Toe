using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class MiniMaxAI
{
	//MaximizingPlayer => AI
	//Generate & check possible move-trees
	public static int MiniMax(Board node, int depth, bool maximizingPlayer)
	{
		//Stopping conditions
		bool opposingWon = node.CheckWinConditionExtended();

		if (node.GetAvailableMoves().Count == 0 && !opposingWon || node.GetAvailableMoves().Count == 0)
		{
			return 0; //tie
		}
		if (opposingWon)
		{
			return maximizingPlayer ? -1 : 1;
		}
		// if (depth == 0)
		// {
		// 	//return static evaluation of position -- return the conditional value of the position/node: 0,1,lose,win,etc..
		// 	return maximizingPlayer ? -1 : 1; //if this depth is the AI: return -1, else return 1.
		// }
		if (maximizingPlayer) //AI
		{
			float maxVal = Mathf.NegativeInfinity;
			//Go over available moves
			foreach (ClickDetector detector in node.GetBoard())
			{
				if (detector.GetPiece().IsTakenByAI() || detector.GetPiece().IsTakenByPlayer())
					continue;
				//Place marker
				detector.GetPiece().SetTaken(true);
				node.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
				//Call MiniMax with the current marker selected
				int value = MiniMax(node, depth - 1, false);
				//Unplace marker
				node.SetLastPieceIndex(0);
				detector.GetPiece().SetTaken(false);
				maxVal = Mathf.Max(maxVal, value);
			}
			return (int)maxVal;
		}
		else //Player
		{
			float minVal = Mathf.Infinity;
			//Go over available moves
			foreach (ClickDetector detector in node.GetBoard())
			{
				if (detector.GetPiece().IsTakenByAI() || detector.GetPiece().IsTakenByPlayer())
					continue;
				//Place marker
				detector.GetPiece().SetTakenByPlayer(true);
				node.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
				//Call MiniMax with the current marker selected
				int value = MiniMax(node, depth - 1, true);
				//Unplace marker
				node.SetLastPieceIndex(0);
				detector.GetPiece().SetTakenByPlayer(false);
				minVal = Mathf.Min(minVal, value);
			}
			return (int)minVal;
		}
	}

	public static int MiniMaxAB(Board node, int depth, bool maximizingPlayer, float alpha, float beta)
	{
		//Stopping conditions
		bool opposingWon = node.CheckWinConditionExtended();
		int score = node.CheckWinConditionHeuristic();
		int available = node.GetAvailableMoves().Count;
		if (available == 0 && !opposingWon || available == 0)
		{
			return 0; //tie
		}
		if (opposingWon) //Heuristic score doesn't matter, a winning state has been discovered in this branch
		{
			return maximizingPlayer ? -node.GetMinToWin() : node.GetMinToWin();
		}
		if (depth == 0) //No one has won yet but depth is reached, check heuristic to determine branch-score
		{
			//return static evaluation of position -- return the conditional value of the position/node: 0,1,lose,win,etc..
			return maximizingPlayer ? -score : score; //if this depth is the AI: return -1, else return 1.
		}
		if (maximizingPlayer) //AI
		{
			float maxVal = Mathf.NegativeInfinity;
			//Go over available moves
			foreach (ClickDetector detector in node.GetBoard())
			{
				if (detector.GetPiece().IsTakenByAI() || detector.GetPiece().IsTakenByPlayer())
					continue;
				//Place marker
				detector.GetPiece().SetTaken(true);
				int lastIndex = node.GetLastPieceIndex();
				node.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
				//Call MiniMax with the current marker selected
				int value = MiniMaxAB(node, depth - 1, false, alpha, beta);
				//Unplace marker
				node.SetLastPieceIndex(lastIndex);
				detector.GetPiece().SetTaken(false);
				maxVal = Mathf.Max(maxVal, value);

				//Alpha-Beta-Pruning
				alpha = Mathf.Max(alpha, maxVal);
				if (beta <= alpha)
					break;
			}
			return (int)maxVal;
		}
		else //Player
		{
			float minVal = Mathf.Infinity;
			//Go over available moves
			foreach (ClickDetector detector in node.GetBoard())
			{
				if (detector.GetPiece().IsTakenByAI() || detector.GetPiece().IsTakenByPlayer())
					continue;
				//Place marker
				detector.GetPiece().SetTakenByPlayer(true);
				int lastIndex = node.GetLastPieceIndex();
				node.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
				//Call MiniMax with the current marker selected
				int value = MiniMaxAB(node, depth - 1, true, alpha, beta);
				//Unplace marker
				node.SetLastPieceIndex(lastIndex);
				detector.GetPiece().SetTakenByPlayer(false);
				minVal = Mathf.Min(minVal, value);

				//Alpha-Beta-Pruning
				beta = Mathf.Min(beta, minVal);
				if (beta <= alpha)
					break;
			}
			return (int)minVal;
		}
	}

	//Let AI go over every available move & check each possible tree for the optimal move
	public static int AIMove(Board board, int maxDepth)
	{
		float bestScore = Mathf.NegativeInfinity;
		int bestIndex = 0;

		foreach (ClickDetector detector in board.GetBoard())
		{
			if (detector.GetPiece().IsTakenByPlayer() ||
				detector.GetPiece().IsTakenByAI())
				continue;

			detector.GetPiece().SetTaken(true);
			int lastIndex = board.GetLastPieceIndex();
			board.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
			int move = MiniMaxAB(board, maxDepth, false, Mathf.NegativeInfinity, Mathf.Infinity);
			board.SetLastPieceIndex(lastIndex);
			detector.GetPiece().SetTaken(false);

			//Makes AI start with first index
			if (move > bestScore)
			{
				bestScore = move;
				bestIndex = detector.GetPiece().BOARDINDEX;
			}
		}
		return bestIndex;
	}
}
