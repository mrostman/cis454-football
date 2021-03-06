﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;

public class PieMenuController : MonoBehaviour {
	public static List<GUIContent> menuContentInitialized;
	public static List<GUIContent> menuContentShifted;
	public static List<GUIContent> menuContentMotioned;
	public static List<List<GUIContent>> menuContentResponsibility;
	public static List<GUIContent> menuContentEditMode;
	
	public List<GUIContent> iMenuContentInitialized;
	public List<GUIContent> iMenuContentShifted;
	public List<GUIContent> iMenuContentMotioned;
	public List<GUIContent> iMenuContentResponsibility;
	public List<GUIContent> imenuContentEditMode;
	
	public static MenuController menuController;
	public MenuController iMenuController;
	
	public GUIContent back, next;
	
	

	// Initializes variaous values
	void Start () {	
		menuContentInitialized = iMenuContentInitialized;
		menuContentShifted = iMenuContentShifted;
		menuContentMotioned = iMenuContentMotioned;
		menuContentEditMode = imenuContentEditMode;
		menuController = iMenuController;
	}
	
	// Partitions the full list of responsibilities into 'pages' for use with the pie menu
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
