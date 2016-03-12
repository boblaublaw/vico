using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSMeter : MonoBehaviour
{
  [SerializeField]
  int windowLength = 5;
  double[] dt_msec;
  int pointer = 0;
  System.DateTime prevTime;
  float fps;

  public float Fps { get { return fps; } }

  [SerializeField]
  Text fpsText;

  void Start()
  {
    dt_msec = new double[windowLength];
    prevTime = System.DateTime.Now;
  }

  void Update()
  {

    System.DateTime newTime = System.DateTime.Now;
    dt_msec[pointer] = (newTime - prevTime).TotalMilliseconds;
    prevTime = newTime;
    pointer++;
    pointer = pointer % windowLength;
    double total = 0;
    for (int i = 0; i < windowLength; i++)
    {
      total += dt_msec[i];
    }
    fps = (float)(windowLength * 1000 / total);
    fpsText.text = "FPS:" + fps.ToString("0");
  }
}
