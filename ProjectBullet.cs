/* Used for turrets that project a physical object */

using UnityEngine;

public class ProjectBullet : TurretMaster
{
    [Header("Projectiles")]
    public int projectileSpeed;
    public GameObject projectile;

    //[Header("Movement Setup")]
    public Transform partToRotate;

    //[Header("Particles")]
    public GameObject creatureBlood;

	public override void UpdateTarget() {
		float dist;
		float closest = Mathf.Infinity;
		int validTargets = 0;

		Collider[] Enemies = Physics.OverlapSphere (this.transform.position, maxrange, mask);

		if (targetFurthest)
			closest = Mathf.NegativeInfinity;

		foreach (Collider g in Enemies) {
			dist = Vector3.Distance (transform.position, g.transform.position);
			if (dist > minrange) {
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

	/*
     public override bool ValidTarget(Collider enemy)
     {
         Vector3 direction = enemy.transform.position - transform.position;

         LayerMask layerMask = ~(1 << enemy.gameObject.layer);
         RaycastHit rh;

         if (Physics.Raycast(transform.position, direction, out rh, Vector3.Distance(transform.position, enemy.transform.position), layerMask))
         {
             Debug.Log("Hit " + LayerMask.LayerToName(rh.transform.gameObject.layer) + " Tag " + rh.collider.tag);

             Debug.DrawRay(transform.position, direction, Color.green, 2);
             return false; //the raycast hit something that isn't in the 
         }
         Debug.DrawRay(transform.position, direction, Color.magenta, 2);
         Debug.Log("Shoot " + enemy.transform.gameObject.layer);

         return true; //the raycast didn't hit anything else (besides another creautre maybe) between the turret and creature
     }
     */

     // Rotates the turret to face the enemy
     // TODO: ensure turret is facing target before firing
     public override void LockOnTarget()
     {
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
     public override void Initialize()
     {
         DrawRadius();
     }
     
    public override void Shoot()
	{
		if (target && facing) {
			base.Shoot ();

			Vector3 fireArc = target.transform.position - firePoint.position;
			fireArc = fireArc.normalized * maxrange;
			Vector3 targ = Random.insideUnitSphere * (1 - accuracy) * firingSpread;
			fireArc += targ;
			fireArc = fireArc.normalized * maxrange;

			GameObject bullet = Instantiate (projectile, firePoint.position, transform.rotation);
			bullet.GetComponent<Rigidbody> ().AddForce (fireArc.normalized * projectileSpeed, ForceMode.Impulse);
			bullet.GetComponentInParent<ProjectileScript> ().damage = damage;
			bullet.GetComponentInParent<ProjectileScript> ().source = gameObject;
			if (bullet) // projectile has a script on itself that destroys itself upon collision
				Destroy (bullet, 1f);
		}
    }
}
