using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class enemyGUI : MonoBehaviour {

	public static enemyGUI instance;

	[Header ("Texts")]
	public Text speed;
	public Text health;
	public Text worth;
	public Text resists;

	private Enemy target;

	void Start(){
		if (instance != null) { //Error check
			Debug.Log ("More than one instance in scene!");
			return;
		}
		instance = this; //Sets the enemyGUI to a static variable which can be accessed from anywhere
	}

	void Update(){
		if (target != null)
			setStats ();
		else
			emptyStats ();
	}

	public void setStats(){
		setSpeed ();
		setHealth ();
		setWorth ();
		setResists ();
	}

	public void emptyStats(){
		speed.text = "-";
		health.text = "-";
		worth.text = "-";
		resists.text = " ";
	}

	public void setSpeed(){
		speed.text = target.moveSpeed.ToString ("F2");
	}

	public void setHealth(){
		health.text = target.health.ToString ("F2") + "/" + target.waveHealth.ToString ();
	}

	public void setWorth(){
		worth.text = target.worth.ToString ();
	}

	public void setResists(){
		foreach (Resist r in target.resistList) {
			resists.text = r.type + "\n";
		}
	}

	public void setTarget(Enemy e){
		target = e;
	}

}
