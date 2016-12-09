using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class waveSpawner : MonoBehaviour {

	public static int enemiesAlive;
	public static int totalEnemiesSpawned;
	public static int enemiesKilled;
	public static List<wave> waves;

	[Header ("Timer Attributes")]
	public float waveTimer; //Time between waves
	public float countdown; //Visible time to player, initial value is wait time before first wave
	public float enemyTimer; //Time between spawning individual enemies
	public float earlyStartBonus; //How much additional money will the player get for starting a round early?
	[Header ("Enemy Count Attributes")]
	public int baseEnemies; //Base number of enemies to spawn on round 1
	public int wavesToAddEnemies; //How many waves until more enemies will be added to the wave
	public int enemiesDelta; //Number of enemies to add to the previous wave
	[Header ("Enemy Object Attributes")]
	public Transform spawnPoint; //Where enemies will spawn
	public Enemy[] enemyPrefabs; //Enemy to spawn
	[Header ("Setup")]
	public Text countdownText; //Text to show to user
	public Text waveNumberText; // Show next wave number to user
	public Button nextWave; //Press this to start the next wave

	private List<Transform> enemyList = new List<Transform>(); //List of enemies to be spawned
	private List<int> enemyCounts = new List<int>(); //Count of each type of enemy
	private int totalCount = 0;

	private bool spawning = false; //Are enemies spawning? (sort of mutex lock for spawners) Needed to stop the countdown while spawning
	private int waveNumber = 0; //Which wave the player is on
	private GameObject enemyGO; //Gameobject which holds the list of enemies

	//Init
	void Start(){
		waves = new List<wave> ();
		totalEnemiesSpawned = 0; //Reset enemies spawned count
		enemiesKilled = 0; //Reset Enemies killed count
		enemiesAlive = 0; //Reset enemies alive before the game starts just as a failsafe
		enemyGO = GameObject.Find ("Enemies"); //Find/Set the Gameobject which will store the enemy list
	}

	//Every frame
	void Update(){
		//If countdown hits 0 spawn another wave
		if (countdown <= 0f && !spawning) {
			StartCoroutine (spawnWave ());
		}

		//Countdown and Wave Number Labels
		countdownText.text = string.Format ("{0:00.00}", countdown); //Show the countdown text as an int even though it's a float (Floor it)
		waveNumberText.text = "Next Wave (" + (waveNumber+1) + ")";
	
		//If spawning, disable GO button
		if (spawning)
			nextWave.interactable = false;
		else
			nextWave.interactable = true;

		//Remove waves from the waves list
		cleanUpWaves ();

		//If there are still enemies on the field, don't start counting down
		if (enemiesAlive > 0)
			return;

		//If enemies are not spawning, subtract from the timer
		if(!spawning)
			countdown -= Time.deltaTime; //Subtract one second
		countdown = Mathf.Clamp (countdown, 0f, Mathf.Infinity); //Dont show neg numbers

	}

	//Allow the user to spawn enemies by pressing the GO button (only if the countdown hasn't already reached 0 and there isn't currently a wave spawning
	public void spawnFromButton(){
		if(countdown >= 0f && !spawning)
			StartCoroutine (spawnWave ());
		gameStats.money += (int) (earlyStartBonus * countdown * waveNumber) + 10; //Grant the player bonus money for starting a wave early
	}

	//Spawn a wave
	IEnumerator spawnWave(){
		spawning = true;
		waveNumber++; // Increase number of enemies spawning per wave
		gameStats.waves++;

		//For each enemy in the list, get its count for this wave and put in count list
		foreach (Enemy enemy in enemyPrefabs){
			totalCount += enemy.getCount ();
			enemyCounts.Add (enemy.getCount ());
		}

		//Order the enemies in some random grouping fashion
		orderEnemies ();

		waves.Add(new wave(waveNumber, enemyList.Count));

		//Spawn the enemies in the wave
		foreach (Transform enemy in enemyList) {
			spawnEnemy (enemy);
			yield return new WaitForSeconds (enemyTimer); // Wait some time between when individual enemies will within a wave
		}

		//Reset the list for the next wave
		enemyList = new List<Transform> ();

		//Reset the countdown after a wave has spawned and give up the spawning lock
		countdown = waveTimer;
		spawning = false;

		//Increase enemy density each round to a minimum of 0.03
		enemyTimer = Mathf.Clamp((enemyTimer - 0.003f), 0.03f, 0.5f);

	}

	//Spawn an enemy at the spawn point
	void spawnEnemy(Transform enemyPrefab){
		Transform e = (Transform) Instantiate (enemyPrefab.transform, spawnPoint.position, spawnPoint.rotation); //Spawn the enemy
		e.parent = enemyGO.transform; //Put the enemy in the enemy list Gameobject
		enemiesAlive++; //Add to the total number of enemies alive
	}

	//Set the order to spawn the enemies for this wave
	void orderEnemies(){
		while (totalCount > 0) {
			int randomType = UnityEngine.Random.Range (0, enemyPrefabs.Length); //select a random Type
			if (enemyCounts [randomType] == 0) {//If this type has no enemies left to spawn then pick another type
				continue;
			}
			
			//Select a random number of enemies between 1 and the Minimum of 5 and How many enemies are left to spawn for that type
			int randomCount = UnityEngine.Random.Range(1,Mathf.Min(4, enemyCounts[randomType]+1)); 

			//Add "RandomCount" enemies from the "randomType" prefab
			for (int i = 0; i < randomCount; i++) {
				enemyList.Add (enemyPrefabs [randomType].transform);
			}

			//Subtract from counts
			enemyCounts [randomType] -= randomCount; 
			totalCount -= randomCount;
		}

		//Reset Counts for next wave
		enemyCounts = new List<int> ();
		totalCount = 0;
	}
		
	//Remove waves from the wave list if the correct amount of enemies have been killed
	void cleanUpWaves(){
		for(int i = waves.Count-1; i >= 0; i--) {
			wave w = waves [i];
			//print (w.ToString());
			if (enemiesKilled >= w.getSpawned ())
				waves.Remove (w);
		}
	}

	//Return the most recently cleared wave
	public static int getLastWave(){
		return waveSpawner.waves [0].getWaveNumber();
	}

}
