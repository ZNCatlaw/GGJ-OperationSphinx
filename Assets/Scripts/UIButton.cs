using UnityEngine;
using System.Collections;

public class UIButton : MonoBehaviour {

	public string messageToGame = "";

	private bool mouseIn = false;

	private static Color highlightColor = new Color(1.0f, 0.75f, 0.75f);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// Color based on state
		var shaderColor = Color.white;
		if (mouseIn){
			shaderColor = shaderColor * highlightColor;
		}		
		renderer.material.color = shaderColor;
	}

	void OnMouseDown () {
		Camera.main.SendMessage(messageToGame);
	}

	void OnMouseEnter() {
		mouseIn = true;
	}

	void OnMouseExit() {
		mouseIn = false;
	}
}
