using UnityEngine;
using System.Collections.Generic;

public class MultitouchPie : MonoBehaviour {

	public Rect OpenWhenTouchedHere;
	private Rect realTouchedHere;

	public Texture AreaImage;
	public bool ExecuteWhenChanged = false;
	public PieMenu Pie; 							//Set this public to show it in editor.
	public ExecutableOption[] Options; 				//The content array, what we WILL pass to the PieMenu as default.

	void Update () {
		realTouchedHere = new Rect (Screen.width * OpenWhenTouchedHere.x, Screen.height * OpenWhenTouchedHere.y, Screen.width * OpenWhenTouchedHere.width, Screen.height * OpenWhenTouchedHere.height);
		//The "realTouchedHere" is calculated by "OpenWhenTouchedHere".
		//Assign screen percent values to "OpenWhenTouchedHere", and "realTouchedHere" automatically calculates them to you to pixel coordinates.


		if (Input.touchCount > 0) {									//If there are any touches...
			if (Pie.Active && !Input.touches.Exists(Pie.FingerID)) {//If Pie is active, but the finger which triggered it no longer exists.
				if (Pie.SelectedExecutableOption != null) {
					Pie.Execute();									//If something is selected, execute the actual selected option. The method name is defined in the inspector.
				}
				Close();											//Close the Pie Menu.
			}
			else if (Pie.Active) {									//else if it's still active, and the finger still exists.
				if (ExecuteWhenChanged && Pie.Changed && (Pie.SelectedExecutableOption != null)) {
						Pie.Execute();								//If you set true to "ExecuteWhenChanged" and the pie menu was changed, Execute.
				}
			}
			else {													//else if it is not active anymore.
				for (int i = 0; i < Input.touchCount; i++) {		//Loop through the touches
					if (Input.GetTouch(i).phase == TouchPhase.Began && realTouchedHere.Contains(Input.GetTouch(i).position)) {		//If the touch is just began and our rect contains it...
						Pie.InitPie (Options);						//...Initialize a new pie menu option set using the Options array,
						Pie.FingerID = Input.GetTouch(i).fingerId;	//assign the fingerID to the Pie menu, so it will know what to obey.
						Pie.Center = PieMenu.UseTouchPos( Input.GetTouch(i).position);	//Relocate the Pie menu to the position of the touch.
					}
				}
			}
		}
		else if (Pie.Active) {										//Else if the touch count is zero, but the pie is active...
			if (Pie.SelectedExecutableOption != null) {
				Pie.Execute();										//If something is selected, execute the actual selected option. The method name is defined in the inspector.
			}
			Close();												//Close the Pie Menu.
		}
	}
	

	void Close () {
		Pie.Close();											//Close the pie menu
	}

	void OnGUI () {
		GUI.DrawTexture(realTouchedHere, AreaImage, ScaleMode.StretchToFill); //Draw textures to fill the Rects, helping us to see the area, which accept touches. 

		Pie.Run();				//This is the function, which will draw the pie on the GUI if it's Active. You can place it anywhere in the OnGUI function.
	}
}
