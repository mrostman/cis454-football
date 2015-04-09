using UnityEngine;
using System.Collections;

public class SomeMethodToExecute : MonoBehaviour {

	public void DebugPos () {
		Debug.Log(transform.position);
	}

	public void MoveRight () {
		transform.position += Vector3.right;
	}

	public void MoveObject (GameObject tr) {
		tr.transform.position += Vector3.right;
	}

	public void MoveLeft () {
		transform.position -= Vector3.right;
	}

	public void Rotate () {
		transform.Rotate(0,5f,0);
	}

	public void RotateBack () {
		transform.Rotate(0,-5f,0);
	}

	public void Quit () {
		Debug.Log("Quitting...");
		Application.Quit();
	}
}
