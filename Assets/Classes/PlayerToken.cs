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
	public PieMenu menu;
	public GUIContent[] menuContentInitialized;
	public GUIContent[] menuContentShifted;
	public GUIContent[] menuContentMotioned;
	public GUIContent[] menuContentResponsibility;
	public Vector2 menuTarget;
	public int menuSpawnTime;			// Counter to prevent clicks being immediately registered on menu
	
	// Grid variables
	private readonly int maxX = 10;  	// Horizontal grid size
	private readonly int maxY = -9;		// Vertacle grid size	
	private Vector3 target;				// Snap to grid - point to snap to
	private Vector3 startPoint; 		// Snap to grid - point the token is snapping from
	private bool snapping;				// Is the token in the process of snapping to a position?
	private static List<Vector3> huddle;// Locations to put player tokens initially (in the huddle)
	private static int huddleIndex;		// Locations to put 
	
	// Game variables (set only at initialization
	private bool controllable;			// Is the token controllable by the player

	private string abbreviation;		// Abbreviation for the player's position
	private string position;			// Name of the player's position
	
	// Assigned variables (The 'correct' input)
	private Vector3 correctLocation;	// Location of the token
	private Vector2 correctMotion;		// Assigned motion
	private List<Vector2> correctShifts;// Assigned shifts
	private ParseObject correctResponsibility;// Assigned responsibility
	
	// User input variables (The user's input)
	private Vector3 location;			// Location of the token
	private Vector2 motion;				// Input motion
	private List<Vector2> shifts;		// Input shifts
	private ParseObject responsibility;	// Input responsibility

	// Display text
	public TextMesh displayText;		// Text to be displayed over player model (abbreviation usually)
	
	// Lines (For indicating input movements
	public List<GameObject> lineHolders;
	private List<LineRenderer> lines;
	private int lineIndex;
	
	// Player Model
	public GameObject bigGuy;
	public Animator bigGuyAnimator;
	
	// Initialization parseObject
	private ParseObject parsePlayer;
	
	// Initialization functions
	public void Initialize(ParseObject iParsePlayer)
	{
		Debug.Log("Initialization Requested");
		parsePlayer = iParsePlayer;
	}
	private void InitializeThreadsafe()
	{
		Debug.Log("Initializing");
		// TODO: Figure out animations
		//bigGuyAnimator.animation.Stop ();
		Debug.Log(Parse.ParseUser.CurrentUser.Get<string>("username"));
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
		if (!parsePlayer.TryGetValue("Responsibility", 	out correctResponsibility)){ correctResponsibility = null; }
		
		// Set token to display abbreviation
		displayText.text = abbreviation;
		
		// If the token is not controllable, set it to it's 'correct' values
		if (!controllable) {
			location = correctLocation;
			motion = correctMotion;
			shifts = correctShifts;
			responsibility = correctResponsibility;
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
	
	private static void newPlay() {
		huddleIndex = 0;
	}
	
	// Set various variables to default values to avoid unset variable issues
	void Awake () {
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
	
	void Start () {
	
	}
	
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
		// Menu handling
		if (popMenu)
		{
			Debug.Log ("ShowMenu " + state);
			switch (state) {
				case STATE.INITIALIZED:
					showMenuInitialized();
					break;
				case STATE.SHIFTED:
					showMenuShifted();
					break;
				case STATE.MOTIONED:
					showMenuMotioned();
					break;
				default:
					Debug.LogError("Attempt to open menu in invalid state: " + state);
					popMenu = false;
					break;
			}
		}
		
		// Initialize the token if needed
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
			menu.InitPie(MenuController.menuContentInitialized.ToArray());
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
				// TODO: Add menu for selecting responsibility
				menu.Close ();
				popMenu = false;
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
		Debug.Log ("ShowMenuShifted");
		if (menuSpawnTime++ == 0)
			menu.InitPie(MenuController.menuContentShifted.ToArray());
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
					// TODO: Responsibilty selection menu
					menu.Close ();
					popMenu = false;
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
		Debug.Log ("ShowMenuMotioned");
		if (menuSpawnTime++ == 0)
			menu.InitPie(MenuController.menuContentMotioned.ToArray());
		else if (menuSpawnTime++ < 20)
			return;
		else if (Input.GetMouseButtonDown(0)){
			Debug.Log ("Menu selected: " + menu.Selected);
			switch (menu.Selected)
			{
			case 0:
				// TODO: Responsibilty selection menu
				menu.Close ();
				popMenu = false;
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
	}

	// Called when the mouse button is pushed
	void OnMouseDown() { 
		Debug.Log ("Mouse down in " + state + ", Menu " + popMenu);
		// Check if this click is the second click of a doubleclick
		if (doubleClickCount > 0) {
			menuTarget = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			Debug.Log ("DoubleClick");
			menuSpawnTime = 0;
			popMenu = true;
			// TODO: Implement menu handling
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
	
		// Move the token to it's new location, then reset the offset position to the new location
		transform.Translate( (Input.mousePosition.x - offset.x) * heightFactor, (Input.mousePosition.y - offset.y) * heightFactor, 0);
		offset.x = Input.mousePosition.x;
		offset.y = Input.mousePosition.y;
	}

	// Called when the mouse button is released
	void OnMouseUp() {
		if (!checkDraggable())
			return;
			
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
		snapping = true;
		
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
	
	// Add an 
	private void addShift(Vector3 start, Vector3 end){
		shifts.Add(new Vector2(end.x - start.x, end.y - start.y));
		shiftsSet = true;

		// Add movement line
		lines[lineIndex].SetColors(Color.red, Color.red);
		lines[lineIndex].SetVertexCount(2);
		lines[lineIndex].SetPosition(0, startPoint);
		lines[lineIndex].SetPosition(1, target);
		lineIndex++;
		
		// Set new state
		state = STATE.SHIFTED;
		
		//bigGuyAnimator.StartPlayback();
		bigGuyAnimator.Play("Running");
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
	
	
	public ParseObject getInputParseObject(){
		ParseObject playerOut = new ParseObject("Player");
		
		playerOut["Abbreviation"] = abbreviation;
		playerOut["Position"] = position;
		playerOut["Location"] = location;
		playerOut["Motion"] = motion;
		playerOut["Shifts"] = shifts;
		playerOut["Responsib"] = ParseObject.CreateWithoutData("Responsibility", responsibility.ObjectId);
		
		return playerOut;
	}
	
	public ParseObject getCorrectParseObject() {
		return parsePlayer;
	}
}	