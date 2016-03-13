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
	nuitrack.Joint bowJoint, headJoint;
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

			Vector3 bowPos = new Vector3 (bowJoint.Real.X, bowJoint.Real.Y, bowJoint.Real.Z); //bowJoint.GetComponent<Rigidbody>().position;
			Vector3 headPos = new Vector3 (headJoint.Real.X, headJoint.Real.Y, headJoint.Real.Z); //headJoint.GetComponent<Rigidbody>().position;
			Vector3 aimingVec = bowPos - headPos;
			arrowObj.transform.position = camTr.position;
			Rigidbody rb = arrowObj.GetComponent <Rigidbody>();
        	rb.velocity = aimingVec.normalized * speed;
        }
        catch
        {
        	exceptionsLogger.AddEntry("shooting problems");
        }
    }
}
