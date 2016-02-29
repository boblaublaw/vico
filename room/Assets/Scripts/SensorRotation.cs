using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class SensorRotation : MonoBehaviour
{
  [SerializeField]Toggle magCorrectionToggle;

  Vector3 magneticHeading = Vector3.zero;
  Vector3 gyroGravity = Vector3.down;
  Vector3 gyroRateUnbiased = Vector3.zero;

  Vector3 crossProd = Vector3.zero;

  [SerializeField]
  bool magneticCorrectionOn = true;

  Vector3
    smoothedMagneticHeading = Vector3.zero,
    smoothedGravity = Vector3.zero;

  [SerializeField]
  float dampCoeffVectors = 0.1f;
  [SerializeField]
  float dampCoeffMag = 1f;

  Quaternion baseRotation = Quaternion.identity;
  Quaternion rotation = Quaternion.identity;

  bool correctionOn = true;
  [SerializeField]
  float angleCorrectionOn = 15f;
  [SerializeField]
  float angleCorrectionOff = 3f;

  static string ROOM_BASE_ROTATION = "RoomBaseRotation";

  IEnumerator Start()
  {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    Input.compass.enabled = true;
    Input.gyro.enabled = true;
    yield return null;
    
    try
    {
      LoadBaseRotation ();
      InitRotation ();
    }
    catch
    {
    }
  }

  public void MagneticCorrectionToggle(bool isOn)
  {
    magneticCorrectionOn = isOn;
  }

  void LoadBaseRotation()
  {
    Debug.Log("Loading baseRotation (nuitrack.Nuitrack.GetConfigValue(ROOM_BASE_ROTATION))");
    string configValue = nuitrack.Nuitrack.GetConfigValue(ROOM_BASE_ROTATION);
    Debug.Log("Config value: " + configValue);
    if (configValue == "") return;

    byte[] calibrationInfo = Convert.FromBase64String(configValue);
    int index = 0;
    float x, y, z, w;
    x = BitConverter.ToSingle(calibrationInfo, index);
    index += sizeof(float);
    y = BitConverter.ToSingle(calibrationInfo, index);
    index += sizeof(float);
    z = BitConverter.ToSingle(calibrationInfo, index);
    index += sizeof(float);
    w = BitConverter.ToSingle(calibrationInfo, index);
    Quaternion newBaseRotation = new Quaternion(x, y, z, w);
    baseRotation = newBaseRotation;
    Debug.Log("baseRotation: " + baseRotation.ToString());
  }

  void SaveBaseRotation()
  {
    Debug.Log("Saving baseRotation (nuitrack.Nuitrack.Configure(ROOM_BASE_ROTATION, val))");
    List<byte> calibratedBaseRotation = new List<byte>();
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.x));
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.y));
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.z));
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.w));
    string val = Convert.ToBase64String(calibratedBaseRotation.ToArray());
    nuitrack.Nuitrack.SetConfigValue(ROOM_BASE_ROTATION, val);
  }

  public void SetBaseRotation(Quaternion additionalRotation)
  {
    baseRotation = additionalRotation * Quaternion.Inverse(rotation);
    try
    {
      SaveBaseRotation ();
    }
    catch
    {
    }
  }

  void Update()
  {
    if (Input.touchCount > 1) SetBaseRotation(Quaternion.identity);
  }

  void FixedUpdate()
  {
    Rotate();
  }

  void InitRotation()
  {
    magneticHeading = Input.compass.rawVector;
    magneticHeading = new Vector3(-magneticHeading.y, magneticHeading.x, -magneticHeading.z); // for landscape left

    gyroGravity = Input.gyro.gravity;
    gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);

    smoothedMagneticHeading = magneticHeading;
    smoothedGravity = gyroGravity;

    crossProd = Vector3.Cross(smoothedMagneticHeading, smoothedGravity).normalized;

    rotation = Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity));
    transform.localRotation = baseRotation * rotation;
  }

  void Rotate()
  {
    magneticHeading = Input.compass.rawVector;
    magneticHeading = new Vector3(-magneticHeading.y, magneticHeading.x, -magneticHeading.z); // for landscape left

    gyroGravity = Input.gyro.gravity;
    gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);

    smoothedMagneticHeading = Vector3.Slerp(smoothedMagneticHeading, magneticHeading, dampCoeffVectors);
    smoothedGravity = Vector3.Slerp(smoothedGravity, gyroGravity, dampCoeffVectors);
    
    crossProd = Vector3.Cross(smoothedMagneticHeading, smoothedGravity).normalized;

    gyroRateUnbiased = Vector3.Scale(Input.gyro.rotationRateUnbiased, new Vector3(-1f, -1f, 1f));


    rotation = rotation * Quaternion.Euler(gyroRateUnbiased * Time.deltaTime * Mathf.Rad2Deg);

//    debugGravityEuler.text = 
//      (Mathf.Atan2(gyroGravity.z, -gyroGravity.y) * Mathf.Rad2Deg).ToString("0") + "; 0; " + 
//      (-Mathf.Atan2(gyroGravity.x, -gyroGravity.y) * Mathf.Rad2Deg).ToString("0") + System.Environment.NewLine + 
//      gyroGravity.x.ToString("0.00") + "; " + gyroGravity.y.ToString("0.00") + "; " + gyroGravity.z.ToString("0.00") + System.Environment.NewLine + 
//      rotation.eulerAngles.ToString("0.00");

    //gravity correction :
    Quaternion gravityDiff = Quaternion.FromToRotation(rotation * gyroGravity, Vector3.down);
    Vector3 gravityDiffXZ = new Vector3(gravityDiff.x, 0f, gravityDiff.z);
    Quaternion correction =  Quaternion.Euler(gravityDiffXZ);
    rotation = correction * rotation;

    if (magneticCorrectionOn)
    {
      float deltaAngle = Quaternion.Angle(rotation, Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity)));
      if (deltaAngle > angleCorrectionOn)
      {
        correctionOn = true;
      }
      if (deltaAngle < angleCorrectionOff)
      {
        correctionOn = false;
      }
      if (correctionOn)
      {
        rotation = Quaternion.RotateTowards(rotation, Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity)), Time.deltaTime * dampCoeffMag * deltaAngle);
      }
    }
    transform.localRotation = baseRotation * rotation;
  }
}
