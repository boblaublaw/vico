using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HeadSkeletonMover : MonoBehaviour 
{
	public static string HandTag = "Hands";

	[SerializeField]float lerpFactor;
	[SerializeField]float maxJointSpeed = 5f;
	[SerializeField]GameObject jointPrefab;
	[SerializeField]GameObject jointHeadPrefab;
	[SerializeField]GameObject connectionPrefab;
	[SerializeField]GameObject leftHandPrefab;
	[SerializeField]float neckHeadDistance = 0.15f;
	[SerializeField]Transform camTr;

	nuitrack.JointType[,] jointConnections;
	GameObject[] connections;

	nuitrack.JointType[] availableJoints;
	Dictionary<nuitrack.JointType, GameObject> joints;

	int NUM_CONNECTIONS = 100;

	static ExceptionsLogger exceptionsLogger;

	void Awake()
	{
		NuitrackManager nuitrackInstance = NuitrackManager.Instance;
		exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();
	}

	void Start ()
	{
		availableJoints = new nuitrack.JointType[]
		{
			nuitrack.JointType.Head,
			nuitrack.JointType.Torso, 

			nuitrack.JointType.LeftShoulder,
			nuitrack.JointType.RightShoulder,
			nuitrack.JointType.LeftElbow,
			nuitrack.JointType.RightElbow,
			nuitrack.JointType.LeftWrist,
			nuitrack.JointType.RightWrist,

			/*
			nuitrack.JointType.LeftHip,
			nuitrack.JointType.RightHip,
			nuitrack.JointType.LeftKnee,
			nuitrack.JointType.RightKnee,
			nuitrack.JointType.LeftAnkle,
			nuitrack.JointType.RightAnkle
			*/
		};

		NUM_CONNECTIONS = availableJoints.Length;

		connections = new GameObject[NUM_CONNECTIONS];
		for (int i = 0; i < connections.Length; i++)
		{
			connections[i] = (GameObject)Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity);
			connections[i].SetActive(false);
		}

		jointConnections = new nuitrack.JointType[NUM_CONNECTIONS, 2];
		{
			jointConnections[0, 0] = nuitrack.JointType.LeftWrist;
			jointConnections[0, 1] = nuitrack.JointType.LeftElbow;
			
			jointConnections[1, 0] = nuitrack.JointType.RightWrist;
			jointConnections[1, 1] = nuitrack.JointType.RightElbow;
			
			jointConnections[2, 0] = nuitrack.JointType.LeftShoulder;
			jointConnections[2, 1] = nuitrack.JointType.LeftElbow;
			
			jointConnections[3, 0] = nuitrack.JointType.RightShoulder;
			jointConnections[3, 1] = nuitrack.JointType.RightElbow;
			
			jointConnections[4, 0] = nuitrack.JointType.LeftShoulder;
			jointConnections[4, 1] = nuitrack.JointType.Torso;
			
			jointConnections[5, 0] = nuitrack.JointType.RightShoulder;
			jointConnections[5, 1] = nuitrack.JointType.Torso;
			
			jointConnections[6, 0] = nuitrack.JointType.LeftShoulder;
			jointConnections[6, 1] = nuitrack.JointType.RightShoulder;
			/*
			jointConnections[7, 0] = nuitrack.JointType.Torso;
			jointConnections[7, 1] = nuitrack.JointType.LeftHip;
			
			jointConnections[8, 0] = nuitrack.JointType.Torso; 
			jointConnections[8, 1] = nuitrack.JointType.RightHip;
			
			jointConnections[9, 0] = nuitrack.JointType.LeftHip;
			jointConnections[9, 1] = nuitrack.JointType.RightHip;
			
			jointConnections[10, 0] = nuitrack.JointType.LeftKnee;
			jointConnections[10, 1] = nuitrack.JointType.LeftHip;
			
			jointConnections[11, 0] = nuitrack.JointType.RightKnee;
			jointConnections[11, 1] = nuitrack.JointType.RightHip;
			
			jointConnections[12, 0] = nuitrack.JointType.LeftKnee;
			jointConnections[12, 1] = nuitrack.JointType.LeftAnkle;
			
			jointConnections[13, 0] = nuitrack.JointType.RightKnee;
			jointConnections[13, 1] = nuitrack.JointType.RightAnkle;
			*/
		}

		joints = new Dictionary<nuitrack.JointType, GameObject>();
		foreach (nuitrack.JointType j in availableJoints)
		{
			if (j == nuitrack.JointType.Head)
			{
				GameObject tmp = (GameObject)Instantiate(jointHeadPrefab, Vector3.zero, Quaternion.identity);
				tmp.SetActive(false);
				joints.Add(j, tmp);
			}
			else if (j == nuitrack.JointType.LeftWrist)
			{
				GameObject tmp = (GameObject)Instantiate(leftHandPrefab, Vector3.zero, Quaternion.identity);
				tmp.SetActive(false);
				joints.Add(j, tmp);
				//joints[j].GetComponent<MeshRenderer>().material.color = new Color(0.25f, 0f, 0f, 1f);
				joints[j].tag = HandTag;
			}
			else
			{
				GameObject tmp = (GameObject)Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
				tmp.SetActive(false);
				joints.Add(j, tmp);
				if ((j == nuitrack.JointType.LeftWrist) || (j == nuitrack.JointType.RightWrist))
				{
					joints[j].GetComponent<MeshRenderer>().material.color = new Color(0.75f, 0f, 0f, 1f);
					joints[j].tag = HandTag;
				}
			}
		}
	}

	void FixedUpdate ()
	{
		if (NuitrackManager.CurrentUser != 0)
		{
			UpdateJoints();
			HeadUpdate();
			BowUpdate();
			UpdateJointConnections();
		}
			else //hide user joints if we have no active user
		{
			foreach(nuitrack.JointType j in joints.Keys)
			{
				if (joints[j].activeSelf) joints[j].SetActive(false);
			}

			for (int i = 0; i < NUM_CONNECTIONS; i++)
			{
				if (connections[i].activeSelf) connections[i].SetActive(false);
			}
		}
	}

	void UpdateJointConnections()
	{
		for (int i = 0; i < NUM_CONNECTIONS; i++) // connections
		{
			if (joints[jointConnections[i,0]].activeSelf &&
			    joints[jointConnections[i,1]].activeSelf)
			{
				Vector3 delta = joints[jointConnections[i, 1]].transform.position - 
					joints[jointConnections[i, 0]].transform.position;
				if (delta.magnitude > 0.01f)
				{
					connections[i].transform.position = joints[jointConnections[i, 0]].transform.position;
					connections[i].transform.rotation = Quaternion.LookRotation(delta);
					connections[i].transform.localScale = new Vector3(connections[i].transform.localScale.x, connections[i].transform.localScale.y, delta.magnitude);
					if (!connections[i].activeSelf) connections[i].SetActive(true);
				}
				else
				{
					//joints are too close, no need to render
					connections[i].SetActive(false); 
				}
			}
			else
			{
				if (connections[i].activeSelf) connections[i].SetActive(false);
			}
		}
	}

	void HeadUpdate()
	{
		nuitrack.Joint neckJoint = NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Neck);
		/*
		Vector3 headPos = 
			TPoseCalibration.SensorOrientation * new Vector3(neckJoint.Real.X * 0.001f, neckJoint.Real.Y * 0.001f, neckJoint.Real.Z * 0.001f) + 
			camTr.rotation * new Vector3(0f, neckHeadDistance, 0f);
		*/
		transform.position = joints[nuitrack.JointType.Head].GetComponent<Rigidbody>().position;
		joints[nuitrack.JointType.Head].GetComponent<Rigidbody>().MoveRotation(camTr.rotation);
	}

	void BowUpdate()
	{
		nuitrack.Joint bowJoint = NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.LeftWrist);
		/*Vector3 bowPos = 
			TPoseCalibration.SensorOrientation * new Vector3(neckJoint.Real.X * 0.001f, neckJoint.Real.Y * 0.001f, neckJoint.Real.Z * 0.001f) + 
			camTr.rotation * new Vector3(0f, neckHeadDistance, 0f);
		
		transform.position = joints[nuitrack.JointType.LeftWrist].GetComponent<Rigidbody>().position;
		*/
		joints[nuitrack.JointType.LeftWrist].GetComponent<Rigidbody>().MoveRotation(camTr.rotation);
	}

	void UpdateJoints()
	{
		foreach(nuitrack.JointType j in joints.Keys)
		{
			nuitrack.Joint joint = NuitrackManager.CurrentSkeleton.GetJoint(j);
			Vector3 vPos = TPoseCalibration.SensorOrientation * new Vector3(joint.Real.X * 0.001f, joint.Real.Y * 0.001f, joint.Real.Z * 0.001f);

			try 
			{
				if (j == nuitrack.JointType.Head)
				{
					Vector3 collar = 0.001f * SkeletonJointToVector3(NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.LeftCollar));
					Vector3 torso = 0.001f * SkeletonJointToVector3(NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Torso));
					Vector3 direction = (collar - torso).normalized;
					vPos = TPoseCalibration.SensorOrientation * (collar + TPoseCalibration.CollarHeadDistance * direction);
				}
			}
			catch
			{
				exceptionsLogger.AddEntry("failed on vPos " + j);
			}
			if ( joint.Confidence > 0.5f)
			{
				Vector3 nextPos;
				try 
				{
					 nextPos = Vector3.Lerp(joints[j].GetComponent<Rigidbody>().position, vPos, lerpFactor);
				}
				catch
				{
					exceptionsLogger.AddEntry("Your Joint needs a Rigidbody: " + j);
				}
				nextPos = Vector3.Lerp(joints[j].GetComponent<Rigidbody>().position, vPos, lerpFactor);

				if ((nextPos - joints[j].GetComponent<Rigidbody>().position).magnitude > maxJointSpeed * Time.deltaTime)
				{
					nextPos = Vector3.MoveTowards(joints[j].GetComponent<Rigidbody>().position, vPos, maxJointSpeed * Time.deltaTime);
				}

				joints[j].GetComponent<Rigidbody>().MovePosition(nextPos);
				if (!joints[j].activeSelf) 	
				{
					joints[j].SetActive(true);
				}
			}	
			else
			{
				if (j != nuitrack.JointType.Head) 
				{
					if (joints[j].activeSelf) 
					{
						joints[j].SetActive(false);
					}
				}
			}

		}
	}

	Vector3 SkeletonJointToVector3 (nuitrack.Joint joint)
	{
		return new Vector3 (joint.Real.X, joint.Real.Y, joint.Real.Z);
	}
}