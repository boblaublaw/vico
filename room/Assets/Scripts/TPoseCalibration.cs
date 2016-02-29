using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TPoseCalibration : MonoBehaviour 
{
	[SerializeField]float calibrationTime;
	[SerializeField]float maxAngle = 30f;
	[SerializeField]float maxSqrDifference = 10000f;
	[SerializeField]TextMesh calibrationStatusText;

	float timer;
	float cooldown;

	Vector3[] initPositions;
	Vector3[] currentPositions;

	bool calibratedOnce = false;

	static Quaternion sensorOrientation = Quaternion.identity;
	static public Quaternion SensorOrientation {get {return sensorOrientation;}}
	static float collarHeadDistance = 0.3f;
	public static float CollarHeadDistance {get {return collarHeadDistance;}}

	nuitrack.JointType[] checkedJoints = new nuitrack.JointType[]
	{
		nuitrack.JointType.Head, nuitrack.JointType.Torso, 
		nuitrack.JointType.LeftShoulder, nuitrack.JointType.LeftElbow, nuitrack.JointType.LeftWrist,
		nuitrack.JointType.RightShoulder, nuitrack.JointType.RightElbow, nuitrack.JointType.RightWrist
	};
	
	bool calibrationStarted;

	void Start () 
	{
		timer = 0f;
		cooldown = 0f;
		calibrationStarted = false;
		initPositions = new Vector3[checkedJoints.Length];
		currentPositions = new Vector3[checkedJoints.Length];
	}
	
	void Update () 
	{
		if (cooldown > 0f)
		{
			cooldown -= Time.deltaTime;
			if (cooldown <= 0f)
			{
				if (!calibratedOnce)
				{
					calibrationStatusText.text = "Stand in T-pose for" + System.Environment.NewLine + "calibration";
				}
				else
				{
					calibrationStatusText.text = "";
				}
			}
		}
		else
		{
			if (NuitrackManager.CurrentUser != 0)
			{
				if (!calibrationStarted)
				{
					StartCalibration();
				}
				else
				{
					if (timer > calibrationTime)
					{
						SetHeadAngles();

						calibrationStatusText.text = "Calibration" + System.Environment.NewLine + "Done";
						calibratedOnce = true;
						calibrationStarted = false;
						timer = 0f;
						cooldown = calibrationTime;
					}
					else
					{
						ProcessCalibration();
						if (!calibrationStarted)
						{
							if (!calibratedOnce)
							{
								calibrationStatusText.text = "Stand in T-pose for" + System.Environment.NewLine + "calibration";
							}
							else
							{
								calibrationStatusText.text = "";
							}
							timer = 0f;
						}
						else
						{
							calibrationStatusText.text = "Calibration" + System.Environment.NewLine + (100f * timer / calibrationTime).ToString("0");
							timer += Time.deltaTime;
						}
					}
				}
			}
		}
	}

	void StartCalibration()
	{
		Dictionary<nuitrack.JointType, nuitrack.Joint> joints = new Dictionary<nuitrack.JointType, nuitrack.Joint>();

		{
			int i = 0;
			foreach (nuitrack.JointType joint in checkedJoints)
			{
				joints.Add(joint, NuitrackManager.CurrentSkeleton.GetJoint(joint));
				if (joints[joint].Confidence < 0.5f) return;
				initPositions[i] = SkeletonJointToVector3(joints[joint]);
				i++;
			}
		}
		Vector3[] handDeltas = new Vector3[6];

		handDeltas[0] = SkeletonJointToVector3(joints[nuitrack.JointType.LeftWrist]) -  SkeletonJointToVector3(joints[nuitrack.JointType.RightWrist]);
		handDeltas[1] = SkeletonJointToVector3(joints[nuitrack.JointType.LeftWrist]) -  SkeletonJointToVector3(joints[nuitrack.JointType.LeftElbow]);
		handDeltas[2] = SkeletonJointToVector3(joints[nuitrack.JointType.LeftElbow]) -  SkeletonJointToVector3(joints[nuitrack.JointType.LeftShoulder]);
		handDeltas[3] = SkeletonJointToVector3(joints[nuitrack.JointType.LeftShoulder]) -  SkeletonJointToVector3(joints[nuitrack.JointType.RightShoulder]);
		handDeltas[4] = SkeletonJointToVector3(joints[nuitrack.JointType.RightShoulder]) -  SkeletonJointToVector3(joints[nuitrack.JointType.RightElbow]);
		handDeltas[5] = SkeletonJointToVector3(joints[nuitrack.JointType.RightElbow]) -  SkeletonJointToVector3(joints[nuitrack.JointType.RightWrist]);

		for (int i = 1; i < 6; i++)
		{
			if ( Vector3.Angle (handDeltas[0], handDeltas[i]) > maxAngle) 
			{
				return;
			}
		}
		calibrationStarted = true;
	}
	
	void ProcessCalibration()
	{
		Dictionary<nuitrack.JointType, nuitrack.Joint> joints = new Dictionary<nuitrack.JointType, nuitrack.Joint>();

		{
			int i = 0;
			foreach (nuitrack.JointType joint in checkedJoints)
			{
				joints.Add(joint, NuitrackManager.CurrentSkeleton.GetJoint(joint));
				if (joints[joint].Confidence < 0.5f) 
				{
					calibrationStarted = false;
					return;
				}
				currentPositions[i] = SkeletonJointToVector3(joints[joint]);
				i++;
			}
		}

		for (int i = 0; i < initPositions.Length; i++)
		{
			if ((initPositions[i] - currentPositions[i]).sqrMagnitude > maxSqrDifference)
			{
				calibrationStarted = false;
				return;
			}
		}
	}

	void SetHeadAngles()
	{
		float angleY = -Mathf.Rad2Deg * Mathf.Atan2 ((currentPositions[4] - currentPositions[7]).z, (currentPositions[4] - currentPositions[7]).x); //left wrist - right wrist
		float angleX = Vector3.Angle(Vector3.Cross(Vector3.right, Input.gyro.gravity), new Vector3(0f, 0f, -1f));

		Vector3 torso = SkeletonJointToVector3(NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Torso));
		Vector3 neck = SkeletonJointToVector3(NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Neck));
		Vector3 diff = neck - torso;

		sensorOrientation = Quaternion.Euler(-Mathf.Atan2(diff.z, diff.y) * Mathf.Rad2Deg, 0f, 0f);

		Vector3 collar = 0.001f * SkeletonJointToVector3(NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.LeftCollar));
		Vector3 head = 0.001f * SkeletonJointToVector3(NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Head));

		collarHeadDistance = (collar - head).magnitude;

		Debug.Log ("Gravity vector: " + Input.gyro.gravity.ToString("0.000") + "; AngleX: " + angleX.ToString("0") + "; AngleY: " + angleY.ToString("0"));

		SensorRotation camSensor = GameObject.FindObjectOfType<SensorRotation>();
		if (camSensor != null)
		{
			camSensor.SetBaseRotation(Quaternion.Euler(angleX, angleY, 0f));
		}
	}

	Vector3 SkeletonJointToVector3 (nuitrack.Joint joint)
	{
		return new Vector3 (joint.Real.X, joint.Real.Y, joint.Real.Z);
	}
}
