using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorSpawner : MonoBehaviour
{
	private NewBoard board;
	private ClickDetector detector;
	private Vector2 leftEdge, rightEdge;
	private MarkPlacer markPlacer;
	private int column, columns, rows;

	void Awake()
	{
		board = FindObjectOfType<NewBoard>();
		markPlacer = FindObjectOfType<MarkPlacer>();
		if (!board.DoDrop())
			Destroy(gameObject);
	}

    void Start()
    {
		transform.position = new Vector2(
            board.GetBoard()[0].transform.position.x,
            board.GetBoard()[0].transform.position.y + board.GetOffset()
        );
		leftEdge = board.GetBoard()[0].transform.position;
		rightEdge = board.GetBoard()[board.GetBoardSize()[1] - 1].transform.position;
		column = board.GetBoard()[0].GetPiece().COLUMN;
		columns = board.GetBoardSize()[1];
		rows = board.GetBoardSize()[0];
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.x > leftEdge.x)
        {
			transform.position = (Vector2)transform.position + Vector2.left * board.GetOffset();
			column--;
		}
        if (Input.GetKeyDown(KeyCode.RightArrow) && transform.position.x < rightEdge.x)
        {
			transform.position = (Vector2)transform.position + Vector2.right * board.GetOffset();
			column++;
		}
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (board.GetAvailableMoves().Count == 0 || board.CheckWinConditionExtended())
				return;
			Drop();
			board.DoMove();
		}
    }

    void Drop()
    {
		int row = 0;
		for (int i = 0; i < rows; i++)
        {
			if (board.Get2DBoard()[i, column].GetPiece().IsTakenByAI() ||
                board.Get2DBoard()[i, column].GetPiece().IsTakenByPlayer())
				break;
			row = i;
		}
		board.UpdateClicks(board.Get2DBoard()[row, column].GetPiece().BOARDINDEX);
	}
}
