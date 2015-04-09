using UnityEngine;
using System.Collections.Generic;

public class SendMessagePie : MonoBehaviour {

	public string ButtonToOpenWidth = "Fire1";	//Fire1 button is the left mouse button or ctrl by default.
	public PieMenu Pie; 						//Set this public to show it in editor.
	public ExecutableOption[] Options; 				//The content array, what we WILL pass to the PieMenu as default.
	public ExecutableOption[] SubOptions; 			//The content array, what we CAN pass to the PieMenu as sub-option set.

	void Update () {
		if (Input.GetButtonUp(ButtonToOpenWidth)) {					//If the "ButtonToOpenWidth" button was pressed and released...
			if (!Pie.Active) Pie.InitPie(Options); 					//...If not Initialized yet, Initialize a new pie menu option set using the Options array,
			else if (Pie.SelectedExecutableOption == null) Close();	//If nothing is selected, close.
			else Pie.Execute();										//Execute the actual selected option. The method name is defined in the inspector.
		}
	}

	void Close () {
		Pie.Close();											//Close the pie menu
	}

	void GoToSubOptions () {
		Pie.TransitionPie(SubOptions);							//Go to sub option set
	}

	void GoToOptions () {
		Pie.TransitionPie(Options);								//Go back to the original option set
	}

	public static Vector2 Invert(Vector2 pos) {
		Vector2 newTouchPos = pos;
		newTouchPos.y = Screen.height - pos.y;
		return newTouchPos;
	}

	void OnGUI () {
		if (GameObject.Find("TargetCube") != null) Pie.Center = Invert( Camera.main.WorldToScreenPoint(GameObject.Find("TargetCube").transform.position)); //This line keeps the pie menu above the cube. If it does not exist, the else statement will executed.
		else Pie.Center = new Vector2 (Screen.width * 0.5f, Screen.height * 0.5f);	//else this line keeps the pie menu in the window, 50% from left, 50% from top. Not necessary, but recommended if you want to use resizeable window.

		Pie.Run();				//This is the function, which will draw the pie on the GUI if it's Active. You can place it anywhere in the OnGUI function.
	}
}
