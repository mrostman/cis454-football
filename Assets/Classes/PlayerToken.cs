using UnityEngine;
using System.Collections; 
using System.Collections.Generic;
using Parse;

[RequireComponent(typeof(BoxCollider))]
[System.Serializable]

public class PlayerToken : MonoBehaviour {
	// State (For determining how to respond based on what input has already been recieved)
	private enum STATE {UNAWAKE, UNINITIALIZED, INITIALIZED, SHIFTING, SHIFTED, MOTIONING, MOTIONED, RESPONSIBLE};
	private STATE state = STATE.UNAWAKE;
	private bool shiftsSet;
	private bool motionSet;

	// Mouse handling variables
	private Vector3 offset; 			// Tracking movement between frames
	private static float heightFactor; 	// Tracking size of screen
	private int doubleClickCount = 0;	// Doubleclick handling
	public bool popMenu = false; 		// Flag indicating the menu has been requested (via doubleclick)
	private bool responsibilityMenu = false;
	public PieMenu menu;
	public GUIContent[] menuContentInitialized;
	public GUIContent[] menuContentShifted;
	public GUIContent[] menuContentMotioned;
	public GUIContent[] menuContentResponsibility;
	private Vector2 menuTarget;
	private int menuSpawnTime;			// Counter to prevent clicks being immediately registered on menu
	private int menuPage = 0;
	
	// Grid variables
	private const int maxX = 10;  	// Horizontal grid size
	private const int maxY = -9;		// Vertacle grid size	
	private Vector3 target;				// Snap to grid - point to snap to
	private Vector3 startPoint; 		// Snap to grid - point the token is snapping from
	private bool snapping;				// Is the token in the process of snapping to a position?
	private static List<Vector3> huddle;// Locations to put player tokens initially (in the huddle)
	private static int huddleIndex;		// Locations to put 
	
	// Game variables (set only at initialization
	public bool controllable;			// Is the token controllable by the player
	private string abbreviation;		// Abbreviation for the player's position
	private string position;			// Name of the player's position
	
	// Assigned variables (The 'correct' input)
	public Vector3 correctLocation;		// Location of the token
	public Vector2 correctMotion;		// Assigned motion
	public List<Vector2> correctShifts; // Assigned shifts
	public ParseObject correctResponsibility;// Assigned responsibility
	private static int correctMaxShifts;// Highest number of assigned shifts among all players 
	
	// User input variables (The user's input)
	public Vector3 location;			// Location of the token
	public Vector2 motion;				// Input motion
	public List<Vector2> shifts;		// Input shifts
	public ParseObject responsibility;	// Input responsibility
	private static int maxShifts;		// Highest number of input shifts among all players

	// Display text
	public TextMesh displayText;		// Text to be displayed over player model (abbreviation usually)
	
	// Lines (For indicating input movements
	public List<GameObject> lineHolders;
	private List<LineRenderer> lines;
	private int lineIndex;
	
	// Player Model
	public GameObject bigGuy;
	public GameObject bigGuyModel;
	public Animator bigGuyAnimator;
	public Material defaultMaterial;
	public Material ghostMaterial;
	private bool running = false;
	private static Vector3 runningOffset = new Vector3(0,-0.6f,0);
	private Vector3 facing = Vector3.zero;
	
	// Animation Variables
	private const float shiftTime = 2f;
	private const float idleTime = 0.5f;
	private const float motionTime = 2f;
	private const float responsibilityTime = 2f;
	private const float turnTime = 0.5f;
	
	// Initialization parseObject
	private ParseObject parsePlayer;
	
