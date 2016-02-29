using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BigRedButton : MonoBehaviour 
{
	public delegate void Action();
	public Action action;

	[SerializeField]GameObject toggledObject;
	[SerializeField]GameObject mapObject;

	[SerializeField]float animationTime = 1f;
	[SerializeField]Transform model;
	[SerializeField]AnimationCurve animX, animY, animZ;
	[SerializeField]MeshRenderer flashingMR;
	[SerializeField]Gradient flashingColor;

	bool isAnimating = false;
	float animationTimer = 0f;
	Vector3 initLocalPos;
	bool isObjEnabled = false;

	void ToggleObject()
	{
		if (toggledObject != null) 
		{
			toggledObject.SetActive(!toggledObject.activeSelf);
			if (toggledObject.activeSelf)
			{
				mapObject.SetActive(false);
			}
			isObjEnabled = !isObjEnabled;
		}
	}

	void Awake()
	{
		action = ToggleObject;
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == HeadSkeletonMover.HandTag)
		{
			if (!isAnimating)
			{
				if (action != null)
				{
					action();
				}
				initLocalPos = model.localPosition;
				isAnimating = true;
				animationTimer = 0f;
			}
		}
	}

	void Update () 
	{
		if (isAnimating)
		{
			animationTimer += Time.deltaTime;

			if (animationTimer > animationTime)
			{
				isAnimating = false;
				animationTimer = 0f;
				model.localPosition = initLocalPos;

				flashingMR.sharedMaterial.color = flashingColor.Evaluate( isObjEnabled ? 1f : 0f);
			}
			else
			{
				model.localPosition = 
					initLocalPos + new Vector3(animX.Evaluate(animationTimer / animationTime), 
					                           animY.Evaluate(animationTimer / animationTime), 
					                           animZ.Evaluate(animationTimer / animationTime));

				flashingMR.sharedMaterial.color = flashingColor.Evaluate( isObjEnabled ? (animationTimer / animationTime) : (1f - animationTimer / animationTime));
			}
		}
	}
}