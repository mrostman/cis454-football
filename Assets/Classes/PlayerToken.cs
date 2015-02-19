using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]

public class PlayerToken : MonoBehaviour {
	private Vector3 screenPoint = new Vector3 (0, 0, 0);
	private Vector3 offset;
	private float heightFactor = 20.0f / Screen.height;
	private int widthFactor = Screen.width;
	Vector3 location;
	byte position;
	byte team;
	enum PositionAbbriviation : byte { QB, F, H, Y, X, Z, C, FG, BG, FT, BT };
	enum Teams : byte { Offense, Defense };

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		Debug.Log ("Clicked" + Input.mousePosition);
		offset = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
		}
	
	void OnMouseDrag()
	{
		Debug.Log ("Dragging");
		transform.Translate( (Input.mousePosition.x - offset.x) * heightFactor, (Input.mousePosition.y - offset.y) * heightFactor, 0);
		offset.x = Input.mousePosition.x;
		offset.y = Input.mousePosition.y;
		                  
	}
}	