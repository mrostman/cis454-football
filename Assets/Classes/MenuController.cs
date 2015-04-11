using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuController : MonoBehaviour {
	public static List<GUIContent> menuContentInitialized;
	public static List<GUIContent> menuContentShifted;
	public static List<GUIContent> menuContentMotioned;
	public static List<GUIContent> menuContentResponsibility;
	
	public List<GUIContent> iMenuContentInitialized;
	public List<GUIContent> iMenuContentShifted;
	public List<GUIContent> iMenuContentMotioned;
	public List<GUIContent> iMenuContentResponsibility;
	
	

	// Use this for initialization
	void Start () {
		menuContentInitialized = iMenuContentInitialized;
		menuContentShifted = iMenuContentShifted;
		menuContentMotioned = iMenuContentMotioned;
		menuContentResponsibility = iMenuContentResponsibility;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
