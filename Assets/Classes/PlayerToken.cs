using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]

public class PlayerToken : MonoBehaviour {
	private Vector3 offset;
	private float heightFactor = 20.0f / Screen.height; 
	private int maxX = 7;
	private int maxY = -9;
	private Vector3 target;
	private Vector3 startPoint;
	private bool snapping = false;
	private Player player;
	private int doubleClickCount = 0;
	Vector3 location;
	byte position;
	byte team;
	enum PositionAbbriviation : byte { QB, F, H, Y, X, Z, C, FG, BG, FT, BT };
	enum Teams : byte { Offense, Defense };

	// Use this for initialization
	void Start () {
		this.gameObject.tag = "token";
	
	}

	// FixedUpdate is called every physics frame (50 times/second)
	void FixedUpdate() {
		if (doubleClickCount > 0)
			doubleClickCount--;
		}
	
	// Update is called once per frame
	void Update () {


		// Handle snapping to grid. Target is determined on mouseUp (that is, when the user 'drops' the playerToken
		float distanceToTarget = Vector3.Distance(this.transform.position, target);
		if (snapping && distanceToTarget > 0.001f)
		{
			if (distanceToTarget > 0.005f)
				this.transform.position = Vector3.MoveTowards (this.transform.position, 
				                                               target,
				                                               distanceToTarget / 0.005f);
			else
				this.transform.position = Vector3.MoveTowards (this.transform.position,
				                                               target,
				                                               distanceToTarget);
		}
		else
		{
			snapping = false;
		}
	}

	void OnMouseDown() {
		// Check for double click
		Debug.Log (doubleClickCount);
		if (doubleClickCount > 0)
						Debug.Log ("DoubleClick");
		// If not a double click, start dragging the player
		else
		{
			doubleClickCount = 50;
			startPoint = new Vector3 (this.transform.position.x,
                         this.transform.position.y,
                         this.transform.position.z);
			snapping = false;
			offset = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
			}
		}
	
	void OnMouseDrag()
	{
		transform.Translate( (Input.mousePosition.x - offset.x) * heightFactor, (Input.mousePosition.y - offset.y) * heightFactor, 0);
		offset.x = Input.mousePosition.x;
		offset.y = Input.mousePosition.y;
		                  
	}

	void OnMouseUp()
	{
		// Set snap position
		target = new Vector3 (Mathf.Round (this.transform.position.x), Mathf.Round (this.transform.position.y), this.transform.position.z);

		// Check for invalid placement (outside of field of play)
		if (Mathf.Abs (target.x) > maxX || target.y > -1 || target.y < maxY) 
		{
			transform.position = startPoint;
			return;
		}

		// Check for invalid placement (overlapping another token)
		var otherTokens = GameObject.FindGameObjectsWithTag ("token");
		foreach (var token in otherTokens)
		{
			if (token != this.gameObject && Vector3.Distance(target, token.transform.position) < 0.01f)
			{
				transform.position = startPoint;
				return;
			}
		}

		// If placement is valid, set snapping so update will move the token
		//	player.setLocation(target);
		snapping = true;
	}
}	