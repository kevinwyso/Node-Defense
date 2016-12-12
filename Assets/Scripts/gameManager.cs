using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour {

	public GameObject gameoverUI;
	public GameObject yes, no, toMenu;
	[Header("Speed")]
	public GameObject speedUp;
	public GameObject slowDown;

	[HideInInspector]
	public static bool gameEnded;

	void Awake(){
		Application.runInBackground = true; //Keep the game playing even if it loses focus

		gameEnded = false; //Need to do this on scene start or static variable will be preserved
		gameObject.GetComponent<activeBuffs> ().applyNodeBuffs(); //Apply node buffs at start
	}

	void Start(){
		//playerStats.current.printBuffs ();
		gameObject.GetComponent<activeBuffs> ().applyEconBuffs (); //Apply econ buffs at start but after Node buffs (can't change gameStats before they are created)
	}

	// Update is called once per frame
	void Update () {
		//Check framerate
		//Debug.Log (Mathf.FloorToInt(1.0f / Time.deltaTime));
		if (gameEnded)
			return;

		//Debug Statement
		if(Input.GetKeyDown("e")){
			playerStats.current.addExpPerDiff (5000);
		}

		//Debug Statement
		if(Input.GetKeyDown("p")){
			if(Time.timeScale == 1.0f)
				Time.timeScale *= 3f;
		}

		//Debug Statement
		if(Input.GetKeyDown("o")){
	
		}

		if (gameStats.lives <= 0) {
			endGame ();
		}
	}

	void endGame(){
		waveSpawner.enemiesAlive = 0;
		gameEnded = true;
		gameoverUI.SetActive(true);

		//Save the players stats
		playerStats.current.addExpPerDiff (waveSpawner.getLastWave());
		playerStats.current.setLevel ();

		//IF the player is using a saved file, save it at that index, if not, don't save
		if(playerStats.saveIndex >= 0)
			SaveLoad.Save (playerStats.saveIndex);
	}
		

	//Confirm the user wants to use the menu
	public void openConfirmation(){
		yes.SetActive (true);
		no.SetActive (true);
		toMenu.SetActive (false);
	}

	//Close in the even that the user does not want to go to the menu
	public void closeConfirmation(){
		yes.SetActive (false);
		no.SetActive (false);
		toMenu.SetActive (true);
	}

	public void incSpeed(){
		speedUp.SetActive (false);
		slowDown.SetActive (true);
		Time.timeScale *= 3f;
	}

	public void decSpeed(){
		speedUp.SetActive (true);
		slowDown.SetActive (false);
		Time.timeScale /= 3f;
	}
		


}
