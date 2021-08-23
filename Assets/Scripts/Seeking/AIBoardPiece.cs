using UnityEngine;

[System.Serializable]
public class AIBoardPiece
{
	[SerializeField] private bool isTakenByAI, isTakenByPlayer;
	[SerializeField] private int boardIndex;
	[SerializeField] private int row, column;
	private Transform transformRef;
	private int placementIndex;
	public int BOARDINDEX { get { return boardIndex; } set { boardIndex = value; } }
	public int ROW { get { return row; } set { row = value; } }
	public int COLUMN { get { return column; } set { column = value; } }
	public Transform TRANSFORMREF { get { return transformRef; } set { transformRef = value; } }

	public void SetTaken()
	{
		isTakenByAI = true;
	}

	public void SetTaken(bool isTaken)
	{
		this.isTakenByAI = isTaken;
	}

	public void SetTakenByPlayer()
	{
		isTakenByPlayer = true;
	}

	public void SetTakenByPlayer(bool isTaken)
	{
		isTakenByPlayer = isTaken;
	}

	public void SetIndex(int index)
	{
		placementIndex = index;
	}

	public void UnsetIndex(int index)
	{
		placementIndex = index;
	}

	public bool IsTakenByAI()
	{
		return isTakenByAI;
	}

	public bool IsTakenByPlayer()
	{
		return isTakenByPlayer;
	}

	public bool IsVacant()
	{
		return isTakenByAI == false && isTakenByPlayer == false;
	}
}