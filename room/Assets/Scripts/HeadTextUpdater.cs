using UnityEngine;
using System.Collections;

public class HeadTextUpdater : MonoBehaviour 
{
	[SerializeField]TextMesh txt;
	[SerializeField]Transform cameraHead;

	string userInfoText = 
		"Head position: " + System.Environment.NewLine + 
		"{0,5:0} {1,5:0} {2,5:0}" + System.Environment.NewLine + 
		"Euler angles: " + System.Environment.NewLine + 
		"{3,5:0} {4,5:0} {5,5:0}";

	const string textNoUser = "No user";

	void Update () 
	{
		if (NuitrackManager.CurrentUser != 0)
		{
			nuitrack.Joint headJoint = NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Head);
			Vector3 eulerAngles = cameraHead.localRotation.eulerAngles;
			txt.text = string.Format(userInfoText, headJoint.Real.X, headJoint.Real.Y, headJoint.Real.Z, eulerAngles.x, eulerAngles.y, eulerAngles.z);
		}
		else
		{
			txt.text = textNoUser;
		}
	}
}
