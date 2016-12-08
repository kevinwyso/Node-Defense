using UnityEngine;
using System.Collections;

//This class handles updating the enemies debuffs and movement every tick 
//Enemy does not have an update function like this class

[RequireComponent(typeof(Enemy))]
public class enemyUpdate : MonoBehaviour {
	
	private Transform target; // Current waypoint we are pursuing (Object)
	private int waypointIndex = 0; //Current waypoint we are pursuing (Index)
	private Enemy enemy;
	private Renderer enemyGraphics;

	void Start(){
		enemy = GetComponent<Enemy> (); //Get enemy component of Gameobject
		target = waypoints.wps [0]; //First waypoint is the target for an enemy off its spawn
		enemyGraphics = enemy.GetComponentInChildren<Renderer> (); //Get renderer component of enemy
	}

	//Every Frame
	void Update(){
		Vector3 dir = target.position - transform.position; // Gives us direction to go in order to get to target
		transform.Translate(dir.normalized * enemy.moveSpeed * Time.deltaTime, Space.World); //Move in this position

		//Check if reached waypoint
		if (Vector3.Distance (transform.position, target.position) <= 1f) { 
			getNextWaypoint();
		}

		//Reset Values at the beginning of update and then check if they need to be changed 

		enemy.moveSpeed = enemy.waveSpeed; //Reset speed before checking debuff
		enemyGraphics.material.color = enemy.baseColor; //Reset color before checking for debuff
		enemy.impaired = false; //Reset impaired check

		//For every debuff in the debuff list, apply this debuff and countdown from its timer
		for(int i = enemy.debuffList.Count-1; i >= 0; i--) {
			debuff d = enemy.debuffList[i];

			//Countdown the time left on this debuff
			//if the debuff timer hasn't run out then effect the enemy, otherwise remove the debuff
			if (d.timeLeft >= 0f) {
				d.timeLeft -= 1f * Time.deltaTime;
				enemy.doDebuff (d);
			} else {
				enemy.removeDebuff (d);
			}
		}

		//Set impaired
		if (enemy.moveSpeed != enemy.waveSpeed)
			enemy.impaired = true;

		//Heal the enemy every tick if it is a healer type
		if (enemy.heals)
			enemy.selfHeal ();

	}

	//Set the enemy to head toward the next waypoint
	void getNextWaypoint(){
		enemy.setConfused (false);
		//If the last waypoint is reached (End Point) then destroy the object
		if (waypointIndex == waypoints.wps.Length - 1) { 
			endPath();
			return;
		}
		waypointIndex++; //Increment waypoint index
		target = waypoints.wps [waypointIndex]; //Set the target equal to the NEXT waypoint
	}

	//When an enemy reaches the end of the path
	void endPath(){
		gameStats.lives--;
		Destroy (gameObject);
	}

	//For Confuse Ray, Set waypoint equal to previous
	public void setPreviousWaypoint(){
		if (waypointIndex > 0) {
			waypointIndex--;
			target = waypoints.wps [waypointIndex];
		}
	}


}
