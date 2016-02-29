using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICustomButton : MonoBehaviour
{
  public delegate void Action();
  public Action action = null;

  [SerializeField]
  Image buttonImage;
  [SerializeField]
  Gradient onHoverGradient;
  [SerializeField]
  float hoverTime;

  float hoverTimer;
  bool isHovered;
  bool[] hadPrevNotPressed = new bool[2];

  void Awake()
  {
    buttonImage.color = onHoverGradient.Evaluate(0f);
    hoverTimer = 0f;
  }

  public void TouchesUpdate(Vector2[] positions, bool[] isPressed)
  {
    int numTouches = positions.Length;
    isHovered = false;
    if (positions == null)
    {
      hoverTimer = Mathf.MoveTowards(hoverTimer, 0f, Time.deltaTime);
      buttonImage.color = onHoverGradient.Evaluate(hoverTimer / hoverTime);
      return;
    }

    for (int i = 0; i < Mathf.Min(numTouches, 2); i++)
    {
      if ((buttonImage.rectTransform.anchorMin.x <= positions[i].x) &&
           (buttonImage.rectTransform.anchorMin.y <= positions[i].y) &&
           (buttonImage.rectTransform.anchorMax.x >= positions[i].x) &&
           (buttonImage.rectTransform.anchorMax.y >= positions[i].y))
      {
        isHovered = true;
        if (!isPressed[i])
        {
          hadPrevNotPressed[i] = true;
        }
        else
        {
          if (hadPrevNotPressed[i])
          {
            if (action != null)
            {
              action();
            }
            hadPrevNotPressed[i] = false;
          }
        }
      }
      else
      {
        hadPrevNotPressed[i] = false;
      }
    }

    if (isHovered)
    {
      hoverTimer = Mathf.MoveTowards(hoverTimer, hoverTime, Time.deltaTime);
      buttonImage.color = onHoverGradient.Evaluate(hoverTimer / hoverTime);
    }
    else
    {
      hoverTimer = Mathf.MoveTowards(hoverTimer, 0f, Time.deltaTime);
      buttonImage.color = onHoverGradient.Evaluate(hoverTimer / hoverTime);
    }
  }
}
