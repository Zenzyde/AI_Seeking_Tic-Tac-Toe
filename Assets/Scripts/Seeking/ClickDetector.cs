using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickDetector : MonoBehaviour
{
	[SerializeField] private int index, row, column;
	private Board board;
	[SerializeField] private AIBoardPiece piece;

	void Awake()
	{
		board = FindObjectOfType<Board>();
	}

	void OnMouseDown()
	{
		if (board.IsGameOver())
		{
			board.PrintWinCondition();
			board.PrintWinConditionHeuristic();
		}

		if (board.GetAvailableMoves().Count == 0 || piece.IsTakenByAI() || piece.IsTakenByPlayer() || board.CheckWinConditionExtended())
			return;
		board.UpdateClicks(index);
		board.DoMove();
		if (board.CheckWinConditionExtended() && !board.IsGameOver() || board.GetAvailableMoves().Count == 0)
			board.SetGameOver();
	}

	public void SetIntegers(int row, int column, int index)
	{
		this.row = row;
		this.column = column;
		this.index = index;
		piece = new AIBoardPiece();
		piece.BOARDINDEX = index;
		piece.COLUMN = column;
		piece.ROW = row;
		piece.TRANSFORMREF = transform;
	}

	public AIBoardPiece GetPiece()
	{
		return piece;
	}
}