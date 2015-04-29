using UnityEngine;
using System.Collections;

public class HarpoonScript : MonoBehaviour {

	public GameObject Player;
	public bool Fired = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col)
	{
		Debug.Log (col.gameObject.name);

		if (col.gameObject.name == "Gun") {
			if (!Player.GetComponent<PlayerScript>().HasHarpoon)
			{
				Player.GetComponent<PlayerScript>().CanPickupHarpoon = true;
			}
		}
	}

	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.name == "Gun") {
			if (!Player.GetComponent<PlayerScript>().HasHarpoon)
			{
				Player.GetComponent<PlayerScript>().CanPickupHarpoon = false;
			}
		}
	}
}
