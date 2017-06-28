using UnityEngine;

public class MachineGunMaster : TurretMaster {
	
	[Header("Movement Setup")]
	public Transform partToRotate;

	[Header("Renderer/Blast Setup")]
	public bool useMachineGun = false;
	public GameObject projectileBeam;
    public GameObject muzzleFlash;
    public GameObject creatureBlood;

    public override void Initialize () {
		base.Initialize ();
		UpdateTarget ();
	}

	public override void UpdateTarget() {
		float dist;
		float closest = Mathf.Infinity;
		int validTargets = 0;

		Collider[] Enemies = Physics.OverlapSphere (this.transform.position, maxrange, mask);

		if (targetFurthest)
			closest = Mathf.NegativeInfinity;

		foreach (Collider g in Enemies) {
			dist = Vector3.Distance (transform.position, g.transform.position);
			if (dist > minrange){
				if (targetFurthest) {
					if (dist > closest && ValidTarget (g)) {
						validTargets++;
						closest = dist;
						target = g.GetComponentInParent<CreatureBody> ().centerMass;
					}
				} else {
					if (dist < closest && ValidTarget (g)) {
						validTargets++;
						closest = dist;
						target = g.GetComponentInParent<CreatureBody> ().centerMass;
					}
				}
			}
		}
		if (validTargets == 0) {
			target = null;
		}
	}

	// Rotates the turret to face the enemy
	public override void LockOnTarget() {
		Quaternion rotation;
		Vector3 dir = target.transform.position - centerPoint.transform.position;
		Quaternion lookRotation = Quaternion.LookRotation (dir);

		if (Vector3.Angle (-partToRotate.transform.forward, centerPoint.transform.position - target.transform.position) < firingAngle) {
			rotation = Quaternion.Slerp (partToRotate.rotation, lookRotation, turnSpeed);
			facing = true;
		} else {
			rotation = Quaternion.Slerp (partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed);
			facing = false;
		}
		partToRotate.rotation = rotation;
		//partToRotate.rotation = Quaternion.Euler (-rotation.x, rotation.y, 0f);
		//partToRotate.rotation = Quaternion.Euler (0f, rotation.y, 0f);
		//partToRotate.rotation = Quaternion.Lerp (partToRotate.rotation, lookRotation, turnSpeed * Time.deltaTime);
	}

	// attack the target enemy, turn on the line renderer
	public override void Shoot()
	{
		if (target && facing) {
			base.Shoot ();
			//Destroy (Instantiate (muzzleFlash, firePoint) as GameObject, 0.1f);
			//Allows the turret to miss, move accuracy up to improve accuracy. 
			/*
			if (Random.Range (0.0f, 1.0f) <= accuracy) {
				Destroy (Instantiate (creatureBlood, target.transform.position, transform.rotation) as GameObject, 1);
				damageDealt += target.GetComponentInParent<CreatureBody> ().TakeDamage (damage, gameObject);
			}
			*/

			/*
			 *	DEBUG FOR AIMING: this will make several hundred attempts to shoot with current settings to determine
			 *	average accuracy of each shot taken and will draw lines to represent them
			 */
			/*
			hits = 0;
			shots = 0;
			for (int i = 0; i < 200; i++) {
				Vector3 fireArc = target.transform.position - firePoint.transform.position;
				fireArc = fireArc.normalized * maxrange;
				Vector3 targ = Random.insideUnitSphere * (1 - accuracy) * firingSpread;
				fireArc += targ;
				fireArc = fireArc.normalized * maxrange;

				shots++;

				RaycastHit hit;
				if (Physics.Raycast (new Ray (firePoint.transform.position, fireArc), out hit, maxrange, LayerMask.GetMask ("Creatures", "CreatureBodyParts"))) {
					Debug.DrawRay (firePoint.transform.position, fireArc, Color.green, 1);
					hits++;
				} else {
					Debug.DrawRay (firePoint.transform.position, fireArc, Color.red, 1);
				}
				Debug.Log ("Accuracy: " + hits / (shots));
			}
			*/
			/* 
			 * DEBUG END
			 */

			Vector3 fireArc = target.transform.position - firePoint.transform.position;
			fireArc = fireArc.normalized * maxrange;
			Vector3 targ = Random.insideUnitSphere * (1 - accuracy) * firingSpread;
			fireArc += targ;
			fireArc = fireArc.normalized * maxrange;

			shots++;

			RaycastHit hit;
			if (Physics.Raycast (new Ray (firePoint.transform.position, fireArc), out hit, maxrange, LayerMask.GetMask ("Creatures", "CreatureBodyParts"))) {
				Destroy (Instantiate (creatureBlood, hit.point, transform.rotation) as GameObject, 1);
				damageDealt += target.GetComponentInParent<CreatureBody> ().TakeDamage (damage, gameObject);

				GameObject beam = Instantiate (projectileBeam, firePoint);
				beam.GetComponent<LineRenderer> ().SetPosition (0, beam.transform.InverseTransformPoint (hit.point));
				Destroy (beam, 0.1f);
				hits++;



				/* TEST
				//GameObject curr = Instantiate (target.GetComponentInParent<CreatureBody> ().effectPrefab, hit.point, hit.collider.transform.rotation) as GameObject;
				//curr.GetComponent<CreatureStatusController> ().SetState (CreatureStatusController.Status.freeze, gameObject, target);
				//Destroy (curr, 2f);
				*/




			} else {
				GameObject beam = Instantiate (projectileBeam, firePoint);
				beam.GetComponent<LineRenderer> ().SetPosition (0, beam.transform.InverseTransformPoint (fireArc + firePoint.transform.position));
				Destroy (beam, 0.1f);
			}

			//Debug.Log ("Acc: " + actualAccuracy + name);
		}
	}

    /*DEBUG PURPOSES ONLY, shows the range of the turrets in the scene view during construction of our masterpiece*/
    void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, maxrange);
	}
}
