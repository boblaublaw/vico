//This script allows us to shoot at the enemies in the game

using UnityEngine;


public class ShootingScript : MonoBehaviour
{
	public ParticleSystem impactEffect;	//Particle effect for visual feedback of shot

	AudioSource gunFireAudio;			//Sound clip for audio feedback of shot
	RaycastHit rayHit;					//What line did we shoot down
	static ExceptionsLogger exceptionsLogger;
	public GameObject arrow;
	int speed = 20;

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

		try {
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
		}
		catch 
		{
			exceptionsLogger.AddEntry("raycast fail: ");
		}
		
		try {
			/*
			var instantiatedArrow:Rigibody = Instantiate(arrow, transform.position, transform.rotation);
			instantiatedArrow.velocity = transform.TransformDirection(Vector3 (0,0,speed));
			Physics.IgnoreCollision(instantiatedArrow.collider, transform.root.collider);

			*/
			GameObject arrowObj = Instantiate (arrow) as GameObject; //, transform.position, transform.rotation) as GameObject;
        	arrowObj.transform.position = transform.position + Camera.main.transform.forward * 2;
        	Rigidbody rb = arrowObj.GetComponent <Rigidbody>();
        	rb.velocity = Camera.main.transform.forward * speed;
			//arrowObj.GetComponent<Rigidbody>().velocity = new Vector3 (0,0,speed);
			
    	}
    	catch 
    	{
    		exceptionsLogger.AddEntry("instantiate fail: ");
    	}
    }
}
