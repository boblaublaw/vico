using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;

public class NuitrackManager : MonoBehaviour
{
  static nuitrack.HandTracker handTracker;
  static nuitrack.SkeletonTracker skeletonTracker;
  static nuitrack.GestureRecognizer gestureRecognizer;
  static int currentUser = 0;

  public static int CurrentUser{ get { return currentUser; } }

  static nuitrack.Skeleton currentSkeleton;

  public static nuitrack.Skeleton CurrentSkeleton { get { return currentSkeleton; } }

  static nuitrack.UserHands currentHands;

  public static nuitrack.UserHands СurrentHands { get { return currentHands; } }

  static NuitrackManager instance;

  static ExceptionsLogger exceptionsLogger;

  public static NuitrackManager Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindObjectOfType<NuitrackManager> ();
        if (instance == null)
        {
          GameObject container = new GameObject ();
          container.name = "NuitrackManager";
          instance = container.AddComponent<NuitrackManager> ();
        }
        DontDestroyOnLoad (instance);
      }
      return instance;
    }
  }

  public delegate void OnNewGestureHandler (nuitrack.Gesture gesture);

  public static event OnNewGestureHandler onNewGesture;

  void Awake ()
  {
    DontDestroyOnLoad (gameObject);
    exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();
    NuitrackInitState state = NuitrackLoader.InitNuitrackLibraries();
    if (state != NuitrackInitState.INIT_OK)
    {  
      exceptionsLogger.AddEntry("Nuitrack native libraries iniialization error: " + Enum.GetName(typeof(NuitrackInitState), state));
    }
  }

  void NuitrackInit ()
  {
    nuitrack.Nuitrack.Init ();
    //all needed nuitrack modules should be created between Init and Run
    skeletonTracker = nuitrack.SkeletonTracker.Create();
    skeletonTracker.OnSkeletonUpdateEvent += HandleOnSkeletonUpdateEvent;
    
    handTracker = nuitrack.HandTracker.Create();
    handTracker.OnUpdateEvent += HandleOnHandsUpdateEvent;
    
    gestureRecognizer = nuitrack.GestureRecognizer.Create();
    gestureRecognizer.OnNewGesturesEvent += OnNewGestures;

    nuitrack.Nuitrack.Run ();
  }

  void Start ()
  {
    NuitrackInit ();
  }

  private void OnNewGestures (nuitrack.GestureData gestures)
  {
    if (gestures.NumGestures > 0)
    {
      if (onNewGesture != null)
      {
        for (int i = 0; i < gestures.Gestures.Length; i++)
        {
          onNewGesture (gestures.Gestures [i]);
        }
      }
    }
  }

  void HandleOnHandsUpdateEvent (nuitrack.HandTrackerData handTrackerData)
  {
    if (handTrackerData == null)
      return;
    if (currentUser != 0)
    {
      currentHands = handTrackerData.GetUserHandsByID (currentUser);
    }
    else
    {
      currentHands = null;
    }
  }

  void HandleOnSkeletonUpdateEvent (nuitrack.SkeletonData skeletonData)
  {
    if (skeletonData == null)
      return;

    if (currentUser != 0)
    {
      currentUser = (skeletonData.GetSkeletonByID (currentUser) == null) ? 0 : currentUser;
    }

    if (skeletonData.NumUsers == 0)
    {
      currentSkeleton = null;
      return;
    }

    if (currentUser == 0)
    {
      currentUser = skeletonData.Skeletons [0].ID;
    }
    currentSkeleton = skeletonData.GetSkeletonByID (currentUser);
  }

  void OnApplicationPause (bool pauseStatus)
  {
    if (pauseStatus)
    {
      CloseUserGen ();
    }
    else
    {
      NuitrackInit ();
    }
  }

  void Update ()
  {
    nuitrack.Nuitrack.Update ();
  }

  public void CloseUserGen ()
  {
    skeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
    handTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;
    gestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
    nuitrack.Nuitrack.Release ();
  }

  void OnDestroy ()
  {
    CloseUserGen ();
  }
}