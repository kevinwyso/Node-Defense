using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Gameover : MonoBehaviour {

	public Text numWaves;
	public Text expGained;

	public GameObject yes, no, toMenu;

	//Set the number of waves reached in the gameover screen
	void OnEnable(){
		numWaves.text = gameStats.waves.ToString();
		expGained.text = Mathf.CeilToInt(gameStats.waves * gameStats.enemyDiff * gameStats.mapDiff).ToString ();
	}

	//Reload level
	public void Retry(){
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}

	//Go to the main menu
	public void ToMenu(){
		SceneManager.LoadScene(0);
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

}