	// Initialization functions
	public void Initialize(ParseObject iParsePlayer)
	{
		parsePlayer = iParsePlayer;
	}
	private void InitializeThreadsafe()
	{
		// Set initialized to avoid re-initialization
		state = STATE.INITIALIZED;
		
		// Create Temporary variables for holding raw lists from parse
		IList iLocation, iShifts, iMotion;
		
		// Try to get raw lists from parse (using TryGetValue to avoid issues with unset values)
		if (!parsePlayer.TryGetValue("Location", 	out iLocation   )) {
			iLocation = new List<float>();
			iLocation.Add(0f); iLocation.Add (0f); }
		if (!parsePlayer.TryGetValue("Motion", 	out iMotion      )) {
			iMotion = new List<float>();
			iMotion.Add(0f); iMotion.Add (0f); }
		if (!parsePlayer.TryGetValue("Shifts" ,    out iShifts    )) {
			iShifts = new List<List<float>>();
		}
		
		// Convert the raw lists to Unity Vectors
		correctLocation = new Vector3(float.Parse(iLocation[0].ToString()), float.Parse(iLocation[1].ToString()), -1f);
		correctMotion = new Vector2(float.Parse(iMotion[0].ToString()), float.Parse(iMotion[1].ToString()));
		correctShifts = new List<Vector2>();
		foreach (IList m in iShifts)
			correctShifts.Add (new Vector2(float.Parse(m[0].ToString()), float.Parse(m[1].ToString())));
		
		// Initialize other values (again using TryGetValue to avoid issues with unset values)
		//                           Value to get       Variable to save      Default value if unset
		if (!parsePlayer.TryGetValue("Controllable", 	out controllable)) 	{ controllable = false; }
		if (!parsePlayer.TryGetValue("Abbreviation", 	out abbreviation)) 	{ abbreviation = ""; }
		if (!parsePlayer.TryGetValue("Position"    , 	out position    )) 	{ position = ""; }
		if (!parsePlayer.TryGetValue("Responsib", 	out correctResponsibility)){ correctResponsibility = null; }
		
		// Set token to display abbreviation
		displayText.text = abbreviation;
		
		// If the token is not controllable, set it to it's 'correct' values
		if (!controllable) {
			location = correctLocation;
			motion = correctMotion;
			shifts = correctShifts;
			responsibility = correctResponsibility;
			correctMaxShifts = System.Math.Max(correctMaxShifts, shifts.Count);
		}
		// Otherwise set it to the 'blank' starting values.
		else {
			location = huddle[huddleIndex++];
			shifts = new List<Vector2>();
			motion = new Vector2(0,0);
			responsibility = null;
			shiftsSet = motionSet = false;
		}
		
		// Move the token to it's location
		this.gameObject.transform.position = location;
		
		//bigGuyAnimator.playbackTime = Random.Range(0f, 100f);
	}
	public void deInitialize() {
		// Set up the line renderers
		lineIndex = 0;
		lines = new List<LineRenderer>();
		foreach (GameObject lineHolder in lineHolders)
			lines.Add( lineHolder.GetComponent<LineRenderer>() );
			
		/// Clear the shifts/motions/responsibilities
		clearInput ();
		
		// Move the player out of sight
		this.gameObject.transform.position = new Vector3(0,0,5);
		
		// Clear the text
		abbreviation = "";
		displayText.text = abbreviation;
		
		// Set other essential variables to default values
		controllable = false;
		snapping = false;
		parsePlayer = null;
		
		// Set the state to uninitialized
		state = STATE.UNINITIALIZED;
	}
	
	// Reset the huddleIndex for a new play
	public static void newPlay() {
		huddleIndex = 0;
	}
	
	// Set various variables to default values to avoid unset variable issues
	void Awake () {
		// Set the animation to a random position, so players arn't perfectly in sync
		bigGuyAnimator.Play ("Idle", -1, Random.Range (0f,2f));
		running = false;
		
		// Set various default values
		deInitialize();
		
		// Set up tags and the screen size
		heightFactor = 20.0f / Screen.height;
		this.gameObject.tag = "token";
			
		// Set up the huddle list
		if(huddle == null){
			huddle = new List<Vector3>();
			huddle.Add (new Vector3( 0, -6, -1));
			huddle.Add (new Vector3(-1, -7, -1));
			huddle.Add (new Vector3(-1, -8, -1));
			huddle.Add (new Vector3( 0, -9, -1));
			huddle.Add (new Vector3( 1, -8, -1));
			huddle.Add (new Vector3( 1, -7, -1));
			huddle.Add (new Vector3( 1, -6, -1));
			huddle.Add (new Vector3(-1, -6, -1));
			huddle.Add (new Vector3( 1, -9, -1));
			huddle.Add (new Vector3(-1, -9, -1));
			huddle.Add (new Vector3( 0, -9, -1));
		}
	}

