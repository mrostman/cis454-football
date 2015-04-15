using UnityEngine;
using System.Collections;
using Parse;

public class MenuController : MonoBehaviour {
	private enum STATE {SPLASHSCREEN, LOGIN, MAINMENU, LOADINGDB, INPLAY, POSTPLAY}; 
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
	private int loadingAnimCount = 0;
	private int loadingDotCount = 0;
	
	[Header("Main Menu")]
	public CanvasGroup mainMenuCanvas;
	private System.Threading.Tasks.Task<ParseUser> login = null;
	
	[Header("In-Play Menu")]
	public CanvasGroup inPlayCanvas;
	public UnityEngine.UI.Text playNameText, correctnessText;
	public GameObject inPlayPanel, postPlayPanel;
	private Vector3 inPlayPanelShow = new Vector3(0.3f, 0.75f, 0.25f);
	private Vector3 inPlayPanelHide = new Vector3(0.15f, 0.75f, 0.25f);
	private Vector3 postPlayPanelShow = new Vector3(0f, -70f, 0f);
	private Vector3 postPlayPanelHide = new Vector3(0f, 70f, 0f);
	private bool postPlayPanelHidden = false;
	
	[Header("Play Editor")]
	public UnityEngine.UI.InputField playNameInput;

	// Use this for initialization
	void Start () {
		HideInPlayButtons();
		HidePostPlayButtons();
	
	}
	
	// Update is called once per frame
	void Update () {
		if (state == STATE.SPLASHSCREEN){
			state = STATE.LOGIN;
			ShowCanvas (loginCanvas);
		}
		if (state == STATE.LOGIN && login != null) {
			CheckLogin ();
		}
		else if (state == STATE.LOADINGDB)
			AnimateLoadingText();
			
		switch (state) {
			case STATE.SPLASHSCREEN:
				state = STATE.LOGIN;
				ShowCanvas (loginCanvas);
				break;
			case STATE.LOGIN:
				CheckLogin ();
				break;
			case STATE.LOADINGDB:
				AnimateLoadingText();
				CheckDBLoaded();
				break;
			default:
				break;
		}

	}
	
	// Show a given canvas (menu)
	private void ShowCanvas(CanvasGroup canvas){
		canvas.alpha = 1f;
		canvas.interactable = true;
		canvas.blocksRaycasts = true;
	}
	
	// Hide a given canvas (menu)s
	private void HideCanvas(CanvasGroup canvas) {
		canvas.alpha = 0f;
		canvas.interactable = false;
		canvas.blocksRaycasts = false;
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
			ShowCanvas (mainMenuCanvas);
			state = STATE.MAINMENU;
		}
		
		// Check for unexpected cases
		else
			Debug.LogError("Unknown login status: " + login.Result);
	}
	
	// Animate the text on the loading panel
	private void AnimateLoadingText() {
		if (loadingAnimCount > 50){
			if (loadingDotCount == 3){
				loadingText.text = "Loading";
				loadingDotCount = 0;
			}
			else {
				loadingText.text = loadingText.text + ".";
				loadingDotCount++;
			}
			loadingAnimCount = 0;
		}
		else
			loadingAnimCount++;		
	}
	
	// Check if the database has been loaded yet (used by loading screen)
	private void CheckDBLoaded() {
		if (!DatabaseController.databaseLoaded)
			return;
		else 
			StartPlay ();
	}

	// Attempt to log into parse, called by the login button in the login Canvas
	public void TryLogin() {
		// Disable the input fields while we wait for the result of the login
		loginButton.interactable = false;
		username.interactable = false;
		password.interactable = false;
		loginButtonText.text = "Logging in...";
		ParseUser.LogInAsync(username.text, password.text).ContinueWith(t => {  login = t; });
	}
	
	// Attempt to start a play
	public void TryStartPlay() {
		// Hide the main menu
		HideCanvas(mainMenuCanvas);
		
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
	
	// Start a play
	private void StartPlay() {
		// Change the state
		state = STATE.INPLAY;
		
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
	
	public void FinishPlay() {
		Debug.Log ("FinishPlay");
		state = STATE.POSTPLAY;
		
		// Switch to the post-play buttons
		HideInPlayButtons();
		ShowPostPlayButtons ();
		
		// Evaluate the play
		correctnessText.text = "Your Score...";
		int score = gameController.EndPlay ();
		Debug.Log (score);
		if (score < 1)
			correctnessText.text = "Your Score: Incorrect";
		else
			correctnessText.text = "Your Score: Correct!";
	}
	
	public void AnimateCorrectPlay() {
		DisableInPlayCanvas();
		float delay = gameController.AnimateCorrectPlay();
		Invoke ("EnableInPlayCanvas",delay);
	}
	
	public void AnimateInputPlay() {
		DisableInPlayCanvas();
		float delay = gameController.AnimateInputPlay();
		Invoke ("EnableInPlayCanvas",delay);
	}
	
	private void ShowInPlayButtons() {
		inPlayPanel.ScaleTo (inPlayPanelShow, 1.5f, 0f);
		Invoke ("EnableInPlayCanvas", 1f);
	}
	private void HideInPlayButtons() {
		inPlayPanel.ScaleTo (inPlayPanelHide, 1.5f, 0f);
		DisableInPlayCanvas();
	}
	private void ShowPostPlayButtons() {
		if (postPlayPanelHidden)
			postPlayPanel.MoveBy (postPlayPanelShow, 1.5f, 0f);
		Invoke ("EnableInPlayCanvas", 1f);
		postPlayPanelHidden = false;
	}
	private void HidePostPlayButtons() {
		if (!postPlayPanelHidden)
			postPlayPanel.MoveBy (postPlayPanelHide, 1.5f, 0f);
		DisableInPlayCanvas();
		postPlayPanelHidden = true;
	}
	private void DisableInPlayCanvas() {
		inPlayCanvas.interactable = false;
	}
	private void EnableInPlayCanvas() {
		inPlayCanvas.interactable = true;
	}
	
	//TODO: Temporary Play Editor
	public void StartEditPlay() {
		gameController.StartEditPlay();
	}
	
	public void SavePlay() {
		gameController.SaveThisPlay(playNameInput.text);
	}

}
