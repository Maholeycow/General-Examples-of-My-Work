/*Turret.cs
 * Base class for our machine gun turret
 */

using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour {

	private Transform target;
	private CreatureBody targetEnemy;

	[Header("Gun Attributes")]
	public float range = 18f;
	public float fireRate = 1f;
	private float fireCountdown = 0f;
	public float timeSinceLastShot;
	public int damageOverTime = 50;
	public float turnSpeed = 10f;

	//public int turretDamage = 1;
	//public float weaponRange = 18f;

	[Header("Health/Armor Attributes")]
	public float health = 100f;

	[Header("Laser Renderer Setup")]
	public bool useLaser = false;
	public LineRenderer lineRenderer;
	public Transform firePoint;

	[Header("Unity Setup Fields")]
	public string enemyTag = "Creature";
	public Transform partToRotate;
	private LineRenderer laserLine;

	void Start () {
		timeSinceLastShot = fireRate; //initialize timeSinceLastShot
		laserLine = GetComponent<LineRenderer> (); 
		InvokeRepeating ("UpdateTarget", 0f, 0.5f); 	
	}

	void UpdateTarget ()
	{
		GameObject[] enemies = GameObject.FindGameObjectsWithTag (enemyTag); //TODO: This may not be optimized, I place every enemy into array rather than just ones that enter range...
		float shortestDistance = Mathf.Infinity; //infinity while we haven't found an enemy
		GameObject nearestEnemy = null;

		//Calculate distances to all enemies, and identify the shortest distance
		foreach (GameObject enemy in enemies) {
			float distanceToEnemy = Vector3.Distance (transform.position, enemy.transform.position);
			if (distanceToEnemy < shortestDistance) {
				shortestDistance = distanceToEnemy;
				nearestEnemy = enemy;
			}
		}

		//If there is a nearest enemy and it's distance is under our range, get enemy transform and set targetEnemy to the Creature body
		if (nearestEnemy != null && shortestDistance <= range) {
			target = nearestEnemy.transform;
			targetEnemy = nearestEnemy.GetComponent<CreatureBody> ();
		} else {
			target = null;
		}
	}

	//Turn off the lineRenderer (shooting image) if we don't have an enemy within range
	void Update () {
		if (target == null) {
			if (useLaser) {
				if (lineRenderer.enabled) {
					lineRenderer.enabled = false;
				}
			}
			return;
		}

		//Target Lock on
		LockOnTarget();

		//TODO: May need to clean this code up without the useLaser, basically if prefab sets useLaser=true, then it's machine gun so may keep
		if (!useLaser) {
			Laser ();
		} else {
			if (fireCountdown <= 0f) {
				//StartCoroutine (Shoot ());
				//Shoot ();
				Laser (); 
				fireCountdown = 1f / fireRate; //shoot two per second
			} else {
				lineRenderer.enabled = false;
			}

			//used for cooldown
			fireCountdown -= Time.deltaTime;
		}
			
	}

	/*attack the target enemy, turn on the line renderer*/
	void Laser()
	{
		targetEnemy.TakeDamage (damageOverTime);

		//turn on line renderer
		if (!lineRenderer.enabled)
			lineRenderer.enabled = true;

		//project image, this might be changed to simulate machine gun bullets
		lineRenderer.SetPosition (0, firePoint.position);
		lineRenderer.SetPosition (1, target.position);
	}

	/*May be taking this function out, most likely will*/
	void Shoot()
	{
		//laserLine.enable = true;
	}

	/*get the vector direction from turret position to target, Set the look rotation via vect dir,
	 * Lerp allows for a smooth transition (eliminates instantaneous motion, Quternion.Euler rotate about
	 * the y axis via Euler angles
	 */
	void LockOnTarget()
	{
		Vector3 dir = target.position - transform.position;
		Quaternion lookRotation = Quaternion.LookRotation (dir);
		Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
		partToRotate.rotation = Quaternion.Euler (0f, rotation.y, 0f);
	}

	/*Destroy game object upon death*/
	void TakeDamage(float damageAmount)
	{
		health -= damageAmount;

		if (health <= 0) {
			//TODO: gameover
		}
	}

	/*DEBUG PURPOSES ONLY, shows the range of the turrets in the scene view during construction of our masterpiece*/
	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, range);
	}
}
