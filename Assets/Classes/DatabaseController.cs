using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;
using System.Threading.Tasks;

public class DatabaseController : MonoBehaviour {
	ParseObject retrieved;
	IEnumerable<ParseObject> playQueryResults, historyQueryResults, srsQueryResults;

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
			
			List<Task> tasks = new List<Task>();
			
			
			Task playTask = playQuery.FindAsync().ContinueWith(u => { playQueryResults = u.Result;});
			tasks.Add (playTask);
			Task historyTask = historyQuery.FindAsync().ContinueWith(u => { historyQueryResults = u.Result ;});
			tasks.Add (historyTask);
			Task srsTask = srsQuery.FindAsync().ContinueWith(u => { srsQueryResults = u.Result ;});
			tasks.Add (srsTask);
			
			Task allQueriesTask = Task.WhenAll (tasks).ContinueWith(q => {

				// testing
				Debug.Log ("Checking...");
				Debug.Log (playTask.IsCompleted);
				//Debug.Log ("ID: " + re.ObjectId);
				foreach(ParseObject i in srsQueryResults)
				{
					Debug.Log (i.ObjectId);
					float s1 = i.Get<float>("BX1Mjat6KY");
					//bool exists = i.TryGetValue<string>("User", out s1);
					Debug.Log (s1);
					//Debug.Log (exists);
				}
				foreach(ParseObject j in playQueryResults)
				{
					Debug.Log ("Looped");
					Debug.Log (j.ObjectId);
				}
				foreach(ParseObject k in historyQueryResults)
				{
					Debug.Log (k.ObjectId);
				}

			});


		
		});
	}

	// Update is called once per frame
	void Update () {
	
	}
}
