//This script allows us to shoot at the enemies in the game

using UnityEngine;


public class ShootingScript : MonoBehaviour
{
	public ParticleSystem impactEffect;	//Particle effect for visual feedback of shot

	AudioSource gunFireAudio;			//Sound clip for audio feedback of shot
	RaycastHit rayHit;					//What line did we shoot down
	static ExceptionsLogger exceptionsLogger;
	public GameObject arrow;
	int speed = 35;
	nuitrack.Joint bowJoint;
	[SerializeField]Transform camTr;

	//OVRPlayerController oVPC;	

	void Start()
	{
		//Get a reference to the audio
		gunFireAudio = GetComponent<AudioSource>();
		exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();

		//oVPC=GetComponent<OVRPlayerController>();
		OVRTouchpad.Create();
		OVRTouchpad.TouchHandler += HandleTouchHandler;
	}

	void HandleTouchHandler (object sender, System.EventArgs e)
    {
        OVRTouchpad.TouchArgs touchArgs = (OVRTouchpad.TouchArgs)e;
        OVRTouchpad.TouchEvent touchEvent = touchArgs.TouchType;

        switch (touchEvent) {
        case OVRTouchpad.TouchEvent.SingleTap :
			//Shoot();
			break;
 
        case OVRTouchpad.TouchEvent.Left :
            break;
 
        case OVRTouchpad.TouchEvent.Right :
            Shoot();
            break;
 
        case OVRTouchpad.TouchEvent.Up :
            break;
 
        case OVRTouchpad.TouchEvent.Down :
            break;
        }
    }

    void Shoot()
    {
    	//...play our audio...
		gunFireAudio.Stop();
		gunFireAudio.Play();
		
		GameObject arrowObj = Instantiate (arrow) as GameObject;
		
		try {
			bowJoint = NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.LeftWrist);
			headJoint = NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Head);

			bowPos = bowJoint.GetComponent<Rigidbody>().position;
			headPos = headJoint.GetComponent<Rigidbody>().position;

			//Vector3 bowPosition = new Vector3 (bowJoint.Real.X, bowJoint.Real.Y, bowJoint.Real.Z);
			//exceptionsLogger.AddEntry("bowPosition " + bowPosition);
			//exceptionsLogger.AddEntry("camera " + camTr.transform.position);
			//Vector3 aimingVec = bowPosition - camTr.transform.position;
			Vector3 aimingVec = bowPos - headPos;

			//exceptionsLogger.AddEntry("aimingVec " + aimingVec);
			arrowObj.transform.position = camTr.position;
        	//arrowObj.transform.rotation = Quaternion.Euler(aimingVec.x, aimingVec.y, aimingVec.z);

			Rigidbody rb = arrowObj.GetComponent <Rigidbody>();
        	rb.velocity = camTr.transform.forward * speed;
        	//rb.velocity = aimingVec * speed;
        	
        }
        catch
        {
        	exceptionsLogger.AddEntry("shooting problems");
        }
    }

	/*
		//...and create a ray
		if (Physics.Raycast(transform.position, transform.forward, out rayHit, 100f))
		{
			//If the ray hits something (didn't shoot the sky), move the impact effect to that
			//location and play it
			impactEffect.transform.position = rayHit.point;
			impactEffect.transform.rotation = Quaternion.Euler(270, 0, 0);
			impactEffect.Stop();
			impactEffect.Play();
			//If we hit an enemy Destroy it
			if (rayHit.transform.tag == "Enemy")
			{
				Destroy(rayHit.transform.gameObject);
			}
		} 
		*/

}
