/* TurretPlacement.cs
 * Places a turret into a scene, limits places you can place by checking for steepness of mouseclickpoint 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPlacement : MonoBehaviour {

	private GameObject turret;
	public Vector3 positionOffset;
	public Camera camera;
	private bool turretBtnPressed;

	void Start()
	{
		turretBtnPressed = true; //TODO: once limiting placement of turrets based on terrain slope, switch to false, get button working
	}

	void Update() {
		if (Input.GetMouseButtonDown (0)) {

			if (!turretBtnPressed)
				return;

			//Set up a ray for turret placement
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			//get terrain
			Terrain terrain = (Terrain)GetComponent (typeof(Terrain));

			//Proceed to check that clicked point is a viable build location
			if (Physics.Raycast (ray, out hit))
			if (hit.rigidbody != null) {
				Debug.Log (hit.normal);

				//Check to see if the angle is too steep
				Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint (hit.point);
				Vector2 normalizedPos = new Vector2 (Mathf.InverseLerp (0.0f, terrain.terrainData.size.x, terrainLocalPos.x), 
					                        Mathf.InverseLerp (0.0f, terrain.terrainData.size.z, terrainLocalPos.z));
				float steepiness = terrain.terrainData.GetSteepness (normalizedPos.x, normalizedPos.y);
				print ("Steepness is " + steepiness); //TODO:debugging purposes, take out when done.
					
				//TODO: don't allow the turret to be built on an angle greater than 30!
				if (steepiness > 30f) {
					Debug.Log ("Can't build there, too steep!");
					return;

					//Create the turret
					GameObject turretToBuild = BuildManager.instance.GetTurretToBuild ();
					turret = (GameObject)Instantiate (turretToBuild, hit.point, transform.rotation);

					//Turn of turretBtnPressed
					//turretBtnPressed = false; //TODO: uncomment this once you get AddTurret.cs to work properly.
				}	
			}
		}
	}

		/*Was the button pushed that adds turret? If so, this function will be accessed by
	 * button script, set the value to true, and inside update, reset to false
	 */
	public void AllowPlacement()
	{
		turretBtnPressed = true;
	}

}
