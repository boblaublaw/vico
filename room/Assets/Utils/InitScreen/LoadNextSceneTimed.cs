using UnityEngine;
using System.Collections;

public class LoadNextSceneTimed : MonoBehaviour 
{
	[SerializeField]float waitTime = 3f;

	IEnumerator Start () 
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		yield return new WaitForSeconds(waitTime);
		Application.LoadLevel(Application.loadedLevel + 1);
	}
}
