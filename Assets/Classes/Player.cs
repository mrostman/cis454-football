using UnityEngine;
using System.Collections;
using Cloudbase;
using Cloudbase.DataCommands;

public class Player : MonoBehaviour { 
	Vector2 location, shiftLocation, Motion;
	int Assignment;
	string positionAbbrevation, positionName;
	bool userTeam;
	bool initialized = false;
	PlayerToken token;
	Responsibility responsibility;	

	
	void Awake() {
	}

	void Start () {
	}
	
	// Update is called once per frame, handles interaction with PlayerToken
	void Update ()
	{
		// 0: If the token hasn't even been created yet or this Player is not ready to initialize it, return
		if (token == null || initialized == false)
			return;

		// 1: Update internal values based on PlayerToken values
		UpdateLocation();
		
		// 2: If the token has been doubleclicked, pop the menu.
		if (token.popMenu)
		{
			PopMenu();
			token.popMenu = false;
		}
	}
	
	// Update subroutines
	void UpdateLocation()
	{
		location.x = token.GetX ();
		location.y = token.GetY ();
	}

	void Initialize(bool iUserTeam, string iPositionAbbreviation, string iPositionName, int iX, int iY)
	{
		// Set initial values
		userTeam = iUserTeam;
		positionAbbrevation = iPositionAbbreviation;
		positionName = iPositionName;
		location = new Vector2(iX, iY);
		
		// Create an initialize new playerToken
		token = new PlayerToken();
		token.Initialize (userTeam, positionAbbrevation, location.x, location.y);
	}
	
	// Method to show the menu when requested
	private void PopMenu()
	{
		Debug.Log ("Menu Popped");
		// TODO
	}
}
