using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
[System.Serializable]

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
	bool userTeam = false;
	string positionAbbreviation = "";
	bool initialized = false;
	public bool popMenu = false; // Flag indicating the menu has been requested
	public Material userTeamMaterial;
	public Material otherTeamMaterial;


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
		{
			Debug.Log ("DoubleClick");
			popMenu = true;
		}
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
		snapping = true;
	}

	// Set up a newly created token
	public void Initialize(bool iUserTeam, string iPositionAbbreviation, float iLocationX, float iLocationY)
	{
		if (initialized)
			// Throw error if an attempt is made to initialize an already initialized token
			Debug.LogError ("Attempting to initialize already initialized token!");
		else {
			// Set values
			userTeam = iUserTeam;
			positionAbbreviation = iPositionAbbreviation;
			location.x = iLocationX;
			location.y = iLocationY;
			location.z = 1f;
			initialized = true;
			
			// Set the token to visually display it's position
			GetComponentInChildren<TextMesh>().text = positionAbbreviation;
			
			// Set the token's color/skin based on it's team
			// TODO
		}
	}

	// Getters:
	
	// Check if the token has been initialized
	public bool IsInitialized()
	{
		return initialized;
	}
	
	// Return the x and y components of location
	public float GetX() { return location.x; }
	public float GetY() { return location.y; }
	
	// Other getters
}	