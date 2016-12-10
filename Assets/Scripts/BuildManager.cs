using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour {

	public static BuildManager instance; //Singleton for use in other classes

	[Header ("Setup")]
	public GameObject buildEffect;
	public NodeUI nodeUI;
	public Text message;

	[HideInInspector]
	//Node skill booleans, if true then this skill has been activated
	public bool basicDiscount, sellIncrease, upgradeDiscount;

	private Node selectedNode; // Holds most recently clicked node
	private towerBlueprint towerToBuild; //Current tower selected
	private GameObject towerGO; //Tower list GO
	private GameObject garbageGO; //Garbage List GO
	private GameObject masterGO; //Master GO (holds activeBuffs)
	private activeBuffs aB;

	//PROPERTIES : Only GETS something
	public bool isSelected { get {return towerToBuild != null;} }  //Is there a tower currently selected?
	public bool hasMoney { get {return gameStats.money >= towerToBuild.cost;}} //Does the player have enough money to buy the tower they chose?
	public bool hasMoneySpecial { get {return gameStats.money >= Mathf.Ceil(towerToBuild.cost / 2);}} //Does the player have enough money to buy the tower they chose?

	//Happens before start
	void Awake(){
		if (instance != null) { //Error check
			Debug.Log ("More than one instance in scene!");
			return;
		}
		instance = this; //Sets the build manager to a static variable which can be accessed from anywhere

		towerGO = GameObject.Find("Towers"); //Find/Set the Towers GO List
		garbageGO = GameObject.Find("Garbage"); //Find/Set the Garbage GO List
		masterGO = GameObject.Find("Game Master"); //Find/Set game master Object
		aB = masterGO.GetComponent<activeBuffs>();

		setMessage(" ");
	}

	void Update(){
		
		//Deselect a node when pressing Escape
		if(Input.GetKeyDown(KeyCode.Escape)){
			if (selectedNode != null)
				deselectNode ();
			else if (towerToBuild != null)
				towerToBuild = null;
		}

	}

	//Called from the Shop, sets the tower to be ready to build
	public void selectTowerToBuild(towerBlueprint Tower){
		towerToBuild = Tower; //Tower to build is the passed in tower from the shop
		deselectNode (); //Deselects any nodes currently selected to allow clean building
	}

	//Called from Node, builds the tower on the Node passed in
	public void buildTowerOn(Node node){
		bool specialCost = false; //boolean to check if tower is specialCost
		int tempSpent = 0; //Help store 'spent on this tower' info
		int tempKills = 0;
		float tempCountdown = 0;
		List<Transform> tempEnemyList = new List<Transform>();


		//If the tower is basic and will be built on a special node, then it is specialCost
		if (towerToBuild.prefab.GetComponent<Tower> ().towerTier == 1 && node.isSpecial)
			specialCost = true;

		//Check if the player has enough money to build the selected tower
		if ((gameStats.money < towerToBuild.cost && specialCost == false)|| (specialCost == true && gameStats.money < Mathf.CeilToInt(towerToBuild.cost / 2))) {
			setMessage( "Not enough money to build this tower!");
			return;
		}
			
		//Tower is going to be built, check if being upgraded after
		setMessage("Tower built!");

		//Upgrading: Destroy old tower and replace with new, set sell for cost temp, if upgrading set message to say so
		if (node.towerHere != null) { 
			tempSpent = node.t.spentOnThisTower;
			tempKills = node.t.killCount;
			tempCountdown = node.t.getCountdown ();
			tempEnemyList = node.t.getEnemyList ();
			Destroy (node.towerHere);
			setMessage("Tower Upgraded!");
		}

		//Build the tower and set up the nodes fields
		GameObject tower = (GameObject) Instantiate (towerToBuild.prefab, node.getBuildPosition (), Quaternion.identity);
		Tower t = tower.GetComponent<Tower> ();
		if(t.projectilePF != null)
			t.setDefaults (t.projectilePF.GetComponent<Projectile>()); //Set default stats (before applying skills)
		
		t.activateSkills (aB, node); //Activate buffs on this tower
		t.killCount = tempKills; //Preserve the killcount from the last tower
		t.setCountdown (tempCountdown); //Save the firecountdown from the last tower to avoid AA reset for upgraded tower
		t.setEnemyList(tempEnemyList); //Save the set of enemies in range so it doesn't do weird targetting

		setupNode (node, tower, t);


		//If the tower costed money apply this tower's stats/players money
		if (!specialCost) {
			gameStats.money -= towerToBuild.cost; 
			node.t.spentOnThisTower = tempSpent + towerToBuild.cost; //Update the sell for cost
		} else {
			//On a special node
			gameStats.money -= Mathf.CeilToInt(towerToBuild.cost / 2);
			node.t.spentOnThisTower = tempSpent + Mathf.CeilToInt(towerToBuild.cost / 2); //Update the sell for cost
		}

		applyUpgradeDiscount (t);

		//Keep the node selected after building
		selectNode (node);

		//Build effect
		GameObject effect = (GameObject) Instantiate (buildEffect, node.getBuildPosition (), Quaternion.identity); //Create Build Effect
		effect.transform.SetParent (garbageGO.transform); //Put effect in garbage GO list
		tower.transform.SetParent (towerGO.transform); //Puts the tower in the Tower GO list
		Destroy (effect, 5f); //Destroy build effect
	}

	/*
	* NODE SELECTION FUNCTIONS
	*/

	//Called from Node, selected node set to clicked node
	public void selectNode(Node node){
		//Disable old node's range indicator
		if(selectedNode != null)
			selectedNode.towerHere.GetComponent<Tower> ().rangeIndicator.SetActive (false);

		//Get current node's tower
		Tower t = node.towerHere.GetComponent<Tower>();

		//Deselect Node if clicking the same node with the UI already showing
		if (selectedNode == node) {
			deselectNode ();
			towerToBuild = null;
			return;
		}

		selectedNode = node; //Set the selected node to the node that has been clicked
		towerToBuild = null; //Keep the selected objects separate

		//Set the NodeUI active on this node
		nodeUI.hideUpgrades ();
		nodeUI.setTarget (node);


		//Show the range indicator for this tower
		t.rangeIndicator.SetActive(true);
	}

	//Deselect the current node
	public void deselectNode(){
		if (selectedNode != null)
			selectedNode.towerHere.GetComponent<Tower> ().rangeIndicator.SetActive (false); //Disable old node's range indicator
		selectedNode = null; //Deselect
		nodeUI.hideUI (); //Hide the UI
		nodeUI.hideStats();
	}

	/*
	* UPGRADE/SELL/COST FUNCTIONS
	*/

	//Upgrade the tower to the next iteration
	public void Upgrade(Node node, towerBlueprint upgradeTo){
		towerToBuild = upgradeTo;

		buildTowerOn (node); //Build the tower on this node

		//node.t = node.towerHere.GetComponent<Tower> ();//Set the nodes tower script to the new tower
		towerToBuild = null;
		selectNode (node); //Keep the tower selected after upgrading
	}

	//Sell the tower for half of what was spent on it
	public void Sell(Node node){
		if(!sellIncrease)
			gameStats.money += (int) Mathf.Ceil(node.t.spentOnThisTower * 0.5f); //Add money to player
		else
			gameStats.money += (int) Mathf.Ceil(node.t.spentOnThisTower * 0.75f); //Add money to player (75% instead of 50% with skill) 
		Destroy (node.towerHere);
		node.t = null;
		deselectNode ();
		setMessage("Tower Sold!");
	}
		
	//Apply discounts to the tower's upgrades upon successfully building
	void applyUpgradeDiscount(Tower t){
		if (upgradeDiscount) {
			t.upgradesTo1.reduceCost (aB.upgradeDiscountAmount);
			t.upgradesTo2.reduceCost (aB.upgradeDiscountAmount);
		}
	}

	/*
	* MISC FUNCTIONS
	*/

	void setupNode(Node node, GameObject tower, Tower t){
		node.towerHere = tower; //Sets the tower on the node to the tower built
		node.t = t; //Set tower component for Node
		//t.onNode = node; //Set which node this tower is built on
	}

	public towerBlueprint getTowerToBuild(){
		return towerToBuild;
	}

	void setMessage(string s){
		message.text = s;
	}


}
