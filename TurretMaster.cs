/*Turret.cs
 * Base class for our machine gun turret
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;

public class TurretMaster : MonoBehaviour {

	//protected allows for these vars to be accessed by child classes, but not elsewhere
	protected GameObject target;
	protected int updateCounter = 0;
	protected int updateRate = 30; // How often (in frames) the turret will acquire a new target
	protected float fireCountdown = 0f;

	//public LayerMask mask = LayerMask.GetMask(new string[] {"Creatures"});

	[Header("Tower Attributes")]
	public float health = 100f;
	public Transform firePoint;
	public Transform centerPoint;
    public Canvas turretCanvas;
    //public 
    public GameObject[] turretPrefabs;
    protected LayerMask mask;
	private int cost = 50;
    public int upgradeOneCost = 100;
    public int upgradeTwoCost = 150;


	[Header("Gun Attributes")]
	public float minrange = 0f;
	public float maxrange = 18f;
	public float fireRate = 1f;
	public int damage = 25;
	public float turnSpeed = 5f;
	[Range(0f,1f)]
	public float accuracy = 0.5f;

	public string shootSoundName;
	protected PoolableReference<AudioObject> shootSound;
    public static TurretMaster instance;
    private Rewired.Player player;
	public GameObject go;
    private int firstClick;
    
	void Start()
    {
        player = ReInput.players.GetPlayer(0);
        instance = this;
        firstClick = 0;
        
        
		if (System.Array.IndexOf (turretPrefabs, transform.gameObject) == 1) {
			GetComponentInChildren<Text> ().text += upgradeTwoCost.ToString ();
		} else {
			GetComponentInChildren<Text> ().text += upgradeOneCost.ToString ();
		}

        mask = LayerMask.GetMask("Creatures");
        turretCanvas.enabled = false;
      //  Debug.Log("Base TurretMaster start");
        Initialize();
    }

	// Anything in inherited classes that needs to be under start() can override
	// Initialize() instead
    public virtual void Initialize()
    {
		DrawRadius ();
    }

    void Update() {
		// Determines when to seek a new target, calls UpdateTarget() to do so
		if (updateCounter > updateRate) {
			UpdateTarget ();
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

	public virtual void UpdateTarget () {

	}

	public virtual void LockOnTarget () {

	}

	public virtual void Shoot () {
		if (shootSound == null)
			shootSound = new PoolableReference<AudioObject> (AudioController.Play (shootSoundName, gameObject.transform));
		else {
			if (shootSound.Get ())
				shootSound.Get ().Play ();
		}
		if (!target) {
			//Debug.Log ("Changing target");
			UpdateTarget ();
		}
	}


	/*Destroy game object upon death*/
	public virtual void TakeDamage(float damageAmount)
	{
		health -= damageAmount;
		if (health <= 0) {
			GameManager._instance.turretManager.turretList.Remove (gameObject);
			Destroy (gameObject);
		}
	}

    void OnMouseOver()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (player.GetButtonDown("LeftClick") /*Input.GetMouseButtonDown(0)*/)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && firstClick == 1)
                {
                    turretCanvas.enabled = !turretCanvas.enabled;
                    TurretManager.instance.CloseMenuOnClick(hit.transform.gameObject);
                    go.SetActive(!go.activeSelf);
                }
                firstClick = 1;
            }
        }
    }

    public virtual void UpgradeOne()
    {
        cost = upgradeOneCost;
        Debug.Log("Upgrade One costs " + cost + ".");
        //GetComponentInChildren<Text>().text = "Upgrade\n" + upgradeTwoCost.ToString();

        if (GameManager._instance.LoseBiomatter(cost))
        {
            GameManager._instance.turretManager.turret = (GameObject)Instantiate(turretPrefabs[0], transform.position, transform.rotation);
            GameManager._instance.turretManager.turretList.Add(GameManager._instance.turretManager.turret);
            GameManager._instance.turretManager.turretList.Remove(gameObject);
            Destroy(gameObject);
        }
        else
        {
            print("Not enough biomatter");
            turretCanvas.enabled = !turretCanvas.enabled;
        }
    }

    public virtual void UpgradeTwo()
    {
        cost = upgradeTwoCost;
        //Debug.Log("Upgrade Two costs " + cost + ".");

        if (GameManager._instance.LoseBiomatter(cost))
        {
            GameManager._instance.turretManager.turret = (GameObject)Instantiate(turretPrefabs[1], transform.position, transform.rotation);
            GameManager._instance.turretManager.turretList.Add(GameManager._instance.turretManager.turret);
            GameManager._instance.turretManager.turretList.Remove(gameObject);
            Destroy(gameObject);
        } else
        {
            print("Not enough biomatter");
            turretCanvas.enabled = !turretCanvas.enabled;
        }
    }

    public virtual bool ValidTarget( Collider target)
	{
		Vector3 curr = centerPoint.transform.position;
		Vector3 enemy = target.gameObject.transform.position;
		Vector3 direction = enemy - curr;

		LayerMask layerMask = (~LayerMask.GetMask("Creatures", "Default", "Ignore Raycast"));
		RaycastHit rh;

		if ( Physics.Raycast (curr, direction, out rh, Vector3.Distance(curr, enemy) , layerMask ) ) {
			//Debug.Log ("Hit " + LayerMask.LayerToName(rh.transform.gameObject.layer) + " Tag " + rh.collider.tag );

			//Debug.DrawRay (curr, direction, Color.green, 2);
			return false; //the raycast hit something that isn't in the 
		}
		//Debug.DrawRay (curr, direction, Color.magenta, 2);
		//Debug.Log ("Shoot " + enemy.transform.gameObject.layer);

		return true; //the raycast didn't hit anything else (besides another creautre maybe) between the turret and creature
	}


	public virtual void DrawRadius() {
		go = Instantiate(Resources.Load("Circle", typeof(GameObject)), transform) as GameObject;
		go.transform.localScale += new Vector3 (maxrange, maxrange, 0);
		go.transform.localPosition += new Vector3 (0, .01f, 0);
		go.SetActive (false);
	}
}
