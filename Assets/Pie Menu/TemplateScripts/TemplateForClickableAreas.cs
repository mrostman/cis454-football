using UnityEngine;
using System.Collections;

public class TemplateForClickableAreas : MonoBehaviour {

	[System.Serializable]
	public class Area {
		public string Name;
		public Rect Position;
		public Texture Image;
		public IPieTemplate TemplateScript;
	}

	public Area[] Areas;
	public GUISkin Skin;
	public static bool showrectangles = true;

	void Update () {
		showrectangles = true;
		foreach (Area item in Areas) {
			if (showrectangles && Input.GetMouseButtonUp(0) && item.Position.Contains(InvertMouse(Input.mousePosition)) && (!Screen.lockCursor) || (Input.GetMouseButtonUp(0) && item.TemplateScript.ApplePie.Active)) {
				item.TemplateScript.Open();
				showrectangles = false;
			}
			else if (item.TemplateScript.ApplePie.Active || item.TemplateScript.ExtraPie.Active) {
				item.TemplateScript.Handle();
				showrectangles = false;
				break;
			}
		}
	}

	void OnGUI () {
		if (GUILayout.Button("Back to Main Menu..")) Application.LoadLevel(0);
		GUILayout.Label("[Mouse input demo] Click in the rectangles to Open a Pie Menu");
		foreach (Area item in Areas) {
			if (showrectangles) {
				GUI.DrawTexture(item.Position, item.Image);
				//GUILayout.BeginArea(new Rect(item.Position.x, item.Position.y + item.Position.height, item.Position.width, item.Position.height * 1.6f), Skin.box);
				GUILayout.BeginArea(new Rect(item.Position.x, item.Position.y + item.Position.height, item.Position.width, 40f), Skin.box);
				GUILayout.Label(item.Name);
				GUILayout.EndArea();
			}

			if (item.TemplateScript.ApplePie != null) {
				item.TemplateScript.ApplePie.Center = item.Position.center;
				item.TemplateScript.ApplePie.Run();
			}

			if (item.TemplateScript.ExtraPie != null) {
				item.TemplateScript.ExtraPie.Center = item.Position.center;
				item.TemplateScript.ExtraPie.Run();
			}
		}
	}

	public Vector2 InvertMouse(Vector2 mousePos) {
		Vector2 newMousePos = mousePos;
		newMousePos.y = Screen.height - mousePos.y;
		return newMousePos;
	}
}

[System.Serializable]
public abstract class IPieTemplate : MonoBehaviour {
	public PieMenu ApplePie;
	[HideInInspector]
	public PieMenu ExtraPie;
	public abstract void Open();
	public abstract void Handle();
}