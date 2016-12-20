using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Shop : MonoBehaviour {

	BuildManager buildManager;

	[Header ("Blueprints")]
	public towerBlueprint[] basicTowers;

	[Header ("Prestige Towers")]
	public GameObject fireButton;

	private GameObject masterGO;
	private activeBuffs aB;

	void Start(){
		//Setup Objects for Shop
		masterGO = GameObject.Find ("Game Master");
		aB = masterGO.GetComponent<activeBuffs> ();
		buildManager = BuildManager.instance;

		if (playerStats.current.getPrestige () <= 1)
			fireButton.SetActive (false);

		//Set labels in the shop for cost
		foreach (towerBlueprint t in basicTowers) {
			t.cost = t.baseCost;
			//Reduce cost if the skill is active from Node Skills
			if (buildManager.basicDiscount) {
				t.reduceCost (aB.basicDiscountAmount);
			}
			t.setCostLabel ();
		}

	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad0))
			selectTurretTower();
		else if(Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad1))
			selectRocketTower();
		else if(Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad2))
			selectLaserTower();
		else if(Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad3))
			selectSniperTower();

	}

	//Select the turret tower 
	public void selectTurretTower(){
		buildManager.selectTowerToBuild (basicTowers[0]);
	}

	//Select the rocket tower
	public void selectRocketTower(){
		buildManager.selectTowerToBuild (basicTowers[1]);
	}

	//Select the laser tower
	public void selectLaserTower(){
		buildManager.selectTowerToBuild (basicTowers[2]);
	}

	//Select the sniper tower
	public void selectSniperTower(){
		buildManager.selectTowerToBuild (basicTowers[3]);
	}

	public void selectFireTower(){
		buildManager.selectTowerToBuild (basicTowers [4]);
	}

}
