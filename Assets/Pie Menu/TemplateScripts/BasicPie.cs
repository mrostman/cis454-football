using UnityEngine;
using System.Collections.Generic;

public class BasicPie : MonoBehaviour {

	public PieMenu Pie;													//Pie Menu declaration. Set this public to show it in editor.
	public string[] Options = {"Red","Green","Blue","Close","White"};	//The text array, what we will pass to the PieMenu.

	void Update () {
		if (Input.GetMouseButtonUp(0)) {						//If the left mouse button was just released...
			if (!Pie.Active) Pie.InitPie(Options); 				//...If not Initialized yet, Initialize a new pie menu option set using the Options array,
			else {												//else
				if (Pie.Selected == 0) {						//if the current selection is the first value (Indexed as 0) from the Options array...
					Pie.CenterImageTint = Color.red;			//...change the center image's tint to red,
					Pie.SelectedOptionTint = Color.red;			//and change the selected option's tint to red.
																//So this is the place where you can assign commands to options.
				}
				else if (Pie.Selected == 1) {					//if the current selection is the second value (Indexed as 1) from the Options array...
					Pie.CenterImageTint = Color.green;			//...change the center image's tint to green,
					Pie.SelectedOptionTint = Color.green;		//and change the selected option's tint to green.
				}
				else if (Pie.Selected == 2) {					//if the current selection is the third value (Indexed as 2) from the Options array...
					Pie.CenterImageTint = Color.blue;			//...change the center image's tint to blue,
					Pie.SelectedOptionTint = Color.blue;		//and change the selected option's tint to blue.
				}
				else if (Pie.Selected == 3) {					//if the current selection is the fourth value (Indexed as 4) from the Options array...
					Pie.Close();								//...close the Pie Menu.
				}
				else {											//if the selected did not match with the statements above...
					Pie.CenterImageTint = Color.white;			//...paint the center image to white,
					Pie.SelectedOptionTint = Color.black;		//and the selected option to black.
				}
			}
		}
	
	}

	void OnGUI () {
		Pie.Center = new Vector2 (Screen.width * 0.5f, Screen.height * 0.5f);	//if our window is resizeable, it's highly recommended to set the center point of the pie menu in every GUI draw. Else we can assign an absolute value here.
		Pie.Run();																//This is the function, which will draw the pie on the GUI if it's Active. You can place it anywhere in the OnGUI function.
	}
}
