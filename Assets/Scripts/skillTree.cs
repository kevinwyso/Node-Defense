using UnityEngine;
using System.Collections;

public class skillTree : MonoBehaviour{

	[Header ("Colors")]
	public Color activeColor;
	public Color inactiveColor;
	[Space(10)]
	public int treeIndex; //Index of this tree in the list of skill trees
	public buffText[] texts; //The list of texts that this tree has and their individual skill costs

	void Update(){
		setActiveBuffLabels ();
	}

	//Lets the user know which buffs are active by changing their color
	void setActiveBuffLabels(){
		int count = 0;
		foreach (buffText t in texts) {
			if (playerStats.current.getSkillPointsUsed () [treeIndex] >= t.cost) {
				t.text.color = activeColor;
				playerStats.current.activateBuffs (treeIndex, count);
			} else {
				t.text.color = inactiveColor;
				playerStats.current.deactivateBuffs (treeIndex, count);
			}
			count++;
		}
	}

	//Add a point to this skill tree
	public void addSkillPoint(){
		if (playerStats.current.getSkillPointsLeft () >= 1)
			if(playerStats.current.skillPointsAtThisIndex(treeIndex) < 12)
				playerStats.current.addSkillPoint(treeIndex);
	}


}
