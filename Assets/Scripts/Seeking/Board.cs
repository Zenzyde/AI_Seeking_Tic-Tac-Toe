using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Board : MonoBehaviour
{
	[SerializeField] private ClickDetector detector;
	[SerializeField] private MarkPlacer markPlacer;
	[SerializeField] private int columns, rows, minToWin, maxDepth;
	[SerializeField] private float offset;
	[SerializeField] private bool AIFirst;
	[SerializeField] private Text actionText;

	private List<ClickDetector> detectors = new List<ClickDetector>();
	private int lastPieceIndex = 0;
	private ClickDetector[,] twoDBoard;
	private bool gameOver, printed, restarted;

	public void SetRows(InputField field)
	{
		rows = Convert.ToInt32(field.text);
		rows = Mathf.Abs(rows);
		if (rows < 3)
			rows = 3;
	}
	public void SetColumns(InputField field)
	{
		columns = Convert.ToInt32(field.text);
		columns = Mathf.Abs(columns);
		if (columns < 3)
			columns = 3;
	}
	public void SetMinToWIn(InputField field)
	{
		minToWin = Convert.ToInt32(field.text);
		minToWin = Mathf.Abs(minToWin);
		if (minToWin < 3)
			minToWin = 3;
		if (minToWin > columns && minToWin > rows)
		{
			minToWin = Mathf.Max(columns, rows);
		}
	}
	public void SetMaxAIDepth(InputField field)
	{
		maxDepth = Convert.ToInt32(field.text);
		maxDepth = Mathf.Abs(maxDepth);
	}
	public void SetAiFirst(Toggle toggle) => AIFirst = toggle.isOn;
	public void Restart()
	{
		RebuildBoard();
		restarted = true;
		gameOver = false;
		printed = false;
	}
	public void Quit() => Application.Quit();

	void OnValidate()
	{
		columns = columns < 3 ? 3 : columns;
		rows = rows < 3 ? 3 : rows;
		minToWin = minToWin < 3 ? 3 : minToWin > columns && minToWin > rows ? Mathf.Max(columns, rows) : minToWin;
		maxDepth = Mathf.Abs(maxDepth);
	}

	void Awake()
	{
		SetLiveBoard();
		Setup2DBoard();
		SetNoAction();
		if (AIFirst)
			SetText("PLAYER TURN");
		else
			SetText("AI TURN");
		StartCoroutine(TextFade());
	}

	void Start()
	{
		if (AIFirst)
			DoMove();
	}

	void Update()
	{
		if (!gameOver)
			return;

		if (!printed)
		{
			PrintWinCondition();
			PrintWinConditionHeuristic();
			printed = true;
		}

		// if (Input.GetKeyDown(KeyCode.Space) && !restarted && gameOver)
		// {
		// 	RebuildBoard();
		// 	restarted = true;
		// }
	}

	IEnumerator TextFade()
	{
		while (true)
		{
			while (actionText.color.a > 0.0f)
			{
				Color fade = actionText.color;
				fade.a -= 0.5f * Time.deltaTime;
				actionText.color = fade;
				yield return null;
			}
			while (actionText.color.a < 1.0f)
			{
				Color fade = actionText.color;
				fade.a += 0.5f * Time.deltaTime;
				actionText.color = fade;
				yield return null;
			}
		}
	}

	public void SetText(string text)
	{
		actionText.text = text;
	}

	public void SetNoAction()
	{
		actionText.text = string.Empty;
	}

	void SetLiveBoard()
	{
		int index = 0;
		GetComponent<Camera>().orthographicSize *= offset - 0.5f;
		Vector2 startPos = transform.position;
		startPos -= new Vector2(columns, -rows) * (offset / 2.5f);
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				detectors.Add(Instantiate(detector, startPos + new Vector2(j * offset, -i * offset), Quaternion.identity));
				detectors[detectors.Count - 1].SetIntegers(i, j, index);
				index++;
			}
		}
		detector.gameObject.SetActive(false);
	}

	void RebuildBoard()
	{
		foreach (ClickDetector detector in detectors)
		{
			Destroy(detector.gameObject);
		}
		detectors.Clear();
		detector.gameObject.SetActive(true);
		GetComponent<Camera>().orthographicSize = 5;
		SetLiveBoard();
		Setup2DBoard();
		SetLastPieceIndex(0);
		gameOver = false;
		restarted = true;
		markPlacer.RemoveMarkers();
		if (AIFirst)
			DoMove();
		else
			SetText("PLAYER TURN");
	}

	public ClickDetector[] GetBoard()
	{
		return detectors.ToArray();
	}

	public List<ClickDetector> GetAvailableMoves()
	{
		List<ClickDetector> available = new List<ClickDetector>();
		for (int i = 0; i < detectors.Count; i++)
		{
			if (!detectors[i].GetPiece().IsTakenByAI() && !detectors[i].GetPiece().IsTakenByPlayer())
			{
				available.Add(detectors[i]);
			}
		}
		return available;
	}

	public void DoMove()
	{
		if (GetAvailableMoves().Count == 0 || CheckWinConditionExtended())
			return;

		SetText("AI THINKING");
		int move = move = MiniMaxAI.AIMove(this, maxDepth);
		SetText("PLAYER TURN");

		markPlacer.PlaceMarker(move, false);
		detectors[move].GetPiece().SetTaken();
		SetLastPieceIndex(move);
		if (CheckWinConditionExtended() || GetAvailableMoves().Count == 0)
		{
			SetGameOver();
		}
	}

	public void UpdateClicks(int index)
	{
		markPlacer.PlaceMarker(detectors[index].GetPiece().BOARDINDEX, true);
		detectors[index].GetPiece().SetTakenByPlayer();
		SetLastPieceIndex(index);
	}

	public float GetOffset()
	{
		return offset;
	}

	private void Setup2DBoard()
	{
		int index = 0;
		twoDBoard = new ClickDetector[rows, columns];
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				twoDBoard[i, j] = detectors[index];
				index++;
			}
		}
	}

	public ClickDetector[,] Get2DBoard()
	{
		return twoDBoard;
	}

	public int GetLastPieceIndex()
	{
		return lastPieceIndex;
	}

	public void SetLastPieceIndex(int index)
	{
		lastPieceIndex = index;
	}

	public int[] GetBoardSize()
	{
		return new int[] { rows, columns };
	}

	public int GetMinToWin()
	{
		return minToWin;
	}

	public void SetGameOver()
	{
		gameOver = true;
		restarted = false;
		if (GetAvailableMoves().Count == 0)
			SetText("NO ONE WINS, TIE!");
		else
			SetText(GetBoard()[lastPieceIndex].GetPiece().IsTakenByAI() ? "AI WINS!" : "PLAYER WINS!");
	}

	public bool IsGameOver()
	{
		return gameOver;
	}

	public bool CheckWinCondition()
	{
		ClickDetector[,] board = Get2DBoard();
		for (int i = 0; i < 3; i++)
		{
			if ((board[i, 0].GetPiece().IsTakenByAI() && board[i, 1].GetPiece().IsTakenByAI() &&
			board[i, 2].GetPiece().IsTakenByAI()) ||
			(board[i, 0].GetPiece().IsTakenByPlayer() && board[i, 1].GetPiece().IsTakenByPlayer() &&
			board[i, 2].GetPiece().IsTakenByPlayer()))
			{
				return true; //win on horizontal
			}
			if (board[0, i].GetPiece().IsTakenByAI() && board[1, i].GetPiece().IsTakenByAI() &&
			board[2, i].GetPiece().IsTakenByAI() ||
			(board[0, i].GetPiece().IsTakenByPlayer() && board[1, i].GetPiece().IsTakenByPlayer() &&
			board[2, i].GetPiece().IsTakenByPlayer()))
			{
				return true; //win on vertical
			}
		}

		//diagonal win
		if ((board[0, 0].GetPiece().IsTakenByAI() && board[1, 1].GetPiece().IsTakenByAI() &&
		board[2, 2].GetPiece().IsTakenByAI()) ||
			(board[0, 0].GetPiece().IsTakenByPlayer() && board[1, 1].GetPiece().IsTakenByPlayer() &&
		board[2, 2].GetPiece().IsTakenByPlayer()))
		{
			return true;
		}
		if (board[2, 0].GetPiece().IsTakenByAI() && board[1, 1].GetPiece().IsTakenByAI() &&
		board[0, 2].GetPiece().IsTakenByAI() ||
			(board[2, 0].GetPiece().IsTakenByPlayer() && board[1, 1].GetPiece().IsTakenByPlayer() &&
		board[0, 2].GetPiece().IsTakenByPlayer()))
		{
			return true;
		}
		return false;
	}

	public bool CheckWinConditionExtended()
	{
		if (GetBoardSize()[0] == 3 && GetBoardSize()[1] == 3 && minToWin == 3)
			return CheckWinCondition();
		ClickDetector[,] board = Get2DBoard();
		int totalVerticalAI = 0, totalVerticalPlayer = 0;
		int totalHorizontalAI = 0, totalHorizontalPlayer = 0;
		int totalDiagonalLeftAI = 0, totalDiagonalRightAI = 0, totalDiagonalLeftPlayer = 0, totalDiagonalRightPlayer = 0;
		AIBoardPiece piece = GetBoard()[lastPieceIndex].GetPiece();
		bool checkForAI = piece.IsTakenByAI();
		bool leftBlocked = false, rightBlocked = false,
			upBlocked = false, downBlocked = false,
			upRightBlocked = false, downRightBlocked = false, downLeftBlocked = false, upLeftBlocked = false;
		if (checkForAI) //Check for AI
		{
			totalVerticalAI++;
			totalHorizontalAI++;
			totalDiagonalLeftAI++;
			totalDiagonalRightAI++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() && !downBlocked)
					totalVerticalAI++;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() && !upBlocked)
					totalVerticalAI++;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() && !rightBlocked)
					totalHorizontalAI++;
				//Check right Player
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() && !leftBlocked)
					totalHorizontalAI++;
				//Check left Player
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !downLeftBlocked)
					totalDiagonalLeftAI++;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !upRightBlocked)
					totalDiagonalLeftAI++;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !upLeftBlocked)
					totalDiagonalRightAI++;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !downRightBlocked)
					totalDiagonalRightAI++;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;

				//Only check for AI
				if (totalDiagonalRightAI == minToWin || totalDiagonalLeftAI == minToWin ||
					totalHorizontalAI == minToWin || totalVerticalAI == minToWin)
				{
					return true;
				}
			}
		}
		else //Check for Player
		{
			totalVerticalPlayer++;
			totalHorizontalPlayer++;
			totalDiagonalLeftPlayer++;
			totalDiagonalRightPlayer++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !downBlocked)
					totalVerticalPlayer++;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !upBlocked)
					totalVerticalPlayer++;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check right Player
				else if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !rightBlocked)
					totalHorizontalPlayer++;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;
				//Check left Player
				else if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !leftBlocked)
					totalHorizontalPlayer++;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !downLeftBlocked)
					totalDiagonalLeftPlayer++;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !upRightBlocked)
					totalDiagonalLeftPlayer++;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !upLeftBlocked)
					totalDiagonalRightPlayer++;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !downRightBlocked)
					totalDiagonalRightPlayer++;

				//Only check for player
				if (totalDiagonalRightPlayer == minToWin || totalDiagonalLeftPlayer == minToWin ||
					totalHorizontalPlayer == minToWin || totalVerticalPlayer == minToWin)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int CheckWinConditionHeuristic()
	{
		ClickDetector[,] board = Get2DBoard();
		int totalVerticalAI = 0, totalVerticalPlayer = 0;
		int totalHorizontalAI = 0, totalHorizontalPlayer = 0;
		int totalDiagonalLeftAI = 0, totalDiagonalRightAI = 0, totalDiagonalLeftPlayer = 0, totalDiagonalRightPlayer = 0;
		AIBoardPiece piece = GetBoard()[lastPieceIndex].GetPiece();
		bool checkForAI = piece.IsTakenByAI();
		bool leftBlocked = false, rightBlocked = false,
			upBlocked = false, downBlocked = false,
			upRightBlocked = false, downRightBlocked = false, downLeftBlocked = false, upLeftBlocked = false;
		if (checkForAI) //Check for AI
		{
			totalVerticalAI++;
			totalHorizontalAI++;
			totalDiagonalLeftAI++;
			totalDiagonalRightAI++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() && !downBlocked)
					totalVerticalAI++;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() && !upBlocked)
					totalVerticalAI++;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() && !rightBlocked)
					totalHorizontalAI++;
				//Check right Player
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() && !leftBlocked)
					totalHorizontalAI++;
				//Check left Player
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !downLeftBlocked)
					totalDiagonalLeftAI++;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !upRightBlocked)
					totalDiagonalLeftAI++;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !upLeftBlocked)
					totalDiagonalRightAI++;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !downRightBlocked)
					totalDiagonalRightAI++;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;
			}
		}
		else //Check for Player
		{
			totalVerticalPlayer++;
			totalHorizontalPlayer++;
			totalDiagonalLeftPlayer++;
			totalDiagonalRightPlayer++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !downBlocked)
					totalVerticalPlayer++;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !upBlocked)
					totalVerticalPlayer++;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check right Player
				else if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !rightBlocked)
					totalHorizontalPlayer++;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;
				//Check left Player
				else if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !leftBlocked)
					totalHorizontalPlayer++;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !downLeftBlocked)
					totalDiagonalLeftPlayer++;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !upRightBlocked)
					totalDiagonalLeftPlayer++;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !upLeftBlocked)
					totalDiagonalRightPlayer++;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !downRightBlocked)
					totalDiagonalRightPlayer++;
			}
		}

		//Return the greatest amount or "score" of moves either by AI or by Player
		int greatestAxisAI = Mathf.Max(totalVerticalAI, totalHorizontalAI);
		int greatestDiagonalAI = Mathf.Max(totalDiagonalLeftAI, totalDiagonalRightAI);

		int greatestAxisPlayer = Mathf.Max(totalVerticalPlayer, totalHorizontalPlayer);
		int greatestDiagonalPlayer = Mathf.Max(totalDiagonalLeftPlayer, totalDiagonalRightPlayer);

		int greatestAxis = Mathf.Max(greatestAxisAI, greatestAxisPlayer);
		int greatestDiagonal = Mathf.Max(greatestDiagonalAI, greatestDiagonalPlayer);

		int greatest = Mathf.Max(greatestAxis, greatestDiagonal);

		return greatest == greatestAxisAI ? greatestAxisAI : greatest == greatestDiagonalAI ? greatestDiagonalAI :
			greatest == greatestAxisPlayer ? greatestAxisPlayer : greatestDiagonalPlayer;
	}

	public bool CheckWinConditionExtendedDrop()
	{
		ClickDetector[,] board = twoDBoard;
		int totalVerticalAI = 0, totalVerticalPlayer = 0;
		int totalHorizontalAI = 0, totalHorizontalPlayer = 0;
		int totalDiagonalLeftAI = 0, totalDiagonalRightAI = 0, totalDiagonalLeftPlayer = 0, totalDiagonalRightPlayer = 0;
		AIBoardPiece piece = detectors[lastPieceIndex].GetPiece();
		if (piece.IsTakenByAI())
		{
			totalVerticalAI++;
			totalHorizontalAI++;
			totalDiagonalLeftAI++;
			totalDiagonalRightAI++;
		}
		else if (piece.IsTakenByPlayer())
		{
			totalVerticalPlayer++;
			totalHorizontalPlayer++;
			totalDiagonalLeftPlayer++;
			totalDiagonalRightPlayer++;
		}

		for (int i = 1; i < minToWin; i++)
		{
			//Check vertically

			//Check down AI
			if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI())
				totalVerticalAI++;
			//Check down Player
			if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer())
				totalVerticalPlayer++;
			//Check up AI
			if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI())
				totalVerticalAI++;
			//Check up Player
			if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer())
				totalVerticalPlayer++;

			//Check horizontally

			//Check right AI
			if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI())
				totalHorizontalAI++;
			//Check right Player
			if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer())
				totalHorizontalPlayer++;
			//Check left AI
			if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI())
				totalHorizontalAI++;
			//Check left Player
			if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer())
				totalHorizontalPlayer++;

			//Check diagonally

			//Left-Down AI
			if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI())
				totalDiagonalLeftAI++;
			//Left-Down Player
			if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer())
				totalDiagonalLeftPlayer++;
			//Right-Up AI
			if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI())
				totalDiagonalLeftAI++;
			//Right-Up Player
			if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer())
				totalDiagonalLeftPlayer++;

			//Left-Up AI
			if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI())
				totalDiagonalRightAI++;
			//Left-Up Player
			if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer())
				totalDiagonalRightPlayer++;
			//Right-Down AI
			if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI())
				totalDiagonalRightAI++;
			//Right-Down Player
			if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer())
				totalDiagonalRightPlayer++;

			//Does not account for opposite directions added together?
			if (totalDiagonalRightAI == minToWin || totalDiagonalLeftAI == minToWin ||
				totalHorizontalAI == minToWin || totalVerticalAI == minToWin)
			{
				return true;
			}
			if (totalDiagonalRightPlayer == minToWin || totalDiagonalLeftPlayer == minToWin ||
				totalHorizontalPlayer == minToWin || totalVerticalPlayer == minToWin)
			{
				return true;
			}
		}
		if (totalDiagonalRightAI == minToWin || totalDiagonalLeftAI == minToWin ||
			totalHorizontalAI == minToWin || totalVerticalAI == minToWin)
		{
			return true;
		}
		if (totalDiagonalRightPlayer == minToWin || totalDiagonalLeftPlayer == minToWin ||
			totalHorizontalPlayer == minToWin || totalVerticalPlayer == minToWin)
		{
			return true;
		}
		return false;
	}

	//Axises are being added in an incorrect manner
	public void PrintWinCondition()
	{
		ClickDetector[,] board = Get2DBoard();
		int totalVerticalAI = 0, totalVerticalPlayer = 0;
		int totalHorizontalAI = 0, totalHorizontalPlayer = 0;
		int totalDiagonalLeftAI = 0, totalDiagonalRightAI = 0, totalDiagonalLeftPlayer = 0, totalDiagonalRightPlayer = 0;
		AIBoardPiece piece = GetBoard()[lastPieceIndex].GetPiece();
		bool checkForAI = piece.IsTakenByAI();
		bool leftBlocked = false, rightBlocked = false,
			upBlocked = false, downBlocked = false,
			upRightBlocked = false, downRightBlocked = false, downLeftBlocked = false, upLeftBlocked = false;

		if (checkForAI) //Check for AI
		{
			totalVerticalAI++;
			totalHorizontalAI++;
			totalDiagonalLeftAI++;
			totalDiagonalRightAI++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() && !downBlocked)
					totalVerticalAI++;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() && !upBlocked)
					totalVerticalAI++;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() && !rightBlocked)
					totalHorizontalAI++;
				//Check right Player
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() && !leftBlocked)
					totalHorizontalAI++;
				//Check left Player
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !downLeftBlocked)
					totalDiagonalLeftAI++;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !upRightBlocked)
					totalDiagonalLeftAI++;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !upLeftBlocked)
					totalDiagonalRightAI++;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !downRightBlocked)
					totalDiagonalRightAI++;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;

				//Only check for AI
				if (totalDiagonalRightAI == minToWin || totalDiagonalLeftAI == minToWin ||
				totalHorizontalAI == minToWin || totalVerticalAI == minToWin)
				{
					print("WIN AI: Diagonal Right AI: " + totalDiagonalRightAI + ". Diagonal Left AI: " + totalDiagonalLeftAI +
						". Vertical AI: " + totalVerticalAI + ". Horizontal AI: " + totalHorizontalAI + ". Piece Index: " +
						piece.BOARDINDEX);
				}
			}
		}
		else //Check for Player
		{
			totalVerticalPlayer++;
			totalHorizontalPlayer++;
			totalDiagonalLeftPlayer++;
			totalDiagonalRightPlayer++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !downBlocked)
					totalVerticalPlayer++;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !upBlocked)
					totalVerticalPlayer++;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check right Player
				else if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !rightBlocked)
					totalHorizontalPlayer++;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;
				//Check left Player
				else if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !leftBlocked)
					totalHorizontalPlayer++;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !downLeftBlocked)
					totalDiagonalLeftPlayer++;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !upRightBlocked)
					totalDiagonalLeftPlayer++;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !upLeftBlocked)
					totalDiagonalRightPlayer++;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !downRightBlocked)
					totalDiagonalRightPlayer++;

				//Only check for player
				if (totalDiagonalRightPlayer == minToWin || totalDiagonalLeftPlayer == minToWin ||
					totalHorizontalPlayer == minToWin || totalVerticalPlayer == minToWin)
				{
					print("WIN PLAYER: Diagonal Right Player: " + totalDiagonalRightPlayer + ". Diagonal Left Player: " + totalDiagonalLeftPlayer +
						". Vertical Player: " + totalVerticalPlayer + ". Horizontal Player: " + totalHorizontalPlayer + ". Piece Index: " +
						piece.BOARDINDEX);
				}
			}
		}
	}

	public void PrintWinConditionHeuristic()
	{
		ClickDetector[,] board = Get2DBoard();
		int totalVerticalAI = 0, totalVerticalPlayer = 0;
		int totalHorizontalAI = 0, totalHorizontalPlayer = 0;
		int totalDiagonalLeftAI = 0, totalDiagonalRightAI = 0, totalDiagonalLeftPlayer = 0, totalDiagonalRightPlayer = 0;
		AIBoardPiece piece = GetBoard()[lastPieceIndex].GetPiece();
		bool checkForAI = piece.IsTakenByAI();
		bool leftBlocked = false, rightBlocked = false,
			upBlocked = false, downBlocked = false,
			upRightBlocked = false, downRightBlocked = false, downLeftBlocked = false, upLeftBlocked = false;
		if (checkForAI) //Check for AI
		{
			totalVerticalAI++;
			totalHorizontalAI++;
			totalDiagonalLeftAI++;
			totalDiagonalRightAI++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() && !downBlocked)
					totalVerticalAI++;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() && !upBlocked)
					totalVerticalAI++;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() && !rightBlocked)
					totalHorizontalAI++;
				//Check right Player
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() && !leftBlocked)
					totalHorizontalAI++;
				//Check left Player
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !downLeftBlocked)
					totalDiagonalLeftAI++;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !upRightBlocked)
					totalDiagonalLeftAI++;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() && !upLeftBlocked)
					totalDiagonalRightAI++;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() && !downRightBlocked)
					totalDiagonalRightAI++;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;
			}
		}
		else //Check for Player
		{
			totalVerticalPlayer++;
			totalHorizontalPlayer++;
			totalDiagonalLeftPlayer++;
			totalDiagonalRightPlayer++;

			for (int i = 1; i < minToWin; i++)
			{
				//Check vertically

				//Check down AI
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsVacant())
					downBlocked = true;
				//Check down Player
				if (piece.ROW + i < rows && board[piece.ROW + i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !downBlocked)
					totalVerticalPlayer++;
				//Check up AI
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsVacant())
					upBlocked = true;
				//Check up Player
				if (piece.ROW - i >= 0 && board[piece.ROW - i, piece.COLUMN].GetPiece().IsTakenByPlayer() && !upBlocked)
					totalVerticalPlayer++;

				//Check horizontally

				//Check right AI
				if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsVacant())
					rightBlocked = true;
				//Check right Player
				else if (piece.COLUMN + i < columns && board[piece.ROW, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !rightBlocked)
					totalHorizontalPlayer++;
				//Check left AI
				if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsVacant())
					leftBlocked = true;
				//Check left Player
				else if (piece.COLUMN - i >= 0 && board[piece.ROW, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !leftBlocked)
					totalHorizontalPlayer++;

				//Check diagonally

				//Left-Down AI
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsVacant())
					downLeftBlocked = true;
				//Left-Down Player
				if (piece.ROW + i < rows && piece.COLUMN - i >= 0 && board[piece.ROW + i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !downLeftBlocked)
					totalDiagonalLeftPlayer++;
				//Right-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsVacant())
					upRightBlocked = true;
				//Right-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN + i < columns && board[piece.ROW - i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !upRightBlocked)
					totalDiagonalLeftPlayer++;

				//Left-Up AI
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByAI() ||
					piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsVacant())
					upLeftBlocked = true;
				//Left-Up Player
				if (piece.ROW - i >= 0 && piece.COLUMN - i >= 0 && board[piece.ROW - i, piece.COLUMN - i].GetPiece().IsTakenByPlayer() && !upLeftBlocked)
					totalDiagonalRightPlayer++;
				//Right-Down AI
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByAI() ||
					piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsVacant())
					downRightBlocked = true;
				//Right-Down Player
				if (piece.ROW + i < rows && piece.COLUMN + i < columns && board[piece.ROW + i, piece.COLUMN + i].GetPiece().IsTakenByPlayer() && !downRightBlocked)
					totalDiagonalRightPlayer++;
			}
		}

		//Return the greatest amount or "score" of moves either by AI or by Player
		print("HEURISTIC AI: Vertical AI: " + totalVerticalAI + ". Horizontal AI: " + totalHorizontalAI +
			". Diagonal Right AI: " + totalDiagonalRightAI + ". Diagonal Left AI: " + totalDiagonalLeftAI);

		print("HEURISTIC PLAYER: Vertical Player: " + totalVerticalPlayer + ". Horizontal Player: " + totalHorizontalPlayer +
		". Diagonal Right Player: " + totalDiagonalRightPlayer + ". Diagonal Left Player: " + totalDiagonalLeftPlayer);
	}
}