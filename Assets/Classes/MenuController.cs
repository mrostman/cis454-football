using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class MenuController : MonoBehaviour {
	private enum STATE {SPLASHSCREEN, LOGIN, MAINMENUPLAYER, MAINMENUSTAFF, LOADINGDB, INPLAY, POSTPLAY, EDIT, PLAYEDITORMENU}; 
	private STATE state = STATE.SPLASHSCREEN;
	
	[Header("Other Controllers")]
	public DatabaseController databaseController;
	public GameController gameController;
	
	[Header("Login Menu")]
	public CanvasGroup loginCanvas;
	public UnityEngine.UI.Button loginButton;
	public UnityEngine.UI.Text loginButtonText, loginErrorText;
	public UnityEngine.UI.InputField username, password;
	
	[Header("Loading Menu")]
	public CanvasGroup loadingCanvas;
	public UnityEngine.UI.Text loadingText;
	private int animCount = 0;
	private int dotCount = 0;
	
	[Header("Main Menu")]
	public CanvasGroup mainMenuCanvasStaff;
	public CanvasGroup mainMenuCanvasPlayer;
	public UnityEngine.UI.Text playerMenuText, playerMenuQuitButtonText;
	public UnityEngine.UI.Button playerMenuQuitButton, staffMenuQuitButton;
	//public UnityEngine.UI.Button staffPlayerSwitchButon;
	private bool isStaff = false;
	private System.Threading.Tasks.Task<ParseUser> login = null;
	
	[Header("Play Editor")]
	public CanvasGroup selectExistingPlayCanvas, playerPropertiesCanvas, editModeCanvas;
	public GameObject scrollPanel;
	public GameObject pButton;
	public InputField playerPositionText, playerAbbreviationText, playNameField;
	public Toggle playerControllableToggle;
	public UnityEngine.UI.Button rootButton;
	private PlayerToken tokenBeingEdited;
	private enum EDITTYPE {BLANK, FROMEXISTING, EDITEXISTING};
	private EDITTYPE editType;
	private bool playListNeedsUpdate = true;
	private List<GameObject> playButtons = new List<GameObject>();
	private string editPlayID;
	private string editPlayIDBlank;
	
	[Header("Play Editor Error")] 
	public CanvasGroup editorErrorCanvas;
	public Text editorErrorText;
	
	[Header("Saving Notification")]
	public CanvasGroup savingAnimCanvas;
	public Text savingText;
	
	[Header("In-Play Menu")]
	public CanvasGroup inPlayCanvas;
	public UnityEngine.UI.Text playNameText, correctnessText;
	public GameObject inPlayPanel, postPlayPanel;
	private Vector3 inPlayPanelShow = new Vector3(0.3f, 0.75f, 0.25f);
	private Vector3 inPlayPanelHide = new Vector3(0.15f, 0.75f, 0.25f);
	private Vector3 postPlayPanelShow = new Vector3(0f, -70f, 0f);
	private Vector3 postPlayPanelHide = new Vector3(0f, 70f, 0f);
	private bool postPlayPanelHidden = false;
	
	private List<CanvasGroup> canvases = new List<CanvasGroup>();
	

	// Initialize various values
	void Start () {
		HideInPlayButtons();
		HidePostPlayButtons();
		
		// Setup list of canvases
		canvases.Add(loginCanvas);
		canvases.Add(loadingCanvas);
		canvases.Add(mainMenuCanvasStaff);
		canvases.Add(mainMenuCanvasPlayer);
		canvases.Add(selectExistingPlayCanvas);
		canvases.Add(playerPropertiesCanvas);
		canvases.Add(editorErrorCanvas);
		canvases.Add(savingAnimCanvas);
		canvases.Add(inPlayCanvas);
		canvases.Add(editModeCanvas);
	}
	
	// Update is called once per frame
	void Update () {
		AnimateText();
		switch (state) {
			case STATE.SPLASHSCREEN:
				state = STATE.LOGIN;
				ShowCanvas (loginCanvas);
				break;
			case STATE.LOGIN:
				if (login != null)
					CheckLogin ();
				break;
			case STATE.LOADINGDB:
				CheckDBLoaded();
				break;
			case STATE.INPLAY:
			case STATE.POSTPLAY:
				playNameText.text = gameController.playName;
				break;
			case STATE.PLAYEDITORMENU:
				Debug.Log ("Test");
				break;
			default:
				break;
		}
		if (playListNeedsUpdate == true && DatabaseController.databaseLoaded) {
			playListNeedsUpdate = false;
			GeneratePlayListButtons ();
		}
	}
	
	/// <summary>
	/// Shows a selected canvas (Making it visible and interactable)
	/// </summary>
	/// <param name="canvas">The canvas to be shown</param>
	public void ShowCanvas(CanvasGroup canvas){
		canvas.alpha = 1f;
		canvas.interactable = true;
		canvas.blocksRaycasts = true;
	}
	
	/// <summary>
	/// Hides a selected canvas (making it invisible and uninteractable)
	/// </summary>
	/// <param name="canvas">The canvas to be hidden</param>
	public void HideCanvas(CanvasGroup canvas) {
		canvas.alpha = 0f;
		canvas.interactable = false;
		canvas.blocksRaycasts = false;
	}
	
	/// <summary>
	/// Hides ALL canvases (Calling HideCavnas on each)
	/// </summary>
	public void HideAll(){
		foreach (CanvasGroup c in canvases)
			HideCanvas (c);
	}
	
	// Generate and arrange buttons corrsponding to the list of existing plays
	private void GeneratePlayListButtons() {
		// Setup first button
		pButton.GetComponentInChildren<Text>().text = databaseController.playQueryResults[0].Get<string>("Name");
		playButtons.Add (pButton);
		
		// Setup remaining buttons
		for(int i = 1; i < databaseController.playQueryResults.Count; i++){
			GameObject newButton = (GameObject)Instantiate (pButton);
			newButton.transform.parent = rootButton.transform.parent;
			newButton.transform.localPosition = rootButton.transform.localPosition - new Vector3(0, 15f * i, 0);
			newButton.transform.localScale = rootButton.transform.localScale;
			Text texty = newButton.GetComponentInChildren<Text>();
			texty.text = databaseController.playQueryResults[i].Get<string>("Name");
			playButtons.Add (newButton);
		}
	}

	
	// Check if the login attempt has succeeded or failed
	private void CheckLogin() {
		// Check for null case
		if (login == null)
			return;
			
		// Check for incorrect login
		if (login.IsFaulted || login.IsCanceled) {
			// If the login was incorrect, re-enable the inputs, show the incorret login message, and clear the login object
			loginButton.interactable = true;
			username.interactable = true;
			password.interactable = true;
			password.text = "";
			loginErrorText.text = "Error logging in: Please check your password and try again.";
			loginButtonText.text = "Login";
			Debug.Log ("Login Failed: " + login.IsFaulted + login.IsCanceled);
			login = null;
		}
		
		// Check for correct login
		else if (login.IsCompleted) {
			// If the login was correct, start loading from the database and show the main menu
			Debug.Log("Logged in!");
			databaseController.GetPlays();
			HideCanvas (loginCanvas);
			
			Parse.ParseUser.CurrentUser.TryGetValue("Staff", out isStaff);
			ShowMainMenu();
		}
		
		// Check for unexpected cases
		else
			Debug.LogError("Unknown login status: " + login.Result);
	}
	
	/// <summary>
	/// Display the main menu
	/// </summary>
	public void ShowMainMenu () {
		HideAll();
		if (isStaff) {
			state = STATE.MAINMENUSTAFF;
			ShowCanvas (mainMenuCanvasStaff);
			playerMenuText.text = "Player Mode";
			playerMenuQuitButtonText.text = "Back";
			
			// TODO: Disable the 'quit' button if on a platform where the application ending itself is not supported
			#if UNITY_IPHONE
				Debug.Log("Iphone");
			#endif
		}
		else {
			state = STATE.MAINMENUPLAYER;
			ShowCanvas (mainMenuCanvasPlayer);
		}
	}
	
	// Animate the text on the loading panel
	private void AnimateText() {
		if (animCount > 50){
			if (dotCount == 3){
				loadingText.text = "Loading";
				savingText.text = "Saving";
				dotCount = 0;
			}
			else {
				loadingText.text = loadingText.text + ".";
				savingText.text = savingText.text + ".";
				dotCount++;
			}
			animCount = 0;
		}
		else
			animCount++;		
	}
	
	// Check if the database has been loaded yet (used by loading screen)
	private void CheckDBLoaded() {
		if (!DatabaseController.databaseLoaded)
			return;
		else
			StartPlay ();
	}

	/// <summary>
	/// Attempt to log into parse with the entered username and password
	/// </summary>
	public void TryLogin() {
		// Disable the input fields while we wait for the result of the login
		loginButton.interactable = false;
		username.interactable = false;
		password.interactable = false;
		loginButtonText.text = "Logging in...";
		ParseUser.LogInAsync(username.text, password.text).ContinueWith(t => {  login = t; });
	}
	
	/// <summary>
	/// Attempt to start a round of play
	/// </summary>
	public void TryStartPlay() {
		// Hide the main menu
		HideCanvas(mainMenuCanvasPlayer);
		
		// If the tatabase is loaded, start the play
		if (DatabaseController.databaseLoaded){
			Debug.Log ("READY!");
			StartPlay ();
		}
		
		// Otherwise, show the loading panel until the database is loaded
		else {
			Debug.Log ("NOT READY");
			ShowCanvas (loadingCanvas);
			state = STATE.LOADINGDB;
		}
	}
	
	/// <summary>
	/// Start edit mode (the process of creating a new play)
	/// </summary>
	/// <param name="playNameText">The name of the existing play to use as a basis for the new play being created</param>
	public void StartEdit(Text playNameText) {
		// Change the state
		state = STATE.EDIT;
		
		// Set the gameController's editMode to true
		gameController.editMode = true;
		
		// TODO: Bring up the play editor UI
		
		// Have the gamecontroller start a new play
		gameController.EditPlay (playNameText.text);
		
		// Display the play name
		//playNameText.text = gameController.playName;
	}
	
	// Start a round of play
	private void StartPlay() {
		// Change the state
		state = STATE.INPLAY;
		
		// Set the gameController's editMode to false
		gameController.editMode = false;
		
		// Bring up the in-play UI
		HideCanvas (loadingCanvas);
		ShowCanvas (inPlayCanvas);
		HidePostPlayButtons();
		ShowInPlayButtons();
		
		// Have the gamecontroller start a new play
		gameController.NewPlay ();
		
		// Display the play name
		playNameText.text = gameController.playName;
	}
	
	/// <summary>
	/// End a round of play
	/// </summary>
	public void FinishPlay() {
		Debug.Log ("FinishPlay");
		state = STATE.POSTPLAY;
		
		// Switch to the post-play buttons
		HideInPlayButtons();
		
		
		// Evaluate the play
		correctnessText.text = "Your Score...";
		int score = gameController.EndPlay ();
		Invoke("ShowPostPlayButtons", gameController.animLength + 1f);
		Debug.Log (score);
		if (score < 1)
			correctnessText.text = "Your Score: Incorrect";
		else
			correctnessText.text = "Your Score: Correct!";
	}
	
	/// <summary>
	/// Show the select play panel
	/// </summary>
	public void ShowSelectPlayPanel() {
		HideCanvas (mainMenuCanvasStaff);
		ShowCanvas (selectExistingPlayCanvas);
	}
	
	/// <summary>
	/// Animate the correct input for the play
	/// </summary>
	public void AnimateCorrectPlay() {
		DisableInPlayCanvas();
		float delay = gameController.AnimateCorrectPlay();
		Invoke ("EnableInPlayCanvas",delay+1f);
	}
	
	/// <summary>
	/// Animate the user input for the play
	/// </summary>
	public void AnimateInputPlay() {
		DisableInPlayCanvas();
		float delay = gameController.AnimateInputPlay();
		Invoke ("EnableInPlayCanvas",delay+1f);
	}
	
	// Show the in play buttons
	private void ShowInPlayButtons() {
		inPlayPanel.ScaleTo (inPlayPanelShow, 1.5f, 0f);
		Invoke ("EnableInPlayCanvas", 1f);
	}
	
	// Hide the in play buttons
	private void HideInPlayButtons() {
		inPlayPanel.ScaleTo (inPlayPanelHide, 1.5f, 0f);
		DisableInPlayCanvas();
	}
	
	// Show the post-play buttons
	private void ShowPostPlayButtons() {
		if (postPlayPanelHidden)
			postPlayPanel.MoveBy (postPlayPanelShow, 1.5f, 0f);
		Invoke ("EnableInPlayCanvas", 1f);
		postPlayPanelHidden = false;
	}
	
	// Hide the post-play buttons
	private void HidePostPlayButtons() {
		if (!postPlayPanelHidden)
			postPlayPanel.MoveBy (postPlayPanelHide, 1.5f, 0f);
		DisableInPlayCanvas();
		postPlayPanelHidden = true;
	}
	
	// Disable input to the in-play menu
	public void DisableInPlayCanvas() {
		inPlayCanvas.interactable = false;
	}
	
	// Enable input to the in-play menu
	public void EnableInPlayCanvas() {
		inPlayCanvas.interactable = true;
	}
	
	/*public void SelectParentPlay(GameObject playNameButton) {
		//playNameButton = 
	}
	public void StartPlayMode() {}*/
	
	/// <summary>
	/// Show the properties popup menu for a given player token
	/// </summary>
	/// <param name="token">The selected player token</param>
	public void ShowPlayerProperties(PlayerToken token) {
		tokenBeingEdited = token;
		playerPositionText.text = token.position;
		playerAbbreviationText.text = token.abbreviation;
		playerControllableToggle.isOn = token.saveControllable;
		ShowCanvas (playerPropertiesCanvas);
	}
	
	/// <summary>
	/// Saves the position (text) of the player token being edited
	/// </summary>
	public void SavePlayerPosition() {
		tokenBeingEdited.position = playerPositionText.text;
	}
	
	/// <summary>
	/// Saves the abbreviation of the player token being edited
	/// </summary>
	public void SavePlayerAbbreviation() {
		tokenBeingEdited.abbreviation = playerAbbreviationText.text;
	}
	
	/// <summary>
	/// Saves the controllable flag of the player token being edited
	/// </summary>
	public void SavePlayerControllable() {
		tokenBeingEdited.saveControllable = playerControllableToggle.isOn;
		
	}

	/// <summary>
	/// Close the player properties pop-up menu
	/// </summary>
	public void ClosePlayerProperties() {
		PlayerToken.propertiesMenu = false;
	}
	
	/// <summary>
	/// Show an error message.
	/// </summary>
	/// <param name="errorText">The text to be displayed as the error</param>
	public void SaveError(string errorText) {
		editorErrorText.text = errorText;
		ShowCanvas (editorErrorCanvas);
	}
	
	/// <summary>
	/// Attempt to save the new play
	/// </summary>
	public void TrySaveEdit() {
		gameController.TrySaveEdit(playNameField.text);
	}
	
	/// <summary>
	/// Abandon the newly created play
	/// </summary>
	public void CancelEdit() {
		gameController.EndEdit();
	}
}
