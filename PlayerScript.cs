using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	public GameObject Camera;
	public GameObject Harpoon;
	public GameObject Gun;

	public bool InvertedVertical = false;
	public bool CanPickupHarpoon = false;
	public bool HasHarpoon = true;

	private float horizontalSpeed = 2.0f;
	private float verticalSpeed = 2.0f;
	private float movementSpeed = 0.2f;
	private float oxygen;
	private float health;

	// Use this for initialization
	void Start () {
		oxygen = 100;
		health = 100;
	}
	
	// Update is called once per frame
	void Update () {
		CameraControl ();
		if (Input.GetKey (KeyCode.W)) {
			Camera.transform.Translate (Vector3.forward * movementSpeed);
		} else if (Input.GetKey (KeyCode.S)) {
			Camera.transform.Translate (Vector3.forward * -movementSpeed);
		}

		if (Input.GetKey (KeyCode.D)) {
			Camera.transform.Translate (Vector3.right * movementSpeed);
		} else if (Input.GetKey (KeyCode.A)) {
			Camera.transform.Translate (Vector3.right * -movementSpeed);
		}

		if (Input.GetKey (KeyCode.Space)) {
			Camera.transform.Translate (Vector3.up * movementSpeed);

		}

		if (Input.GetMouseButtonDown (0) && HasHarpoon) {	
			Gun.GetComponent<FixedJoint>().connectedBody = null;
			Harpoon.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward*25000);
			Harpoon.GetComponent<Rigidbody>().useGravity = true;
			Harpoon.GetComponent<HarpoonScript>().Fired = true;
			HasHarpoon = false;
		}
		if (Input.GetKeyDown(KeyCode.R)){
			Application.LoadLevel ("First");
		}
		oxygen -= 1/60f;
		if (Input.GetKey (KeyCode.E) && CanPickupHarpoon && !HasHarpoon) {
			PickUpHarpoon();
		}
	}

	void PickUpHarpoon(){
		Harpoon.transform.position = Gun.transform.position;
		Harpoon.transform.rotation = Gun.transform.rotation;
		Gun.GetComponent<FixedJoint>().connectedBody = Harpoon.GetComponent<Rigidbody>();
		Harpoon.GetComponent<Rigidbody>().useGravity = false;
		Harpoon.GetComponent<HarpoonScript>().Fired = false;
		HasHarpoon = true;
	}

	void CameraControl(){
		float h = horizontalSpeed * Input.GetAxis ("Mouse X");
		float v = verticalSpeed * Input.GetAxis ("Mouse Y");
		if (!InvertedVertical)
			Camera.transform.Rotate (-v, h, 0);
		else
			Camera.transform.Rotate (v, h, 0);
	}
}
