using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class skillMenu : MonoBehaviour {

	public Text skillPointsLeft;
	public Text[] skillCounts; //Array holding the text objects for each tree tally
	
	// Update is called once per frame
	void Update () {
		//Let the player know how many points they have left to allocate
		skillPointsLeft.text = playerStats.current.getSkillPointsLeft ().ToString();
		setCounts ();
	}

	//Set the tree that the user clicked to active, disable all other trees
	public void openTree(GameObject toOpen){
		if (GameObject.FindGameObjectWithTag ("Tree") != null)
			GameObject.FindGameObjectWithTag ("Tree").SetActive (false);
		toOpen.SetActive (true);
	}
		
	//Set the count of each tree with how many skill points have been allocated to it
	void setCounts(){
		for(int i = 0; i < skillCounts.Length; i++) {
			skillCounts [i].text = playerStats.current.getSkillPointsUsed () [i].ToString();
		}
	}

	public void resetPoints(){
		playerStats.current.resetSkills ();
	}

}
