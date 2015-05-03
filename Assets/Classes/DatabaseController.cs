using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;
using System.Threading.Tasks;

public class DatabaseController : MonoBehaviour {
	public static bool databaseLoaded;
	float srsDefaultValue = 1;
	float recencyDelayFactor = 1;
	public List<ParseObject> playQueryResults = new List<ParseObject> ();
	List<ParseObject> historyQueryResults = new List<ParseObject> ();
	List<ParseObject> srsQueryResults = new List<ParseObject> ();
	public static List<ParseObject> responsibilityQueryResults = new List<ParseObject> ();
	public GameController gameController;

	// Use this for initialization
	void Start () {
	}
	
	/// <summary>
	/// Download the plays (And other necessary data) from the cloud database
	/// </summary>
	public void GetPlays () 
	{
		// Set up the queries
		ParseQuery<ParseObject> playQuery = ParseObject.GetQuery("Play");
		ParseQuery<ParseObject> historyQuery = ParseObject.GetQuery("History")
			.WhereEqualTo("User",ParseUser.CurrentUser);
		ParseQuery<ParseObject> srsQuery = ParseObject.GetQuery("SRSData")
			.WhereEqualTo("User",ParseUser.CurrentUser);
		ParseQuery<ParseObject> responsibilityQuery = ParseObject.GetQuery("Responsibility");
		
		// Create a task for each query
		Task playTask = playQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {playQueryResults.Add (i);}});
		Task historyTask = historyQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {historyQueryResults.Add (i);}});
		Task srsTask = srsQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {srsQueryResults.Add (i);}});
		Task responsibilityTask = responsibilityQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {responsibilityQueryResults.Add (i);}});
		
		// Combine the tasks into an overtask
		List<Task> tasks = new List<Task>();
		tasks.Add (playTask);
		tasks.Add (historyTask);
		tasks.Add (srsTask);
		tasks.Add (responsibilityTask);

		Task allQueriesTask = Task.WhenAll (tasks).ContinueWith(q => { databaseLoaded = true; });
	}
	
	/// <summary>
	/// Save the history of a completed round of play
	/// </summary>
	/// <returns><c>true</c>, if the play history was saved successfully, <c>false</c> otherwise.</returns>
	/// <param name="playHistory">A parseobject containing the play history to be saved</param>
	public bool UpdatePlay (ParseObject playHistory)
	{
		Task saveTask = playHistory.SaveAsync();
		
		if (saveTask.IsCompleted) 
		{
			return true;
		} 
		else 
		{
			return false;
		}
	}
	
	/// <summary>
	/// Select the play for the next round of play, as calculated by the SRS algorithm
	/// </summary>
	public void SelectPlay ()
	{
		Debug.Log (databaseLoaded);
		float coefficientTotal = CalculateSRSTotal();
		float selectionValue = Random.Range(0, coefficientTotal);
		Debug.Log (selectionValue);
		float runningSrsSum = 0;
		ParseObject selectedPlay = null;
		Debug.Log ("Selecting");
		foreach (ParseObject play in playQueryResults) 
		{
			float srsValue;
			
			if (srsQueryResults.Count == 1)
			{
				ParseObject srsRow = srsQueryResults[0];
				bool result = srsRow.TryGetValue(play.ObjectId.ToString (), out srsValue);
				//Debug.Log ("play object ID: " + m.ObjectId);
				//Debug.Log ("result: " + result);
				if (!result)
				{
					srsValue = srsDefaultValue;
					srsRow[play.ObjectId] = srsValue;
				}
				runningSrsSum = runningSrsSum + srsValue;
				if (runningSrsSum >= selectionValue){
					selectedPlay = play;
					break;
				}
				
			}
			else
			{
				Debug.Log ("more than one set of srs coefficients for current user: ");
			}
		}
		Debug.Log ("selected play ID: " + selectedPlay.ObjectId);
		CheckIfRecentlySeen(selectedPlay);
	}

	// Calculate SRS variables
	private float CalculateSRSTotal ()
	{
		float srsTotal = 0;

		foreach (ParseObject play in playQueryResults) 
		{
			float srsValue;

			if (srsQueryResults.Count == 1)
			{
				ParseObject srsRow = srsQueryResults[0];
				bool result = srsRow.TryGetValue(play.ObjectId.ToString (), out srsValue);
				if (!result)
				{
					srsValue = srsDefaultValue;
					srsRow[play.ObjectId] = srsValue;
				}
				srsTotal = srsTotal + srsValue;
			}
			else
			{
				Debug.Log ("more than one set of srs coefficients for current user: ");
			}
		}

		return srsTotal;
	}

	// Check if a play has been recently seen
	private void CheckIfRecentlySeen(ParseObject play)
	{
		ParseQuery<ParseObject> playQuery = ParseObject.GetQuery ("Play")
			.WhereEqualTo ("Name", play.Get<string>("Name"));
		playQuery.FindAsync ().ContinueWith (t => {foreach (ParseObject i in t.Result){Debug.Log ("play query matches: " + i.ObjectId);}});
		ParseQuery<ParseObject> historyQuery = ParseObject.GetQuery ("History")
			.WhereEqualTo("User", ParseUser.CurrentUser).WhereMatchesQuery("Play", playQuery)
			.OrderByDescending("updatedAt");

		List<ParseObject> historyQueryResult = new List<ParseObject> ();
		historyQuery.FindAsync().ContinueWith(t => {
			foreach (ParseObject i in t.Result){
				historyQueryResult.Add (i);
			}
			if (historyQueryResult.Count > 0)
			{
				ParseObject pastPlay = historyQueryResult[0];

				string pastPlayID;
				bool found = pastPlay.TryGetValue(pastPlay.ObjectId, out pastPlayID);
				if (found)
				{
					System.DateTime updatedAt = (System.DateTime)pastPlay.UpdatedAt;
	
					// If the play was last chosen recently enough to fall within the (day) span set in recencyDelayFactor,
					// restart the process of selecting a play.
					System.DateTime difference = System.DateTime.Now - System.TimeSpan.FromDays(1*recencyDelayFactor);
					if (updatedAt > System.DateTime.Now - System.TimeSpan.FromDays(1*recencyDelayFactor))
					{
						SelectPlay ();
					}
					gameController.playLastSeen = updatedAt;
				}
			}
			gameController.currentPlay = play;
			loadPlay(play.ObjectId);
		});
	}
	
	/// <summary>
	/// Check if a play with a given name exists
	/// </summary>
	/// <param name="playName">The name of the play to be checked</param>
	public void checkPlayExists(string playName) {
		Debug.Log ("CheckPlayExistsCalled");
		ParseQuery<ParseObject> playQuery = ParseObject.GetQuery ("Play")
			.WhereEqualTo("Name", playName);
		int count;
		playQuery.CountAsync().ContinueWith(t => { gameController.ExistingFound(t.Result); } );
	}
	
	/// <summary>
	/// Load a play from the database by name
	/// </summary>
	/// <param name="playName">The name of the play to be loaded</param>
	public void loadPlayByName(string playName) {
		ParseQuery<ParseObject> playQuery = ParseObject.GetQuery ("Play")
			.WhereEqualTo("Name", playName)
			.Include("OffensiveTeam")
			.Include("DefensiveTeam")
			.Include("OffensiveTeam.Player0")
			.Include("OffensiveTeam.Player1")
			.Include("OffensiveTeam.Player2")
			.Include("OffensiveTeam.Player3")
			.Include("OffensiveTeam.Player4")
			.Include("OffensiveTeam.Player5")
			.Include("OffensiveTeam.Player6")
			.Include("OffensiveTeam.Player7")
			.Include("OffensiveTeam.Player8")
			.Include("OffensiveTeam.Player9")
			.Include("OffensiveTeam.Player10")
			.Include("DefensiveTeam.Player0")
			.Include("DefensiveTeam.Player1")
			.Include("DefensiveTeam.Player2")
			.Include("DefensiveTeam.Player3")
			.Include("DefensiveTeam.Player4")
			.Include("DefensiveTeam.Player5")
			.Include("DefensiveTeam.Player6")
			.Include("DefensiveTeam.Player7")
			.Include("DefensiveTeam.Player8")
			.Include("DefensiveTeam.Player9")
			.Include("DefensiveTeam.Player10")
			.Include("OffensiveTeam.Player0.Responsib")
			.Include("OffensiveTeam.Player1.Responsib")
			.Include("OffensiveTeam.Player2.Responsib")
			.Include("OffensiveTeam.Player3.Responsib")
			.Include("OffensiveTeam.Player4.Responsib")
			.Include("OffensiveTeam.Player5.Responsib")
			.Include("OffensiveTeam.Player6.Responsib")
			.Include("OffensiveTeam.Player7.Responsib")
			.Include("OffensiveTeam.Player8.Responsib")
			.Include("OffensiveTeam.Player9.Responsib")
			.Include("OffensiveTeam.Player10.Responsib")
			.Include("DefensiveTeam.Player0.Responsib")
			.Include("DefensiveTeam.Player1.Responsib")
			.Include("DefensiveTeam.Player2.Responsib")
			.Include("DefensiveTeam.Player3.Responsib")
			.Include("DefensiveTeam.Player4.Responsib")
			.Include("DefensiveTeam.Player5.Responsib")
			.Include("DefensiveTeam.Player6.Responsib")
			.Include("DefensiveTeam.Player7.Responsib")
			.Include("DefensiveTeam.Player8.Responsib")
			.Include("DefensiveTeam.Player9.Responsib")
			.Include("DefensiveTeam.Player10.Responsib");
			
		// Extract the loaded play from the query, and return it (via GameController's 'PlayLoaded' method
		playQuery.FirstAsync().ContinueWith(loadedPlay => { gameController.PlayLoaded(loadedPlay.Result); } );
	}
	
	/// <summary>
	/// Load a play from the database by objectID
	/// </summary>
	/// <param name="playID">The objectID of the play to be loaded</param>
	public void loadPlay(string playID) {
		ParseQuery<ParseObject> playQuery = ParseObject.GetQuery ("Play")
			.Include("OffensiveTeam")
			.Include("DefensiveTeam")
			.Include("OffensiveTeam.Player0")
			.Include("OffensiveTeam.Player1")
			.Include("OffensiveTeam.Player2")
			.Include("OffensiveTeam.Player3")
			.Include("OffensiveTeam.Player4")
			.Include("OffensiveTeam.Player5")
			.Include("OffensiveTeam.Player6")
			.Include("OffensiveTeam.Player7")
			.Include("OffensiveTeam.Player8")
			.Include("OffensiveTeam.Player9")
			.Include("OffensiveTeam.Player10")
			.Include("DefensiveTeam.Player0")
			.Include("DefensiveTeam.Player1")
			.Include("DefensiveTeam.Player2")
			.Include("DefensiveTeam.Player3")
			.Include("DefensiveTeam.Player4")
			.Include("DefensiveTeam.Player5")
			.Include("DefensiveTeam.Player6")
			.Include("DefensiveTeam.Player7")
			.Include("DefensiveTeam.Player8")
			.Include("DefensiveTeam.Player9")
			.Include("DefensiveTeam.Player10")
			.Include("OffensiveTeam.Player0.Responsib")
			.Include("OffensiveTeam.Player1.Responsib")
			.Include("OffensiveTeam.Player2.Responsib")
			.Include("OffensiveTeam.Player3.Responsib")
			.Include("OffensiveTeam.Player4.Responsib")
			.Include("OffensiveTeam.Player5.Responsib")
			.Include("OffensiveTeam.Player6.Responsib")
			.Include("OffensiveTeam.Player7.Responsib")
			.Include("OffensiveTeam.Player8.Responsib")
			.Include("OffensiveTeam.Player9.Responsib")
			.Include("OffensiveTeam.Player10.Responsib")
			.Include("DefensiveTeam.Player0.Responsib")
			.Include("DefensiveTeam.Player1.Responsib")
			.Include("DefensiveTeam.Player2.Responsib")
			.Include("DefensiveTeam.Player3.Responsib")
			.Include("DefensiveTeam.Player4.Responsib")
			.Include("DefensiveTeam.Player5.Responsib")
			.Include("DefensiveTeam.Player6.Responsib")
			.Include("DefensiveTeam.Player7.Responsib")
			.Include("DefensiveTeam.Player8.Responsib")
			.Include("DefensiveTeam.Player9.Responsib")
			.Include("DefensiveTeam.Player10.Responsib");
				
		// Extract the loaded play from the query, and return it (via GameController's 'PlayLoaded' method
		playQuery.GetAsync(playID).ContinueWith(loadedPlay => { gameController.PlayLoaded(loadedPlay.Result);} );
	}

	// Update is called once per frame
	void Update () {
	
	}
}
