using UnityEngine;
using System.Collections;

[System.Serializable]
public class playerStats{

	public static playerStats current;
	public static int saveIndex = -1; //Used to save after a given game ends

	public string saveName;

	private int level = 0, exp = 0, skillPointsTotal = 0, prestiges = 0;
	private int[] skillPointsAllocated;
	private bool[,] buffsActive;

	public playerStats(string name, int _level, int _exp, int _skillPointsTotal, int[] _skillPointsAllocated, bool[,] _buffsActive){
		saveName = name;
		level = _level;
		exp = _exp;
		skillPointsTotal = _skillPointsTotal;
		skillPointsAllocated = _skillPointsAllocated;
		buffsActive = _buffsActive;
		current = this;
	}

	//Level Functions
	public void setLevel(){
		level = getLevelFunction();
		setSkillPoints ();
	}

	public int getLevel(){
		return level;
	}
	//End Level Functions

	//EXP functions
	void setExp(int e){
		exp = e;
	}

	public int getExp(){
		return exp;
	}

	public void addExpPerDiff(int amount){
		//Don't give the user exp for not passing the first round
		if (amount == 1)
			return;

		//Grant multipliers based on Map and Enemy Difficulty
		amount =  Mathf.CeilToInt (amount * gameStats.mapDiff);
		addExp (Mathf.CeilToInt (amount * gameStats.enemyDiff));

	}

	public void addExp(int amount){
		setExp (exp + amount);
	}
	//End EXP Functions

	//Skill Functions
	void setSkillPoints(){
		skillPointsTotal = level;
	}

	public int getSkillPoints(){
		return skillPointsTotal;
	}


	public int[] getSkillPointsUsed(){
		return skillPointsAllocated;
	}

	public int getSkillPointsLeft(){
		int used = 0;
		foreach (int count in skillPointsAllocated) {
			used += count;
		}
		return skillPointsTotal - used;
	}
		
	public void addSkillPoint(int index){
		skillPointsAllocated [index] += 1;
	}

	public int skillPointsAtThisIndex(int index){
		return skillPointsAllocated [index];
	}

	public void activateBuffs(int treeIndex, int tier){
		buffsActive [treeIndex,tier] = true;
	}

	public void deactivateBuffs(int treeIndex, int tier){
		buffsActive [treeIndex,tier] = false;
	}

	public bool[,] getBuffMatrix(){
		return buffsActive;
	}

	public void resetSkills(){
		skillPointsAllocated = new int[] { 0, 0, 0, 0, 0, 0 };
		buffsActive = getEmptyBuffMatrix (6,6);
		//printBuffs ();
	}

	//End Skill Functions
	 
	/*
	* Math Functions
	*/

	//Function returns how much exp required for the next level up
	//Each successive level has an increasing difference of 0.5
	//     0, 0.5, 1.5, 3
	//       0.5  1  1.5
	//         0.5 0.5 
	//Formula: 1/4n^2 - 1/4n = exp;
	public int getLevelFunction(){
		return (int) Mathf.Floor((float)quadFormSolve(0.25f,-0.25f,-exp, true));
	}

	//Solve a quadratic equation for finding the level 
	public double quadFormSolve(float a, float b, float c, bool pos)
	{
		var preRoot = b * b - 4 * a * c;
		if (preRoot < 0)
		{
			return double.NaN;
		}
		else
		{
			var sgn = pos ? 1.0 : -1.0;
			return (sgn * Mathf.Sqrt(preRoot) - b) / (2.0 * a);
		}
	}

	/*
	* Debug Functions
	*/

	public override string ToString(){
		return "Name: " + saveName + "\nLevel: " + level + "\nExp: " + exp;
	}

	//For debugging
	public void printBuffs(){
		
		for (int i = 0; i < buffsActive.GetLength (0); i++) {
			string row = "";
			for (int j = 0; j < buffsActive.GetLength (1); j++) {
				row += buffsActive [i, j].ToString ();
			}
			Debug.Log (row);
		}
	}

	//Returns the matrix of skills and sets them to inactive (false)
	public static bool[,] getEmptyBuffMatrix(int rows, int cols){
		bool[,] matrix = new bool[rows,cols];
		for(int i = 0; i < matrix.GetLength(0); i++){
			for (int j = 0; j < matrix.GetLength(1); j++) {
				matrix [i,j] = false;
			}
		}
		return matrix;
	}

}



