using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class PieMenuController : MonoBehaviour {
	public static List<GUIContent> menuContentInitialized;
	public static List<GUIContent> menuContentShifted;
	public static List<GUIContent> menuContentMotioned;
	public static List<List<GUIContent>> menuContentResponsibility;
	
	public List<GUIContent> iMenuContentInitialized;
	public List<GUIContent> iMenuContentShifted;
	public List<GUIContent> iMenuContentMotioned;
	public List<GUIContent> iMenuContentResponsibility;
	
	public GUIContent back, next;
	
	

	// Use this for initialization
	void Start () {	
		menuContentInitialized = iMenuContentInitialized;
		menuContentShifted = iMenuContentShifted;
		menuContentMotioned = iMenuContentMotioned;
		
	}
	
	private List<List<GUIContent>> Partition() {
		List<List<GUIContent>> outList = new List<List<GUIContent>>();
		int size = DatabaseController.responsibilityQueryResults.Count;
		int index = 0;
		while (index < size) {
			List<GUIContent> page = new List<GUIContent>();
			for (int i = 0; i < 6; i++) {
				if (index == size) {
					break;
				}
				else if (i == 0 && outList.Count > 0)
					page.Add (back);
				else if (i == 5 && index < size)
					page.Add (next);
				else {
					ParseObject responsibility = DatabaseController.responsibilityQueryResults[index++];
					string name = responsibility.Get<string>("Name");
					page.Add (new GUIContent("", 
											 Resources.Load<Texture2D>("Icons/" + name),
											 name));
				}
			}
			outList.Add(page);
		}
		return outList;
	}
	
	// Update is called once per frame
	void Update () {
		if (DatabaseController.databaseLoaded && menuContentResponsibility == null)
			menuContentResponsibility = Partition();
	}
}
