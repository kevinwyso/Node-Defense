using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour {

	//KEY: 
	//deltaX = how much this value will change after some number of waves
	//baseX = what this value starts at in the beginning of any given game
	//waveX = What this value will be for this enemy at a given wave
	//xWaves = how many waves until the value is buffed
	[Header ("Speed")]
	public float deltaSpeed; 
	public float baseSpeed; 
	public float waveSpeed;
	public int speedWaves;
	[Space(10)]
	public bool impaired;
	[Header ("Health")]
	public float deltaHealth;
	public float baseHealth;
	public float waveHealth;
	public int healthWaves;
	[Space(10)]
	public bool heals; //Does this enemy self-heal?
	public float healAmount; //How much per second does it heal for
	[Header ("Worth")]
	public int deltaWorth;
	public int baseWorth;
	public int worthWaves;
	[Header ("Spawning Controls")]
	public int baseCount;
	public int deltaCount;
	public int waveCount;
	public int countWaves;
	public int whichWaves; //Determines on which multiple of this wave will the enemy spawn (i.e Default = 1 (Every wave) else if whichWave = 5, the enemy should only spawn on every multiple of 5)
	[Header ("Resist Controls")]
	public List<Resist> resistList = new List<Resist> ();
	public float baseResistStrength;
	public float deltaResistStrength;
	public float waveResistStrength;
	public int resistStrengthWaves;
	[Space(10)]
	public int baseResistCount; //How many resists does this enemy start with
	public int wavesToAddResistCount; //How many waves until another resist is added
	[Header ("Colors")]
	public Color baseColor;
	[Space(10)]
	public Color slowedColor;
	public Color hasBombColor;
	public Color poisonedColor;
	public Color burnColor;
	[Header ("Debuffs")]
	public List<debuff> debuffList  = new List<debuff> (); //Debuffs that this enemy possesses

	//[HideInInspector]
	//Current values for health/move speed/Worth throughout game, CHANGED WHILE ALIVE
	[Header ("Current Stats")]
	public float health;
	public float moveSpeed; 
	public float resistStrength;
	public int worth;
	public float distanceTravelled;

	private Tower mostRecentlyHitBy; //Which tower most recently hit this enemy
	private bool isConfused = false; //Is this enemy confused
	private bool isPentrated = false; //is this enemy's resists penetrated
	private float healthScale = 2; // Percentage of HP missing used to show how much health it has left 0 to 1
	private bool isDead = false; //Mutex lock for a dying enemy. Stop and enemy from dying more than once from a bullet
	private Renderer graphics;
	private GameObject garbageGO;

	[Header ("Setup")]
	public GameObject deathEffect; //Effect animation on death


	/*
	* UNITY FUNCTIONS
	*/

	//Init Function
	void Start(){ 
		//Set Current wave stats for this enemy
		moveSpeed =  waveSpeed = getSpeed();
		health = waveHealth = getHealth();
		worth = getWorth();
		waveCount = getCount ();
		resistStrength = waveResistStrength = getResistStrength ();

		//Assign Resists
		assignResists();

		//Set graphics and Get garbage GO
		graphics = gameObject.GetComponent<Renderer> ();
		garbageGO = GameObject.Find("Garbage");
	}
		
	/*
	* ENEMY DEATH/DAMAGE FUNCTIONS
	*/

	//Enemy takes the damage equal to amount (TODO: reduced by resists)
	public void takeDamage(float amount, Resist.resistType damageType, Tower hitBy){
		//Lockout for any other hit's on the same tick that would destroy this enemy
		//Debug.Log(amount);
		if (isDead)
			return;

		//Which tower most recently hit this enemy, used to give kills
		mostRecentlyHitBy = hitBy;

		//Set isPenetrated = to false to it will add a stack of penetration
		if(!hitBy.isLaser)
			if(hitBy.projectilePF.GetComponent<Projectile>().stacksPen)
				isPentrated = false;

		//Check if the enemy has a resist that matches the damageType coming in and set damage taken based on this amount
		Resist r = resistsThis(damageType);
		if (r != null){
			health -= (amount * (1-r.amount)); //subtract damage from hp based on resist
		} else{
			health -= amount; //otherwise just do damage normally
		}

		if (health <= 0) { //If no hp then die
			Die ();
		}

		//Ratio of current health to total health
		healthScale = Mathf.Clamp (health / waveHealth, 0.2f, 1f);

		transform.localScale = new Vector3 (healthScale, healthScale, healthScale); //Make enemy smaller based on hp left
		transform.GetComponent<SphereCollider>().radius = (1f / healthScale) * 0.5f; //Scale up the collider back to original size so it can be hit by collision more easily
	}


	//Kill the enemy and add to the users money
	void Die(){
		isDead = true; //Lock out the enemy from dying more than once at a time

		if (activeBuffs.luckyKill && !activeBuffs.luckyKillHigh) {
			
			if (rollChance (activeBuffs.luckyChance)) {
				gameStats.money += Mathf.FloorToInt (worth * activeBuffs.luckyChanceBonus);
			} else {
				gameStats.money += worth;
			}
		} else if (activeBuffs.luckyKill && activeBuffs.luckyKillHigh) {
			if (rollChance (activeBuffs.luckyChanceHigh)) {
				gameStats.money += Mathf.CeilToInt(worth * activeBuffs.luckyChanceBonusHigh);
			} else if (rollChance (activeBuffs.luckyChance)) {
				gameStats.money += Mathf.FloorToInt (worth * activeBuffs.luckyChanceBonus);
			} else
				gameStats.money += worth;
		} else {
			gameStats.money += worth;
		}
			
		waveSpawner.enemiesAlive--; //Reduce the enemies alive count
		waveSpawner.enemiesKilled++;
		mostRecentlyHitBy.killCount++;

		//If the enemy has a Bomb debuff then explode the bomb
		debuff d = hasDebuff("Bomb");
		if (d != null) {
			bombExplode (d);
		}

		//Show Death visually
		GameObject effect = (GameObject)Instantiate (deathEffect, transform.position, Quaternion.identity); //Death effect
		effect.transform.SetParent(garbageGO.transform); //Put effect in Garbage

		Destroy (effect, 5f); //Destroy effect
		Destroy (gameObject); //Destroy Enemy	


	}

	/*
	* DEBUFF FUNCTIONS
	*/

	//Check if this enemy has more than one debuff
	public bool hasOtherDebuffs { get {return debuffList.Count > 1;} }

	//Apply the debuff's stat effects
	public void doDebuff(debuff d){
		switch (d.toAffect){
		case debuff.Stat.HEALTH: //used for poisons
			takeDamage (d.amount * Time.deltaTime, Resist.resistType.SPELL, d.appliedBy);
			graphics.material.color = burnColor;
			break;
		case debuff.Stat.HEALTHPERCCURR:
			takeDamage (d.amount * health * Time.deltaTime, Resist.resistType.SPELL, d.appliedBy);
			graphics.material.color = poisonedColor;
			break;
		case debuff.Stat.HEALTHPERCMAX:
			takeDamage (d.amount * waveHealth * Time.deltaTime, Resist.resistType.SPELL, d.appliedBy);
			graphics.material.color = poisonedColor;
			break;
		case debuff.Stat.MOVESPEED: //used for slow/stuns
			Slow(d.amount);
			break;
		case debuff.Stat.WORTH: //Maybe for a "lucky" tower?
			worth += (int)d.amount;
			break;
		case debuff.Stat.BOMB:
			if (!hasOtherDebuffs)
				graphics.material.color = hasBombColor;
			break;
		case debuff.Stat.LIMP:
			float r = UnityEngine.Random.value;
			if (r >= 1 - d.amount) //if r > limp chance
				apply(debuff.stunDebuff(1f, d.appliedBy));
			break;
		case debuff.Stat.CONFUSE:
			//If not already confused, offer a chance for the enemy's waypoint to be set to the previous one before allowing it to continue forward
			if (!isConfused) {
				float r2 = UnityEngine.Random.value;
				if (r2 >= 1 - d.amount) {
					gameObject.GetComponent<enemyUpdate> ().setPreviousWaypoint ();
					isConfused = true;
				}
			}
			break;
		case debuff.Stat.PENETRATION:
			if (!isPentrated) {
				decreaseResists (d.amount);
				isPentrated = true;
			}
			break;
		case debuff.Stat.PENETRATIONSTACK:
			if (!isPentrated) {
				debuff penDebuff = hasDebuffExactly ("Resist Reduction Stacking");
				if (penDebuff != null) {
					penDebuff.amount = Mathf.Clamp(penDebuff.amount + d.amount, 0, 1);
				}
				decreaseResists (penDebuff.amount);
				isPentrated = true;
			}
			break;
		}
	}

	//Add the debuff from the tower to the debuff list
	public void apply(debuff _debuff){
		//Check if this debuff is already in the list, if it is and it is of equal or stronger value, reapply it
		for(int i = debuffList.Count - 1; i >= 0; i--) {
			debuff d = debuffList [i];
			if (d.tag.Equals (_debuff.tag)) {
				if (d.amount <= _debuff.amount) {
					debuffList.Remove (d);
					debuffList.Add (new debuff (_debuff)); //Add a copy of the tower's debuff onto this debuffList
					return;
				} else //If the new incoming buff is not stronger then don't do anything
					return;
			}
		}
		debuffList.Add (new debuff(_debuff)); //Add a copy of the tower's debuff onto this debuffList
	}
	

	//Remove the debuff d from the debuffList
	public void removeDebuff(debuff d){
		debuffList.Remove (d);
	}

	//Does the enemy debuff list contain a debuff with this name?
	public debuff hasDebuff(string s){
		for(int i = debuffList.Count-1; i >= 0; i--) {
			if (debuffList [i].tag.Contains(s))
				return debuffList[i];
		}
		return null;
	}

	//Does the enemy debuff list contain a debuff with EXACTLY this name?
	public debuff hasDebuffExactly(string s){
		for(int i = debuffList.Count-1; i >= 0; i--) {
			if (debuffList [i].tag.Equals(s))
				return debuffList[i];
		}
		return null;
	}
		

	//Explode the bomb when an enemy dies
	public void bombExplode(debuff d){
		Collider[] colliders = Physics.OverlapSphere (transform.position, d.radius);

		foreach (Collider c in colliders) {
			if (c.tag == "Enemy") {
				Enemy e = c.GetComponent<Enemy> ();
				//If the bomb is of type chain, reapply the bomb to enemies hit by this bomb
				if (d.tag == "Bomb Chain") {
					e.apply (d);
				} else if (d.tag == "Burn Bomb") {
					debuff burnDebuff = hasDebuffExactly ("Burn");
					e.apply (burnDebuff);
				}
				e.takeDamage (d.amount, Resist.resistType.EXPLOSION, d.appliedBy); //Do the bomb's damage to these enemies in the radius
			}
			
		}
	}

	//Slow the enemy
	void Slow(float percent){
		moveSpeed = waveSpeed * (1f - percent);
		graphics.material.color = slowedColor;
	}
		
	/*
	* RESIST FUNCTIONS
	*/

	//Assign the resists for this enemy
	void assignResists(){
		for (int i = 0; i < getNumResists(); i++) {
			addResist (chooseRandomResist ());
		}
	}

	//Add a resist to the list of this enemys resists
	void addResist(Resist resist){
		if(!hasThisResist(resist))
			resistList.Add (resist);
	}

	//Decrease the resist strength of resists on this enemy
	void decreaseResists(float amount){
		foreach (Resist r in resistList) {
			r.amount = waveResistStrength * (1 - amount);
		}
	}

	//Check if an enemy has the passed resist
	bool hasThisResist(Resist resist){
		foreach (Resist r in resistList) {
			if (r.type == resist.type)
				return true;
		}
		return false;
	}

	//Picks a random resist
	public Resist chooseRandomResist(){
		//Choose one of the 5 resists at random and create a resist based on the current resist strength of the wave (//Exclusive)
		return new Resist((Resist.resistType)UnityEngine.Random.Range (1, 6), getResistStrength());
	}

	//Check if the enemy resists this incoming damage type
	public Resist resistsThis(Resist.resistType incomingDamage){
		foreach (Resist resist in resistList) {
			if (resist.type == incomingDamage)
				return resist;
		}
		return null;
	}

	/*
	* GET/SET ATTRIBUTE FUNCTIONS
	*/

	//Function for changing stats for an enemy by X after buffAfter amount of waves
	float waveDeltaFn(float x, int waves){
		return Mathf.Ceil (gameStats.waves / (waves)) * x;
	}

	public int getCount(){
		if (gameStats.waves % whichWaves == 0)
			return baseCount + (int)waveDeltaFn ((float)deltaCount, countWaves);
		else
			return 0;
	}
		
	public float getSpeed(){
		return baseSpeed + waveDeltaFn (deltaSpeed * (gameStats.enemyDiffDelta / 2f), speedWaves); //Difficulty affects the speed less, don't want enemies moving TOO fast
	}

	public float getHealth(){
		return baseHealth + waveDeltaFn (deltaHealth * gameStats.enemyDiffDelta, healthWaves); //Difficulty affects health 1:1
	}

	public int getWorth(){
		int w = (baseWorth + (int)waveDeltaFn((float)deltaWorth * gameStats.enemyDiffDelta , worthWaves)); //Difficulty affects worth 1:1 
		return  w + (int) (w * Mathf.Max(activeBuffs.enemyBonusPerc,activeBuffs.enemyBonusPercHigh));
	}
		
	public float getResistStrength(){
		return Mathf.Clamp(baseResistStrength + waveDeltaFn (deltaResistStrength * gameStats.enemyDiffDelta, resistStrengthWaves), 0f, 0.90f); //Difficulty affects resists 1:1, resists should never be >= 90%
	}

	public int getNumResists(){
		return baseResistCount + (int)Mathf.Ceil(gameStats.waves / wavesToAddResistCount);
	}

	public void setConfused(bool b){
		isConfused = b;
	}
		
	public void selfHeal(){
		if (health < waveHealth)
			health = Mathf.Clamp (health + (healAmount * Time.deltaTime), 0, waveHealth);
	}

	/*
	* MISC FUNCTIONS
	*/

	bool rollChance(float chance){
		float r = UnityEngine.Random.value;
		//Debug.Log(UnityEngine.Random.value);
		return r < chance;
	}

	public float CompareTo(Enemy other){
		return this.distanceTravelled.CompareTo (other.distanceTravelled);
	}

	/*
	* GUI FUNCTIONS
	*/

	void OnMouseDown(){
		setTarget ();
	}

	void setTarget(){
		enemyGUI.instance.setTarget (this);
	}

}
