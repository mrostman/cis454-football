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

	// Use this for initialization
	void Start () {
		TESTCreatePlay1();
		//TESTCreateResponsibility();
		//TestGET ();
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
		ParseQuery<ParseObject> query = ParseObject.GetQuery("Play");
		query.GetAsync("L6t6IqbF8h").ContinueWith(t =>
		                                          {
			retrieved = t.Result;
		});

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
			if (responsib != null && !playCreated)
			{
				playCreated = true;
				TestCreatePlay2();
				Debug.Log ("Creating");
			}
	}
}
