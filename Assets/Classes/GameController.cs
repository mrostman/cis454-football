using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	Play[] allPlays;
	Play currentPlay;
	Play userPlay;
	double coefficientTotal;
	int timeLeft;

	Queue playsToUpdate = new Queue ();
	Queue userPlaysToUpload = new Queue ();

	// Use this for initialization
	void Start () {
	
	}

	void NewPlay () {
		currentPlay = SelectPlay ();
		userPlay = new Play ();
		// TODO: display initial player positions on field
	}

	Play SelectPlay () {
		double selectionValue = Random.value*coefficientTotal;
		int numPlays = allPlays.Length;
		Play selectedPlay = new Play ();
		double runningSum;
		for (int i=0; i<numPlays; i++) 
		{
			// TODO: cumulative sum of coefficients from allPlays array
			// runningSum = runningSum + allPlays[i].getCoefficient ();
			// if (runningSum > selectionValue)
			// {
			//    selectedPlay = allPlays[i];
			//    break;
			// }
		}
		return selectedPlay;
	}

	void EndPlay () {
		// TODO: display and animate currentPlay;
		int correctness = EvaluateCorrectness ();
		// TODO: calculate days since last played from timestamp in play wrapper class
		double sinceLastPlayed = 0.0;
		double newSRS = SRSRecalculate (correctness,sinceLastPlayed);
		// TODO: update allPlays entry for currentPlay with new SRS coefficient
		playsToUpdate.Enqueue (userPlay);
		QueueUpdate ();
	}
	
	int EvaluateCorrectness () {
		// TODO: iterate through userPlay.players
		// for each, iterate through currentPlay.players to find a match on position/type
		// check location, shift, motion correctness with Vector2.Distance(a,b) < threshhold
		return 1;
	}
	
	double SRSRecalculate (int correctness, double sinceLastPlayed) {
		// TODO: SRS coefficient product of age component, progress component, effort component
		// Age A(x) = Cn^x			(x is time elapsed, C is initial constant)
		// Progress P(x) = Cn^-x	(x is previous success)
		// Effort {1,10}
		return  0.0;
	}

	void QueueUpdate () {

		while (playsToUpdate.Count > 0)
		{
			// TODO: send playsToUpdate[i] to DatabaseController function
			// if (return indicates failure) 
			// {
			//    break;
			// }
			// else 
			// {
			//    playsToUpdate.Dequeue();
			// }
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
