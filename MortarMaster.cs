/* This turret works a lot different from others, since it shoots along an arc */

using UnityEngine;
using UnityEngine.AI;

public class MortarMaster : TurretMaster {

	protected NavMeshAgent targetAgent;
	protected Transform groundTarget;
	protected bool manualMode = false;

	[Header("Movement Setup")]
	public Transform partToRotate;

	[Header("Tower Attributes")]
	public float effectArea = 15f;
	[Range(0f,1f)]
	public float falloff = 0f;

    [Header("Projectile Attributes")]
	public GameObject markPrefab;
	public float timeToImpact = 2f;
    public GameObject[] explosions;


	public override void UpdateTarget () {
		// manualMode tracks whether it is firing at a basic target
		if (!manualMode) {
			float dist;
			float closest = Mathf.Infinity;
			int validTargets = 0;
			NavMeshAgent tempagent;

			Collider[] Enemies = Physics.OverlapSphere (this.transform.position, maxrange, mask);

			foreach (Collider g in Enemies) {
				
				//Debug.Log (this.name + ", " + g.gameObject.name);
				dist = 0;
				if (g.tag == "Creature") {
					tempagent = g.GetComponentInParent<NavMeshAgent> ();
					dist = Vector3.Distance (transform.position, g.transform.position + (tempagent.velocity * timeToImpact));
				}
				else dist = Vector3.Distance (transform.position, g.transform.position);
				if (dist > minrange) {
					if (dist < closest) {
						validTargets++;
						closest = dist;
						target = g.gameObject;
					}
				}
			}
			if (validTargets > 0) {
				if (target.tag == "Creature")
					targetAgent = target.GetComponentInParent<NavMeshAgent> ();
				groundTarget = target.transform;
			} else {
				target = null;
			}
		}
	}

    //Used for toggling between turret AI mode and user mode. Do we want this?? If so should we be able to tell any turret to shoot in specific direction??
    public void ModeChange() {
		// Manual mode, on ground click
		// TODO: set groundTarget to the area to bombard here, toggle
		manualMode = (!manualMode);
		if (manualMode) {

		}
	}

	public override void LockOnTarget() {
		Vector3 dir = groundTarget.position - firePoint.position;
		Quaternion lookRotation = Quaternion.LookRotation (dir);
		Vector3 rotation = Quaternion.Lerp (partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
		partToRotate.rotation = Quaternion.Euler (0f, rotation.y, 0f);
	}

	public override void Shoot() {
		base.Shoot ();
		Vector3 LeadTarget;
		if (target) {
			if (manualMode || groundTarget.tag != "Creature") {
				LeadTarget = groundTarget.position;
			} else {
				LeadTarget = targetAgent.transform.position + (targetAgent.velocity * timeToImpact);
			}
			LeadTarget.y += 40;

			Vector2 accMove = (Random.insideUnitCircle / accuracy) * effectArea;
			LeadTarget.x += (accMove.x);
			LeadTarget.z += (accMove.y);


			RaycastHit hit;
			Physics.Raycast (LeadTarget, Vector3.down, out hit);

			GameObject mark = (GameObject)Instantiate (markPrefab, hit.point, Quaternion.LookRotation (Vector3.right));
			incomingMarker markScript = mark.GetComponentInChildren<incomingMarker> ();
            if (markScript) { 
                markScript.lifetime = timeToImpact;
                markScript.damage = damage;
                markScript.radius = effectArea;
				markScript.falloff = falloff;
				markScript.explosionPrefab = explosions [Random.Range (0, explosions.Length - 1)];
				markScript.source = gameObject;
			}
		}
	}

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.position, maxrange);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position, minrange);
	}

	public override void DrawRadius() {
		base.DrawRadius ();
		GameObject go1 = Instantiate(Resources.Load("Circle", typeof(GameObject)), transform) as GameObject;
		go1.transform.localScale += new Vector3 (minrange, minrange, 0);
		go1.transform.localPosition += new Vector3 (0, .02f, 0);
		go1.GetComponent<SpriteRenderer> ().color = Color.red;
		go1.transform.parent = go.transform;
	}

}
