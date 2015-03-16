using UnityEngine;
using System.Collections;
using Cloudbase;
using Cloudbase.DataCommands;

public class Player : MonoBehaviour { 
	Vector2 location;
	Vector2 shiftLocation;
	Vector2 Motion;
	int Assignment;
	string positionAbbrivation;
	bool userTeam;
	bool initialized = false;
	PlayerToken token;
	Responsibility responsibility;	

	
	void Awake() {

	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame, handles interaction with PlayerToken
	void Update ()
	{
		// 0: If the token hasn't even been created yet or this Player is not ready to initialize it, return
		if (token == null || initialized == false)
			return;

		// 1: If the PlayerToken has not yet been initialized, initialize it.
		if (!(token.IsInitialized()))
			token.Initialize (userTeam, positionAbbrivation, location.x, location.y);
		
		// 2: Update internal values based on PlayerToken values
		location.x = token.GetX ();
		location.y = token.GetY ();
		
		// 3: If the token has been doubleclicked, pop the menu.
		if (token.popMenu)
		{
			PopMenu();
			token.popMenu = false;
		}
	}

	public void setLocation(Vector3 loc)
	{
		location = new Vector2 (loc.x, loc.y);
	}

	public void setPosition(byte pos)
	{
	}

	public void createToken()
	{

	}
	
	// Method to show the menu when requested
	private void PopMenu()
	{
		Debug.Log ("Menu Popped");
		// TODO
	}
}
