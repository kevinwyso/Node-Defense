using UnityEngine;
using System.Collections;

[System.Serializable]
public class wave {

	private int totalEnemiesSpawned;
	private int waveNumber;

	public wave(int wn, int enemiesToSpawn){
		totalEnemiesSpawned = waveSpawner.totalEnemiesSpawned + enemiesToSpawn;
		waveSpawner.totalEnemiesSpawned += enemiesToSpawn;
		waveNumber = wn;
	}

	public int getSpawned(){
		return totalEnemiesSpawned;
	}

	public int getWaveNumber(){
		return waveNumber;
	}

	public string ToString(){
		return "Wave Number: " + waveNumber + "\nSpawned Total: " + totalEnemiesSpawned;
	}

}
