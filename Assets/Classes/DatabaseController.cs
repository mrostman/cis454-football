using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;
using System.Threading.Tasks;

public class DatabaseController : MonoBehaviour {
	public bool databaseLoaded;
	float srsDefaultValue = 1;
	float recencyDelayFactor = 1;
	List<ParseObject> playQueryResults = new List<ParseObject> ();
	List<ParseObject> historyQueryResults = new List<ParseObject> ();
	List<ParseObject> srsQueryResults = new List<ParseObject> ();
	List<ParseObject> oTeamQueryResults = new List<ParseObject> ();
	List<ParseObject> dTeamQueryResults = new List<ParseObject> ();
	List<ParseObject> playerQueryResults = new List<ParseObject> ();
	public static List<ParseObject> responsibilityQueryResults = new List<ParseObject> ();
	public GameController gameController;

	// Use this for initialization
	void Start () {
		GetPlays ();
	}

	void GetPlays () 
	{
		ParseUser.LogInAsync("kris", "password").ContinueWith(t =>
		{	
			ParseQuery<ParseObject> playQuery = ParseObject.GetQuery("Play");
			ParseQuery<ParseObject> historyQuery = ParseObject.GetQuery("History")
				.WhereEqualTo("User",ParseUser.CurrentUser);
			ParseQuery<ParseObject> srsQuery = ParseObject.GetQuery("SRSData")
				.WhereEqualTo("User",ParseUser.CurrentUser);
			ParseQuery<ParseObject> oTeamQuery = ParseObject.GetQuery("OffensiveTeam");
			ParseQuery<ParseObject> dTeamQuery = ParseObject.GetQuery("DefensiveTeam");
			ParseQuery<ParseObject> playerQuery = ParseObject.GetQuery("Player");
			ParseQuery<ParseObject> responsibilityQuery = ParseObject.GetQuery("Responsibility");

			Task playTask = playQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {playQueryResults.Add (i);}});
			Task historyTask = historyQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {historyQueryResults.Add (i);}});
			Task srsTask = srsQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {srsQueryResults.Add (i);}});
			Task oTeamTask = oTeamQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {oTeamQueryResults.Add (i);}});
			Task dTeamTask = dTeamQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {dTeamQueryResults.Add (i);}});
			Task playerTask = playerQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {playerQueryResults.Add (i);}});
			Task responsibilityTask = responsibilityQuery.FindAsync().ContinueWith(u => { foreach (ParseObject i in u.Result) {responsibilityQueryResults.Add (i);}});

			List<Task> tasks = new List<Task>();
			tasks.Add (playTask);
			tasks.Add (historyTask);
			tasks.Add (srsTask);
			tasks.Add (oTeamTask);
			tasks.Add (dTeamTask);
			tasks.Add (playerTask);
			tasks.Add (responsibilityTask);

			Task allQueriesTask = Task.WhenAll (tasks).ContinueWith(q => {

				// testing
				//Debug.Log ("Play Task complete:");
				//Debug.Log (playTask.IsCompleted);
				//Debug.Log ("History Task complete:");
				//Debug.Log (historyTask.IsCompleted);
				//Debug.Log ("SRS Task complete:");
				//Debug.Log (srsTask.IsCompleted);
				//Debug.Log ("Offensive Team Task complete:");
				//Debug.Log (oTeamTask.IsCompleted);
				//Debug.Log ("Defensive Team Task complete:");
				//Debug.Log (dTeamTask.IsCompleted);
				//Debug.Log ("Player Task complete:");
				//Debug.Log (playerTask.IsCompleted);
				//Debug.Log ("Responsibility Task complete:");
				//Debug.Log (responsibilityTask.IsCompleted);

				//CalculateSRSTotal();
				RebuildReferences();
			});
		});
	}

	void RebuildReferences ()
	{
		foreach(ParseObject i in playQueryResults)
		{
			//Debug.Log ("Play Name");
			//Debug.Log (i.Get<string>("Name"));
			ParseObject oTeamReference;
			bool result = i.TryGetValue("OffensiveTeam", out oTeamReference);
			if (result)
			{
				string oTeamID = oTeamReference.ObjectId;
				ParseObject oTeam = oTeamQueryResults.Find (e => e.ObjectId == oTeamID);
				//Debug.Log(oTeam.ObjectId);
				//Debug.Log (oTeam.Get<ParseObject>("Player0").ObjectId);
				//i.SetProperty<ParseObject>(oTeam,"OffensiveTeam");
				i["OffensiveTeam"] = oTeam;
				//Debug.Log (i.Get<ParseObject>("OffensiveTeam").Get<ParseObject>("Player0"));
			}

			ParseObject dTeamReference;
			result = i.TryGetValue("DefensiveTeam", out dTeamReference);
			if (result)
			{
				string dTeamID =dTeamReference.ObjectId;
				ParseObject dTeam = dTeamQueryResults.Find (e => e.ObjectId == dTeamID);
				i["DefensiveTeam"] = dTeam;
			}
		}
		foreach(ParseObject j in historyQueryResults)
		{
			ParseObject playReference;
			bool result = j.TryGetValue("Play", out playReference);
			if (result)
			{
				string playID = playReference.ObjectId;
				ParseObject play = playQueryResults.Find (e => e.ObjectId == playID);
				j["OffensiveTeam"] = play;
			}

			//Debug.Log (j.Get<ParseObject>("Play").ObjectId);
			//Debug.Log (play.Get<ParseObject>("OffensiveTeam").ObjectId);

			ParseObject oTeamReference;
			result = j.TryGetValue("OffensiveTeam", out oTeamReference);
			if (result)
			{
				string oTeamID = oTeamReference.ObjectId;
				ParseObject oTeam = oTeamQueryResults.Find (e => e.ObjectId == oTeamID);
				j["OffensiveTeam"] = oTeam;
			}

		}
		foreach(ParseObject k in oTeamQueryResults)
		{
			ParseObject player0Reference;
			bool result = k.TryGetValue("Player0", out player0Reference);
			if (result)
			{
				string player0ID = player0Reference.ObjectId;
				ParseObject player0 = playerQueryResults.Find (e => e.ObjectId == player0ID);
				k["Player0"] = player0;
			}
			
			ParseObject player1Reference;
			result = k.TryGetValue("Player1", out player1Reference);
			if (result)
			{
				string player1ID = player1Reference.ObjectId;
				ParseObject player1 = playerQueryResults.Find (e => e.ObjectId == player1ID);
				k["Player1"] = player1;
			}
			
			ParseObject player2Reference;
			result = k.TryGetValue("Player2", out player2Reference);
			if (result)
			{
				string player2ID = player2Reference.ObjectId;
				ParseObject player2 = playerQueryResults.Find (e => e.ObjectId == player2ID);
				k["Player2"] = player2;
			}
			
			ParseObject player3Reference;
			result = k.TryGetValue("Player3", out player3Reference);
			if (result)
			{
				string player3ID = player3Reference.ObjectId;
				ParseObject player3 = playerQueryResults.Find (e => e.ObjectId == player3ID);
				k["Player3"] = player3;
			}
			
			ParseObject player4Reference;
			result = k.TryGetValue("Player4", out player4Reference);
			if (result)
			{
				string player4ID = player4Reference.ObjectId;
				ParseObject player4 = playerQueryResults.Find (e => e.ObjectId == player4ID);
				k["Player4"] = player4;
			}
			
			ParseObject player5Reference;
			result = k.TryGetValue("Player5", out player5Reference);
			if (result)
			{
				string player5ID = player5Reference.ObjectId;
				ParseObject player5 = playerQueryResults.Find (e => e.ObjectId == player5ID);
				k["Player5"] = player5;
			}
			
			ParseObject player6Reference;
			result = k.TryGetValue("Player6", out player6Reference);
			if (result)
			{
				string player6ID = player6Reference.ObjectId;
				ParseObject player6 = playerQueryResults.Find (e => e.ObjectId == player6ID);
				k["Player6"] = player6;
			}
			
			ParseObject player7Reference;
			result = k.TryGetValue("Player7", out player7Reference);
			if (result)
			{
				string player7ID = player7Reference.ObjectId;
				ParseObject player7 = playerQueryResults.Find (e => e.ObjectId == player7ID);
				k["Player7"] = player7;
			}
			
			ParseObject player8Reference;
			result = k.TryGetValue("Player8", out player8Reference);
			if (result)
			{
				string player8ID = player8Reference.ObjectId;
				ParseObject player8 = playerQueryResults.Find (e => e.ObjectId == player8ID);
				k["Player8"] = player8;
			}	
			
			ParseObject player9Reference;
			result = k.TryGetValue("Player9", out player9Reference);
			if (result)
			{
				string player9ID = player9Reference.ObjectId;
				ParseObject player9 = playerQueryResults.Find (e => e.ObjectId == player9ID);
				k["Player9"] = player9;
			}

			ParseObject player10Reference;
			result = k.TryGetValue("Player10", out player10Reference);
			if (result)
			{
				string player10ID = player10Reference.ObjectId;
				ParseObject player10 = playerQueryResults.Find (e => e.ObjectId == player10ID);
				k["Player10"] = player10;
			}
		}

		foreach(ParseObject l in dTeamQueryResults)
		{
			ParseObject player0Reference;
			bool result = l.TryGetValue("Player0", out player0Reference);
			if (result)
			{
				string player0ID = player0Reference.ObjectId;
				ParseObject player0 = playerQueryResults.Find (e => e.ObjectId == player0ID);
				l["Player0"] = player0;
			}
				
			ParseObject player1Reference;
			result = l.TryGetValue("Player1", out player1Reference);
			if (result)
			{
				string player1ID = player1Reference.ObjectId;
				ParseObject player1 = playerQueryResults.Find (e => e.ObjectId == player1ID);
				l["Player1"] = player1;
			}

			ParseObject player2Reference;
			result = l.TryGetValue("Player2", out player2Reference);
			if (result)
			{
				string player2ID = player2Reference.ObjectId;
				ParseObject player2 = playerQueryResults.Find (e => e.ObjectId == player2ID);
				l["Player2"] = player2;
			}

			ParseObject player3Reference;
			result = l.TryGetValue("Player3", out player3Reference);
			if (result)
			{
				string player3ID = player3Reference.ObjectId;
				ParseObject player3 = playerQueryResults.Find (e => e.ObjectId == player3ID);
				l["Player3"] = player3;
			}

			ParseObject player4Reference;
			result = l.TryGetValue("Player4", out player4Reference);
			if (result)
			{
				string player4ID = player4Reference.ObjectId;
				ParseObject player4 = playerQueryResults.Find (e => e.ObjectId == player4ID);
				l["Player4"] = player4;
			}

			ParseObject player5Reference;
			result = l.TryGetValue("Player5", out player5Reference);
			if (result)
			{
				string player5ID = player5Reference.ObjectId;
				ParseObject player5 = playerQueryResults.Find (e => e.ObjectId == player5ID);
				l["Player5"] = player5;
			}

			ParseObject player6Reference;
			result = l.TryGetValue("Player6", out player6Reference);
			if (result)
			{
				string player6ID = player6Reference.ObjectId;
				ParseObject player6 = playerQueryResults.Find (e => e.ObjectId == player6ID);
				l["Player6"] = player6;
			}

			ParseObject player7Reference;
			result = l.TryGetValue("Player7", out player7Reference);
			if (result)
			{
				string player7ID = player7Reference.ObjectId;
				ParseObject player7 = playerQueryResults.Find (e => e.ObjectId == player7ID);
				l["Player7"] = player7;
			}

			ParseObject player8Reference;
			result = l.TryGetValue("Player8", out player8Reference);
			if (result)
			{
				string player8ID = player8Reference.ObjectId;
				ParseObject player8 = playerQueryResults.Find (e => e.ObjectId == player8ID);
				l["Player8"] = player8;
			}	

			ParseObject player9Reference;
			result = l.TryGetValue("Player9", out player9Reference);
			if (result)
			{
				string player9ID = player9Reference.ObjectId;
				ParseObject player9 = playerQueryResults.Find (e => e.ObjectId == player9ID);
				l["Player9"] = player9;
			}

			ParseObject player10Reference;
			result = l.TryGetValue("Player10", out player10Reference);
			if (result)
			{
				string player10ID = player10Reference.ObjectId;
				ParseObject player10 = playerQueryResults.Find (e => e.ObjectId == player10ID);
				l["Player10"] = player10;
			}
		
			//Debug.Log ("DTEAM");
		}

		foreach(ParseObject m in playerQueryResults)
		{
			ParseObject responsibilityReference;
			bool result = m.TryGetValue("Responsib", out responsibilityReference);
			if (result)
			{
				string responsibilityID = responsibilityReference.ObjectId;
				ParseObject responsibility = responsibilityQueryResults.Find (e => e.ObjectId == responsibilityID);
				m["Responsib"] = responsibility;

				//Debug.Log (m.Get<ParseObject>("Responsib").ObjectId);
			}
		}

		databaseLoaded = true;
		initializeTeamsTEST();
	}

	// TODO: Remove this function! (For testing only
	void initializeTeamsTEST(){
		ParseObject play = playQueryResults[0];
		ParseObject oTeam = play.Get<ParseObject>("OffensiveTeam");
		ParseObject dTeam = play.Get<ParseObject>("DefensiveTeam");
		for( int i = 0; i < 11; i++)
		{
			gameController.offensiveTeam[i].Initialize(oTeam.Get<ParseObject>("Player" + i));
			gameController.defensiveTeam[i].Initialize(dTeam.Get<ParseObject>("Player" + i));
		}
	}

	// TODO: implement once Play class structure is decided
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

	public ParseObject SelectPlay () 
	{
		float coefficientTotal = CalculateSRSTotal();
		float selectionValue = Random.value*coefficientTotal;
		float runningSrsSum = 0;
		ParseObject selectedPlay = null;

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
			}
			else
			{
				Debug.Log ("more than one set of srs coefficients for current user: ");
			}

			bool recentlySeen = TooRecentlySeen(play);
			if (recentlySeen)
			{
				selectedPlay = SelectPlay();
			}
			else
			{
				selectedPlay = play;
			}
		}

		if (selectedPlay == null) 
		{
			Debug.Log ("selectplay return value is null; the system failed to select a play.");
		}

		return selectedPlay;
	}

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
				//Debug.Log ("play object ID: " + m.ObjectId);
				//Debug.Log ("result: " + result);
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

		//Debug.Log ("srs total: ");
		//Debug.Log (srsTotal);
		return srsTotal;
	}

	bool TooRecentlySeen(ParseObject play)
	{
		System.DateTime mostRecent = System.DateTime.MinValue;
		foreach (ParseObject pastPlay in historyQueryResults) 
		{
			string pastPlayID;
			bool result = pastPlay.TryGetValue("Play", out pastPlayID);
			if (result){
				if (play.ObjectId == pastPlayID)
				{
					Debug.Log ("play id: " + play.ObjectId);
					System.DateTime updatedAt = (System.DateTime)pastPlay.UpdatedAt;
					if (updatedAt > mostRecent) 
					{
						mostRecent = updatedAt;
					}
					System.DateTime difference = System.DateTime.Now - System.TimeSpan.FromDays(1*recencyDelayFactor);
					Debug.Log ("One day ago: " + difference.ToString());
					if (updatedAt > System.DateTime.Now - System.TimeSpan.FromDays(1*recencyDelayFactor))
					{
						return true;
					}
				}
			}
		}
		gameController.playLastSeen = mostRecent;
		return false;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
