using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class GameController : MonoBehaviour {
	// State handler
	public enum STATE {INACTIVE, LOADING, LOADED, SETUP, INPLAY, POSTPLAY, SAVING};
	public STATE state = STATE.INACTIVE;
	public bool editMode = false;
	
	public AudioSource iBuzzSound;
	public AudioClip iBuzzClip;
	public static AudioSource buzzSound;
	public static AudioClip buzzClip;

	// Player Tokens
	public List<PlayerToken> offensiveTeam;
	public List<PlayerToken> defensiveTeam;
	public DatabaseController databaseController;
	
	// For edit mode
	public int existingCount = -1;
	private string playNameToSave;

	// Variables for keeping track of the current play
	public ParseObject currentPlay;
	private string currentPlayID;
	private string editPlayName;
	public float animLength;
	
	public System.DateTime playLastSeen;
	public string playName;
	private int timeLeft;
	private const float distanceCorrectnessThreshold = 6;

	Queue playsToUpdate = new Queue ();
	Queue userPlaysToUpload = new Queue ();
	
	public MenuController menuController;

	public static void playBuzz() {
		buzzSound.PlayOneShot(buzzClip);
	}

	// Use this for initialization
	void Start () {
		buzzSound = iBuzzSound;
		buzzClip = iBuzzClip;
	}
	
	public void testFunc() { Debug.Log("Test"); }
	
	// Special NewPlay method for editplay
	public void EditPlay (string playName) {
		editMode = true;
		editPlayName = playName;
		NewPlay ();
	}

	// called when a new round of gameplay begins
	public void NewPlay () {
		// Reset the displayed play name
		playName = "";
		
		// Reset all PlayerTokens
		for (int i=0; i<11; i++) 
		{
			offensiveTeam[i].deInitialize();
			defensiveTeam[i].deInitialize();
		}
		PlayerToken.newPlay ();
		PlayerToken.editMode = editMode;
		
		// TODO: Statement to make DBController load the play here

		// Select the next play to be displayed, and load it from the database. Or, if in edit mode, load the play to edit
		if (editMode) {
			if (editPlayName == null)
				databaseController.loadPlayByName ("BLANK");
			else
				databaseController.loadPlayByName (editPlayName);
		}
		else {
			databaseController.SelectPlay ();
//			databaseController.loadPlay(currentPlay.ObjectId);
		}
		state = STATE.LOADING;
	}
	
	// Called by update when a selected play has been loaded successfully
	private void SetupNewPlay() {
		currentPlay.TryGetValue("Name", out playName);
		
		Debug.Log (playName);
		
		ParseObject oTeam;
		bool oTeamResult = currentPlay.TryGetValue("OffensiveTeam", out oTeam);
		if (oTeamResult) 
		{
			Debug.Log ("Offensive Team Loaded");
			ParseObject oPlayer;
			string playerKey;
			foreach (string s in oTeam.Keys)
				Debug.Log ("Key: " + s);
			for (int i=0;i<11;i++)
			{
				playerKey = "Player" + i.ToString();
				bool oPlayerResult = oTeam.TryGetValue(playerKey, out oPlayer);
				Debug.Log ("TryGetValueResult: " + oPlayerResult + ", ReturnedNull: " + (oPlayer == null));
				if (oPlayerResult)
				{
					//Debug.Log ("Loaded offensive player " + i + oPlayer.IsDataAvailable);
					offensiveTeam[i].Initialize(oPlayer);
				}
				else
					
					Debug.LogError ("Failed to get Offensive Player " + i);
			}
		}
		else
			Debug.LogError ("Failed to get offensive Team!");
		
		ParseObject dTeam;
		bool dTeamResult = currentPlay.TryGetValue("DefensiveTeam", out dTeam);
		if (dTeamResult) 
		{
			Debug.Log ("Defensive Team Loaded");
			ParseObject dPlayer;
			string playerKey;
			for (int i=0;i<11;i++)
			{
				playerKey = "Player" + i.ToString();
				bool dPlayerResult = dTeam.TryGetValue(playerKey, out dPlayer);
				Debug.Log ("TryGetValueResult: " + dPlayerResult + ", ReturnedNull: " + (dPlayer == null));
				if (dPlayerResult)
				{
					defensiveTeam[i].Initialize(dPlayer);
				}
				else
					Debug.LogError ("Failed to get Defensive Player " + i);
			}
		}
		else
			Debug.LogError ("Failed to get offensive Team!");
	}
	
	// Used by DatabaseController to return the requested play
	public void PlayLoaded(ParseObject loadedPlay) {
		currentPlay = loadedPlay;
		Debug.Log ("Play loaded: " + currentPlay.Get<string>("Name"));
		state = STATE.LOADED;
	}
	
	// Animate each player (With the correct data)
	public float AnimateCorrectPlay() {
		menuController.DisableInPlayCanvas();
		float delay = 0f;
		foreach (PlayerToken oPlayer in offensiveTeam)
			delay = System.Math.Max(delay, oPlayer.AnimateCorrectPlay())	;
		animLength = delay;
		return delay;
	}
	
	// Animate each player (With the data input by the user
	public float AnimateInputPlay() {
		menuController.DisableInPlayCanvas();
		float delay = 0f;
		foreach (PlayerToken oPlayer in offensiveTeam)
			delay = System.Math.Max(delay, oPlayer.AnimateInputPlay());
		animLength = delay;
		return delay;
	}

	
	public void TrySaveEdit(string playName) {
		Debug.Log ("TrySaveEditCalled");
		playNameToSave = playName;
		databaseController.checkPlayExists(playName);
		state = STATE.SAVING;
	}
	
	private void SaveEditFail() {
		menuController.SaveError ("Unable to save play: Play with name \"" + playNameToSave + "\" already exists. " +
								  "Choose a new name and try again.");
		state = STATE.INPLAY;
	}
	
	private void SaveEdit() {
		state = STATE.INPLAY;
		Debug.Log ("SaveEditCalled");
		ParseObject newPlay = new ParseObject("Play");
		ParseObject oTeam = new ParseObject("OffensiveTeam");
		ParseObject dTeam = new ParseObject("DefensiveTeam");
		
		for (int i = 0; i < 11; i++) 
		{
			ParseObject oPlayer = offensiveTeam[i].getInputParseObject();
			ParseObject dPlayer = defensiveTeam[i].getInputParseObject();
			
			string fieldName = "Player" + i;
			
			oTeam.Add(fieldName, oPlayer);
			dTeam.Add(fieldName, dPlayer);
		}
		
		newPlay["Name"] = playNameToSave;
		//newPlay["Parent"] = ParseObject.CreateWithoutData("Play", currentPlayID);
		newPlay["OffensiveTeam"] = oTeam;
		newPlay["DefensiveTeam"] = dTeam;
		
		newPlay.SaveAsync().ContinueWith( t => { Debug.Log("SAVED: " + t.Exception); }); ;
		
		// TODO: Switch over to que-based updating
		//playsToUpdate.Enqueue (newPlay);
		EndEdit();
	}
	
	public void ExistingFound(int found) {
		Debug.Log ("ExistingFoundCalled" + found + state);
		existingCount = found;
	}
	
	// End edit mode, cleaning up and scrubbing the player tokens
	public void EndEdit() {
		foreach(PlayerToken token in offensiveTeam)
			token.deInitialize();
		foreach(PlayerToken token in defensiveTeam)
			token.deInitialize();
		PlayerToken.editMode = false;
		editMode = false;
		state = STATE.INACTIVE;

		menuController.ShowMainMenu();
	}

	// Called at the end of gameplay of each play to grade the play, animate the results, and save them
	public int EndPlay () {
		Debug.Log (playName);
		
		// Animate the play
		AnimateInputPlay ();

		// check correctness and calculate new SRS coefficient
		int correctness = EvaluateCorrectness ();
		Debug.Log(correctness);
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
		//QueueUpdate ();
		
		return correctness;
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
			//bool inputLocationResult = inputParsePlayer.TryGetValue("Location", out inputLocation);
			// Location should always be set, so if no location is found output a debug message
			/*if (!inputLocationResult)
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
			}*/

			bool wrongLocation = CheckTooFar(oPlayer.location, oPlayer.correctLocation);
			if (wrongLocation)
			{
				return 0;
			}

			// check correctness of motion
			//bool inputMotionResult = inputParsePlayer.TryGetValue("Motion", out inputMotion);
			//bool correctMotionResult = correctParsePlayer.TryGetValue("Motion", out correctMotion);

			// if one of the two has a motion set and the other doesn't, return incorrect
			/*if (!inputMotionResult || !correctMotionResult)
			{
				string position;
				bool nameResult = inputParsePlayer.TryGetValue("Position", out position);
				if (nameResult)
				{
					Debug.Log ("error in correctness evaluation, null motion for player: " + position);
				}
				return 0;
			}*/

			// if both have a motion, check how far apart they are and return incorrect if it exceeds the threshold


			bool wrongMotion = CheckTooFar (oPlayer.motion, oPlayer.correctMotion);
			if (wrongMotion)
			{
				return 0;
			}

			// check correctness of shifts
			/*bool inputShiftResult = inputParsePlayer.TryGetValue("Shifts", out inputShifts);
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
			}*/


			if(oPlayer.shifts.Count != oPlayer.correctShifts.Count)
			{
				return 0;
			}

			for(int i=0; i<oPlayer.shifts.Count;i++)
			{
				bool wrongShift = CheckTooFar(oPlayer.shifts[i],oPlayer.correctShifts[i]);
				if (wrongShift)
				{
					return 0;
				}
			}

			// check correctness of responsibility
			/*bool inputResponsibilityResult = inputParsePlayer.TryGetValue("Responsib", out inputResponsibility);
			bool correctResponsibilityResult = correctParsePlayer.TryGetValue("Responsib", out correctResponsibility);

			if (inputResponsibilityResult ^ correctResponsibilityResult)
			{
				return 0;
			}

			if (inputResponsibilityResult && correctResponsibilityResult)
			{

			}*/
			if (oPlayer.responsibility.ObjectId != oPlayer.correctResponsibility.ObjectId)
			{
				return 0;
			}
		}
		return 1;
	}

	public static bool CheckTooFar(Vector2 correct, Vector2 input)
	{
		if (correct == Vector2.zero && input !=Vector2.zero) 
		{
			return true;
		}
		
		float distance = Vector2.Distance (correct, input);
		if (distance > distanceCorrectnessThreshold)
		{
			return true;
		}
		
		return false;
	}

	public static bool CheckTooFar(Vector3 correct, Vector3 input)
	{
		float distance = Vector3.Distance (correct, input);
		if (distance > distanceCorrectnessThreshold)
		{
			return true;
		}
		
		return false;
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
	//   Begins setting up the play once it has been loaded
	void Update () {
		if (state == STATE.LOADED && !editMode){
			state = STATE.SETUP;
			SetupNewPlay();
		}
		else if (state == STATE.LOADED){
			state = STATE.SETUP;
			SetupNewPlay();
		}
		switch (state) {
			case STATE.LOADED:
				state = STATE.SETUP;
				SetupNewPlay();
				break;
			case STATE.SAVING:
				if (existingCount == 0)
					SaveEdit();
				else if (existingCount > 0)
					SaveEditFail();
				break;
			default:
				break;
		}
	}
	
	
	// TODO: Temporary play editor functions
	public void StartEditPlay() {
		editMode = true;
	}


}
