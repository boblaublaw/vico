//This script allows us to exit the game if it is fullscreen
//or run on mobile platforms. Does not work in editor or on iOS

using UnityEngine;

public class QuitApplicationScript : MonoBehaviour
{
	void Update ()
	{
		//Check for Cancel input axis or Escape keyboard key. Checking for the 
		//keyboard key is a requirement of the Gear VR back button
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
			Application.Quit();
	}
}
