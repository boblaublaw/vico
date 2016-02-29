using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using nuitrack;
using nuitrack.issues;

public class NuitrackIssuesProcessor : MonoBehaviour
{
  [SerializeField]
  Text issueWarningText;

  [SerializeField]
  GameObject 
    bordersParent,
    borderLeft,
    borderRight,
    borderTop;

  void Start ()
  {
    Nuitrack.onIssueUpdateEvent += OnIssues;
  }

  void OnDestroy ()
  {
    Nuitrack.onIssueUpdateEvent -= OnIssues;
  }

  void OnIssues (IssuesData issuesData)
  {
    string issuesString = "";
    {
      OcclusionIssue issue = issuesData.GetUserIssue<OcclusionIssue> (NuitrackManager.CurrentUser);

      if (issue != null)
      {
        issuesString = "Occlusion warning";
      }
      issueWarningText.text = issuesString;
    }
    {
      FrameBorderIssue issue = issuesData.GetUserIssue<FrameBorderIssue> (NuitrackManager.CurrentUser);
      if (issue != null)
      {
        UpdateFrameBorders(issue.Top, issue.Left, issue.Right);
      }
      else
      {
        UpdateFrameBorders(false, false, false);
      }
    }
  }

  void UpdateFrameBorders(bool top, bool left, bool right)
  {
    //TODO: borderParent orientation from TPoseCalibration.SensorOrientation, maybe rotate 
    bordersParent.transform.rotation = TPoseCalibration.SensorOrientation;

    if (borderLeft.activeSelf != left) borderLeft.SetActive(left);
    if (borderRight.activeSelf != right) borderRight.SetActive(right);
    if (borderTop.activeSelf != top) borderTop.SetActive(top);
  }
}
