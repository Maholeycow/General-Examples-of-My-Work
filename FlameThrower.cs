/* My pride and joy */

using UnityEngine;
using UnityEngine.UI;

public class FlameThrower : TurretMaster {
	
    [Header("Movement Setup")]
    public Transform partToRotate;

    [Header("Renderer/Blast Setup")]
    public bool useMachineGun = false;
    public GameObject flameThrower;
    public GameObject afterBurn;
    public GameObject creatureBlood;
    public GameObject wildfireBurn;
	private GameObject flames;

    [Header("Upgrade: Share the Love")]
	[Range(0,3)]
    public int shareTheLove;
	public float shareChance;

	[Header("Upgrade: Wildfire")]
	[Range(0,3)]
	public int wildfire;
	public float wildChance;
	public float wildDamage = 50f;
	public float wildDuration = 2f;

	public float hitRadius = 2f;
	public float tickSpeed = 0.5f;
	public int collidersPerCreature = 7;

    private Button[] upgradeBtns;

    public override void Initialize()
    {
        upgradeBtns = GetComponentsInChildren<Button>();
        shareTheLove = 0;
        wildfire = 0;
		firingAngle = 10f;
        UpdateTarget();
		fireRate = (int)(fireRate / tickSpeed);
		damage /= (int)(fireRate * collidersPerCreature);

		fireRate = tickSpeed;
    }

    public override void UpdateTarget()
    {
        float dist;
        float closest = Mathf.Infinity;
        int validTargets = 0;

        Collider[] Enemies = Physics.OverlapSphere(this.transform.position, maxrange, mask);

        foreach (Collider g in Enemies)
        {
            dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist > minrange)
            {
                if (dist < closest && ValidTarget(g))
                {
                    validTargets++;
                    closest = dist;
					target = g.GetComponentInParent<CreatureBody>().centerMass;
                }
            }
        }
        if (validTargets == 0)
        {
            target = null;
        }
		if (!target && flames) {
			Destroy (flames);
		}
    }

	public override void DecrementPower() {
		if (power <= 1 && flames) {
			Destroy (flames);
		}
		base.DecrementPower ();
	}

    // Rotates the turret to face the enemy
    // TODO: ensure turret is facing target before firing
    public override void LockOnTarget()
    {
		Vector3 rotation;
		Vector3 dir = target.transform.position - partToRotate.transform.position;
		Quaternion lookRotation = Quaternion.LookRotation (dir);

		//Debug.Log(180f - Vector3.Angle (partToRotate.transform.forward, partToRotate.transform.position - target.transform.position));

		if (Vector3.Angle (-partToRotate.transform.forward, partToRotate.transform.position - target.transform.position) < firingAngle) {
			rotation = Quaternion.Lerp (partToRotate.rotation, lookRotation, turnSpeed).eulerAngles;
			facing = true;
		} else {
			rotation = Quaternion.Lerp (partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
			facing = false;
		}

		//partToRotate.rotation = Quaternion.Euler (rotation.x, rotation.y, 0f);
		partToRotate.rotation = Quaternion.Euler (0f, rotation.y, 0f);

		if (!flames) {
			flames = Instantiate (flameThrower, firePoint.position, firePoint.rotation) as GameObject;
		} else {
			flames.transform.position = firePoint.position;
			flames.transform.rotation = firePoint.rotation;
		}
    }

	public override void Intermission ()
	{
		if (flames) {
			Destroy (flames);
		}
	}

	// we need to move the instantiations to the creatures
    // attack the target enemy, turn on the line renderer
    public override void Shoot()
    {
        //base.Shoot();
		if (flames) {
            base.Shoot();
            Collider[] ToDamage = Physics.OverlapCapsule (firePoint.position + (firePoint.forward * hitRadius), partToRotate.position + (firePoint.forward * maxrange), hitRadius, LayerMask.GetMask("Creatures","CreatureBodyParts"));

			bool wildfireBurn = Random.Range(0f,1f) < wildfire * wildChance; // true if wildfire will occur on this tick

			if (Random.Range(0f,1f) < shareTheLove * shareChance) { // true if sharethelove will occur on this tick
				
			}

			foreach (Collider g in ToDamage) {
				//Destroy (Instantiate (creatureBlood, g.gameObject.GetComponentInParent<CreatureBody> ().transform.position, transform.rotation) as GameObject, 1);\
				if (wildfireBurn) { 
					GameObject burn = Instantiate(g.GetComponentInParent<CreatureBody>().effectPrefab, g.transform.position, g.transform.rotation) as GameObject;
					burn.GetComponent<CreatureStatusController> ().SetState (CreatureStatusController.Status.burn, gameObject, g.gameObject, wildDamage / collidersPerCreature);
					Destroy (burn, wildDuration);
				}
				damageDealt += g.gameObject.GetComponentInParent<CreatureBody> ().TakeDamage (damage, gameObject, CreatureBody.DamageType.Fire);
			}
		}
    }

	/*
    //cause enemies to stay on fire
    private int CauseBurning()
    {
        int timeToBurn = 2;
        timeToBurn *= shareTheLove;
        return timeToBurn;
    }

    //duration of fire on ground
    private int SpreadFire()
    {
        int timeToBurn = 2;
        timeToBurn += wildfire;
        return timeToBurn;
    }
	*/

	public override void TakeDamage(int damageAmount) {
		damageTaken += damageAmount;
		health -= damageAmount;
		if (health <= 0) {
			if (flames) {
				Destroy (flames);
			}
			Destroy (warnMarker);
			Destroy (attackedMarker);
			GameManager._instance.turretManager.turretList.Remove (gameObject);
			Die ();
		}
	}

    //upgradeShareTheLove
	public override GameObject UpgradeOne()
    {
        int cost = upgradeOneCost;

		if (shareTheLove < 3 && GameManager._instance.LoseBiomatter (cost)) {
			shareTheLove++;
			//if (shareTheLove == 3)
			//    upgradeBtns[0].enabled = false;
			//turretCanvas.enabled = !turretCanvas.enabled;
			//buttonText[0].GetComponentInChildren<Text>().text += "\n" + shareTheLove + "/3";
			StatUpgrade ();
			upgradeOneCounter++;
		}
		return null;
    }

    //upgrade wildfire attribute
	public override GameObject UpgradeTwo()
    {
        int cost = upgradeTwoCost;

		if (wildfire < 3 && GameManager._instance.LoseBiomatter (cost)) {
			wildfire++;
			//if (wildfire == 3)
			//    upgradeBtns[1].enabled = false;
			//buttonText[1].GetComponentInChildren<Text>().text += "\n" + wildfire + "/3";
			//turretCanvas.enabled = !turretCanvas.enabled;
			StatUpgrade ();
			upgradeTwoCounter++;
		}
		return null;
    }

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(firePoint.position + (firePoint.forward * hitRadius), hitRadius);
		Gizmos.DrawWireSphere(partToRotate.position + (firePoint.forward * maxrange) , hitRadius);
	}
}
