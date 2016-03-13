//This script uses the navmesh agent to move the enemy
//towards their target

using UnityEngine;

public class EnemyNavigation : MonoBehaviour {

	public Transform target;	//Target to attack
	NavMeshAgent agent;			//Reference to navmesh agent

	void Start()
	{
		//Get a reference to the agent component
		agent = GetComponent<NavMeshAgent> ();
    }

	void Update()
	{
		//Check to see if the enemy has a target but needs a new path
		//Needs to be rechecked every so often in case the player pauses or
		//minimizes the game on mobile platforms
		if (target && !agent.hasPath)
		{
			//Tell the enemy to go towards the target
			agent.SetDestination(target.position);
		}
	}
}
