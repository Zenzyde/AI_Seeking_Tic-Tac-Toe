using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkPlacer : MonoBehaviour
{
    [SerializeField] private Transform playerMarker, aiMarker;
    [SerializeField] private NewBoard board;

	private List<GameObject> markers = new List<GameObject>();

	void Awake()
    {
        board = FindObjectOfType<NewBoard>();
    }

    public void PlaceMarker(int unit, bool player)
    {
        if (player)
        {
			markers.Add(Instantiate(playerMarker, board.GetBoard()[unit].GetPiece().TRANSFORMREF.position, Quaternion.identity).gameObject);
			//Instantiate(playerMarker, board.GetCurrentBoard().LookAtBoard(unit).TRANSFORMREF.position, Quaternion.identity);
		}
        else
        {
			markers.Add(Instantiate(aiMarker, board.GetBoard()[unit].GetPiece().TRANSFORMREF.position, Quaternion.identity).gameObject);
			//Instantiate(aiMarker, board.GetCurrentBoard().LookAtBoard(unit).TRANSFORMREF.position, Quaternion.identity);
		}
    }

    public void RemoveMarkers()
    {
        foreach (GameObject marker in markers)
        {
			Destroy(marker);
		}
		markers.Clear();
	}
}