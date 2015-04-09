using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public bool JustABackToMenuButton = false;

	void OnGUI () {
		if (JustABackToMenuButton) {
			if (GUILayout.Button("Back to Main Menu..")) Application.LoadLevel(0);
		}
		else {
			GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 200, Screen.height * 0.2f, 400, Screen.height * 0.6f));
			
			GUIStyle title = new GUIStyle();
			title.fontStyle = FontStyle.Bold;
			title.fontSize = 32;
			title.alignment = TextAnchor.MiddleCenter;
			title.normal.textColor = Color.Lerp(Color.white, Color.black, 0.7f);
			
			GUILayout.Label("Pie Menu Z", title);
			GUILayout.Space(Screen.height * 0.05f);
			
			if (GUILayout.Button("Mouse Input Demo", GUILayout.ExpandHeight(true))) Application.LoadLevel(1);
			if (GUILayout.Button("Gamepad Axis Demo", GUILayout.ExpandHeight(true))) Application.LoadLevel(2);
			if (GUILayout.Button("Android Touch Demo", GUILayout.ExpandHeight(true))) Application.OpenURL("https://dl.dropboxusercontent.com/u/64779859/PieMenuZ/piemenuz.apk");
			
			GUILayout.EndArea();
		}
	}
}
