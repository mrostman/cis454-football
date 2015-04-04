using UnityEngine;
using System.Collections;
using Parse;

public class Player : MonoBehaviour { 
	Vector2 location { get; }
	Vector2 motion { get; }
	Vector2[] shifts { get; }
	string abbreviation{ get; }
	string name { get; }
	bool userTeam { get; }
	bool initialized  { get; }
	bool controllable, display;
	PlayerToken token;
	Responsibility responsibility { get; }

	
	void Awake() {
	initialized = false;
	}

	void Start () {
	}
	
	void initialize(ParseObject iPlayer)
	{
		display = false;
		controllable = iPlayer.Get(con;
		location = new Vector2( iPlayer.Get ("	
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

	}
	
	// Method to show the menu when requested
	private void PopMenu()
	{
		Debug.Log ("Menu Popped");
		// TODO
	}
}
