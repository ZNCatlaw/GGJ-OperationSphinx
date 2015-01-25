using UnityEngine;
using System.Collections;

public class CardLogic : MonoBehaviour {

	public string cardName = "none";
	public bool faceUp = false;
	public bool locked = false;
	public bool selected = false;
	
	private bool busy = false;
	private bool mouseIn = false;

	private bool visLocked = false;
	private bool visSelected = false;
	private bool visFaceUp = false;

	private static Quaternion faceUpRotation = new Quaternion(0, 180, 0, 0);
	private static Color highlightColor = new Color(1.0f, 0.75f, 0.75f, 1.0f);
	private static Color selectedColor = new Color(0.75f, 0.75f, 1.0f, 1.0f);

	// Called by GameLogic.CardInstantiate
	public void SetUp () {
		var cardMaterial = Resources.Load("Materials/Cards/" + name, typeof(Material)) as Material;
		var frontQuad = this.transform.FindChild("Front").gameObject;
		frontQuad.renderer.material = cardMaterial;
	}

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		// Transition into states
		if (faceUp != visFaceUp){
			visFaceUp = faceUp;
			if (faceUp) {
				transform.rotation = transform.rotation * faceUpRotation;
			}else{
				transform.rotation = transform.rotation * Quaternion.Inverse(faceUpRotation);
			}

		}

		if (selected != visSelected){
			visSelected = selected;
			if (selected) {
				transform.Translate(0.0f, 0.25f, 0.0f);
			}else{
				transform.Translate(0.0f, -0.25f, 0.0f);
			}
		}

		// Color based on state
		var shaderColor = Color.white;
		if (selected) {
			shaderColor = shaderColor * selectedColor;
		}

		if (mouseIn && !locked && !busy && faceUp){
			shaderColor = shaderColor * highlightColor;
		}

		var frontQuad = this.transform.FindChild("Front").gameObject;
		frontQuad.renderer.material.color = shaderColor;
	}
	
	void OnMouseDown() {
		//Debug.Log(string.Format("MouseDown `{0}`, hand `{1}`", name, this.transform.parent.name));
		if (!locked && !busy && faceUp) {
			//Check if any other card in the same hand is locked
			var children = transform.parent.GetComponentsInChildren<CardLogic>();
			foreach (var child in children) {
				if (child.selected && child != this) {
					child.selected = false;
				}
			}

			//Swap selected state
			selected = !selected;
		}
	}

	void OnMouseEnter() {
		//Debug.Log(string.Format("MouseEnter `{0}`, hand `{1}`", name, this.transform.parent.name));
		mouseIn = true;
	}

	void OnMouseExit() {
		//Debug.Log(string.Format("MouseExit `{0}`, hand `{1}`", name, this.transform.parent.name));
		mouseIn = false;
	}
}
