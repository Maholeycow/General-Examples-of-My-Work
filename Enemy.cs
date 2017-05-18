/*Enemy.cs
 * Simple pathfinding script for enemies along a path using only the x axis, staying constant in the y and z
 */

using UnityEngine;

public class Enemy : MonoBehaviour {

	public float speed = 5f;

	private Transform target;
	private int wavepointIndex = 0;
	public float health = 100;
	public int value = 50;

	//public GameObject deathEffect;

	void Start ()
	{
		//serieis of waypoints requires this to be an array
		target = Waypoints.points [0]; 
	}

	public void TakeDamage (float amount)
	{
		health -= amount;

		if (health <= 0) {
			Die ();
		}
	}

	void Die()
	{
		Destroy (gameObject);
	}

	void Update ()
	{
		//every frame, keep the enemy moving towards the waypoint
		Vector3 dir = target.position - transform.position;
		transform.Translate (dir.normalized * speed * Time.deltaTime, Space.World); 

		//Once enemy is close, move to a new waypoint
		if (Vector3.Distance (transform.position, target.position) <= 0.4f) {
			GetNextWaypoint ();
		}
	}

	void GetNextWaypoint()
	{
		//if it is at the end of the set path, destroy object
		//ISSUE: we are seeing that the Destroy function can't keep up when a large # of enemies are present.
		if (wavepointIndex >= Waypoints.points.Length - 1) {
			Destroy (gameObject);
			return;
		}

		wavepointIndex++;

		//sets the target to next way point
		target = Waypoints.points [wavepointIndex];
	}

}
