using UnityEngine;
using Parse;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;


public class DatabaseController : MonoBehaviour {
List<Play> plays;
string UserName;
string Password;
Task saveTask;
ParseObject retrieved;
bool playCreated = false;
ParseObject responsib = null;
ParseObject srsLine;
ParseObject testParseObj;
Task t1, t2, t3;
IEnumerable<ParseObject> result1, result2, result3;

	// Use this for initialization
	void Start () {
		//TESTCreateHistory();
		//TESTCreateResponsibility();
		//TestGET ();
		TestLogin ();
	}
	
	void testQuery() {
		//ParseQuery<ParseObject> SRS = new ParseQuery<ParseObject>.
	}
	
	void TestLogin() {
		ParseUser.LogInAsync("kris", "password").ContinueWith(t => { TestGET (); });
	}
	
	void HandleLogin(Task t){
			Debug.Log ("Testing123");
			
			if (t.IsFaulted || t.IsCanceled)
			{
				Debug.Log ("Login Failed!");
			}
			else
			{
				Debug.Log ("Success!");
				ParseQuery<ParseObject> query = ParseObject.GetQuery("SRSData").WhereEqualTo("User",ParseUser.CurrentUser);
				query.FirstAsync().ContinueWith(u => { srsLine = u.Result ; CheckSRS (); } );
				
			}
		}
		
	void CheckSRS(){
		Debug.Log ("Printing Results");
		Debug.Log (srsLine.CreatedAt);
		Debug.Log (srsLine.Get<float>("BX1Mjat6KY"));
		float i;
		
		bool exists = srsLine.TryGetValue("KpAWTtyKhn", out i);
		if (exists)
			Debug.Log (i);
		else
			Debug.Log (exists);
		
	}
	
	void TESTCreateResponsibility()
	{
		ParseObject test = new ParseObject("Responsibility");
		List<int[]> cords = new List<int[]>();
		cords.Add (new int[2] { 1, 1});
		cords.Add (new int[2] { -1, 1});
		cords.Add (new int[2] { 1, 1});
		cords.Add (new int[2] { -1, 1});
		test["Name"] = "ZigZag";
		test["Coordinates"] = cords;
		saveTask = test.SaveAsync();
		Debug.Log (saveTask.IsCompleted);
	}
	
	void TestGET(){
		ParseQuery<ParseObject> query1 = ParseObject.GetQuery("SRSData").WhereEqualTo("User",ParseUser.CurrentUser);
		ParseQuery<ParseObject> query2 = ParseObject.GetQuery("Play").WhereNotEqualTo("Name","THIS NAME IS FAKE");
		ParseQuery<ParseObject> query3 = ParseObject.GetQuery("History").WhereEqualTo("User",ParseUser.CurrentUser);
		Debug.Log (ParseUser.CurrentUser.ObjectId);
		
		
		List<Task> tasks = new List<Task>();
		//query1.FindAsync().ContinueWith(t => { Debug.Log (t); result1 = t.Result ;
			//foreach(ParseObject obje in result1) { testParseObj = obje; Debug.Log (obje.ObjectId); }
			//								   checkGet ();});
		t1 = query1.FindAsync().ContinueWith(t => { result1 = t.Result;});
		tasks.Add (t1);
		t2 = query2.FindAsync().ContinueWith(t => { result2 = t.Result ;});
		tasks.Add (t2);
		t3 = query3.FindAsync().ContinueWith(t => { result3 = t.Result ;});
		tasks.Add (t3);
		Task task2 = Task.WhenAll (tasks).ContinueWith(q => {checkGet();});
		//return Task.WhenAll(tasks);
	}
	
	void checkGet() {
		Debug.Log ("Checking...");
		Debug.Log (t2.IsCompleted);
		//Debug.Log ("ID: " + re.ObjectId);
		foreach(ParseObject i in result1)
		{
			Debug.Log (i.ObjectId);
			float s1 = i.Get<float>("BX1Mjat6KY");
			//bool exists = i.TryGetValue<string>("User", out s1);
			Debug.Log (s1);
			//Debug.Log (exists);
		}
		foreach(ParseObject j in result2)
		{
			Debug.Log ("Looped");
			Debug.Log (j.ObjectId);
		}
		foreach(ParseObject k in result3)
		{
			Debug.Log (k.ObjectId);
		}
	}

	void TESTCreateHistory () {
		// Get responsibility!
		Debug.Log (0);
		ParseQuery<ParseObject> query = ParseObject.GetQuery("Responsibility");
		query.GetAsync("L6t6IqbF8h").ContinueWith(t => { responsib = t.Result; TESTCreateHistory2(); });
	}
	
