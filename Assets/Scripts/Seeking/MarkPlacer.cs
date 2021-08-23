using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkPlacer : MonoBehaviour
{
	[SerializeField] private Transform playerMarker, aiMarker;
	[SerializeField] private Board board;

	private List<GameObject> markers = new List<GameObject>();

	void Awake()
	{
		board = FindObjectOfType<Board>();
	}

	public void PlaceMarker(int unit, bool player)
	{
		if (player)
			markers.Add(Instantiate(playerMarker, board.GetBoard()[unit].GetPiece().TRANSFORMREF.position, Quaternion.identity).gameObject);
		else
			markers.Add(Instantiate(aiMarker, board.GetBoard()[unit].GetPiece().TRANSFORMREF.position, Quaternion.identity).gameObject);
	}

	public void RemoveMarkers()
	{
		foreach (GameObject marker in markers)
			Destroy(marker);
		markers.Clear();
	}
}