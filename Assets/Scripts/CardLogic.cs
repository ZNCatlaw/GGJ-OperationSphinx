using UnityEngine;
using System.Collections;

public class CardLogic : MonoBehaviour {

	public string cardName = "none";
	public bool faceUp = false;
	public bool locked = false;

	private static Quaternion faceUpRotation = new Quaternion(0, 180, 0, 0);

	// Called by GameLogic.CardInstantiate
	public void SetUp () {
		
	}

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		if (faceUp) {
			this.transform.localRotation = faceUpRotation;
		} else {
			this.transform.localRotation = Quaternion.identity;
		}
	}

	void OnMouseDown() {
		Debug.Log(string.Format("Clicked {0} in {1}", cardName, this.transform.parent.name));
	}
}
