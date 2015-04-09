using UnityEngine;
using System.Collections;

public class TemplateForUsingWithImages : IPieTemplate {


	//public PieMenu ApplePie;
	public GUIContent[] Options;
	public GUIContent[] SubOptions;
	public string ButtonToOpen = "Fire2";
	public IPieTemplate extraMenu;

	//We don't need the update now, because we give the control over this script to "TemplateForClickableAreas".
//	public void Update () {
//		Handle();
//	}

	public override void Open ()
	{
		if (!ApplePie.Active) ApplePie.InitPie(Options); 								//Initialize a new pie menu option set.
		else if (!(extraMenu != null && ExtraPie.Active)) {
			int selected_Command = ApplePie.Selected; 				//Returns the selected command's number and closes the menu.
			
			if (ApplePie.MenuSet == Options)
			{
				
				switch (selected_Command)
				{
				case 0: 																//The 0 is "Next" so we enter the SubOptions.
					if (extraMenu == null) ApplePie.TransitionPie(SubOptions);
					else {
						ApplePie.Freeze(true);											//Freezes down the Pie Menu, so you can't control it.
						ExtraPie = extraMenu.ApplePie.InitPie(SubOptions);
					}
					break;
				default: 																// The default is leave it closed.
					Close();
					break;
				}
			}
			else if (ApplePie.MenuSet == SubOptions)
			{
				
				switch (selected_Command)
				{
				case 0: 																//The 0 command is "Back" so we "reload" the original Options.
					ApplePie.TransitionPie(Options);
					break;
				default: 																// The default is leave it closed.
					break;
				}
			}
			if (!ApplePie.Frozen) Close();
		}
		else if (ExtraPie != null && extraMenu != null && ExtraPie.Active) {
			int sel = ExtraPie.Selected;
			if (sel == 0) {
				ExtraPie.TransitionPie(new string[] {"Reload", "Return","Quit"});
			}
			else if (sel == 1) {
				ExtraPie.Return();
				ApplePie.Freeze(false);
			}
			else {
				ExtraPie.Close();
				ApplePie.Freeze(false);
				Close();
			}
		}
	}

	public void Close () {
		ApplePie.Close();
	}

	public override void Handle (){
	}

//	public override void _OnGUI () {
//		ApplePie.Center = new Vector2 (Screen.width/2,Screen.height/2); //Align it to the center of the screen
//		ApplePie.Run(); //Run the main Pie Menu wrapper.
//	}
}