using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NodeUI : MonoBehaviour {

	BuildManager buildManager;

	[Header ("Setup")]
	public GameObject main;
	public GameObject ups;
	[Header ("Buttons")]
	public Button showUps;
	public Button hideUps;
	[Header ("Cost Labels")]
	public Text costUp1Label;
	public Text costUp2Label; 
	public Text sellForLabel;
	[Header ("Upgrade Labels")]
	public Text upgrade1Label;
	public Text upgrade2Label;
	[Header ("Stat Labels")]
	public Text range;
	public Text rof;
	public Text damage;
	public Text damageType;
	public Text description;
	public Text towerName;
	public Text killCount;

	private Node target;

	void Start(){
		buildManager = BuildManager.instance;

	}

	void Update(){
		//Update kill count while tower is selected
		if (target != null)
			killCount.text = target.towerHere.GetComponent<Tower> ().killCount.ToString ();

		if(buildManager.isSelected)
			showStats(buildManager.getTowerToBuild().prefab.GetComponent<Tower>());
	}

	//Set the target node for the UI to show up above, passed in from buildManager
	public void setTarget(Node t){
		target = t;
		Tower tower = target.towerHere.GetComponent<Tower> (); //Tower component of tower on this node

		main.SetActive(true);

		//If the tower on the selected node is of max tier then it can't be upgraded
		if (tower.upgradesTo1.cost == -1 || !tower.nextTierIsUnlocked())
			showUps.interactable = false;
		else
			showUps.interactable = true;

		//Show the values for upgrading/selling the tower
		costUp1Label.text = "$" + tower.upgradesTo1.cost;
		costUp2Label.text = "$" + tower.upgradesTo2.cost;
		upgrade1Label.text = tower.upgradesTo1.name;
		upgrade2Label.text = tower.upgradesTo2.name;
		if (!buildManager.sellIncrease)
			sellForLabel.text = "\n$" + (int) Mathf.Ceil(tower.spentOnThisTower * 0.5f);
		else
			sellForLabel.text = "\n$" + (int) Mathf.Ceil(tower.spentOnThisTower * 0.75f);

		//Show tower's stats on the side
		showStats(tower);

		//Put the UI above the targetNode
		transform.position = target.getBuildPosition ();
	}

	//Disable UI for user
	public void hideUI(){
		main.SetActive(false);
		ups.SetActive (false);
	}

	//Show range,rof,and damage stats of selected tower
	void showStats(Tower tower){
		range.text = tower.range.ToString();

		//If not a laser then show damage as flat not per second
		if (!tower.isLaser) {
			Projectile p = tower.projectilePF.GetComponent<Projectile> ();
			//Depending on damage type, change the text to express this
			float bonusDmg = (tower.getBonusProjDamage() * p.baseDamage);
			if (p.damageOp == Projectile.DamageOp.FLAT)
				damage.text = (p.damage + bonusDmg).ToString ();
			else if (p.damageOp == Projectile.DamageOp.PERCENTCURR)
				damage.text = (p.damage + bonusDmg).ToString () + "%Curr";
			else if (p.damageOp == Projectile.DamageOp.PERCENTMAX)
				damage.text = (p.damage + bonusDmg).ToString () + "%Max";
			rof.text = tower.fireRate.ToString ();
		}else{
			damage.text = (tower.DOT + tower.bonusDOT).ToString() + "/sec";
				rof.text = "DOT";
		}

		//Show rest of stats which don't depend on anything else
		towerName.text = tower.Name;
		description.text = tower.description;
		damageType.text = tower.damageType.ToString ();
		killCount.text = tower.killCount.ToString();
	}

	//Hide stats of the tower
	public void hideStats(){
		range.text = "0";
		rof.text = "0";
		damage.text = "0";
		towerName.text = "";
		description.text = "";
		damageType.text = "-";
		killCount.text = "0";
		target = null;
	}

	//Upgrade the tower at this node (First Opt)
	public void upgradeOption1(){
		buildManager.Upgrade (target, target.towerHere.GetComponent<Tower>().upgradesTo1);
	}

	//Upgrade the tower at this node (Second Opt)
	public void upgradeOption2(){
		buildManager.Upgrade (target, target.towerHere.GetComponent<Tower>().upgradesTo2);
	}

	//Show the UI for upgrades
	public void showUpgrades(){
		ups.SetActive (true);
		showUps.interactable = false;
	}

	//Hides the UI for upgrades
	public void hideUpgrades(){
		ups.SetActive (false);
		showUps.interactable = true;
	}

	//Sell the tower at this node
	public void sellTower(){
		buildManager.Sell (target);
	}
		


}
