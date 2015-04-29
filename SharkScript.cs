using UnityEngine;
using System.Collections;

public class SharkScript : MonoBehaviour {

	public GameObject Harpoon; // Our mortal enemy

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision col)
	{
		Debug.Log (col.gameObject.name);
		if (col.gameObject == Harpoon && Harpoon.GetComponent<HarpoonScript>().Fired) {
			Destroy (this.gameObject);
		}
	}
}
 