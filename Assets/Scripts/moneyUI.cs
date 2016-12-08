using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class moneyUI : MonoBehaviour {

	public Text moneyText;
	
	//Update money stat on GUI
	void Update () {
		moneyText.text = "$" + gameStats.money.ToString();
	}
}
