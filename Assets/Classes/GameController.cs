using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class GameController : MonoBehaviour {
	// Player Tokens
	public List<PlayerToken> offensiveTeam;
	public List<PlayerToken> defensiveTeam;
	public DatabaseController databaseController;

	private ParseObject currentPlay;
	public System.DateTime playLastSeen;
	private string playName;
	private int timeLeft;
	private float distanceCorrectnessThreshold = 4;

	Queue playsToUpdate = new Queue ();
	Queue userPlaysToUpload = new Queue ();

	// Use this for initialization
	void Start () {
	
	}

	// called when a new round of gameplay begins
	void NewPlay () {
		currentPlay = databaseController.SelectPlay ();

		string pName;
		bool nameResult = currentPlay.TryGetValue("Name", out pName);
		if (nameResult) 
		{
			playName = pName;
		}

		ParseObject oTeam;
		bool oTeamResult = currentPlay.TryGetValue("OffensiveTeam", out oTeam);
		if (oTeamResult) 
		{
			ParseObject oPlayer;
			string playerKey;
			for (int i=0;i<11;i++)
			{
				playerKey = "Player" + i.ToString();
				bool oPlayerResult = oTeam.TryGetValue(playerKey, out oPlayer);
				if (oPlayerResult)
				{
					offensiveTeam[i].Initialize(oPlayer);
				}
			}
		}

		ParseObject dTeam;
		bool dTeamResult = currentPlay.TryGetValue("DefensiveTeam", out dTeam);
		if (dTeamResult) 
		{
			ParseObject dPlayer;
			string playerKey;
			for (int i=0;i<11;i++)
			{
				playerKey = "Player" + i.ToString();
				bool dPlayerResult = dTeam.TryGetValue(playerKey, out dPlayer);
				if (dPlayerResult)
				{
					defensiveTeam[i].Initialize(dPlayer);
				}
			}
		}
	}

	// called to perform cleanup at the end of a round of play
	void EndPlay () {
		// reset all PLayerTokens
		for (int i=0; i<11; i++) 
		{
			offensiveTeam[i].deInitialize();
			defensiveTeam[i].deInitialize();
		}

		// check correctness and calculate new SRS coefficient
		int correctness = EvaluateCorrectness ();
		float newSRS = SRSRecalculate (correctness,playLastSeen);

		// save user input into a "History" ParseObject to be send to DB
		ParseObject playHistory = new ParseObject("History");
		ParseObject historyOTeam = new ParseObject("OffensiveTeam");
		playHistory["User"] = ParseUser.CurrentUser;
		string currentPlayID = currentPlay.ObjectId;
		playHistory["Play"] = ParseObject.CreateWithoutData("Play",currentPlayID);
		for (int i = 0; i < 11; i++) 
		{
			ParseObject historyOPlayer = offensiveTeam[i].getInputParseObject();
			string fieldName = "Player" + i;
			historyOTeam.Add(fieldName, historyOPlayer);
		}
		playsToUpdate.Enqueue (playHistory);
		QueueUpdate ();
	}

	// iterate through all offensive team PlayerTokens and compare the user input data
	// to the correct data (both sets of data are stored in each PlayerToken)
	int EvaluateCorrectness () {
		foreach (PlayerToken oPlayer in offensiveTeam) 
		{
			ParseObject inputParsePlayer = oPlayer.getInputParseObject();
			ParseObject correctParsePlayer = oPlayer.getCorrectParseObject();
			Vector3 inputLocation;
			Vector3 correctLocation;
			Vector2 inputMotion;
			Vector2 correctMotion;
			List<Vector2> inputShifts;
			List<Vector2> correctShifts;
			ParseObject inputResponsibility;
			ParseObject correctResponsibility;

			// if the player is an offensive lineman (which take no user input), it cannot be incorrect
			if (oPlayer.controllable == false)
			{
				return 1;
			}

			// check correctness of location
			bool inputLocationResult = inputParsePlayer.TryGetValue("Location", out inputLocation);
			// Location should always be set, so if no location is found output a debug message
			if (!inputLocationResult)
			{
				string position;
				bool nameResult = inputParsePlayer.TryGetValue("Position", out position);
				if (nameResult)
				{
					Debug.Log ("error in correctness evaluation, no location input for player " + position);
				}
				return 0;
			}

			bool correctLocationResult = correctParsePlayer.TryGetValue("Location", out correctLocation);
			// Location should always be set, so if no location is found output a debug message
			if (!correctLocationResult)
			{
				string position;
				bool nameResult = inputParsePlayer.TryGetValue("Position", out position);
				if (nameResult)
				{
					Debug.Log ("error in correctness evaluation, no correct location for player " + position);
				}
				return 0;
			}

			bool wrongLocation = CheckTooFar(inputLocation, correctLocation);
			if (wrongLocation)
			{
				return 0;
			}

			// check correctness of motion
			bool inputMotionResult = inputParsePlayer.TryGetValue("Motion", out inputMotion);
			bool correctMotionResult = correctParsePlayer.TryGetValue("Motion", out correctMotion);

			// if one of the two has a motion set and the other doesn't, return incorrect
			if (!inputMotionResult || !correctMotionResult)
			{
				string position;
				bool nameResult = inputParsePlayer.TryGetValue("Position", out position);
				if (nameResult)
				{
					Debug.Log ("error in correctness evaluation, null motion for player: " + position);
				}
				return 0;
			}

			// if both have a motion, check how far apart they are and return incorrect if it exceeds the threshold


			bool wrongMotion = CheckTooFar (inputMotion, correctMotion);
			if (wrongMotion)
			{
				return 0;
			}

			// check correctness of shifts
			bool inputShiftResult = inputParsePlayer.TryGetValue("Shifts", out inputShifts);
			bool correctShiftResult = correctParsePlayer.TryGetValue("Shifts", out correctShifts);

			// if one of the two has a shift set and the other doesn't, return incorrect
			if (!inputShiftResult || !correctShiftResult)
			{
				string position;
				bool nameResult = inputParsePlayer.TryGetValue("Position", out position);
				if (nameResult)
				{
					Debug.Log ("error in correctness evaluation, null shift for player: " + position);
				}
				return 0;
			}


			if(inputShifts.Count != correctShifts.Count)
			{
				return 0;
			}

			for(int i=0;i<inputShifts.Count;i++)
			{
				bool wrongShift = CheckTooFar(inputShifts[i],correctShifts[i]);
				if (wrongShift)
				{
					return 0;
				}
			}

			// check correctness of responsibility
			bool inputResponsibilityResult = inputParsePlayer.TryGetValue("Responsib", out inputResponsibility);
			bool correctResponsibilityResult = correctParsePlayer.TryGetValue("Responsib", out correctResponsibility);

			if (inputResponsibilityResult ^ correctResponsibilityResult)
			{
				return 0;
			}

			if (inputResponsibilityResult && correctResponsibilityResult)
			{
				if (inputResponsibility.ObjectId != correctResponsibility.ObjectId)
				{
					return 0;
				}
			}
		}
		return 1;
	}

	bool CheckTooFar(Vector2 correct, Vector2 input)
	{
		if (correct == Vector2.zero && input !=Vector2.zero) 
		{
			return false;
		}
		
		float distance = Vector2.Distance (correct, input);
		if (distance > distanceCorrectnessThreshold)
		{
			return false;
		}
		
		return true;
	}

	bool CheckTooFar(Vector3 correct, Vector3 input)
	{
		float distance = Vector3.Distance (correct, input);
		if (distance > distanceCorrectnessThreshold)
		{
			return false;
		}
		
		return true;
	}

	float SRSRecalculate (int correctness, System.DateTime sinceLastPlayed) {
		// TODO: SRS coefficient product of age component, progress component, effort component
		// Age A(x) = Cn^x			(x is time elapsed, C is initial constant)
		// Progress P(x) = Cn^-x	(x is previous success)
		// Effort {1,10}
		return  0;
	}

	void QueueUpdate () {
		foreach (ParseObject play in playsToUpdate)
		{
			bool success = databaseController.UpdatePlay(play);
			if (success)
			{
				playsToUpdate.Dequeue();
			}
		}
	}

	void animationComplete ()
	{
		
	}

	// Update is called once per frame
	void Update () {

	}
}
