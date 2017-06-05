/*
 * TurretManager.cs
 * Places a turret into a scene, limits places you can place by checking for steepness of mouseclickpoint 
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Rewired;

public class TurretManager : MonoBehaviour {

	[Header("Miscellaneous")]
	//I think this can be condensed to a local reference in PlaceTurret()
	public GameObject turret;
	public List<GameObject> turretList = new List<GameObject>();
	public Terrain terrain;
	public GameObject workerHut;
	private int cost = 0;
    public static TurretManager instance;

	[Header("Variables for turret placement")]
	public float restrictionRange = 5f;
	public float maxDistance = 15f;
	public float goalDist = 15f;

	[Header("Variables used for buttons")]
	public bool turretBtnPressed;

	[Header("Tower Prefabs")]
	public GameObject machineGunPrefab;
	public GameObject mortarPrefab;
	public GameObject lightningPrefab;

	private GameObject turretToBuild;
	private GameObject turretGhost;
	private GameObject dispGhost;
	private GameObject selectedTurret;
	private bool canPlace;
	private Rewired.Player player;

	/* Thoughts about method for choosing turret placement:
	 * Create a new script to place on buttons. It holds a cost and a prefab.
	 * Have a function that takes that script as an argument. 
	 * Put that function in the button OnClick() and drag the button gameobject itself in.
	 * The function gets the cost and prefab that way--from the script given as an argument.
	 * void SelectTurretToBuild (TurretInfo turretInfo) {
	 * 		cost = turretInfo.cost;
	 * 		turret = turretInfo.turretPrefab;
	 * }
	 * 
	 * public class TurretInfo : MonoBehaviour {
	 * 		public int cost;
	 * 		public GameObject turretPrefab;
	 * }
	 */

	//Please use GameManager._instance.turretManager to reference this script
	void Start()
	{
		player = ReInput.players.GetPlayer (0);
        instance = this;
		turretBtnPressed = false;
		canPlace = false;

	}

	void Update() {
        if (turretBtnPressed && GameManager._instance.biomatter >= cost)
        {
            DisplayGhost();
			if (player.GetButtonDown("LeftClick"))
            {
                PlaceTurret();
            }
        }
        else
        {
			if(player.GetButtonDown("LeftClick"))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                LayerMask layer_mask = LayerMask.GetMask("Towers");

                if (!Physics.Raycast(ray, 100, layer_mask) && !EventSystem.current.IsPointerOverGameObject())
                    CloseMenuOnClick();
            }
        }
        //TODO: The below if statement block can be taken out, was used for debugging lists
		//if (player.GetButtonDown("RightClick"))
        //    print(turretList.Count);
	}

	/*Logic for placing a turret into the scene*/
	private void PlaceTurret()
	{
		if (canPlace) {
			if (!EventSystem.current.IsPointerOverGameObject ()) {
				if (GameManager._instance.LoseBiomatter (cost)) {
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					Physics.Raycast (ray, out hit);
					//Create the turret
					GameObject turretToBuild = GetTurretToBuild ();
					//Debug.Log (turretToBuild.name);
					turret = (GameObject)Instantiate (turretToBuild, hit.point, transform.rotation);
					turretList.Add (turret);
					Destroy (dispGhost);
					//Turn off turretBtnPressed
					turretBtnPressed = false;
					turretToBuild = turretGhost = null;
				} /*else {
					print ("NOT ENOUGH BIOMATTER, KILL MORE CREATURES!!");
				}*/
				canPlace = false;
			}
		}
	}

	void DisplayGhost ()
	{
		canPlace = false;
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit)){
			if (hit.collider != null && terrain.name == terrain.name) {
				//float steepness = Vector3.Angle(hit.normal, transform.up);
				// Check build angle
				float steepness = terrain.terrainData.GetSteepness (hit.point.x / terrain.terrainData.size.x, hit.point.z / terrain.terrainData.size.z);
				if (steepness <= 30) {
					// Check restrictionrange
					LayerMask mask = LayerMask.GetMask ("Towers");
					Collider[] Turrets = Physics.OverlapSphere (hit.point, restrictionRange, mask);
					Collider[] TurretBuildDistance = Physics.OverlapSphere (hit.point, maxDistance, mask);
					//Debug.Log (Turrets.Length + " " + TurretBuildDistance.Length);
					//Debug.Log ("Turret count is " + turretList.Count);
					float dis = Vector3.Distance (GameManager._instance.regionManager.goal.transform.position, hit.point);
					//Debug.Log ("Distance is " + dis);
					if (Turrets.Length == 0 && (TurretBuildDistance.Length >= 1 || dis < goalDist)) {
						canPlace = true;
					}
				}

				if (dispGhost) {
					dispGhost.transform.position = hit.point;
					dispGhost.transform.rotation = hit.transform.rotation;
				} else {
					dispGhost = Instantiate (turretGhost, hit.point, hit.transform.rotation) as GameObject;
				}
			}
		}
		ghostTurret curr = dispGhost.GetComponent<ghostTurret> ();
		curr.SetMode (canPlace);
	}

	public GameObject GetTurretToBuild()
	{
		return turretToBuild;
	}

	/* Note for the below functions, may be able to be condensed/optimized. I know
     * I have a lot going on, but I was cramming the buildmanager script and shop script
     * I had into this script so let me know if you want edits to this Sam =)
     */ 

	/* Set the value for turretToBuild to the turret selected from GUI */
	public void SelectTurretToBuild(GameObject turret)
	{
		turretBtnPressed = (!turretBtnPressed);
		if (turretBtnPressed) {
            ghostTurret curr = turret.GetComponent<ghostTurret> ();
            turretToBuild = curr.turret;
            turretGhost = turret;
		} else {
			Destroy (dispGhost);
			turretBtnPressed = false;
			if (turret.name != turretGhost.name) {
				SelectTurretToBuild (turret);
			}
		}
	}


    public void TurretSelected(int amount, GameObject model)
    {
        cost = amount;
        SelectTurretToBuild(model);
    }

    /* If the user clicks anywhere, or on another turret, close the upgrade menu for turret 
     * IDEA FOR OPTIMIZATION: Store whatever turret that has its upgrade menu up and upon click close that menu
     */
     public void CloseMenuOnClick()
     {
		foreach (GameObject tower in turretList)
			if (tower.GetComponentInChildren<Canvas> ().enabled == true) {
				tower.GetComponentInChildren<Canvas> ().enabled = false;
				tower.GetComponent<TurretMaster> ().go.SetActive (false);
			}
    }

    public void CloseMenuOnClick(GameObject turretClicked)
    {
        foreach (GameObject tower in turretList)
        {
            if (turretClicked.GetInstanceID() == tower.GetInstanceID())
            {
                //Debug.Log("Instance ID section of CloseMenuOnClick***");
                continue;
            }
            if (tower.GetComponentInChildren<Canvas>().enabled == true)
            {
                tower.GetComponentInChildren<Canvas>().enabled = false;
				tower.GetComponent<TurretMaster> ().go.SetActive (false);
                //Debug.Log("Second if of CloseMenuOnClick***");
            }
        }
    }

    /* Destory all game objects (turrets) in our list, and then empty the list out */
    public void ResetTurrets()
	{
		foreach (GameObject tower in turretList)
			Destroy(tower);

		turretList.Clear();
	}

}