	void TESTCreateHistory2(){
		// Test values
		Debug.Log (1);
		string[] positionAbbriviations = new string[11] {"C","G","QB","RB","FB","WR","LG","RG","RB","OL","OL"};
		string[] positionNames = new string[11] {"Center","Guard","Quarterback","Running Back","Fullback",
			"Wide Receiver","Left Guard","Right Guard","Running Back",
			"Offensive Lineman","Offensive Lineman"};
		int[] locX = new int[11] {-6,-5,-4,-3,-2,-1,0,1,2,3,4};
		int[] locY = new int[11] {1,2,3,4,5,6,5,4,3,2,1};
		
		ParseObject testOffensiveTeam = new ParseObject("OffensiveTeam");
		
		// Wait if the responsibility hasn't been retrieved yet
		while (responsib == null) {Debug.Log(responsib);}
		
		// Create offensive team players
		for (int i = 0; i < 11; i++)
		{
			ParseObject testPlayer = new ParseObject("Player");
			testPlayer["Position"] = positionNames[i];
			testPlayer["Abbreviation"] = positionAbbriviations[i];
			testPlayer["Location"] = new int[2] { locX[i], (locY[i] * -1) };
			testPlayer["Motion"] = new int[2] {0,0};
			List<int[]> shifts = new List<int[]>();
			shifts.Add(new int[2] {0,-1});
			shifts.Add(new int[2] {0,1});
			testPlayer["Shifts"] = shifts;
			testPlayer["Responsib"] = responsib;
			testPlayer["Controllable"] = true;
			string fieldName = "Player" + i;
			testOffensiveTeam.Add(fieldName, testPlayer);
		}
		
		ParseObject testHistory = new ParseObject("History");
		testHistory["User"] = ParseUser.CurrentUser;
		testHistory["Play"] = ParseObject.CreateWithoutData("Play","QyqNwzSh3q");
		testHistory["OffensiveTeam"] = testOffensiveTeam;
		
		saveTask = testHistory.SaveAsync();
		
		Debug.Log (saveTask.IsCompleted);
	}
	
	
	void TESTCreatePlay1 () {
		// Get responsibility!
		Debug.Log (0);
		ParseQuery<ParseObject> query = ParseObject.GetQuery("Responsibility");
		query.GetAsync("IoHD0Ue18n").ContinueWith(t => { responsib = t.Result; });
		return;
		}
	void TestCreatePlay2(){
		// Test values
		Debug.Log (1);
		string[] positionAbbriviations = new string[11] {"C","G","QB","RB","FB","WR","LG","RG","RB","OL","OL"};
		string[] positionNames = new string[11] {"Center","Guard","Quarterback","Running Back","Fullback",
												 "Wide Receiver","Left Guard","Right Guard","Running Back",
												 "Offensive Lineman","Offensive Lineman"};
		int[] locX = new int[11] {-6,-5,-4,-3,-2,-1,0,1,2,3,4};
		int[] locY = new int[11] {1,2,3,4,5,6,5,4,3,2,1};
		
		// Create Parse Objects
		ParseObject testPlay = new ParseObject("Play");
		ParseObject testOffensiveTeam = new ParseObject("OffensiveTeam");
		ParseObject testDefensiveTeam = new ParseObject("DefensiveTeam");
		
		// Wait if the responsibility hasn't been retrieved yet
		while (responsib == null) {Debug.Log(responsib);}
		
		// Create offensive team players
		for (int i = 0; i < 11; i++)
		{
			ParseObject testPlayer = new ParseObject("Player");
			testPlayer["Position"] = positionNames[i];
			testPlayer["Abbreviation"] = positionAbbriviations[i];
			testPlayer["Location"] = new int[2] { locX[i], (locY[i] * -1) };
			testPlayer["Motion"] = new int[2] {0,0};
			List<int[]> shifts = new List<int[]>();
			shifts.Add(new int[2] {0,-1});
			shifts.Add(new int[2] {0,1});
			testPlayer["Shifts"] = shifts;
			testPlayer["Responsib"] = responsib;
			testPlayer["Controllable"] = true;
			string fieldName = "Player" + i;
			testOffensiveTeam.Add(fieldName, testPlayer);
		}
		// Create defensive team players
		for (int i = 0; i < 11; i++)
		{
			ParseObject testPlayer = new ParseObject("Player");
			testPlayer["Position"] = positionNames[i];
			testPlayer["Abbreviation"] = positionAbbriviations[i];
			testPlayer["Location"] = new int[2] { locX[i], locY[i] };
			testPlayer["Controllable"] = false;
			string fieldName = "Player" + i;
			Debug.Log (fieldName);
			testDefensiveTeam.Add(fieldName, testPlayer);
		}
		
		testPlay["OffensiveTeam"] = testOffensiveTeam;
		testPlay["DefensiveTeam"] = testDefensiveTeam;
		testPlay["Name"] = "Everybody Run Left, with Shift";
		testPlay["Parent"] = "QyqNwzSh3q";
		
		saveTask = testPlay.SaveAsync();
		
		Debug.Log (saveTask.IsCompleted);
	}
	
	// Update is called once per frame
	void Update () {

	}
}
