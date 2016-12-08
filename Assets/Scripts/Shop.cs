using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour {

	BuildManager buildManager;

	[Header ("Blueprints")]
	public towerBlueprint[] basicTowers;

	private GameObject masterGO;
	private activeBuffs aB;

	void Start(){
		//Setup Objects for Shop
		masterGO = GameObject.Find ("Game Master");
		aB = masterGO.GetComponent<activeBuffs> ();
		buildManager = BuildManager.instance;

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


}
