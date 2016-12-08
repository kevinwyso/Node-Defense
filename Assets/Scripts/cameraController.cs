using UnityEngine;

public class cameraController : MonoBehaviour {

	[Header ("Camera Attributes")]
	public float panSpeed; //How fast to pan 
	public float panBoarderBuffer; //Gives an area around the outer edge of the screen where the panning will be caught
	public float scrollSpeed; //How fast to zoom in/out
	public float minY, maxY, minPan, maxPan; //Min/Max distances to Pan

	private Vector3 origin; //Original location of camera
	private bool move = true; //Movement disable/enabler

	void Start(){
		origin = transform.position; //Origin = where the camera starts
	}

	// Update is called once per frame
	void Update () {
		//If game is over, stop camera movement
		if (gameManager.gameEnded)
			return;

		//Lets the user stop the camera from moving
		if (Input.GetKeyDown (KeyCode.Escape))
			move = !move;
		
		//Stop movement if disabled
		if (!move)
			return;

		//Check for Keypress
		if (Input.GetKey ("w") || Input.GetKey("up") || Input.mousePosition.y >= Screen.height - panBoarderBuffer) {
			if (cameraOffset().z <= maxPan)
				transform.Translate (Vector3.forward * panSpeed * Time.deltaTime, Space.World);
		} else if (Input.GetKey ("a") || Input.GetKey("left") || Input.mousePosition.x <= panBoarderBuffer) {
			if (cameraOffset().x >= minPan)
				transform.Translate (Vector3.left * panSpeed * Time.deltaTime, Space.World);
		} else if (Input.GetKey ("s") || Input.GetKey("down") || Input.mousePosition.y <= panBoarderBuffer) {
			if (cameraOffset().z >= minPan + 20) //Extra offset added here due to camera origin favoring the down direction
				transform.Translate (Vector3.back * panSpeed * Time.deltaTime, Space.World);
		} else if (Input.GetKey ("d") || Input.GetKey("right") || Input.mousePosition.x >= Screen.width - panBoarderBuffer) {
			if (cameraOffset().x <= maxPan)
				transform.Translate (Vector3.right * panSpeed * Time.deltaTime, Space.World);
		}

		Vector3 pos = transform.position; //Get position of camera
		float scrollAmnt = Input.GetAxis ("Mouse ScrollWheel"); //Get Mouse Wheel input

		pos.y -= scrollAmnt * scrollSpeed * Time.deltaTime * 1000; //ScrollAmnt is very small in magnitude 0.1-0.4 so mutliply by arbitrary big number
		pos.y = Mathf.Clamp(pos.y, minY, maxY); //Stop the position of the camera from being below or above the min/max

		transform.position = pos; //Set the position of the camera

	}

	//Gets the camera position based on it's origin
	Vector3 cameraOffset(){
		return transform.position - origin;
	}
}
