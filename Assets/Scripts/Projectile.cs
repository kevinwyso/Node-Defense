using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	
	public enum DamageOp { FLAT, PERCENTMAX, PERCENTCURR }

	[Header ("Basic Attributes")]
	public float projectileSpeed; //Speed of the projectile in flight
	public float explosionRadius; //AoE Damage
	public float baseExplosionRadius; //Base radius (to be increased by skills)
	[Space (10)]
	public float damage; //Damage to deal
	public float baseDamage; //Base damage (to be increased by skills)
	public float bonusDamageFromTower; //How much damage should be added from Tower effects/skills
	public DamageOp damageOp; //Flat / Percent
	[Space(10)]
	public bool canCrit; //Can this bullet crit
	public float critChance; //Chance for the bullet to crit
	public float critAmount; //How much bonus damage for critting the target

	[Header ("Setup")]
	public GameObject impactPS; //Particles for impact

	[Header ("Shotgun Attributes")]
	public bool isShotgun; //Is this a shotgun bullet (Moves forward without aiming)
	public float timeOut; //NEEDS TO BE SET FOR PIERCING BULLETS AS WELL

	[Header ("Cluster Attributes")]
	public bool clusters; //Does this bullet release clusters upon impact?
	public GameObject clusterPF; //What are the cluster projectiles?
	public int clusterCount; //How many clusters?
	
	[Header ("Piercing Attributes")]
	public bool isPiercing;  //Does this pierce
	private float targetsPierced = 0; //How many targets has this pierced
	//NOTE: PIERCING REQUIRES TIMEOUT FROM SHOTGUN ATTRIBUTES TO BE SET

	[Header ("Killcount attributes")]
	public bool dealsKillCountDamage; //Does this tower deal bonus damage based on kill?
	public bool killCountByDefault; 

	[Header ("Range Damage Attributes")]
	public bool rangeAffectsDamage; //Does range affect this tower's damage?
	public float rangeDamageFactor; //How much does range affect it?

	[Header ("Oppressor Attributes")]
	public bool oppresses; //Does the tower deal more damage based on CC?
	public float oppressFactor; //How much bonus damage is dealt while target is CC'd

	[Header ("Penetration Attributes")]
	public bool stacksPen; //If penetration stacks on hit


	private Tower tower; //Tower that shot this projectile
	private debuff _debuff; //Debuff dealt by the projectile
	private Transform target; //Tells the bullet which enemy to chase/hit
	private Projectile[] bullets; //List of bullets for multishot capability
	private GameObject garbageGO;

	void Start(){
		tower = gameObject.GetComponentInParent<Tower> (); //Link the tower and the projectile shooting from it
		garbageGO = GameObject.Find ("Garbage"); //Find/Set the Garbage Gameobject
	
		//Apply bonuses to damage
		bonusDamageFromTower = baseDamage * tower.getBonusProjDamage();

		//Check if the tower has Killcount enabled (add to its bonus damage)
		if(dealsKillCountDamage)
			bonusDamageFromTower += tower.killCount * 1;
		else if (dealsKillCountDamage && killCountByDefault)
			bonusDamageFromTower += tower.killCount * 2;
	}

	//Set the projectiles target 
	public void Seek(Transform _target){ 
		target = _target;
	}

	// Update is called once per frame
	void Update () {
		//If the projectile is part of a shutgun shot
		if (isShotgun || isPiercing) {
			if (timeOut >= 0f) {
				//Instead of chasing a target, just go forward at Projectile Speed pace
				float distanceThisFrame = projectileSpeed * Time.deltaTime; //How far the projectile will move in this frame
				transform.Translate (Vector3.forward * distanceThisFrame, Space.Self); //Don't use world space, want to use the "forward" of the bullet
			} else {
				//Timed out bullet (out of range)
				Destroy (gameObject);
			}
			
			timeOut -= 1f * Time.deltaTime; //Countdown from the timeout 
		} else {
			//If the target is destroyed or reaches the end, destroy the bullet
			if (target == null) {
				Destroy (gameObject);
				return;
			}

			//Direction and Speed
			Vector3 dir = target.position - transform.position; //Get the direction to the target
			float distanceThisFrame = projectileSpeed * Time.deltaTime; //How far the projectile will move in this frame
			//If length of direction vector is less than its speed, then it will have hit something
			if (dir.magnitude <= distanceThisFrame) {
				HitTarget (target);
				return;
			}

			//Move the bullet towards its target
			transform.Translate (dir.normalized * distanceThisFrame, Space.World);

			//Have the missile aim toward the target
			transform.LookAt (target);

		}

	}

	//For Projectiles that actually hit their target to check dmg, REQUIRES: Enemy Rigidbody component and isTrigger set on Enemy collider
	void OnTriggerEnter(Collider c){
		HitTarget (c.transform);
	}

	//Hit the target
	void HitTarget(Transform target){
		GameObject PS = (GameObject) Instantiate (impactPS, transform.position, transform.rotation); //Spawn the impact particles
		PS.transform.SetParent(garbageGO.transform); //Put the impact particle system in the Garbage Gameobject
		Destroy (PS, 3f); //Destroy effect after 3s

		//If Aoe then explode, if it is a cluster shot then add more projectiles after exploding
		//Else just single target damage
		if (explosionRadius > 0f) {
			Explode ();
			if (clusters)
				Cluster ();
		} else {
			Damage (target, tower.damageType);
		}

		//If the projectile pierces and hasn't pierced through its max number of targets yet then don't destroy it and add fall off dmg
		if (isPiercing && targetsPierced < tower.targetsToPierce) {
			damage = Mathf.Clamp(damage - tower.falloff, 30, damage);
			targetsPierced++;
			return;
		}
		
		Destroy (gameObject); //Destroy bullet

	}

	//Deal damage to the enemy equal to the damage variable for this tower(calls takeDamage)
	public void Damage(Transform enemy, Resist.resistType damageType){

		//If Enemy isn't null then do damage
		if (enemy != null) {
			Enemy e = enemy.GetComponent<Enemy> ();

			float damageToDeal = damage + bonusDamageFromTower; //calculate the damage to deal based on its damage + bonus damage from stats/other sources

			//Check damage type then apply debuffs and damage
			if (damageOp == DamageOp.FLAT) {
				foreach (debuff d in tower.debuffs) {
					e.apply (d);
				}

				if (rangeAffectsDamage) {
					//Increase the damage dealt based on how far the target is from the tower. Deal (Damage + Damage * Ratio of distance to max range * rangeFactor)
					//If range factor = 1 then max damage dealt is 2xDamage
					e.takeDamage ((damageToDeal * (rangeDamageFactor * (distanceToTarget () / tower.range)) + damageToDeal), damageType, tower);
					return;
				} else if (oppresses) {
					//Increase the damage dealt if the target is impaired
					if(e.impaired)
						e.takeDamage (damageToDeal + (damageToDeal * oppressFactor), damageType, tower);
					else 
						e.takeDamage (damageToDeal, damageType, tower);
					return;
				} else if (canCrit) {
					if (UnityEngine.Random.value >= (1f - critChance))
						e.takeDamage (critAmount * damageToDeal, damageType, tower);
					else
						e.takeDamage (damageToDeal, damageType, tower);
				} else {
					e.takeDamage (damageToDeal, damageType, tower);
				}
			} else if (damageOp == DamageOp.PERCENTMAX) {
				foreach (debuff d in tower.debuffs) {
					e.apply (d);
				}
				e.takeDamage (damageToDeal * e.waveHealth, damageType, tower);
			} else if (damageOp == DamageOp.PERCENTCURR) {
				foreach (debuff d in tower.debuffs) {
					e.apply (d);
				}
				e.takeDamage ((damageToDeal * e.health) + 10, damageType, tower);
			}
		}

	}

	//Call the damage function for any enemy in the explosion radius
	void Explode (){
		Collider[] hits = Physics.OverlapSphere (transform.position, explosionRadius); //Get list of enemies in explosion radius

		//For each collider, check if it is an enemy, then deal damage to it
		foreach (Collider c in hits) {
			if (c.tag == "Enemy") {
				Damage (c.transform, tower.damageType);
			}
		}

	}

	//Create rocket clusters and explode each of them
	void Cluster(){
		for (int i = 0; i < clusterCount; i++) {
			//Create random vector to place the clusters
			Vector3 rand = Random.insideUnitSphere * 5;
			rand = new Vector3 (rand.x, 1 , rand.z); //Don't want a random Y value

			//Create the new Cluster rocket and set it up
			GameObject cluster = (GameObject)Instantiate (clusterPF, transform.position + rand, transform.rotation);
			Projectile p = cluster.GetComponent<Projectile> ();
			cluster.transform.SetParent (tower.transform); //Organize
			p.setTower(tower); //Make sure the projectile knows what tower it came from

			//Explode the rocket
			p.Explode();

			//Show visuals
			GameObject PS = (GameObject) Instantiate (clusterPF.GetComponent<Projectile>().impactPS, transform.position + rand, transform.rotation); //Spawn the impact particles
			PS.transform.SetParent(garbageGO.transform); //Put the impact particle system in the Garbage Gameobject
			Destroy (PS, 3f); //Destroy effect after 2s
		}
	} 

	//Get distance to the target
	float distanceToTarget(){
		return Vector3.Distance (tower.transform.position, target.position);
	}

	//set the tower field for this projectile (where the projectile originated from so it knows it's debuffs)
	public void setTower(Tower t){
		tower = t;
	}

	//Show explosion radius for a selected projectile
	void OnDrawGizmosSelected(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, explosionRadius);
	} 
} 
