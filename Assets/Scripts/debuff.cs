using UnityEngine;
using System.Collections;

[System.Serializable]
public class debuff {

	//List of Attributes to affect
	public enum Stat { HEALTH, HEALTHPERCMAX, HEALTHPERCCURR, MOVESPEED, WORTH, BOMB, LIMP, CONFUSE, PENETRATION, PENETRATIONSTACK};

	public float duration; //How long after the debuff is applied does this debuff last (0 for only when being hit by tower)
	public float timeLeft; //How much time is left on this debuff
	[Space (10)]
	public Stat toAffect; //Which Stat to affect
	public float amount; //How much to affect this attribute by
	public float radius; //For the BOMB effect, how big is the radius for a given explosion?
	[Space (10)]
	public string tag; //How to identify this debuff for reapplication and modification through the list
	public Tower appliedBy; //Which tower applied this debuff

	//Construct a copy of the passed debuff
	public debuff(debuff d){
		this.duration = d.duration;
		this.timeLeft = d.timeLeft;
		this.toAffect = d.toAffect;
		this.amount = d.amount;
		this.radius = d.radius;
		this.tag = d.tag;
		this.appliedBy = d.appliedBy;
	}

	//Constructor for a debuff
	public debuff(float _duration, Stat _toAffect, float _amount, float _radius, string _tag, Tower _appliedBy){
		this.duration = _duration;
		this.timeLeft = _duration;
		this.toAffect = _toAffect;
		this.amount = _amount;
		this.radius = _radius;
		this.tag = _tag;
		this.appliedBy = _appliedBy;
	}

	//A stun debuff
	public static debuff stunDebuff(float duration, Tower appliedBy){
		return new debuff (duration, Stat.MOVESPEED, 1f, 0, "Stun", appliedBy);
	}
		
}
