using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class GameController : MonoBehaviour {
	// State handler
	public enum STATE {INACTIVE, LOADING, LOADED, SETUP, INPLAY, POSTPLAY, SAVING};
	public STATE state = STATE.INACTIVE;
	public bool editMode = false;
	
	// Sound handler for 'wrong answer' sound
	public AudioSource iBuzzSound;
	public AudioClip iBuzzClip;
	public static AudioSource buzzSound;
	public static AudioClip buzzClip;

	// Player Tokens
	public List<PlayerToken> offensiveTeam;
	public List<PlayerToken> defensiveTeam;
	public DatabaseController databaseController;
	
	// Edit mode variables
	public int existingCount = -1;
	private string playNameToSave;
	private string editPlayName;

	// Variables for keeping track of the current play
	public ParseObject currentPlay;
	private string currentPlayID;
	public float animLength;
	
	// Correctness tracking variables
	public System.DateTime playLastSeen;
	public string playName;
	private int timeLeft;
	private const float distanceCorrectnessThreshold = 6;

	// Queues for information to be saved back to the cloud database
	Queue playsToUpdate = new Queue ();
	Queue userPlaysToUpload = new Queue ();
	
	// Menu controller
	public MenuController menuController;

	/// <summary>
	/// Play the 'buzz' sound for incorrect input
	/// </summary>
	public static void playBuzz() {
		buzzSound.PlayOneShot(buzzClip);
	}

	// Initialize variables
	void Start () {
		buzzSound = iBuzzSound;
		buzzClip = iBuzzClip;
	}
	
	/// <summary>
	/// Called when a play is being edited. 
	/// </summary>
	/// <param name="playName">The name of the play being edited</param>
	public void EditPlay (string playName) {
		editMode = true;
		editPlayName = playName;
		NewPlay ();
	}

	/// <summary>
	/// Called when a new round of play begins
	/// </summary>
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
		

		// Select the next play to be displayed, and load it from the database. Or, if in edit mode, load the play to edit
		if (editMode) {
			if (editPlayName == null)
				databaseController.loadPlayByName ("BLANK");
			else
				databaseController.loadPlayByName (editPlayName);
		}
		else {
			databaseController.SelectPlay ();
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
	/// <summary>
	/// Callback used by DatabaseController to return the requested play (after it has been loaded asyncronously
	/// </summary>
	/// <param name="loadedPlay">The parse object containing the play that has been loaded</param>
	public void PlayLoaded(ParseObject loadedPlay) {
		currentPlay = loadedPlay;
		Debug.Log ("Play loaded: " + currentPlay.Get<string>("Name"));
		state = STATE.LOADED;
	}
	
	/// <summary>
	/// Animate the correct play
	/// </summary>
	/// <returns>The length of the animation being played (used by calling functions to proceed after the animation is finished but not before)</returns>
	public float AnimateCorrectPlay() {
		menuController.DisableInPlayCanvas();
		float delay = 0f;
		foreach (PlayerToken oPlayer in offensiveTeam)
			delay = System.Math.Max(delay, oPlayer.AnimateCorrectPlay())	;
		animLength = delay;
		return delay;
	}
	
	/// <summary>
	/// Animate the input play
	/// </summary>
	/// <returns>The length of the animation being played (used by calling functions to proceed after the animation is finished but not before)</returns>
	public float AnimateInputPlay() {
		menuController.DisableInPlayCanvas();
		float delay = 0f;
		foreach (PlayerToken oPlayer in offensiveTeam)
			delay = System.Math.Max(delay, oPlayer.AnimateInputPlay());
		animLength = delay;
		return delay;
	}

	/// <summary>
	/// Attempt to save the newly created play. Begins the asyncronous process of checking to make sure a play with the selected name does not already exist.
	/// </summary>
	/// <param name="playName">The name of the new play to be saved</param>
	public void TrySaveEdit(string playName) {
		Debug.Log ("TrySaveEditCalled");
		playNameToSave = playName;
		databaseController.checkPlayExists(playName);
		state = STATE.SAVING;
	}
	
	// Called by to abandon the saving of a play when the play name is already used. Pops an error message and returns to the play editing state.
	private void SaveEditFail() {
		menuController.SaveError ("Unable to save play: Play with name \"" + playNameToSave + "\" already exists. " +
								  "Choose a new name and try again.");
		state = STATE.INPLAY;
	}
	
	// Callback to finish the saving of a play when the play name used is not already in use.
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
		newPlay["OffensiveTeam"] = oTeam;
		newPlay["DefensiveTeam"] = dTeam;
		
		newPlay.SaveAsync().ContinueWith( t => { Debug.Log("SAVED: " + t.Exception); }); ;
		
		EndEdit();
	}
	/// <summary>
	/// Callback indicating if any plays with the selected play name already exist when saving
	/// </summary>
	/// <param name="found">The number of plays with the indicated name exist. Should always be 0 or 1</param>
	public void ExistingFound(int found) {
		Debug.Log ("ExistingFoundCalled" + found + state);
		existingCount = found;
	}
	
	/// <summary>
	/// Ends edit mode, cleaning up variables, resetting the player tokens, and returning to the main menu.
	/// </summary>
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

	/// <summary>
	/// Called at the end of gameplay of each play to grade the play, animate the results, and save them
	/// </summary>
	/// <returns>The player's score - currently only returns 1 (correct) or 0 (incorrect)</returns>
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

	/// <summary>
	/// Correctness checker for positions - insures that an inputted position/shift/motion is within the acceptable margin of error from the correct position
	/// </summary>
	/// <returns><c>true</c>, if the input is too far from the correct value, <c>false</c> otherwise.</returns>
	/// <param name="correct">The correct location</param>
	/// <param name="input">The input location</param>
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

	// SRS coefficient is typically the product of age component, progress component, effort component
	float SRSRecalculate (int correctness, System.DateTime sinceLastPlayed) {
		float ageFactor;
		float effortFactor;
		float totalCoefficient;
		
		// Age component A(x) = Cn^x	(x is time elapsed, C is initial constant)
		// n used for age component = e^(ln(2)/5)
		System.DateTime now = System.DateTime.Now;
		System.TimeSpan elapsed = now.Subtract(sinceLastPlayed);
		float daysAgo = (float)elapsed.TotalDays;
		float ageBase = (float)System.Math.Exp (System.Math.Log (2) / 5);
		ageFactor = (float)System.Math.Pow (ageBase, daysAgo);

		// Progress component P(x) = Cn^-x	(x is previous success)
		// Omitted in current version, would need user testing to tune parameters

		// Effort component is {1,10}
		if (correctness == 0) 
		{
			effortFactor = 10.0f;
		}
		else
		{
			effortFactor = 10f;
		}

		totalCoefficient = ageFactor * effortFactor;

		return  totalCoefficient;
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
	
	/// <summary>
	/// Starts the play editing mode
	/// </summary>
	public void StartEditPlay() {
		editMode = true;
	}


}
