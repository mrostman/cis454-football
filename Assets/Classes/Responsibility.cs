using UnityEngine;
using System.Collections;

public class Responsibility : MonoBehaviour {
	int id { get; set;}
	Vector2[] stops { get; set;}
	Texture icon, displayPath;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	// Override the default Equals method to compare responsibilities based on ID alone (increases performance and prevents false negatives)
	/*public override bool Equals(System.Object obj)
	{
		// If parameter is null return false.
		if (obj == null)
		{
			return false;
		}
		
		// If parameter cannot be cast to Responsibility return false.
		Responsibility p = obj as Responsibility;
		if ((System.Object)p == null)
		{
			return false;
		}
		
		// Return true if the fields match:
		return (id == p.id);
	}*/
	
	// Getters
	public int GetID() { return id; }
	public Texture GetIcon() { return icon; }
	public Texture GetDisplayPath() { return displayPath; }
	
}
