using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Gameover : MonoBehaviour {

	public Text numWaves;
	public Text expGained;

	//Set the number of waves reached in the gameover screen
	void OnEnable(){
		numWaves.text = waveSpawner.getLastWave().ToString();
		if (waveSpawner.getLastWave () != 1)
			expGained.text = Mathf.CeilToInt (waveSpawner.getLastWave () * gameStats.enemyDiff * gameStats.mapDiff).ToString ();
		else
			expGained.text = "0";
	}

	//Reload level
	public void Retry(){
		Time.timeScale = 1f;
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}

	//Go to the main menu
	public void ToMenu(){
		Time.timeScale = 1f;
		SceneManager.LoadScene(0);
	}


}
