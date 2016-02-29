using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TouchController : MonoBehaviour
{
  [SerializeField]
  RawImage mapImage;
  [SerializeField]
  UICustomButton 
		closeButton,
  		gameButton,
		quitButton;
  [SerializeField]
  GameObject hangingCube;

  [SerializeField]
  RectTransform cursorL, cursorR;
  [SerializeField]
  Image imageL, imageR;

  [SerializeField]
  Gradient pressureGradient;
  [SerializeField]
  AnimationCurve pressureScale;


  nuitrack.UserHands prev = null, current = null;
  Vector2 baseScale;
  [SerializeField]
  float maxMult = 5f;

  void CloseTouchPanel()
  {
    gameObject.SetActive(false);
  }

  void SwitchCube()
  {
    if (hangingCube != null)
    {
      hangingCube.SetActive(true);
      gameObject.SetActive(false);
    }
  }

	void QuitApplication ()
	{
		Application.Quit ();
	}

  void Start()
  {
    if (closeButton != null) closeButton.action = CloseTouchPanel;
    if (gameButton != null) gameButton.action = SwitchCube;
		if (quitButton != null) quitButton.action = QuitApplication;
    baseScale = new Vector2(mapImage.uvRect.width, mapImage.uvRect.height);
  }

  void LateUpdate()
  {
    if (NuitrackManager.СurrentHands != null)
    {
      prev = current;
      current = NuitrackManager.СurrentHands;

      List<Vector2> handsPos = new List<Vector2>();
      List<bool> handsPress = new List<bool>();

      if ((current.LeftHand != null) && (current.LeftHand.Value.X != -1f))
      {
        Vector3 hand = new Vector3(current.LeftHand.Value.X, 1f - current.LeftHand.Value.Y, 0.5f);
        Vector3 oneMinusHand = Vector3.one - hand;

        handsPos.Add(new Vector2(current.LeftHand.Value.X, 1f - current.LeftHand.Value.Y));
        handsPress.Add(current.LeftHand.Value.Click);

        cursorL.anchorMin = hand;
        cursorL.anchorMax = hand;

        cursorL.localScale = Vector3.one * pressureScale.Evaluate(Mathf.Clamp01(current.LeftHand.Value.Pressure / 200f));
        imageL.color = pressureGradient.Evaluate(Mathf.Clamp01(current.LeftHand.Value.Pressure / 200f));
        imageL.enabled = true;
      }
      else
      {
        handsPos.Add(Vector2.zero);
        handsPress.Add(false);
        imageL.enabled = false;
      }

      if ((current.RightHand != null) && (current.RightHand.Value.X != -1f))
      {
        Vector3 hand = new Vector3(current.RightHand.Value.X, 1f - current.RightHand.Value.Y, 0.5f);
        Vector3 oneMinusHand = Vector3.one - hand;

        handsPos.Add(new Vector2(current.RightHand.Value.X, 1f - current.RightHand.Value.Y));
        handsPress.Add(current.RightHand.Value.Click);

        cursorR.anchorMin = hand;
        cursorR.anchorMax = hand;

        cursorR.localScale = Vector3.one * pressureScale.Evaluate(Mathf.Clamp01(current.RightHand.Value.Pressure / 200f));
        imageR.color = pressureGradient.Evaluate(Mathf.Clamp01(current.RightHand.Value.Pressure / 200f));
        imageR.enabled = true;
      }
      else
      {
        handsPos.Add(Vector2.zero);
        handsPress.Add(false);

        imageR.enabled = false;
      }

      closeButton.TouchesUpdate(handsPos.ToArray(), handsPress.ToArray());
      gameButton.TouchesUpdate(handsPos.ToArray(), handsPress.ToArray());
      quitButton.TouchesUpdate(handsPos.ToArray(), handsPress.ToArray());
      ProcessTouches();
    }
    else
    {
      if (imageL.enabled) imageL.enabled = false;
      if (imageR.enabled) imageR.enabled = false;
    }
  }

  void ProcessTouches()
  {
    /*
    if (current.LeftHand.HasValue) Debug.Log("CL: " + current.LeftHand.Value.X.ToString("0.000000"));
    if (current.RightHand.HasValue) Debug.Log("CR: " + current.RightHand.Value.X.ToString("0.000000"));
    if (prev.LeftHand.HasValue) Debug.Log("PL: " + prev.LeftHand.Value.X.ToString("0.000000"));
    if (prev.RightHand.HasValue) Debug.Log("PR: " + prev.RightHand.Value.X.ToString("0.000000"));
    */
    bool curLeftHandOn = (current.LeftHand.HasValue) && (current.LeftHand.Value.X != -1f);    //-1f in coordinates shows that hand is tracked in world space, but not in virtual plane (can be used with skeleton data to track clicks for example)
    bool curRightHandOn = (current.RightHand.HasValue) && (current.RightHand.Value.X != -1f);
    bool prevLeftHandOn = (prev.LeftHand.HasValue) && (prev.LeftHand.Value.X != -1f);
    bool prevRightHandOn = (prev.RightHand.HasValue) && (prev.RightHand.Value.X != -1f);

    if ((current != null) && (prev != null))
    {
      if ((curLeftHandOn && (current.LeftHand.Value.Click)) &&
           (prevLeftHandOn && (prev.LeftHand.Value.Click)) &&
           (!curRightHandOn || (!current.RightHand.Value.Click)) &&
           (!prevRightHandOn || (!prev.RightHand.Value.Click)))
      {
        HandsTranslate(prev.LeftHand.Value, current.LeftHand.Value);
      }
      else
        if ((curRightHandOn && (current.RightHand.Value.Click)) &&
             (prevRightHandOn && (prev.RightHand.Value.Click)) &&
             (!curLeftHandOn || (!current.LeftHand.Value.Click)) &&
             (!prevLeftHandOn || (!prev.LeftHand.Value.Click)))
      {
        HandsTranslate(prev.RightHand.Value, current.RightHand.Value);
      }
      else
      if ((curLeftHandOn && (current.LeftHand.Value.Click)) &&
         (prevLeftHandOn && (prev.LeftHand.Value.Click)) &&
         (curRightHandOn && (current.RightHand.Value.Click)) &&
         (prevRightHandOn && (prev.RightHand.Value.Click)))
      {
        HandsScale();
      }
    }
  }

  void HandsTranslate(nuitrack.HandContent prevHand, nuitrack.HandContent currHand)
  {
    Vector2 currOffset = new Vector2(mapImage.uvRect.x, mapImage.uvRect.y);
    Vector2 currScale = new Vector2(mapImage.uvRect.width, mapImage.uvRect.height);
    Vector2 delta = Vector2.Scale(new Vector2(currHand.X - prevHand.X, -currHand.Y + prevHand.Y), currScale);
    Vector2 newOffset = new Vector2(Mathf.Repeat(currOffset.x - delta.x, 1f), Mathf.Repeat(currOffset.y - delta.y, 1f));
    mapImage.uvRect = new Rect(newOffset.x, newOffset.y, mapImage.uvRect.width, mapImage.uvRect.height);
  }

  void HandsScale()
  {
    nuitrack.HandContent lhb, rhb, lha, rha; //left/right hand before/after

    lhb = prev.LeftHand.Value;
    rhb = prev.RightHand.Value;
    lha = current.LeftHand.Value;
    rha = current.RightHand.Value;

    float lenBefore, lenAfter;

    lenBefore = Mathf.Sqrt((lhb.X - rhb.X) * (lhb.X - rhb.X) + (lhb.Y - rhb.Y) * (lhb.Y - rhb.Y));
    lenAfter = Mathf.Sqrt((lha.X - rha.X) * (lha.X - rha.X) + (lha.Y - rha.Y) * (lha.Y - rha.Y));

    Vector2 cb, ca; //center before /after

    cb = new Vector2(0.5f * (lhb.X + rhb.X), 0.5f * (2f - lhb.Y - rhb.Y));
    ca = new Vector2(0.5f * (lha.X + rha.X), 0.5f * (2f - lha.Y - rha.Y));

    Vector2 newScale = new Vector2(mapImage.uvRect.width, mapImage.uvRect.height) * (lenBefore / lenAfter);

    if (newScale.x > (baseScale.x * maxMult))
    {
      newScale = baseScale * maxMult;
    }
    if (newScale.x < (baseScale.x / maxMult))
    {
      newScale = baseScale / maxMult;
    }

    Vector2 newOffset =
      new Vector2(mapImage.uvRect.x, mapImage.uvRect.y) +
      Vector2.Scale(cb, new Vector2(mapImage.uvRect.width, mapImage.uvRect.height)) -
      Vector2.Scale(ca, newScale);
    
    //mapImage.material.mainTextureScale = newScale;
    //mapImage.material.mainTextureOffset = newOffset;
    mapImage.uvRect = new Rect(newOffset.x, newOffset.y, newScale.x, newScale.y);
  }
}