	// Unity function for GUI updates, used to reposition pie menu
	void OnGUI () {
		menu.Center = menuTarget;	//Set the center point of the pie menu in every GUI draw.
		menu.Run();					//This is the function, which will draw the pie on the GUI if it's Active. You can place it anywhere in the OnGUI function.
	}

	// FixedUpdate is called every physics frame (50 times/second)
	// Handles doubleclick tracking
	void FixedUpdate() {
		if (doubleClickCount > 0)
			doubleClickCount--;
		}
	
	// Update is called once per frame
	void Update () {
		// Compensate for animation drift
		bigGuy.transform.localPosition = Vector3.zero;
		
		// Menu handling
		if (popMenu)
		{
			if (responsibilityMenu)
				showMenuResponsibility();
			else
				switch (state) {
					case STATE.INITIALIZED:
						showMenuInitialized();
						break;
					case STATE.SHIFTED:
						showMenuShifted();
						break;
					case STATE.MOTIONED:
					case STATE.RESPONSIBLE:
						showMenuMotioned();
						break;
					default:
						Debug.Log("Attempt to open menu in invalid state: " + state);
						popMenu = false;
						break;
				}
		}
		
		// Initialize the token if it's ready for initialization
		if (state == STATE.UNINITIALIZED && parsePlayer != null)
			InitializeThreadsafe();
	
		// Handle snapping to grid. Target is determined on mouseUp (that is, when the user 'drops' the playerToken)
		float distanceToTarget = Vector3.Distance(this.transform.position, target);
		if (snapping && distanceToTarget > 0.001f) {
			if (distanceToTarget > 0.005f)
				this.transform.position = Vector3.MoveTowards (this.transform.position, 
				                                               target,
				                                               distanceToTarget / 0.005f);
			else
				this.transform.position = Vector3.MoveTowards (this.transform.position,
				                                               target,
				                                               distanceToTarget);
		}
		else { snapping = false; }
	}
	
	void showMenuInitialized() {
		if (menuSpawnTime++ == 0)
			menu.InitPie(PieMenuController.menuContentInitialized.ToArray());
		else if (menuSpawnTime++ < 20)
			return;
		else if (Input.GetMouseButtonDown(0)){
			Debug.Log (menu.Selected);
			if (menu.Selected == 0) { // "Add Shift" Selected
				state = STATE.SHIFTING;
				menu.Close ();
				popMenu = false;
			}
			else if (menu.Selected == 1) {
				state = STATE.MOTIONING;
				menu.Close ();
				popMenu = false;
			}
			else if (menu.Selected == 2) {
				responsibilityMenu = true;
				menuPage = 0;
				menuSpawnTime = 1;
				menu.TransitionPie(PieMenuController.menuContentResponsibility[menuPage].ToArray());
			}
			else if (menu.Selected == 3 && Input.GetMouseButtonDown(0)) {	// "Cancel" Selected
				menu.Close();
				popMenu = false;
			}
			else {
				menu.CenterImageTint = Color.white;
				menu.SelectedOptionTint = Color.black;
			}
		}
	}

	void showMenuShifted(){
		if (menuSpawnTime++ == 0)
			menu.InitPie(PieMenuController.menuContentShifted.ToArray());
		else if (menuSpawnTime++ < 20)
			return;
		else if (Input.GetMouseButtonDown(0)){
			Debug.Log ("Menu selected: " + menu.Selected);
			switch (menu.Selected)
			{
				case 0:
					state = STATE.SHIFTING;
					menu.Close ();
					popMenu = false;
					break;
				case 1:
					state = STATE.MOTIONING;
					menu.Close ();
					popMenu = false;
					break;
				case 2:
					responsibilityMenu = true;
					menuPage = 0;
					menuSpawnTime = 1;
					menu.TransitionPie(PieMenuController.menuContentResponsibility[menuPage].ToArray());
					break;
				case 3:
					clearInput();
					menu.Close ();
					popMenu = false;
					break;
				case 4:
					menu.Close();
					popMenu = false;
					break;
				default:
					break;
			}
		}
	}
	
