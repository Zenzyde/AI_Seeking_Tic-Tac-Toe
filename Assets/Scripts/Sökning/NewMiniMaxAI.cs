using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class NewMiniMaxAI
{
	//MaximizingPlayer => AI
	//Generate & check possible move-trees
	public static int MiniMax(NewBoard node, int depth, bool maximizingPlayer)
	{
		//Stopping conditions
		bool opposingWon = node.CheckWinConditionExtended();//AskPiece(!maximizingPlayer);
															//bool currentWon = node.CheckWinConditionAskPiece(maximizingPlayer);
		if (node.GetAvailableMoves().Count == 0 &&
			!opposingWon || node.GetAvailableMoves().Count == 0)
        {
			return 0; //tie
		}
		if (opposingWon)
        {
			return maximizingPlayer ? -1 : 1;//return maximizingPlayer ? -1 : 1;
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
			foreach (ClickDetector detector in node.GetBoard())//AvailableMoves())
			{
				if (detector.GetPiece().IsTakenByAI() || detector.GetPiece().IsTakenByPlayer())
					continue;
				//Place marker
				//node.GetAvailableMoves()[i].GetPiece().SetTaken(true);
				detector.GetPiece().SetTaken(true);
				node.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
				//Call MiniMax with the current marker selected
				int value = MiniMax(node, depth - 1, false);
				//Unplace marker
				//node.GetAvailableMoves()[i].GetPiece().SetTaken(false);
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
			foreach (ClickDetector detector in node.GetBoard())//AvailableMoves())
			{
				if (detector.GetPiece().IsTakenByAI() || detector.GetPiece().IsTakenByPlayer())
					continue;
				//Place marker
				//node.GetAvailableMoves()[i].GetPiece().SetTaken(true);
				detector.GetPiece().SetTakenByPlayer(true);
				node.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
				//Call MiniMax with the current marker selected
				int value = MiniMax(node, depth - 1, true);
				//Unplace marker
				//node.GetAvailableMoves()[i].GetPiece().SetTaken(false);
				node.SetLastPieceIndex(0);
				detector.GetPiece().SetTakenByPlayer(false);
				minVal = Mathf.Min(minVal, value);
			}
			return (int)minVal;
		}
	}

	public static int MiniMaxAB(NewBoard node, int depth, bool maximizingPlayer, float alpha, float beta)
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
			return maximizingPlayer ? -node.GetMinToWin() : node.GetMinToWin();//return maximizingPlayer ? -1 : 1;
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
			foreach (ClickDetector detector in node.GetBoard())//AvailableMoves())
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
			foreach (ClickDetector detector in node.GetBoard())//AvailableMoves())
			{
				if (detector.GetPiece().IsTakenByAI() || detector.GetPiece().IsTakenByPlayer())
					continue;
				//Place marker
				//node.GetAvailableMoves()[i].GetPiece().SetTaken(true);
				detector.GetPiece().SetTakenByPlayer(true);
				int lastIndex = node.GetLastPieceIndex();
				node.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
				//Call MiniMax with the current marker selected
				int value = MiniMaxAB(node, depth - 1, true, alpha, beta);
				//Unplace marker
				//node.GetAvailableMoves()[i].GetPiece().SetTaken(false);
				node.SetLastPieceIndex(lastIndex);
				detector.GetPiece().SetTakenByPlayer(false);
				minVal = Mathf.Min(minVal, value);

				//Alpha-Bet-Pruning
				beta = Mathf.Min(beta, minVal);
				if (beta <= alpha)
					break;
			}
			return (int)minVal;
		}
	}

	//MaximizingPlayer => AI
	//Generate & check possible move-trees
	public static int MiniMaxDrop(NewBoard node, int depth, bool maximizingPlayer)
	{
		//Stopping conditions
		bool opposingWon = node.CheckWinConditionExtended();//AskPiece(!maximizingPlayer);
															//bool currentWon = node.CheckWinConditionAskPiece(maximizingPlayer);
		if (node.GetAvailableMoves().Count == 0 &&
			!opposingWon || node.GetAvailableMoves().Count == 0)
		{
			return 0; //tie
		}
		if (opposingWon)
		{
			return maximizingPlayer ? -1 : 1;//return maximizingPlayer ? -1 : 1;
		}
		// if (depth == 0)
		// {
		// 	//return static evaluation of position -- return the conditional value of the position/node: 0,1,lose,win,etc..
		// 	return maximizingPlayer ? -1 : 1; //if this depth is the AI: return -1, else return 1.
		// }
		if (maximizingPlayer) //AI
		{
			float maxVal = Mathf.NegativeInfinity;

			//Go over available moves, skip the ones who has no unavailable piece below them
			//Row
			for (int i = node.GetBoardSize()[0] - 1; i > 0; i--)
			{
				//Column
				for (int j = 0; j < node.GetBoardSize()[1]; j++)
				{
					//Current space not occupied
					if (node.Get2DBoard()[i, j].GetPiece().IsTakenByPlayer() ||
						node.Get2DBoard()[i, j].GetPiece().IsTakenByAI())
						continue;
					//Any occupied space below to place above?
					if (i < node.GetBoardSize()[0] - 1 &&
					node.Get2DBoard()[i + 1, j].GetPiece().IsTakenByAI() ||
					i < node.GetBoardSize()[0] - 1 &&
					node.Get2DBoard()[i + 1, j].GetPiece().IsTakenByPlayer())
					{
						node.Get2DBoard()[i, j].GetPiece().SetTaken(true);
						node.SetLastPieceIndex(node.Get2DBoard()[i, j].GetPiece().BOARDINDEX);
						int value = MiniMaxDrop(node, depth - 1, false);
						node.SetLastPieceIndex(0);
						node.Get2DBoard()[i, j].GetPiece().SetTaken(false);
						maxVal = Mathf.Max(value, maxVal);
					}
				}
			}
			return (int)maxVal;
		}
		else //Player
		{
			float minVal = Mathf.Infinity;
			//Go over available moves, skip the ones who has no unavailable piece below them
			//Row
			for (int i = node.GetBoardSize()[0] - 1; i > 0; i--)
			{
				//Column
				for (int j = 0; j < node.GetBoardSize()[1]; j++)
				{
					//Current space not occupied
					if (node.Get2DBoard()[i, j].GetPiece().IsTakenByPlayer() ||
						node.Get2DBoard()[i, j].GetPiece().IsTakenByAI())
						continue;
					//Any occupied space below to place above?
					if (i < node.GetBoardSize()[0] - 1 &&
					node.Get2DBoard()[i + 1, j].GetPiece().IsTakenByAI() ||
					i < node.GetBoardSize()[0] - 1 &&
					node.Get2DBoard()[i + 1, j].GetPiece().IsTakenByPlayer())
					{
						node.Get2DBoard()[i, j].GetPiece().SetTakenByPlayer(true);
						node.SetLastPieceIndex(node.Get2DBoard()[i, j].GetPiece().BOARDINDEX);
						int value = MiniMaxDrop(node, depth - 1, true);
						node.SetLastPieceIndex(0);
						node.Get2DBoard()[i, j].GetPiece().SetTakenByPlayer(false);
						minVal = Mathf.Min(value, minVal);
					}
				}
			}
			return (int)minVal;
		}
	}

	//Let AI go over every available move & check each possible tree for the optimal move
	public static int AIMove(NewBoard board, int maxDepth)
    {
		float bestScore = Mathf.NegativeInfinity;
		int bestIndex = 0;

		foreach (ClickDetector detector in board.GetBoard())//AvailableMoves())
		{
			if (detector.GetPiece().IsTakenByPlayer() ||
				detector.GetPiece().IsTakenByAI())
				continue;

			//board[i].GetPiece().SetTaken(true);
			detector.GetPiece().SetTaken(true);
			int lastIndex = board.GetLastPieceIndex();
			board.SetLastPieceIndex(detector.GetPiece().BOARDINDEX);
			int move = MiniMaxAB(board, maxDepth, false, Mathf.NegativeInfinity, Mathf.Infinity);
			//board[i].GetPiece().SetTaken(false);
			board.SetLastPieceIndex(lastIndex);
			detector.GetPiece().SetTaken(false);

			//Makes AI start with first index
			if (move > bestScore)
			{
				bestScore = move;
				bestIndex = detector.GetPiece().BOARDINDEX;
			}
			// //Makes AI start with last index
			// bestScore = Mathf.Max(move, bestScore);
			// if ((int)bestScore == move)
			// 	bestIndex = detector.GetPiece().BOARDINDEX;
		}
		return bestIndex;
	}

	public static int AIMoveDrop(NewBoard board, int depth)
	{
		float bestScore = Mathf.NegativeInfinity;
		int bestIndex = 0;

		int move = 0;
		//Row
		for (int i = board.GetBoardSize()[0] - 1; i > 0; i--)
		{
			//Column
			for (int j = 0; j < board.GetBoardSize()[1]; j++)
			{
				//Current space not occupied
				if (board.Get2DBoard()[i, j].GetPiece().IsTakenByPlayer() ||
					board.Get2DBoard()[i, j].GetPiece().IsTakenByAI())
					continue;
				//Any occupied space below to place above?
				if (i < board.GetBoardSize()[0] - 1 &&
					!board.Get2DBoard()[i + 1, j].GetPiece().IsTakenByAI() ||
					i < board.GetBoardSize()[0] - 1 &&
					!board.Get2DBoard()[i + 1, j].GetPiece().IsTakenByPlayer())
					continue;
				board.Get2DBoard()[i, j].GetPiece().SetTaken(true);
				board.SetLastPieceIndex(board.Get2DBoard()[i, j].GetPiece().BOARDINDEX);
				move = MiniMaxDrop(board, depth, false);
				board.SetLastPieceIndex(0);
				board.Get2DBoard()[i, j].GetPiece().SetTaken(false);

				//Makes AI start with first index
				if (move > bestScore)
				{
					bestScore = move;
					bestIndex = board.Get2DBoard()[i, j].GetPiece().BOARDINDEX;
				}
			}
		}
		return bestIndex;
	}
}
