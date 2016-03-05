using UnityEngine;
using System;
using System.Collections;

public class NuitrackModules : MonoBehaviour 
{
	[SerializeField]GameObject depthUserVisualizationPrefab;
	[SerializeField]GameObject skeletonsVisualizationPrefab;
	[SerializeField]GameObject issuesProcessorPrefab;

	GameObject 
		depthUserVisualization,
		skeletonsVisualization,
		issuesProcessor;


	nuitrack.DepthSensor depthSensor = null;
	nuitrack.UserTracker userTracker = null;
	nuitrack.SkeletonTracker skeletonTracker = null;

	public nuitrack.DepthSensor DepthSensor {get {return this.depthSensor;}}
	public nuitrack.UserTracker UserTracker {get {return this.userTracker;}}
	public nuitrack.SkeletonTracker SkeletonTracker {get {return this.skeletonTracker;}}

	nuitrack.DepthFrame depthFrame = null;
	nuitrack.UserFrame userFrame = null;
	nuitrack.SkeletonData skeletonData = null;

	public nuitrack.DepthFrame DepthFrame {get {return this.depthFrame;}}
	public nuitrack.UserFrame UserFrame {get {return this.userFrame;}}
	public nuitrack.SkeletonData SkeletonData {get {return this.skeletonData;}}

	ExceptionsLogger exceptionsLogger;

	void Awake () 
	{
		exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();
		NuitrackInitState state = NuitrackLoader.InitNuitrackLibraries();
		if (state != NuitrackInitState.INIT_OK)
		{
			exceptionsLogger.AddEntry("Nuitrack native libraries iniialization error: " + Enum.GetName(typeof(NuitrackInitState), state));
		}
	}
		
	public void InitModules()
	{
		try
		{
      nuitrack.Nuitrack.Init();

      depthSensor = nuitrack.DepthSensor.Create();
      userTracker = nuitrack.UserTracker.Create();
      skeletonTracker = nuitrack.SkeletonTracker.Create();

      depthSensor.OnUpdateEvent += DepthUpdate;
      userTracker.OnUpdateEvent += UserUpdate;
      skeletonTracker.OnSkeletonUpdateEvent += SkeletonsUpdate;

		nuitrack.Nuitrack.Run ();

      issuesProcessor = (GameObject)Instantiate(issuesProcessorPrefab);
      depthUserVisualization = (GameObject)Instantiate(depthUserVisualizationPrefab);
      skeletonsVisualization = (GameObject)Instantiate(skeletonsVisualizationPrefab);
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}


	public void ReleaseNuitrack()
	{
    if (issuesProcessor != null)  Destroy(issuesProcessor);
		if (depthUserVisualization != null) Destroy (depthUserVisualization);
		if (skeletonsVisualization != null) Destroy (skeletonsVisualization);

		depthSensor = null;
		userTracker = null;
		skeletonTracker = null;

		nuitrack.Nuitrack.Release();
	}

	void OnDestroy()
	{
		ReleaseNuitrack();
	}

	void Update () 
	{
		try
		{
			nuitrack.Nuitrack.Update();
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	void DepthUpdate(nuitrack.DepthFrame _depthFrame)
	{
		depthFrame = _depthFrame;
	}

	void UserUpdate(nuitrack.UserFrame _userFrame)
	{
		userFrame = _userFrame;
	}

	void SkeletonsUpdate(nuitrack.SkeletonData _skeletonData)
	{
		skeletonData = _skeletonData;
	}
}