using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickDetector : MonoBehaviour
{
    [SerializeField] private int index, row, column;
    private NewBoard board;
	[SerializeField] private AIBoardPiece piece;

	private bool dropMode;

	void Awake()
    {
        board = FindObjectOfType<NewBoard>();
		dropMode = board.DoDrop();
	}

    void OnMouseDown()
    {
		if (dropMode)
			return;
		
		if (board.IsGameOver())
		{
			board.PrintWinCondition();
			board.PrintWinConditionHeuristic();
		}

		if (board.GetAvailableMoves().Count == 0 || piece.IsTakenByAI() || piece.IsTakenByPlayer() ||
			board.CheckWinConditionExtended())//board.CheckWinConditionAskPiece(false) || board.CheckWinConditionAskPiece(true))
			return;
		board.UpdateClicks(index);
		//board.NewMiniMaxMove();
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