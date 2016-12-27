using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class leaderboardMenu : MonoBehaviour {

	public Text leaderBoardText;
	public int maxScoresToDisplay;

	List<dreamloLeaderBoard.Score> scoreList;


	dreamloLeaderBoard leaderboard;
	dreamloPromoCode pc;

	void Start ()
	{
		// get the reference here...
		this.leaderboard = dreamloLeaderBoard.GetSceneDreamloLeaderboard ();

		leaderboard.AddScore ("mmmmmmmmmm", 0); //Need to do this or the leaderboard won't show... wtf
	}
	
	// Called once per frame
	void OnGUI () {
		scoreList = leaderboard.ToListHighToLow ();

		if (scoreList == null || scoreList.Count == 0) {
			leaderBoardText.text = "No Scores Yet!";
		} else {

			int count = 0;
			leaderBoardText.text = "";
			foreach (dreamloLeaderBoard.Score score in scoreList) {
				count++;

				leaderBoardText.text += score.playerName + " - Wave:" + score.score + "\n";

				if(count >= maxScoresToDisplay) break;
			}
		}
	}
}