	void showMenuMotioned(){
		if (menuSpawnTime++ == 0)
			menu.InitPie(PieMenuController.menuContentMotioned.ToArray());
		else if (menuSpawnTime++ < 20)
			return;
		else if (Input.GetMouseButtonDown(0)){
			Debug.Log ("Menu selected: " + menu.Selected);
			switch (menu.Selected)
			{
			case 0:
				responsibilityMenu = true;
				menuPage = 0;
				menuSpawnTime = 1;
				menu.TransitionPie(PieMenuController.menuContentResponsibility[menuPage].ToArray());
				break;
			case 1:
				clearInput();
				menu.Close ();
				popMenu = false;
				break;
			case 2:
				menu.Close();
				popMenu = false;
				break;
			default:
				break;
			}
		}
	}
	
	void showMenuResponsibility() {
		List<GUIContent> page = PieMenuController.menuContentResponsibility[menuPage];
		if (menuSpawnTime++ == 0)
			menu.TransitionPie(PieMenuController.menuContentResponsibility[menuPage].ToArray());
		else if (menuSpawnTime++ < 20)
			return;
		else if (Input.GetMouseButtonDown(0)){
			GUIContent selected = page[menu.Selected];
			if (selected.tooltip.Equals("Back")) {
				menu.TransitionPie(PieMenuController.menuContentResponsibility[--menuPage].ToArray ());
				menuSpawnTime = 1;
			}
			else if (selected.tooltip.Equals("Next")) {
				menu.TransitionPie(PieMenuController.menuContentResponsibility[++menuPage].ToArray ());
				menuSpawnTime = 1;
			}
			else {
				int index;
				if (menuPage == 0) 
					index = menu.Selected;
				else
					index = (7 + ( (menuPage - 1) * 6 ) + menu.Selected);
				responsibility = DatabaseController.responsibilityQueryResults[index];
				responsibilityMenu = false;
				menu.Close ();
				popMenu = false;
			}
		}
	}
	
	// Function to clear input (Shifts, motion, and responsibility), called by pie menu (as well as by re-initializer?)
	void clearInput() {
		// Move back to the set location
		this.transform.position = location;
		
		// Clear shifts, motion, and responsibility
		shifts = new List<Vector2>();
		motion = new Vector2(0,0);
		responsibility = null;
		
		// Erase the lines
		foreach(LineRenderer line in lines)
		{
			line.SetVertexCount(0);
		}
		
		// Set the state back to initialized
		state = STATE.INITIALIZED;
	}
	
	// Helper function to determine if it should currently be possible to drag the token
	private bool checkDraggable() {
		if (state == STATE.UNINITIALIZED || state == STATE.SHIFTED || state == STATE.MOTIONED || state == STATE.RESPONSIBLE || !controllable) {
		    return false;
		}
		return true;
	}
	
	// Helper function to revert the state on a failed drag
	private void rollbackState() {
		if (!motionSet)
			state = STATE.SHIFTED;
		if (!shiftsSet)
			state = STATE.INITIALIZED;
		responsibilityMenu = false;
		running = false;
	}

