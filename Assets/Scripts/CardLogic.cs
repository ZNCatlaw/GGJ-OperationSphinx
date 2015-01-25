using UnityEngine;
using System.Collections;

public class CardLogic : MonoBehaviour {

	public string cardName = "none";
	public bool faceUp = false;
	public bool locked = false;
	
	private bool visFaceUp = false;

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
		//
		if (faceUp != visFaceUp){
			visFaceUp = faceUp;
			transform.rotation = transform.rotation * new Quaternion(0, 180, 0, 0);
		}
	}

	void OnMouseDown() {
		Debug.Log(string.Format("Clicked `{0}` in hand `{1}`", name, this.transform.parent.name));
	}
}
