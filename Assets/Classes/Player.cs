using UnityEngine;
using System.Collections;
using Cloudbase;
using Cloudbase.DataCommands;

public class Player : MonoBehaviour { 
	Vector2 location;
	Vector2 shiftLocation;
	Vector2 Motion;
	int Assignment;
	byte position;
	enum PositionAbbriviations : byte { QB, F, H, Y, X, Z, C, FG, BG, FT, BT };


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void setLocation(Vector3 loc)
	{
		location = new Vector2 (loc.x, loc.y);
	}

	public void setPosition(byte pos)
	{
		position = pos;
	}
}
