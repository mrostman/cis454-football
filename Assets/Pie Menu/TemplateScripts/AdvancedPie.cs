using UnityEngine;
using System.Collections.Generic;

public class AdvancedPie : MonoBehaviour {

	public string ButtonToOpenWidth = "Fire1";	//Fire1 button is the left mouse button or ctrl by default.
	public PieMenu Pie; 						//Set this public to show it in editor.
	public GUIContent[] Options; 				//The content array, what we WILL pass to the PieMenu as default.
	public GUIContent[] SubOptions; 			//The content array, what we CAN pass to the PieMenu as sub-option set.

	void Update () {
		Pie.Center.x = 500;
		Pie.Center.y =-400;
		if (Input.GetButtonUp(ButtonToOpenWidth)) {				//If the "ButtonToOpenWidth" button was pressed and released...
			//Pie.Center = new Vector2(0,0);
			//Pie.
			if (!Pie.Active){
				Pie.InitPie(Options); 				//...If not Initialized yet, Initialize a new pie menu option set using the Options array,
				Pie.Center.x = 500;
				Pie.Center.y =-400;}
			else {												//else
				if (Pie.MenuSet == Options)						//If the Pie Menu's current Menu Set is equal with the Option's option set... (So if we are in the Options menu)
				{
					if (Pie.Selected == 0) {					//If the current selection is the first (0 in the index) item in the Options set, repaint some stuff...
						Pie.CenterImageTint = Color.red;
						Pie.SelectedOptionTint = Color.red;
					}
					else if (Pie.Selected == 1) {				//If the current selection is the second (1 in the index) item in the Options set, repaint some stuff...
						Pie.CenterImageTint = Color.green;
						Pie.SelectedOptionTint = Color.green;
					}
					else if (Pie.Selected == 2) {				//If the current selection is the third (2 in the index) item in the Options set, repaint some stuff...
						Pie.CenterImageTint = Color.blue;
						Pie.SelectedOptionTint = Color.blue;
					}
					else if (Pie.Selected == 3) {				//If the current selection is the fourth (3 in the index) item in the Options set, close the menu...
						Pie.Close();
					}
					else if (Pie.Selected == 4) {				//If the current selection is the fifth (4 in the index) item in the Options set, Fade out, and then fade in with another option set: the "SubOptions" set...
						Pie.TransitionPie(SubOptions);
					}
					else {										//if the selection did not match with the statements above, repaint some stuff...
						Pie.CenterImageTint = Color.white;
						Pie.SelectedOptionTint = Color.white;
					}
				}
				else if (Pie.MenuSet == SubOptions){			//If the Pie Menu's current Menu Set is equal with the SubOptions's option set... (So if we are in the SubOptions menu)
					if (Pie.Selected == 0) {					//If the current selection is the first (0 in the index) item in the Options set, Fade out, and then fade in with another option set: the "Options" set...
						Pie.TransitionPie(Options);
					}
					else if (Pie.Selected == 1) {				//If the current selection is the second (1 in the index) item in the Options set, repaint some stuff...
						Pie.CenterImageTint = Color.yellow;
						Pie.SelectedOptionTint = Color.yellow;
					}
					else {										//if the selection did not match with the statements above, repaint some stuff...
						Pie.CenterImageTint = Color.magenta;
						Pie.SelectedOptionTint = Color.magenta;
					}
				}
			}
		}
	
	}

	void OnGUI () {
		Pie.Center = new Vector2 (Screen.width * 0.3f, Screen.height * 0.5f);	//This line keeps the pie menu in the window, 30% from left, 50% from top. Not necessary, but recommended if you want to use resizeable window.
		Pie.Run();				//This is the function, which will draw the pie on the GUI if it's Active. You can place it anywhere in the OnGUI function.
	}
}
