using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class activeBuffs : MonoBehaviour{

	[Header ("Turret Skills")]
	//Stat Bonuses for turrets
	public float turretDamageBonus;
	public float turretRangeBonus;

	[Header ("Rocket Skills")]
	//Stat Bonuses for Rockets
	public float rocketDamageBonus;
	public float rocketAOEBonus;

	[Header ("Laser Skills")]
	//Stat Bonuses for Lasers
	public float laserDamageBonus;
	public float laserSlowBonus; 
	public float laserSlowBonusHigh; //Second Tier slow bonus

	[Header ("Sniper Skills")]
	//Stat Bonuses for Snipers
	public float sniperDamageBonus;
	public float sniperROFBonus;

	[Header ("Node Skills")]
	public GameObject nodes; //Gameobject that holds list of nodes
	public List<Node> specialNodes = new List<Node>(); //Actual list of special nodes
	public int specialCount; //How many nodes should be special

	//Stat Bonuses for Nodes
	public float nodeDamageBonus;
	public float nodeRangeBonus;

	//Discount values for Nodes
	public float basicDiscountAmount = 0.1f;
	public float upgradeDiscountAmount = 0.1f;

	[Header ("Econ Skills")]
	//Bonus to start Money
	public int startBonus1;
	public int startBonus2;

	//Enemy Worth Percent Values
	public float bonusPerc;
	public float bonusPercHigh;

	//Values saved here for static use
	public static float enemyBonusPerc = 0; 
	public static float enemyBonusPercHigh = 0;

	//Values/Bools for lucky chance stat
	public static bool luckyKill; //True when first tier lucky kill is active
	public static bool luckyKillHigh; //True when second tier lucky kill is active
	public static float luckyChance = 0.1f; //10% of the kills will result in lucky tier 1 bonus
	public static float luckyChanceHigh = 0.05f; //0.5% chance that kill will result in a lucky tier 2 bonus
	public static float luckyChanceBonus = 1.5f; //Tier 1 bonus: 1.5*worth
	public static float luckyChanceBonusHigh = 5f; //Tier 2 bonus: 5*worth

	/*
	HOW BUFF ACTIVATION WORKS

	For each tree apply"TreeName"Buffs() is called.

	This calls each tree's set of skill methods.

	Each method checks the buff/skill matrix to see if it is active (has enough points)

	If the buff is activated in the menu does then it does its method before the game starts
	
	*/


	/*
	* Turret Buffs
	*/

	//Tower passed in, applies all buffs that are active according to the buff matrix
	public void applyTurretBuffs(Tower tower){
		bool[,] matrix = playerStats.current.getBuffMatrix ();

		turretTier2 (matrix, tower);
		turretBonusDamage (matrix, tower);
		turretTier3 (matrix, tower);
		turretBonusRange (matrix, tower);
		turretTier4 (matrix, tower);
		turretDoubleRoF (matrix, tower);
	}

	public void turretTier2(bool[,] matrix, Tower tower){
		if (matrix [0, 0]) {
			if (tower.towerTier == 1)
				tower.nextTierUnlocked = true;
		}
	}
		
	public void turretBonusDamage(bool[,] matrix, Tower tower){
		if (matrix [0, 1]) {
			tower.addBonusProjDamage(turretDamageBonus);
		}
	}

	public void turretTier3(bool[,] matrix, Tower tower){
		if (matrix [0, 2]) {
			if (tower.towerTier == 2)
				tower.nextTierUnlocked = true;
		}
	}

	public void turretBonusRange(bool[,] matrix, Tower tower){
		if (matrix [0, 3]) {
			tower.range += tower.range * turretRangeBonus;
		}
	}

	public void turretTier4(bool[,] matrix, Tower tower){
		if (matrix [0, 4]) {
			if (tower.towerTier == 3)
				tower.nextTierUnlocked = true;
		}
	}

	public void turretDoubleRoF(bool[,] matrix, Tower tower){
		if (matrix [0, 5]) {
			tower.fireRate *= 2;
		}
	}


	/*
	* Rocket Buffs
	*/

	public void applyRocketBuffs(Tower tower){
		bool[,] matrix = playerStats.current.getBuffMatrix ();

		rocketTier2 (matrix, tower);
		rocketBonusDamage (matrix, tower);
		rocketTier3 (matrix, tower);
		rocketBonusAOE (matrix, tower);
		rocketTier4 (matrix, tower);
		rocketIgnoreResists(matrix, tower);
	}

	public void rocketTier2(bool[,] matrix, Tower tower){
		if (matrix [1, 0]) {
			if (tower.towerTier == 1)
				tower.nextTierUnlocked = true;
		}
	}

	public void rocketBonusDamage(bool[,] matrix, Tower tower){
		if (matrix [1, 1]) {
			tower.addBonusProjDamage (rocketDamageBonus);
		}
	}

	public void rocketTier3(bool[,] matrix, Tower tower){
		if (matrix [1, 2]) {
			if (tower.towerTier == 2)
				tower.nextTierUnlocked = true;
		}
	}

	public void rocketBonusAOE(bool[,] matrix, Tower tower){
		if (matrix [1, 3]) {
			Projectile p = tower.projectilePF.GetComponent<Projectile> ();
			p.explosionRadius = p.baseExplosionRadius + (p.baseExplosionRadius * rocketAOEBonus);
		}
	}

	public void rocketTier4(bool[,] matrix, Tower tower){
		if (matrix [1, 4]) {
			if (tower.towerTier == 3)
				tower.nextTierUnlocked = true;
		}
	}

	public void rocketIgnoreResists(bool[,] matrix, Tower tower){
		if (matrix [1, 5]) {
			tower.damageType = Resist.resistType.NONE;
		}
	}

	/*
	* Laser Buffs
	*/


	public void applyLaserBuffs(Tower tower){
		bool[,] matrix = playerStats.current.getBuffMatrix ();

		laserTier2 (matrix, tower);
		laserBonusDamage (matrix, tower);
		laserTier3 (matrix, tower);
		laserBonusSlow (matrix, tower);
		laserTier4 (matrix, tower);
		laserBonusSlow2(matrix, tower);
	}

	public void laserTier2(bool[,] matrix, Tower tower){
		if (matrix [2, 0]) {
			if (tower.towerTier == 1)
				tower.nextTierUnlocked = true;
		}
	}

	public void laserBonusDamage(bool[,] matrix, Tower tower){
		if (matrix [2, 1]) {
			tower.bonusDOT = (tower.baseDOT * laserDamageBonus);
		}
	}

	public void laserTier3(bool[,] matrix, Tower tower){
		if (matrix [2, 2]) {
			if (tower.towerTier == 2)
				tower.nextTierUnlocked = true;
		}
	}

	public void laserBonusSlow(bool[,] matrix, Tower tower){
		if (matrix [2, 3]) {
			if (tower.hasDebuff ("Slow") != null)
				tower.hasDebuff("Slow").amount = tower.baseSlowAmount + (tower.baseSlowAmount * laserSlowBonus);
		}
	}

	public void laserTier4(bool[,] matrix, Tower tower){
		if (matrix [2, 4]) {
			if (tower.towerTier == 3)
				tower.nextTierUnlocked = true;
		}
	}

	public void laserBonusSlow2(bool[,] matrix, Tower tower){
		if (matrix [2, 5]) {
			if (tower.hasDebuff ("Slow") != null){
				debuff d = tower.hasDebuff ("Slow");
				d.amount = Mathf.Clamp(d.amount + (d.amount * laserSlowBonusHigh), 0f, 0.9f); //Double the slow but don't let it slow by more than 0.9
			}
		}
	}

	/*
	* Sniper Buffs
	*/

	public void applySniperBuffs(Tower tower){
		bool[,] matrix = playerStats.current.getBuffMatrix ();

		sniperTier2 (matrix, tower);
		sniperBonusDamage (matrix, tower);
		sniperTier3 (matrix, tower);
		sniperBonusRoF(matrix, tower);
		sniperTier4 (matrix, tower);
		sniperKillCounts(matrix, tower);
	}

	public void sniperTier2(bool[,] matrix, Tower tower){
		if (matrix [3, 0]) {
			if (tower.towerTier == 1)
				tower.nextTierUnlocked = true;
		}
	}

	public void sniperBonusDamage(bool[,] matrix, Tower tower){
		if (matrix [3, 1]) {
			tower.addBonusProjDamage(sniperDamageBonus);
		}
	}

	public void sniperTier3(bool[,] matrix, Tower tower){
		if (matrix [3, 2]) {
			if (tower.towerTier == 2)
				tower.nextTierUnlocked = true;
		}
	}

	public void sniperBonusRoF(bool[,] matrix, Tower tower){
		if (matrix [3, 3]) {
			tower.fireRate += tower.fireRate * sniperROFBonus;
		}
	}

	public void sniperTier4(bool[,] matrix, Tower tower){
		if (matrix [3, 4]) {
			if (tower.towerTier == 3)
				tower.nextTierUnlocked = true;
		}
	}

	public void sniperKillCounts(bool[,] matrix, Tower tower){
		if (matrix [3, 5]) {
			Projectile p = tower.projectilePF.GetComponent<Projectile> ();
			p.dealsKillCountDamage = true;
		}
	}


	/*
	* Econ Buffs
	*/
	public void applyEconBuffs(){
		bool[,] matrix = playerStats.current.getBuffMatrix ();

		bonusStart1 (matrix);
		bonusEnemyWorth1 (matrix);
		_luckyKill1 (matrix);
		bonusStart2 (matrix);
		bonusEnemyWorth2(matrix);
		_luckyKill2 (matrix);
	}

	public void bonusStart1(bool[,] matrix){
		if (matrix [4, 0]) {
			gameStats.addStartMoney (startBonus1);
		}
	}

	public void bonusEnemyWorth1(bool[,] matrix){
		if (matrix [4, 1])
			enemyBonusPerc = bonusPerc;
	}

	public void _luckyKill1(bool[,] matrix){
		if (matrix [4, 2]) {
			luckyKill = true;
		}
	}

	public void bonusStart2(bool[,] matrix){
		if (matrix [4, 3])
			gameStats.addStartMoney (startBonus2);
	}

	public void bonusEnemyWorth2(bool[,] matrix){
		if (matrix [4, 4])
			enemyBonusPercHigh = bonusPercHigh;
	}

	public void _luckyKill2(bool[,] matrix){
		if (matrix [4, 5])
			luckyKillHigh = true;
	}


	/*
	* Node Buffs
	*/

	public void applyNodeBuffs(){
		bool[,] matrix = playerStats.current.getBuffMatrix ();

		basicBuildingReduc (matrix);
		sellBuff (matrix);
		upgradeReduc (matrix);
		specialNodesFree (matrix);
		specialNodesRange (matrix);
		specialNodesDamage (matrix);
	}

	public void basicBuildingReduc(bool[,] matrix){
		if (matrix [5, 0]) {
			BuildManager.instance.basicDiscount = true;
		}
	}

	public void sellBuff(bool[,] matrix){
		if (matrix [5, 1]) {
			BuildManager.instance.sellIncrease = true;
		}
	}

	public void upgradeReduc(bool[,] matrix){
		if (matrix [5, 2]) {
			BuildManager.instance.upgradeDiscount = true;
		}
	}

	public void specialNodesFree(bool[,] matrix){
		if (matrix [5, 3]) {
			setSpecialNodes ();
		}
	}

	public void specialNodesRange(bool[,] matrix){
		if (matrix [5, 4]) {
			foreach (Node node in specialNodes) {
				node.bonusRange = true;
			}
		}
	}

	public void specialNodesDamage(bool[,] matrix){
		if (matrix [5, 5]) {
			foreach (Node node in specialNodes) {
				node.bonusDamage = true;
			}
		}

	}

	//Sets up the map to have special nodes on it
	public void setSpecialNodes(){
		Node[] Nodes = nodes.GetComponentsInChildren<Node>();

		for(int i = 0; i < specialCount; i++){
			int rand = UnityEngine.Random.Range (0, Nodes.Length - 1);

			//Choose a random node and if it hasn't already been chosen, set it to special and add the bonuses to the node
			if (!Nodes [rand].isSpecial) {
				specialNodes.Add (Nodes [rand]);
				Nodes [rand].isSpecial = true;
				Nodes [rand].bonusDamageAmount = nodeDamageBonus;
				Nodes [rand].bonusRangeAmount = nodeRangeBonus;
			}
			else
				i--;
		}

	}

}
