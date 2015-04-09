using UnityEngine;
using System.Collections;

public class TemplateForUsingWithTexts : IPieTemplate {


	//public PieMenu ApplePie;
	public string[] Options = {"Reload","Throw","Use","Kiss","Submenu","Punch","Use","Close"};
	public string[] SubOptions = {"<- Back","Throw","Other","Menu","Like","No","Other","One"};
	public string ButtonToOpen = "Fire1";
	public AudioClip OpenSound;
	public AudioClip ChangeSound;

	//We don't need the update now, because we give the control over this script to "TemplateForClickableAreas".
	//	public void Update () {
	//		if (Input.GetButtonDown(ButtonToOpen)) Handle();
	//	}

	public override void Open () {
		if (audio != null && OpenSound != null) audio.Play();
		if (!ApplePie.Active) ApplePie.InitPie(Options); //Initialize a new pie menu option set.
		else {
			int selected_Command = ApplePie.GetSelectedPieAndClose(); //Returns the selected command's number and closes the menu.
			
			if (ApplePie.MenuSet == Options)
			{
				switch (selected_Command)
				{
				case 0: //The 0 command is "Reload" so we "reload" the pie.
					ApplePie.TransitionPie(Options);
					break;
				case 4: //The 4th is "Submenu" so we enter the SubOptions.
					ApplePie.TransitionPie(SubOptions);
					break;
				default: // The default is leave it closed.
					Close();
					break;
				}
			}
			else if (ApplePie.MenuSet == SubOptions)
			{
				switch (selected_Command)
				{
				case 0: //The 0 command is "Back" so we "reload" the original Options.
					ApplePie.TransitionPie(Options);
					break;
				default: // The default is leave it closed.
					break;
				}
			}
		}

	}

	public void Close () {
		ApplePie.Close();
	}

	public override void Handle (){
		if (ApplePie.Active && ApplePie.Changed && audio != null && ChangeSound != null) {
			audio.PlayOneShot(ChangeSound);
		}
	}


//	public override void OnGUI () {
//		ApplePie.Center = new Vector2 (Screen.width/2,Screen.height/2); //Align it to the center of the screen
//		ApplePie.Run(); //Run the main Pie Menu wrapper.
//	}
}