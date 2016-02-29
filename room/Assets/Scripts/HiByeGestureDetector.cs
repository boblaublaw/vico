using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HiByeGestureDetector : MonoBehaviour 
{
	[SerializeField]GameObject guestureScreen;
	[SerializeField]GameObject cubeOnRope;

	public delegate void OnHiBye ();

	OnHiBye onGuesture = null;

	class Difference
	{
		public Vector3 diff;
		public float lifeTime;
	}

	void ShowGeustureScreen()
	{
		if (guestureScreen != null) guestureScreen.SetActive(true);
		if (cubeOnRope.activeSelf) cubeOnRope.SetActive(false);
		onGuesture = HideGeustureScreen;
	}

	void HideGeustureScreen()
	{
		if (guestureScreen != null) guestureScreen.SetActive(false);
		if (!cubeOnRope.activeSelf) cubeOnRope.SetActive(true);
		onGuesture = ShowGeustureScreen;
	}

	void Start()
	{
    NuitrackManager.onNewGesture += OnNewGesture;
		if (guestureScreen.activeSelf) 
		{
			onGuesture = ShowGeustureScreen;
		}
		else
		{
			onGuesture = HideGeustureScreen;
		}
	}

  private void OnNewGesture(nuitrack.Gesture gesture)
  {
    if ((gesture.UserID == NuitrackManager.CurrentUser) && (gesture.Type == nuitrack.GestureType.GestureWaving))
    {
      if (onGuesture != null) onGuesture();
    }
  }
}