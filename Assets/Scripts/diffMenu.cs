using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class diffMenu : MonoBehaviour {

	public enum Difficulties {EASY, MEDIUM, HARD, TORTURE, HELL, IMPOSSIBLE} 

	//Multiplier values for experience gains after a game ends
	public static float easyEnemyMult = 0.6f, easyMapMult = 0.9f;
	public static float medEnemyMult = 1f, medMapMult = 1f;
	public static float hardEnemyMult = 1.3f, hardMapMult = 1.2f;
	public static float tortureEnemyMult = 1.5f, tortureMapMult = 1.4f;
	public static float hellEnemyMult = 1.7f, hellMapMult = 1.6f;
	public static float impossibleEnemyMult = 2f, impossibleMapMult = 2f;

	[Header ("Buttons")]
	public Button start;
	public Button medEnemy, medMap;
	public Button hardEnemy, hardMap;

	[Header ("Text")]
	public Text bonusText;

	[Header ("Values")]
	public int medUnlock;
	public int hardUnlock;
	public int tortureUnlock;
	public int hellUnlock;
	public int impossibleUnlock;

	//private int bonusVal = ;

	void Update(){
		//Show experience bonus percentage
		if (gameStats.enemyDiff >= 0 && gameStats.mapDiff >= 0)
			bonusText.text = (gameStats.enemyDiff * gameStats.mapDiff * 100).ToString () + "%";
		else
			bonusText.text = "0%";

		//Don't allow the player to start unless they've selected a difficulty/map yet
		if (gameStats.enemyDiff >= 0 && gameStats.mapDiff >= 0) {
			start.enabled = true;
		} else {
			start.enabled = false;
		}

		//Unlocks medium and hard difficulties at certain levels
		if (playerStats.current.getLevel () >= hardUnlock) {
			medEnemy.interactable = true;
			medMap.interactable = true;
			hardEnemy.interactable = true;
			hardMap.interactable = true;
		} else if (playerStats.current.getLevel () >= medUnlock) {
			medEnemy.interactable = true;
			medMap.interactable = true;
			hardEnemy.interactable = false;
			hardMap.interactable = false;
		} else {
			medEnemy.interactable = false;
			medMap.interactable = false;
			hardEnemy.interactable = false;
			hardMap.interactable = false;
		}
	}

	public void startGame(){
		SceneManager.LoadScene (gameStats.mapDiffDelta);
	}

	public void changeMap(float diff){
		gameStats.setMapDiff(diff);
	}

	public void setEnemyDiff(float diff){
		gameStats.setEnemyDiff (diff);
	}

}
