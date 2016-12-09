using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tower : MonoBehaviour {

	[Header ("General Setup")]
	public string enemyTag = "Enemy"; //What tag to look for when seeking an enemy
	public Transform rotator; //Rotator GO on the tower
	public Transform firePoint; //Where the bullet should spawn from
	public towerBlueprint upgradesTo1; //Towers that this tower upgrades to (IF COST = -1 THEN DISABLES UPGRADING)
	public towerBlueprint upgradesTo2;
	[Space(10)]
	public int towerTier; //What tier is this tower in the upgrade hierarchy
	public string Name; //Tower's name
	public string description; //Tower's Description

	[Header ("Tower Attributes")]
	//public float baseRange; //Base range (to be increased by skills)
	public float range; //Tower range
	public GameObject rangeIndicator; //Sphere to show rangeIndicator
	public bool targetHighHealth; //Does this tower target enemies based on health?
	public Resist.resistType damageType; //DamageTYpe of this tower (reduced by matching resistance)

	//public Node onNode; //What node this tower is on

	[Header ("Uses Bullets")]
	public GameObject projectilePF; //Prefab of projectile from tower
	public float fireRate; //Fire rate of tower per second
	private float fireCountdown; //How long til next shot can shoot

	[Header ("For Multishot")]
	public bool multishot; //Does this tower have multishot capability
	public int multiCount; //Number of targets to shoot

	[Header ("For Shotgun")]
	public bool shotgun; //Is this tower a shotgun?
	public int shotgunBullets; //How many bullets in the shot

	[Header ("For Requiem")]
	public bool requiem; //Is this tower a Requiem Tower
	public GameObject muzzlePF; //Prefab for Muzzle flash animation

	[Header ("For Shotty Snipes")]
	public bool shottySnipes; //Is this a shotty snipe tower
	public float cutoffRange; //In what range will the bullets start being shot like a shotgun
	public GameObject shotgunBulletPF; //What bullet PF to switch to for shooting

	[Header ("For Piercing Bullets")]
	public bool pierces; //Does this tower's bullets pierce
	public int targetsToPierce; //How many targets to pierce
	public float falloff; //Falloff damage after a pierce

	[Header ("For Laser")]
	public bool isLaser;
	public float DOT; //Damage over time
	public float bonusDOT; //Damage added by skills
	public float baseDOT; //Starting value for this laser (Used with charging mechanic)
	public float baseDOTCharging;
	public float AOE; //Radius for AoE slowing/dmg
	public bool charges; //Does this laser use the charge mechanic
	public float chargesBy; //How much does the laser charge by each second
	public Projectile.DamageOp damageOp;

	[Header ("Laser Effects Setup")]
	public LineRenderer lineRenderer;
	public ParticleSystem impactEffect;
	public Light impactLight;

	[Header ("Tower Debuffs to Apply")]
	public float baseSlowAmount; //Base slow amount (to be increased by laser skills)
	public debuff[] debuffs;

	[HideInInspector]
	public int spentOnThisTower, killCount = 0;
	[HideInInspector]
	public bool nextTierUnlocked; //Tells the node ui to enable or disable the upgrade section 

	private float bonusDamageToDeal;
	private List<Transform> enemyList = new List<Transform> (); //List of enemies in range
	private Projectile p; //The projectile script attached to this tower's bullets
	private float turnSpeed = 15; //How fast the tower turns towards its target
	private GameObject garbageGO; //Garbage Object to put stuff in
	private activeBuffs aB;
	//private GameObject masterGO; //Master Object
	private Transform target; // Change to private after testing


	/* 
	* UNITY FUNCTIONS
	*/

	void Start(){
		InvokeRepeating ("updateTarget", 0f, 0.5f); // Last number is repeat rate (every x seconds), calls updateTarget every 0.5 seconds
		rangeIndicator.transform.localScale = new Vector3(range*2, 0, range*2); //Change the scale of the rangeIndicator (multiply by 2 because scale is from the center of one axis)

		//If not a laser, setup the projectile 
		if (projectilePF != null) { 
			p = projectilePF.GetComponent<Projectile> ();
			p.setTower (this);
		}

		//masterGO = GameObject.Find ("Game Master");
		garbageGO = GameObject.Find ("Garbage");

		//Set up the debuffs so they know this tower applied it
		foreach (debuff d in debuffs) {
			d.appliedBy = this;
		}
		
		//activateSkills (aB, ); //Apply skills
	}

	// Called once per frame
	void Update () {
		//If no target, don't move or shoot
		if (target == null) {
			//if no target then disable lineRenderer
			if (isLaser)
				if (lineRenderer.enabled) {
					lineRenderer.enabled = false;
					impactEffect.Stop ();
					impactLight.enabled = false;
					if(charges)
						DOT = baseDOTCharging; //Reset damage for a charging laser
				}

			fireCountdown -= Time.deltaTime; //Countdown is reduced by 1 per second even if there isn't a target
			return;
		}

		//Look at target, don't use lerp as a shotgun or piercer or else you get weird looking bullet paths
		if (shotgun || pierces)
			rotateToLookAtTarget (false);
		else
			rotateToLookAtTarget (true);

		//Check type of tower fire
		if (isLaser) {
			Laser ();
		} else {//Algorithm to simulate fire rate
			if (fireCountdown <= 0f) { //If the countdown reaches 0 then fire a new shot
				
				//Determine what type of tower is shooting and use that shot type
				if (multishot)
					Multishot (multiCount);
				else if (shotgun)
					shotgunShot (projectilePF);
				else if (requiem)
					Requiem ();
				else if (shottySnipes && distanceToTarget (target) <= cutoffRange) //Only shoot shotgun if within cutoff range
					shotgunShot (shotgunBulletPF);
				else if (pierces)
					piercingShot ();
				else
					Shoot ();
			}

			fireCountdown -= Time.deltaTime; //Countdown is reduced by 1 per second
		}

	}

	//Only drawn when tower is selected
	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red; // Change color of sphere
		Gizmos.DrawWireSphere (transform.position, range); // Draw a sphere to show range of tower
	}

	/* 
	* LASER FUNCTIONS
	*/

	//If the tower uses a laser functionality then use this instead of shoot
	void Laser(){
		//Similar to explode in projectile, check for overlapping enemies in the aoe radius and damage them
		if (AOE > 0) {
			Collider[] hits = Physics.OverlapSphere (target.position, AOE);
			foreach (Collider c in hits) {
				if (c.tag == "Enemy") {
					laserDamage (c.transform);
				}
			}
		} else { //Single target
			laserDamage (target);
		}

		//Show laser graphics
		laserGraphics ();
		//If the laser is of type charging increase its DOT 
		if (charges)
			DOT += chargesBy * Time.deltaTime;
	}

	//Deal damage as a laser
	void laserDamage(Transform enemy){
		float damageToDeal = DOT + bonusDOT;

		Enemy e = enemy.GetComponent<Enemy> (); //OPTIMIZE - Get enemy component only when damaging rather than everytime

		//Apply debuff before damaging
		foreach (debuff d in debuffs) {
			e.apply (d);
		}

		//Damage/Slow Enemy (based on damage type)
		if (damageOp == Projectile.DamageOp.FLAT)
			e.takeDamage (damageToDeal * Time.deltaTime, damageType, this); //Have enemy take damage every second
		else if (damageOp == Projectile.DamageOp.PERCENTCURR)
			e.takeDamage (damageToDeal * e.health * Time.deltaTime, damageType, this);
		else if (damageOp == Projectile.DamageOp.PERCENTMAX)
			e.takeDamage (damageToDeal * e.waveHealth * Time.deltaTime, damageType, this);
		
	}

	//Show the laser's graphics
	void laserGraphics(){
		
		//Enable the line Renderer and the effects
		if (!lineRenderer.enabled) {
			lineRenderer.enabled = true;
			impactEffect.Play();
			impactLight.enabled = true;
		}

		//Aim the line renderer from start to end point
		lineRenderer.SetPosition (0, firePoint.position); //Start point
		lineRenderer.SetPosition (1, target.position); //End point
		
		//Have PS aim back toward tower
		Vector3 dir = firePoint.position - target.position;
		impactEffect.transform.rotation = Quaternion.LookRotation (dir);
		impactEffect.transform.position = target.position + dir.normalized; //Put the PS on the enemy (and offset a bit)

	}
		
	/* 
	* SHOOTING FUNCTIONS
	*/

	//Insantiate the projectile object for this tower, 
	void Shoot (){
		GameObject projectileGO = (GameObject) Instantiate (projectilePF, firePoint.position, firePoint.rotation); //GameObject of this projectile (GO = gameobject)
		Projectile projectile = projectileGO.GetComponent<Projectile> (); //Get the Projectile script from this projectile
		projectileGO.transform.SetParent(gameObject.transform); // Put the projectile as this towers child

		//If there is a projectile then have it look for a target
		if (projectile != null)
			projectile.Seek (target);

		fireCountdown = 1f / fireRate; // Reset the countdown (Invert the function so that higher fireRate = faster, lower = slower: More intuitive)
	}

	//Shoot numTargets in range, if numTargets == -1 then shoot all targets
	void Multishot(int numTargets){
		int enemiesHit = 0;
		Collider[] colliders = Physics.OverlapSphere (transform.position, range); //Get colliders in range

		//If the numTargets is set to -1 then it should hit ALL targets in range
		if (numTargets == -1)
			numTargets = colliders.Length;

		for(int i = 0; enemiesHit < numTargets && i < colliders.Length; i++){
			//If collider is an enemy then spawn projectile for it and have it seek for its target
			//TODO make sure the closest enemies are chosen for the multishot
			if (colliders[i].tag == "Enemy") {
				GameObject projectileGO = (GameObject)Instantiate (projectilePF, firePoint.position, firePoint.rotation); //GameObject of this projectile (GO = gameobject)
				Projectile projectile = projectileGO.GetComponent<Projectile> (); //Get the Projectile script from this projectile
				projectileGO.transform.SetParent (gameObject.transform); // Put the projectile as this towers child

				//Bullets seek target
				if (projectile != null)
					projectile.Seek (colliders[i].transform);

				enemiesHit++; //Increment enemies hit so that we only iterate numTargets times
			}
		}

		fireCountdown = 1f / fireRate; //Reset countdown for shot

	}

	//Shoot like a shotgun, spawn a series of bullets and have them fan out forward from their initial spawn
	void shotgunShot(GameObject shotgunBullet){
		
		for (int i = 0; i < shotgunBullets; i++) {
			//Initialize a random vector for bullets to vary from the original spawn as well as a random rotation corresponding to this spawn point
			Vector3 randVector = (Random.insideUnitSphere);
			randVector = new Vector3 (randVector.x, 0, randVector.z);
			Quaternion randQuat = Quaternion.identity;
			randQuat.eulerAngles = new Vector3 (0, randVector.x * 20, 0); //Set direction based on spawn point

			//Spawn the bullets
			GameObject projectileGO = (GameObject)Instantiate (shotgunBullet, firePoint.position + randVector, firePoint.rotation * randQuat); //GameObject of this projectile (GO = gameobject)
			projectileGO.transform.SetParent (gameObject.transform); // Put the projectile as this towers child
		}

		fireCountdown = 1f / fireRate; //Reset countdown for shot
	}

	//Spawn a projectile hit particle for each Enemy on the map and damage them
	void Requiem(){
		//Create Explosion effect at the requiem tower
		GameObject muzzle = (GameObject) Instantiate (muzzlePF, transform.position + new Vector3 (0,4f,0), transform.rotation); //Spawn the impact particles a little higher than at base height
		muzzle.transform.SetParent(garbageGO.transform); //Put the impact particle system in the Garbage Gameobject
		Destroy (muzzle, 4f); //Destroy effect after 2s

		//Get enemies in range
		Collider[] colliders = Physics.OverlapSphere (transform.position, range);

		foreach (Collider c in colliders) {
			if (c.tag == "Enemy") {
				GameObject PS = (GameObject) Instantiate (p.impactPS, c.transform.position, transform.rotation); //Spawn the impact particles
				PS.transform.SetParent(garbageGO.transform); //Put the impact particle system in the Garbage Gameobject
				Destroy (PS, 4f); //Destroy effect after 2s

				p.Damage (c.transform, damageType);
			}
		}

		fireCountdown = 1f / fireRate;
	}

	void piercingShot(){
		//Spawn the bullets
		GameObject projectileGO = (GameObject)Instantiate (projectilePF, firePoint.position, firePoint.rotation); //GameObject of this projectile (GO = gameobject)
		projectileGO.transform.SetParent (gameObject.transform); // Put the projectile as this towers child

		fireCountdown = 1f / fireRate;
	}

	//Rotate the tower to look at its target : REUSABLE
	void rotateToLookAtTarget(bool lerp){
		//Don't rotate if requiem tower
		if (requiem)
			return;
		Vector3 dir = target.position - transform.position; //Get direction to target
		Quaternion lookRotation = Quaternion.LookRotation(dir); //Get the Quaternion pertaining to looking in the direction found
		if(lerp){
			Vector3 rotation = Quaternion.Lerp(rotator.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles; //Turn the Quaternion into a Vector, Lerp smoothes the transition
			rotator.rotation = Quaternion.Euler (0f, rotation.y, 0f); //Set the rotation using the rotation values created
		}
		else{
			Vector3 rotationNL = lookRotation.eulerAngles;
			rotator.rotation = Quaternion.Euler (0f, rotationNL.y, 0f); //Set the rotation using the rotation values created
		}
	}
		
	public void setCountdown(float t){
		fireCountdown = t;
	}

	public float getCountdown(){
		return fireCountdown;
	}


	/* 
	* TARGETTING FUNCTIONS
	*/


	//Search for target (Don't do this every frame)
	void updateTarget(){
		List<Collider> enemies = new List<Collider>(Physics.OverlapSphere (transform.position, range));

		//Add any new enemies to the list
		for(int i = enemies.Count - 1; i >= 0; i--) {
			Collider c = enemies [i];
			if(c.tag == "Enemy")
				if (!enemyList.Contains (c.transform))
					enemyList.Add (c.transform);
		}

		//If an enemy is destroyed or it is not in the most recent check within its range then remove it from the list
		for(int i = enemyList.Count - 1; i >= 0; i--) {
			if (enemyList [i] == null) {
				enemyList.RemoveAt (i);
				continue;
			}
			Transform t = enemyList [i];
			if (!enemies.Contains (t.GetComponent<Collider> ()))
				enemyList.Remove (t);
			
		}

		//Target selection
		//Pick the target with the highest distance travelled (closest to end point)
		if (enemyList.Count > 0 && !targetHighHealth) {
			float highestDistance = Mathf.NegativeInfinity;
			Transform highestDistanceTransform = null;
			foreach (Transform t in enemyList) {
				Enemy e = t.GetComponent<Enemy> ();
				if (e.distanceTravelled > highestDistance) {
					highestDistance = e.distanceTravelled;
					highestDistanceTransform = t;
				}
			}
			target = highestDistanceTransform;
		}
		//Select the target with the highest hp (for snipers and for percent dmg towers)
		else if (enemyList.Count > 0 && targetHighHealth) {
			float highestHealth = Mathf.NegativeInfinity;
			Transform highestHealthTransform = null;
			foreach(Transform t in enemyList){
				Enemy e = t.GetComponent<Enemy> ();
				if (e.health > highestHealth) {
					highestHealth = e.health;
					highestHealthTransform = t;
				}
			}
			target = highestHealthTransform;
		}
		else
			target = null;
	}

	//Get distance to the target
	public float distanceToTarget(Transform target){
		return Vector3.Distance (transform.position, target.position);
	}
		
	public void setEnemyList(List<Transform> l){
		enemyList = l;
	}

	public List<Transform> getEnemyList(){
		return enemyList;
	}

	/* 
	* SKILL FUNCTIONS
	*/

	//Is the next tier unlocked?
	public bool nextTierIsUnlocked() { return nextTierUnlocked; }

	//Activates the skills for the specific tower type
	public void activateSkills(activeBuffs aB, Node node){
		if (gameObject.tag == "Turret")
			aB.applyTurretBuffs (this);
		else if (gameObject.tag == "Rocket")
			aB.applyRocketBuffs (this);
		else if (gameObject.tag == "Laser")
			aB.applyLaserBuffs (this);
		else if (gameObject.tag == "Sniper")
			aB.applySniperBuffs (this);

		if (node.isSpecial) {
			if (node.bonusRange)
				specialNodeBonusRange (node);
			if (node.bonusDamage)
				specialNodeBonusDamage (node);
		}
			
	}

	//Set up default tower/projectile stats before applying skills
	public void setDefaults(Projectile p){
		p.damage = p.baseDamage;
		p.explosionRadius = p.baseExplosionRadius;
		p.dealsKillCountDamage = p.killCountByDefault;
		p.bonusDamageFromTower = 0;
	}

	//Set much extra damage this tower deals (projectile based)
	public void addBonusProjDamage(float amount){
		bonusDamageToDeal += amount;
	}

	public float getBonusProjDamage(){
		return bonusDamageToDeal;
	}

	//Add bonus damage to the tower if the tower is on a special node
	void specialNodeBonusDamage(Node node){
		if (isLaser)
			bonusDOT += (DOT * node.bonusDamageAmount);
		else{
			addBonusProjDamage (node.bonusDamageAmount);
		}
	}

	//Add bonus range to the tower if the tower is on a special node
	void specialNodeBonusRange(Node node){
		range += range * node.bonusRangeAmount;
	}

	/* 
	* DEBUFF FUNCTIONS
	*/

	//Does the enemy debuff list contain a debuff with this name?
	public debuff hasDebuff(string s){
		for(int i = debuffs.Length-1; i >= 0; i--) {
			if (debuffs[i].tag.Contains(s))
				return debuffs[i];
		}
		return null;
	}
		
}
