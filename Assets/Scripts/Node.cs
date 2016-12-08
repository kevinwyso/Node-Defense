using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Node : MonoBehaviour {

	[Header ("Node Attributes")]
	public Color hoverColor; //Default color when mouse hovered over a node
	public Color notEnoughMoneyColor; //Color of hover if not enough money to buy the tower
	public Color startColor; //Color the node will start as
	[Space (10)]
	public Vector3 positionOffset; //Offset to put the tower in the center

	[Header ("Skill Settings")]
	public bool isSpecial;
	public Color specialColor; //Color if the node is special as deemed by the skill in Node mastery
	[Space (10)]
	public bool bonusRange;
	public float bonusRangeAmount;
	[Space (10)]
	public bool bonusDamage;
	public float bonusDamageAmount;

	[Header ("Setup")]
	BuildManager buildManager;

	[Header ("Optional Parameter")]
	public GameObject towerHere = null; //Tower on this node
	public Tower t; //The tower component of the tower on this node

	private Renderer rend; //Renderer to control color/skin/etc


	void Start(){
		rend = GetComponent<Renderer> (); // Find/Set the renderer
		if(isSpecial)
			startColor = specialColor; //Node is special
		else
			startColor = rend.material.color; //Find/Set the renderer
		
		buildManager = BuildManager.instance; //Set the buildmanager instance
		rend.material.color = startColor;
	}

	//Get the position of the node and its position offset (puts build position in the center when instantiating)
	public Vector3 getBuildPosition(){
		return transform.position + positionOffset;
	}

	//Called when mouse enters the collider of the object
	void OnMouseEnter(){
		//If no tower selected, return
		if (!buildManager.isSelected) 
			return;
		
		//If hovering over another UI element, disable this
		if (EventSystem.current.IsPointerOverGameObject ()) 
			return;

		//Change color of node to note it is being hovered
		if (buildManager.hasMoney || (isSpecial && buildManager.getTowerToBuild().prefab.GetComponent<Tower>().towerTier == 1))
			rend.material.color = hoverColor; 
		else
			rend.material.color = notEnoughMoneyColor;
	}

	//Called when mouse leaves the object
	void OnMouseExit(){
		rend.material.color = startColor; //Change color back to normal
	}

	//When mouse clicked
	void OnMouseDown(){
		//If hovering over another UI element, disable this. Fix hovering over other gameobjects by removing colliders if necessary
		if (EventSystem.current.IsPointerOverGameObject()) 
			return;

		//if something is built here
		if(towerHere != null){ 
			buildManager.selectNode (this); //Set selected node
			return;
		}

		//If no tower selected, return
		if (!buildManager.isSelected) {
			return;
		}

		//Build Turret and set the tower component
		buildManager.buildTowerOn(this);

		//If tower successfully built then set t = to it
		if(towerHere != null)
			t = towerHere.GetComponent<Tower> ();

	}

}
