using UnityEngine;
using System.Collections;

public class InstructionsLogic : MonoBehaviour
{

    GameLogic gameLogic;
    Transform tr;
    bool dismissed = false;

	// Use this for initialization
	void Start()
    {
        gameLogic = Camera.main.GetComponent<GameLogic>();
        tr = transform;
	}
	
	// Update is called once per frame
	void Update()
    {
        if (dismissed) {
            if (transform.position.y < 15.0f) {
                tr.position = tr.position + (Vector3.up * Time.deltaTime * 10.0f);
            } else {
                Destroy(gameObject);
            }
        }
	}

    void OnMouseDown()
    {
        if (dismissed) { return; }
        dismissed = true;
        Destroy(GameObject.Find("Instructions/Background"));
        gameLogic.SendMessage("NewGame");
    }
}
