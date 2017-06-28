/*Turret.cs
 * Base class for our machine gun turret
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;
using System;

public class TurretMaster : MonoBehaviour {

	public GameObject target;

	//public GameObject warnMarkPrefab;
	public GameObject killMarkPrefab;
	public GameObject warnMarkPrefab;
	public float warnDuration = 3f;
	protected float warnTime;
	protected GameObject attackedMarker;
	protected GameObject warnMarker;

	protected int updateCounter = 0;
	protected int updateRate = 30; 
	protected float fireCountdown = 0f;

	[Header("Tower Attributes")]
	public float maxHealth = 100f;
	public float health;
	public Transform firePoint;
	public Transform centerPoint;
    	public Canvas turretCanvas;
	public Projector groundIndicator;
   	 public GameObject hitPointEffect; //particle effect when hitting aliens
	public GameObject[] turretPrefabs;
	public StatUpgrade statUpgrade;
    	protected LayerMask mask;
	protected int cost = 50;
    	public int upgradeOneCost = 100;
    	public int upgradeTwoCost = 150;
	public int upgradeOneNum = 1;
	[HideInInspector]public int upgradeOneCounter = 0;
	public int upgradeTwoNum = 0;
	[HideInInspector]public int upgradeTwoCounter = 0;
	public int repairCost = 25;
	public bool targetFurthest = false;


	[Header("Gun Attributes")]
	public float minrange = 0f;
	public float maxrange = 18f;
	public float fireRate = 1f;
	public int damage = 25;
	public float turnSpeed = 5f;
	[Range(0f,1f)]
	public float accuracy = 0.5f;
	public float firingAngle = 1f;
	public float firingSpread = 3f;

	public string shootSoundName;
	protected PoolableReference<AudioObject> shootSound;
	protected Rewired.Player player;
	public GameObject go;
	protected int firstClick;
    	public Component[] buttonText;
	public int power = 0;
	protected bool facing;
	public string XMLpath;
	protected float hits = 0;
	protected float shots = 0;
	public float actualAccuracy;


	/*
	[HideInInspector]public int kills;
	[HideInInspector]public int damageDealt;
	[HideInInspector]public float DPS;
	[HideInInspector]public int damageTaken;
	*/
	public int kills;
	public int damageDealt;
	public float DPS;
	public int damageTaken;
	protected float timeActive;
    
	void Awake()
    {
		health = maxHealth;
		facing = true;
        player = ReInput.players.GetPlayer(0);
        firstClick = 0;

		updateCounter = UnityEngine.Random.Range (0, updateRate);

        buttonText = GetComponentsInChildren<Button>();
		if (buttonText.Length > 0) {
			buttonText [0].GetComponentInChildren<Text> ().text += "\n" + upgradeOneCost.ToString ();
		}
		if (buttonText.Length > 1) {
			buttonText [1].GetComponentInChildren<Text> ().text += "\n" + upgradeTwoCost.ToString ();
		}

		mask = LayerMask.GetMask(new string [] {"Creatures"});
		if (turretCanvas != null)
	        turretCanvas.enabled = false;
        Initialize();
		//groundIndicator.material = new Material(cookieMat);
		groundIndicator.orthographicSize = maxrange;
		groundIndicator.enabled = false;
		turnSpeed = 10f;
		CalculatePower ();
    }

	void Start() {
		warnMarker = Instantiate (warnMarkPrefab, transform.position, transform.rotation);
	}

	void CalculatePower() {
		GameObject pylon = GameManager._instance.regionManager.goal;
		float dist = Vector3.Distance (pylon.transform.position, transform.position);
		if (dist < GameManager._instance.turretManager.maxDistance) {
			power++;
		}
		for (int i = 0; i < GameManager._instance.turretManager.pylonList.Count; i++) {
			pylon = GameManager._instance.turretManager.pylonList [i];
			if (pylon.GetComponent<Pylon> ().activated) {
				dist = Vector3.Distance (pylon.transform.position, transform.position);
				if (dist < GameManager._instance.turretManager.maxDistance) {
					power++;
				}
			}
		}
	}

	public void IncrementPower() {
		power++;
	}

	public virtual void DecrementPower() {
		power--;
		if (power <= 0) {
			//turretCanvas.enabled = false;
			groundIndicator.enabled = false;
			enabled = false;
		}
	}

	// Anything in inherited classes that needs to be under start() can override
	// Initialize() instead
    public virtual void Initialize()
	{
		//DrawRadius ();
	}

    void Update() {
		if (GameManager._instance.IsIntermission ()) {
			Intermission ();
			return;
		}

		if (attackedMarker) {
			warnTime -= Time.deltaTime;
		}
		if (warnTime <= 0) {
			Destroy (attackedMarker);
		}

		timeActive += Time.deltaTime;


		// Determines when to seek a new target, calls UpdateTarget() to do so
		if (updateCounter > updateRate) {
			UpdateTarget ();
			DPS = damageDealt / timeActive;
			if (shots != 0) {
				actualAccuracy = hits / shots;
			} else {
				actualAccuracy = 0;
			}
			updateCounter = 0;
		} else {
			updateCounter++;
		}

		// Keeps track of firing cooldown
		if (fireCountdown > 0)
			fireCountdown -= Time.deltaTime;

		// TODO: check for a mouse click

		if (target == null)
			return;

		// Rotate turret toward target if target exists
		LockOnTarget ();

		// Fires on a target if it's selected
		if (fireCountdown <= 0) {
			fireCountdown = fireRate;
			Shoot ();
		}
	}

	public virtual void Intermission() {
		if (attackedMarker)
			Destroy (attackedMarker);
	}

	public virtual void UpdateTarget () {

	}

	public virtual void LockOnTarget () {
		
	}

	public virtual void Shoot () {
		/*if (shootSound == null)
			shootSound = new PoolableReference<AudioObject> (AudioController.Play (shootSoundName, gameObject.transform));
		else {
			if (shootSound.Get ())
				shootSound.Get ().Play ();
		}*/
		AudioController.Play (shootSoundName, transform);
		/*if (!target) { 
			UpdateTarget ();
		}*/
	}


	/*Destroy game object upon death*/
	public virtual void TakeDamage(int damageAmount)
	{
		health -= damageAmount;
		damageTaken += damageAmount;
		warnTime = warnDuration;

		if (health <= 0) {
			Destroy (warnMarker);
			Destroy (attackedMarker);
			GameManager._instance.turretManager.turretList.Remove (gameObject);
			Die ();
		} else if (!attackedMarker) {
			attackedMarker = Instantiate (killMarkPrefab, transform.position, transform.rotation);
		}
	}

	protected virtual void Die() {
		Destroy (Instantiate (killMarkPrefab, transform.position, transform.rotation), 15f);
		Destroy (gameObject);
	}

    public void Clicked()
    {
		if (power <= 0)
			return;
		GameManager._instance.guiManager.SetInfoPanel (gameObject);
		GameManager._instance.turretManager.CloseMenuOnClick (gameObject);
		groundIndicator.enabled = true;
    }

	public virtual GameObject UpgradeOne()
    {
        cost = upgradeOneCost;

        if (GameManager._instance.LoseBiomatter(cost))
        {
            GameManager._instance.turretManager.turret = (GameObject)Instantiate(turretPrefabs[0], transform.position, transform.rotation);
            GameManager._instance.turretManager.turretList.Add(GameManager._instance.turretManager.turret);
            GameManager._instance.turretManager.turretList.Remove(gameObject);
			GameManager._instance.turretManager.turret.GetComponent<TurretMaster> ().health = GameManager._instance.turretManager.turret.GetComponent<TurretMaster> ().maxHealth - (maxHealth - health);
            Destroy(gameObject);
			upgradeOneCounter++;
			return GameManager._instance.turretManager.turret;
        }
        else
        {
            print("Not enough biomatter");
            //turretCanvas.enabled = !turretCanvas.enabled;
			groundIndicator.enabled = !groundIndicator.enabled;
        }
		return null;
    }

	public virtual GameObject UpgradeTwo()
    {
        cost = upgradeTwoCost;

		if (GameManager._instance.LoseBiomatter (cost)) {
			GameManager._instance.turretManager.turret = (GameObject)Instantiate (turretPrefabs [1], transform.position, transform.rotation);
			GameManager._instance.turretManager.turretList.Add (GameManager._instance.turretManager.turret);
			GameManager._instance.turretManager.turretList.Remove (gameObject);
			TurretMaster curr = GameManager._instance.turretManager.turret.GetComponent<TurretMaster> ();

			curr.health = GameManager._instance.turretManager.turret.GetComponent<TurretMaster> ().maxHealth - (maxHealth - health);
			curr.kills = kills;
			curr.damage = damage;
			curr.accuracy = accuracy;
			curr.DPS = DPS;
			curr.damageTaken = damageTaken;

			Destroy (gameObject);
			upgradeTwoCounter++;
			return GameManager._instance.turretManager.turret;
		} else {
			print ("Not enough biomatter");
			//turretCanvas.enabled = !turretCanvas.enabled;
			groundIndicator.enabled = !groundIndicator.enabled;
		}
		return null;
    }

    public virtual bool ValidTarget( Collider target)
	{
		Vector3 curr = firePoint.transform.position;
		Vector3 enemy = target.gameObject.transform.position;
		enemy = new Vector3 (enemy.x, enemy.y + 0.5f, enemy.z);
		Vector3 direction = enemy - curr;

		LayerMask layerMask = (~LayerMask.GetMask("Creatures", "Default", "Ignore Raycast", "SpawnPoint"));
		RaycastHit rh;

		if ( Physics.Raycast (curr, direction, out rh, Vector3.Distance(curr, enemy) - 1.5f , layerMask ) && rh.collider.gameObject != gameObject) {
			//Debug.Log ("Hit " + LayerMask.LayerToName(rh.transform.gameObject.layer) + " Tag " + rh.collider.tag );

			//Debug.DrawRay (curr, direction, Color.green, 2);
			return false; //the raycast hit something that isn't in the 
		}
		//Debug.DrawRay (curr, direction, Color.magenta, 2);
		//Debug.Log ("Shoot " + enemy.transform.gameObject.layer);

		return true; //the raycast didn't hit anything else (besides another creautre maybe) between the turret and creature
	}

	public void addKill() {
		kills++;
	}

	public virtual void DrawRadius() {
		go = Instantiate(Resources.Load("Circle", typeof(GameObject)), transform) as GameObject;
		go.transform.localScale += new Vector3 (maxrange, maxrange, 0);
		go.transform.localPosition += new Vector3 (0, .01f, 0);
		go.SetActive (false);
	}

	public virtual void Repair() {
		if (GameManager._instance.LoseBiomatter(repairCost))
		{
			health = maxHealth;
		}
	}

	public virtual void StatUpgrade() {
		maxHealth += statUpgrade.health;
		health += statUpgrade.health;
	}
}

[Serializable]
public struct StatUpgrade {
	public int health;
}
