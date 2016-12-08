using UnityEngine;
using System.Collections;

public class gameStats : MonoBehaviour {

	//In game stats
	public static int money, lives, waves;
	public static float enemyDiff, mapDiff;
	public static int enemyDiffDelta, mapDiffDelta; //Difficulties converted to an int for map selection and wave deltas

	[Header ("Player Attributes")]
	public int startMoney;
	[Space(10)]
	public int startLives;

	void Start(){
		money = startMoney;
		lives = startLives;
		waves = 0;
	}

	//Set difficulty from menu
	public static void addStartMoney (int amount) { money += amount; } //Add money to the start (if econ skill activated)


	//Map difficulty: Inherently increases difficulty based on the path layout (shorter/harder to overlap tower range with paths)
	public static void setMapDiff (float diff) { 
		mapDiff = diff; 

		//Set the index value of the map diff
		if(diff == diffMenu.easyMapMult)
			mapDiffDelta = 1;
		else if (diff == diffMenu.medMapMult)
			mapDiffDelta = 2;
		else if (diff == diffMenu.hardMapMult)
			mapDiffDelta = 3;

	} 

	//Difficulty ranges from 0-2. Going to subtract difficulty amount from wavesBeforeBuff stats on enemy for increased difficulty
	public static void setEnemyDiff (float diff) { 
		enemyDiff = diff; 

		//Set the index value of the enemy diff
		if(diff == diffMenu.easyEnemyMult)
			enemyDiffDelta = 0;
		else if (diff == diffMenu.medEnemyMult)
			enemyDiffDelta = 1;
		else if (diff == diffMenu.hardEnemyMult)
			enemyDiffDelta = 2;

	} 
}
