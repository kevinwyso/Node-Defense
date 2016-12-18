using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class prestigeMenu : MonoBehaviour {

	[Header ("Buttons")]
	public Button prestigeButton;

	//[Header ("Texts")]


	// Use this for initialization
	void Start () {
		prestigeButton.interactable = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (playerStats.current.getLevel () >= 72)
			prestigeButton.interactable = true;
		else
			prestigeButton.interactable = false;
	}

	public void prestige(){
		playerStats.current = new playerStats (playerStats.current);
	}
}
