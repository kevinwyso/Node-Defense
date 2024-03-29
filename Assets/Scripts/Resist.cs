using UnityEngine;
using System.Collections;

[System.Serializable]
public class Resist {

	//List of resists
	public enum resistType { NONE = 0, PIERCING = 1, EXPLOSION = 2, LASER = 3, SPELL = 4, NORMAL = 5};

	public resistType type;
	public float amount; //How strong the resist is

	public Resist(Resist r){
		this.type = r.type;
		this.amount = r.amount;
	}

	public Resist(resistType _type, float _amount){
		this.type = _type;
		this.amount = _amount;
	}

	//Possible resist types to create
	public static Resist explosionResist(float _amount){
		return new Resist (resistType.EXPLOSION, _amount);
	}

	public static Resist pierceResist(float _amount){
		return new Resist (resistType.PIERCING, _amount);
	}

	public static Resist normalResist(float _amount){
		return new Resist (resistType.NORMAL, _amount);
	}

	public static Resist spellResist(float _amount){
		return new Resist (resistType.SPELL, _amount);
	}

	public static Resist laserResist(float _amount){
		return new Resist (resistType.LASER, _amount);
	}



}