	// Called when the mouse button is pushed
	void OnMouseDown() { 
		Debug.Log ("Mouse down in " + state + ", Menu " + popMenu);
		// Check if this click is the second click of a doubleclick, if so, we call the menu
		if (doubleClickCount > 0) {
			menuTarget = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			Debug.Log ("DoubleClick");
			menuSpawnTime = 0;
			popMenu = true;
		}
	
		// Check for states in which we want to ignore mouse dragging
		if (!checkDraggable()){
			doubleClickCount = 50;
			return;
		}
		
		
		// If not a double click, start dragging the player
		else {
			Debug.Log ("Drag");
			
			startPoint = new Vector3 (this.transform.position.x,
                         this.transform.position.y,
                         this.transform.position.z);
			snapping = false;
			offset = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0);
			}
		}
	
	// Called when the mouse is moved with the button down
	void OnMouseDrag() {
		if (!checkDraggable())
			return;
			
		Vector3 oldPos = transform.position;
		// Move the token to it's new location, then reset the offset position to the new location
		transform.Translate( (Input.mousePosition.x - offset.x) * heightFactor, (Input.mousePosition.y - offset.y) * heightFactor, 0);
		
		running = true;

		// Reset the offset
		offset.x = Input.mousePosition.x;
		offset.y = Input.mousePosition.y;
	}

	// Called when the mouse button is released
	void OnMouseUp() {
		// If we weren't dragging (Because we were not able to do so), do nothing
		if (!checkDraggable())
			return;
		running = false;
		
		// Handle animation drift
		bigGuy.transform.localPosition = Vector3.zero;
			
		// Set snap position
		target = new Vector3 (Mathf.Round (this.transform.position.x), Mathf.Round (this.transform.position.y), this.transform.position.z);
		
		// Check for invalid placement (outside of offensive side of the field of play)
		if (Mathf.Abs (target.x) > maxX || target.y > -1 || target.y < maxY) {
			transform.position = startPoint;
			rollbackState();
			return;
		}
		
		// Check for invalid placement (overlapping another token
		var otherTokens = GameObject.FindGameObjectsWithTag ("token");
		foreach (var token in otherTokens) {
			if (token != this.gameObject && Vector3.Distance(target, token.transform.position) < 0.01f) {
				transform.position = startPoint;
				rollbackState();
				return;
			}
		}
		
		// If placement is valid, set snapping so update will move the token
		this.gameObject.MoveTo (target,0.5f,0);

		// Check to insure we actually moved (If not, THEN we set the doubleclick value)
		if (target == startPoint) {
			rollbackState();
			doubleClickCount = 50;
			return;
		}
		
		// Save movement in appropriate location
		switch (state){
			case STATE.INITIALIZED:
				relocate(target);
				break;
			case STATE.SHIFTING:
				addShift (startPoint, target);
				break;
			case STATE.MOTIONING:
				setMotion (startPoint, target);
				break;
			default:
				Debug.LogError("Mouse drag in invalid state: " + state);
				break;
		}
	}
	
	private void relocate(Vector3 end){
		location = end;
	}
	
	// Add a Shift
	private void addShift(Vector3 start, Vector3 end){
		// Add the shift to the list of shifts
		shifts.Add(new Vector2(end.x - start.x, end.y - start.y));
		shiftsSet = true;
		maxShifts = System.Math.Max(maxShifts, shifts.Count);

		// Add movement line
		lines[lineIndex].SetColors(Color.red, Color.red);
		lines[lineIndex].SetVertexCount(2);
		lines[lineIndex].SetPosition(0, startPoint);
		lines[lineIndex].SetPosition(1, target);
		lineIndex++;
		
		// Set new state
		state = STATE.SHIFTED;
	}
	
	// Set the assigned motion (called on mouseup after a drag in motioning state)
	private void setMotion(Vector3 start, Vector3 end){
		motion = new Vector2(end.x - start.x, end.y - start.y);
		motionSet = true;
		
		// Add movement line
		lines[lineIndex].SetColors(Color.magenta, Color.magenta);
		lines[lineIndex].SetVertexCount(2);
		lines[lineIndex].SetPosition(0, startPoint);
		lines[lineIndex].SetPosition(1, target);
		lineIndex++;
		
		// Set new state
		state = STATE.MOTIONED;
	}
	
	// Return a parseobject of the Player containing the user input values
	public ParseObject getInputParseObject(){
		ParseObject playerOut = new ParseObject("Player");
		
		float[] pLocation = new float[2];
		float[] pMotion = new float[2];
		List<float[]> pShifts = new List<float[]>();
		
		pLocation[0] = location.x;
		pLocation[1] = location.y;
		pMotion[0] = motion.x;
		pMotion[1] = motion.y;
		foreach (Vector2 shift in shifts)
			pShifts.Add (new float[2] {shift.x, shift.y});
		
		
		playerOut["Abbreviation"] = abbreviation;
		playerOut["Position"] = position;
		playerOut["Location"] = pLocation;
		playerOut["Motion"] = pMotion;
		playerOut["Shifts"] = pShifts;
		if (responsibility != null)
			playerOut["Responsib"] = ParseObject.CreateWithoutData("Responsibility", responsibility.ObjectId);
		
		return playerOut;
	}
	
	// Return a parseobject of the Player containing the correct values (The original ParseObject used for initialization)
	public ParseObject getCorrectParseObject() {
		return parsePlayer;
	}
	
	// Animate the input play
	public float AnimateInputPlay() {
		// Move to the start of the path
		ClearTokenIncorrect ();
		MoveToStart ();
		
		// Hide the input lines
		linesShow (false);
		
		// Set up local variables
		float delay = 0f;
		GameObject token = this.gameObject;
		Vector3 loc = location;
		Vector3 prevLoc = loc;
		
		// Check for correct location
		if (GameController.CheckTooFar(location, correctLocation))
			SetTokenIncorrect();
		
		// Loop through and animate each shift 
		int i = 0;
		if (shifts != null)
			for (i = 0; i < shifts.Count; i++){
				// Set target for the shift
				prevLoc = loc;
				loc = loc + new Vector3 (shifts[i].x, shifts[i].y, 0);
				
				// Check correctness
				if (i < correctShifts.Count && !GameController.CheckTooFar(shifts[i], correctShifts[i])) {}
				else
					Invoke ("SetTokenIncorrect", delay);
				
				
				// Set up the shift options
				Hashtable shiftOptions = new Hashtable();
				Hashtable lookOptions = new Hashtable();
				shiftOptions.Add ("position", loc);
				shiftOptions.Add ("time", shiftTime);
				shiftOptions.Add ("delay", delay);
				shiftOptions.Add ("easetype", iTween.EaseType.easeInOutQuad);
				shiftOptions.Add ("onstart", "SetAnimationRun");
				shiftOptions.Add ("oncomplete", "SetAnimationIdle");
				lookOptions.Add ("rotation", Quaternion.LookRotation(new Vector3 (0,0,1), loc - prevLoc).eulerAngles);
				lookOptions.Add ("time", turnTime);
				lookOptions.Add ("delay", delay);
				
				// Animate the shift
				iTween.MoveTo (token, shiftOptions);
				iTween.RotateTo (token, lookOptions);
				
				// Stand idle during the delay between shifts
				Hashtable idleOptions = new Hashtable();
				idleOptions.Add ("y", 1f);
				idleOptions.Add ("islocal", true);
				idleOptions.Add ("time", idleTime);
				idleOptions.Add ("delay", delay + shiftTime);
				iTween.RotateTo (token, idleOptions);
				
				// Update Delay value
				delay += shiftTime + idleTime;
			}
		
		// If any other players have more shifts then this one, wait for them to finish
		delay += (maxShifts - i) * (shiftTime + idleTime);
		
		// Set correctness of the motions
		if (GameController.CheckTooFar(motion, correctMotion))
			Invoke ("SetTokenIncorrect", delay);
			
		// Animate the motion (if any)
		if (motion != Vector2.zero) {
			// Set target for the motion
			prevLoc = loc;
			loc = loc + new Vector3 (motion.x, motion.y, 0);
			
			// Set up the options for the motion
			Hashtable motionOptions = new Hashtable ();
			Hashtable lookOptions = new Hashtable();
			motionOptions.Add ("position", loc);
			motionOptions.Add ("time", motionTime);
			motionOptions.Add ("delay", delay);
			motionOptions.Add ("easetype", iTween.EaseType.easeInOutQuad);
			motionOptions.Add ("onStart", "SetAnimationRun");
			motionOptions.Add ("oncomplete", "SetAnimationIdle");
			lookOptions.Add ("rotation", Quaternion.LookRotation(new Vector3 (0,0,1), loc - prevLoc).eulerAngles);
			lookOptions.Add ("time", turnTime);
			lookOptions.Add ("delay", delay);
			
			// Animate the motion
			iTween.MoveTo(token, motionOptions);
			iTween.RotateTo (token, lookOptions);
			
			// Increment the delay value
			delay += motionTime;
		}
		
		// Responsibility
		if (responsibility != null) {
			// Get the responsibility values out of the parseobject
			IList responsibilityCoordsI;
			if (!responsibility.TryGetValue("Coordinates" , out responsibilityCoordsI)) {
				Debug.LogError ("Cannot load responsibilty coordinates!");
			}
			else {
				List <Vector2> responsibilityCoords = new List<Vector2>();
				foreach (IList c in responsibilityCoordsI)
					responsibilityCoords.Add (new Vector2(float.Parse(c[0].ToString()), float.Parse(c[1].ToString())));
				
				// Check Responsibility correctness
				if (responsibility.ObjectId != correctResponsibility.ObjectId)
					Invoke ("SetTokenIncorrect", delay);
				
				// Animator for the responsibility
				foreach (Vector2 v in responsibilityCoords){
					// Set the target for this leg of the responsibility
					prevLoc = loc;
					loc = loc + new Vector3 (v.x, v.y, 0);
					
					// Options for the leg of the responsibility
					Hashtable respOptions = new Hashtable();
					Hashtable lookOptions = new Hashtable();
					respOptions.Add ("position", loc);
					respOptions.Add ("time", responsibilityTime / responsibilityCoords.Count);
					respOptions.Add ("delay", delay);
					respOptions.Add ("easetype", iTween.EaseType.linear);
					respOptions.Add ("onstart", "SetAnimationRun");
					lookOptions.Add ("rotation", Quaternion.LookRotation(new Vector3 (0,0,1), loc - prevLoc).eulerAngles);
					lookOptions.Add ("time", 0.1f);
					lookOptions.Add ("delay", delay);
					
					// Animate the leg of the responsibility
					iTween.MoveTo (token, respOptions);
					iTween.RotateTo (token, lookOptions);
					
					// Update the delay
					delay += responsibilityTime / responsibilityCoords.Count;
				}
				// Stop motion at the end of the responsiblity
				Invoke("SetAnimationIdle", delay);
			}
		}
		else if (correctResponsibility != null){
			Invoke("SetTokenIncorrect", delay);
			delay += responsibilityTime;
		}
		return delay;
	}
	
	public float AnimateCorrectPlay() {
		// Move to the start of the path
		ClearTokenIncorrect ();
		MoveToCorrectStart ();
		
		// Hide the input lines
		linesShow (false);
		
		// Set up local variables
		float delay = 0f;
		GameObject token = this.gameObject;
		Vector3 loc = correctLocation;
		Vector3 prevLoc = loc;
		
		// Loop through and animate each shift 
		int i = 0;
		if (shifts != null)
			for (i = 0; i < correctShifts.Count; i++){
				// Set target for the shift
				prevLoc = loc;
				loc = loc + new Vector3 (correctShifts[i].x, correctShifts[i].y, 0);
				
				// Set up the shift options
				Hashtable shiftOptions = new Hashtable();
				Hashtable lookOptions = new Hashtable();
				shiftOptions.Add ("position", loc);
				shiftOptions.Add ("time", shiftTime);
				shiftOptions.Add ("delay", delay);
				shiftOptions.Add ("easetype", iTween.EaseType.easeInOutQuad);
				shiftOptions.Add ("onstart", "SetAnimationRun");
				shiftOptions.Add ("oncomplete", "SetAnimationIdle");
				lookOptions.Add ("rotation", Quaternion.LookRotation(new Vector3 (0,0,1), loc - prevLoc).eulerAngles);
				lookOptions.Add ("time", turnTime);
				lookOptions.Add ("delay", delay);
				
				// Animate the shift
				iTween.MoveTo (token, shiftOptions);
				iTween.RotateTo (token, lookOptions);
				
				// Stand idle during the delay between shifts
				Hashtable idleOptions = new Hashtable();
				idleOptions.Add ("y", 1f);
				idleOptions.Add ("islocal", true);
				idleOptions.Add ("time", idleTime);
				idleOptions.Add ("delay", delay + shiftTime);
				iTween.RotateTo (token, idleOptions);
				
				// Update Delay value
				delay += shiftTime + idleTime;
			}
		
		// If any other players have more shifts then this one, wait for them to finish
		delay += (correctMaxShifts - i) * (shiftTime + idleTime);
		
		// Animate the motion (if any)
		if (correctMotion != Vector2.zero) {
			// Set target for the motion
			prevLoc = loc;
			loc = loc + new Vector3 (correctMotion.x, correctMotion.y, 0);
			
			// Set up the options for the motion
			Hashtable motionOptions = new Hashtable ();
			Hashtable lookOptions = new Hashtable();
			motionOptions.Add ("position", loc);
			motionOptions.Add ("time", motionTime);
			motionOptions.Add ("delay", delay);
			motionOptions.Add ("easetype", iTween.EaseType.easeInOutQuad);
			motionOptions.Add ("onStart", "SetAnimationRun");
			motionOptions.Add ("oncomplete", "SetAnimationIdle");
			lookOptions.Add ("rotation", Quaternion.LookRotation(new Vector3 (0,0,1), loc - prevLoc).eulerAngles);
			lookOptions.Add ("time", turnTime);
			lookOptions.Add ("delay", delay);
			
			// Animate the motion
			iTween.MoveTo(token, motionOptions);
			iTween.RotateTo (token, lookOptions);
			
			// Increment the delay value
			delay += motionTime;
		}
		
		// Responsibility
		if (correctResponsibility != null) {
			// Get the responsibility values out of the parseobject
			IList responsibilityCoordsI;
			if (!correctResponsibility.TryGetValue("Coordinates" , out responsibilityCoordsI)) {
				Debug.LogError ("Cannot load responsibilty coordinates!");
			}
			else {
				List <Vector2> responsibilityCoords = new List<Vector2>();
				foreach (IList c in responsibilityCoordsI)
					responsibilityCoords.Add (new Vector2(float.Parse(c[0].ToString()), float.Parse(c[1].ToString())));
				
				// Animator for the responsibility
				foreach (Vector2 v in responsibilityCoords){
					// Set the target for this leg of the responsibility
					prevLoc = loc;
					loc = loc + new Vector3 (v.x, v.y, 0);
					
					// Options for the leg of the responsibility
					Hashtable respOptions = new Hashtable();
					Hashtable lookOptions = new Hashtable();
					respOptions.Add ("position", loc);
					respOptions.Add ("time", responsibilityTime / responsibilityCoords.Count);
					respOptions.Add ("delay", delay);
					respOptions.Add ("easetype", iTween.EaseType.linear);
					respOptions.Add ("onstart", "SetAnimationRun");
					lookOptions.Add ("rotation", Quaternion.LookRotation(new Vector3 (0,0,1), loc - prevLoc).eulerAngles);
					lookOptions.Add ("time", 0.1f);
					lookOptions.Add ("delay", delay);
					
					// Animate the leg of the responsibility
					iTween.MoveTo (token, respOptions);
					iTween.RotateTo (token, lookOptions);
					
					// Update the delay
					delay += responsibilityTime / responsibilityCoords.Count;
				}
				// Stop motion at the end of the responsiblity
				Hashtable endOptions = new Hashtable();
				endOptions.Add ("position",loc);
				endOptions.Add ("time", 0f);
				endOptions.Add ("delay", delay);
				endOptions.Add ("oncomplete", "SetAnimationIdle");
				iTween.MoveTo (token, endOptions);
			}
		}
		else
			delay += responsibilityTime;
		return delay;	
	}
	
	// Helper functions for play animation
	// Move the player token to the starting location
	private void MoveToStart() {
		this.transform.position = location;
	}
	private void MoveToCorrectStart() {
		this.transform.position = correctLocation;
	}
	// Animate a Shift
	// Animate the Motion
	// Animate the Responsibility
	// Set Idle the token for a given amount of time
	private void AnimateIdle (float seconds, float delay) {
		
	}
	// Set the token animation to Running
	private void SetAnimationRun () {
		bigGuyAnimator.Play ("run_cycle");
	}
	
	// Set the token animation to Idle
	private void SetAnimationIdle () {
		bigGuyAnimator.Play ("Idle");
	}
	
	private void SetTokenIncorrect () {
		bigGuyModel.GetComponent<SkinnedMeshRenderer>().material = ghostMaterial;
	}
	
	private void ClearTokenIncorrect () {
		bigGuyModel.GetComponent<SkinnedMeshRenderer>().material = defaultMaterial;
	}
	
	private void linesShow(bool visible) {
		foreach(LineRenderer line in lines)
		{
			line.enabled = visible;
		}
	}
}	