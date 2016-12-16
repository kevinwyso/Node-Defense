using UnityEngine;
using System.Collections;

public class gameStats : MonoBehaviour {

	//In game stats
	public static int money, lives, waves;
	public static float enemyDiff, mapDiff;
	public static int mapDiffDelta; //Difficulties converted to an int for map selection and wave deltas
	public static float enemyDiffDelta;

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
		else if (diff == diffMenu.tortureMapMult)
			mapDiffDelta = 4;
		else if (diff == diffMenu.hellMapMult)
			mapDiffDelta = 5;
		else if (diff == diffMenu.impossibleMapMult)
			mapDiffDelta = 6;

	} 

	//Difficulty ranges from 1-2. (affects how big the changes in stats are)
	public static void setEnemyDiff (float diff) { 
		enemyDiff = diff; 

		//Set the index value of the enemy diff
		if(diff == diffMenu.easyEnemyMult)
			enemyDiffDelta = 1f;
		else if (diff == diffMenu.medEnemyMult)
			enemyDiffDelta = 1.2f;
		else if (diff == diffMenu.hardEnemyMult)
			enemyDiffDelta = 1.4f;
		else if (diff == diffMenu.tortureEnemyMult)
			enemyDiffDelta = 1.6f;
		else if (diff == diffMenu.hellEnemyMult)
			enemyDiffDelta = 1.8f;
		else if (diff == diffMenu.impossibleEnemyMult)
			enemyDiffDelta = 2f;

	} 
}
