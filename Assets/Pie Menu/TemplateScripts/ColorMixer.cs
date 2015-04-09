using UnityEngine;
using System.Collections;

public class ColorMixer : MonoBehaviour {

	public Color A = Color.white;
	public Color B = Color.white;

	public void Update () {
		renderer.material.color = Color.Lerp(A,B,0.5f);
	}

	public void WhiteA () { A = Color.white; }
	public void WhiteB () { B = Color.white; }
	public void BlackA () { A = Color.black; }
	public void BlackB () { B = Color.black; }
	public void BlueA () { A = Color.blue; }
	public void BlueB () { B = Color.blue; }
	public void RedA () { A = Color.red; }
	public void RedB () { B = Color.red; }
	public void YellowA () { A = Color.yellow; }
	public void YellowB () { B = Color.yellow; }
	public void GreenA () { A = Color.green; }
	public void GreenB () { B = Color.green; }

}
