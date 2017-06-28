/* Part of ice towers in tower defense game */

using UnityEngine;
using UnityEngine.UI;

public class ChipShredder : ProjectBullet {

	[Header("Attribute Upgrades")]
	[Range(0,3)]
    public int shatter = 0;
	[Range(0f,1f)]
	public float shatterChance = 0.1f;
	public float rendAmount;
	public float rendDuration;
	[Range(0,3)]
    public int shard = 0;
	[Range(0f,1f)]
	public float shardChance = 0.1f;
	public float shardDuration;
	public float shardRange;
	public float shardIntensity;

    private Button[] upgradeBtns;

    public override void Initialize()
    {
        upgradeBtns = GetComponentsInChildren<Button>();
    }

    public override void Shoot()
    {
        if (target && facing)
        {
		    base.Shoot(); // plays firing sound

			Vector3 fireArc = target.transform.position - firePoint.position;
			fireArc = fireArc.normalized * maxrange;
			Vector3 targ = Random.insideUnitSphere * (1 - accuracy) * firingSpread;
			fireArc += targ;
			fireArc = fireArc.normalized * maxrange;

			GameObject bullet = Instantiate (projectile, firePoint.position, transform.rotation);
			bullet.GetComponent<Rigidbody> ().AddForce (fireArc.normalized * projectileSpeed, ForceMode.Impulse);
			bullet.GetComponentInParent<ProjectileScript> ().damage = damage;
			bullet.GetComponentInParent<ProjectileScript> ().source = gameObject;

			if (Random.Range (0f, 1f) < shatterChance * shatter) {
				bullet.GetComponent<ProjectileChipShredder> ().shatter = true;
				bullet.GetComponent<ProjectileChipShredder> ().rendAmount = rendAmount;
				bullet.GetComponent<ProjectileChipShredder> ().rendDuration = rendDuration;
			}

			if (Random.Range (0f, 1f) < shardChance * shard) {
				bullet.GetComponent<ProjectileChipShredder> ().shard = true;
				bullet.GetComponent<ProjectileChipShredder> ().shardRange = shardRange;
				bullet.GetComponent<ProjectileChipShredder> ().shardIntensity = shardIntensity;
				bullet.GetComponent<ProjectileChipShredder> ().shardTime = shardDuration;
			}

            if (bullet != null) //projectile has a script on itself that destroys itself upon collision
                Destroy(bullet, 1f);
        }
    }

	public override GameObject UpgradeOne()
    {
        int cost = upgradeOneCost;

		if (shatter < 3 && GameManager._instance.LoseBiomatter (cost)) {
			shatter++;
			//if (frostbite == 3)
			//	upgradeBtns [0].enabled = false;

			//turretCanvas.enabled = !turretCanvas.enabled;
			upgradeOneCounter++;
			StatUpgrade ();
		}
		return null;
    }

	public override GameObject UpgradeTwo()
    {
        int cost = upgradeTwoCost;

		if (shard < 3 && GameManager._instance.LoseBiomatter (cost)) {
			shard++;
			//if (slipperySlope == 3)
			//	upgradeBtns [1].enabled = false;

			//turretCanvas.enabled = !turretCanvas.enabled;
			upgradeTwoCounter++;
			StatUpgrade ();
		}
		return null;
    }

}
