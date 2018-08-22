using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Seiro.Scripts.Utility;
public class RadialMenu : MonoBehaviour {

	public float innerRadius = 1f;
	public float radiusRange = 0.5f;
	public float sectorInterval = 0.1f;
	public float trackInterval = 1f;

	[Header("Range")]
	public float startAngle = 0f;
	public float endAngle = 360f;

	private Stack<Transform> stack;
	public bool Visibled { get { return stack.Count > 0; } }

	private Dictionary<string, Action<GameObject>> clickCallbackDic;

	private int currentFocus = 0;
	private int fragmentCount = 0;
	private float fragmentOffset = 0;

	private GameObject target;
	private GameObject focusPointer;
	public bool fixedFocus = false;

	public GameObject menuController;

	private LerpFloat lerpFloat;
	public float animationT = 10f;
	private float rotateAngle = 0f;
	private float currentAngle = 0f;
	private bool isParentMode = false;

	public Vector3 focusOrigin;
	public Vector3 focusYes;
	public Vector3 focusNo;

	private bool isRotationQueued = false;

	private float rotationAngle = 0.0f;

	#region UnityEvent

	private void Awake() {
		stack = new Stack<Transform>();
		clickCallbackDic = new Dictionary<string, Action<GameObject>>();

		lerpFloat = new LerpFloat();

		if (fixedFocus)
		{
			target = this.gameObject;
			focusPointer = GameObject.Find("FocusPointer").gameObject;
			focusPointer.transform.parent = menuController.transform;
		}
	}

	private void Update() {

		if (!Visibled) return;

		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (isParentMode)
			{
				focusPointer.transform.position= focusYes;
				return;
			}
			currentFocus++;
			if (currentFocus > fragmentCount-1) currentFocus--;
			//print("Up FOCUS="+currentFocus);

			rotateAngle = 360.0f / fragmentCount;
			RotateMenu(target, rotateAngle);
			//print("Angle="+rotateAngle + " Value="+lerpFloat.Value + " Target="+lerpFloat.Target);

		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (isParentMode)
			{
				focusPointer.transform.position = focusNo;
				return;
			}
			currentFocus--;
			if (currentFocus < 0) currentFocus = 0;
			//print("Down FOCUS="+currentFocus);

			rotateAngle = -360.0f / fragmentCount;
			RotateMenu(target, rotateAngle);
			//print("Angle="+rotateAngle + " Value="+lerpFloat.Value + " Target="+lerpFloat.Target);
		}
	}

	#endregion

	#region Function

	public void SetParentMode(bool mode)
	{
		isParentMode = mode;

	}

	private void RotateMenu(GameObject g, float angle)
	{
		if (iTween.Count(g) > 0)
		{
			isRotationQueued = true;
		}

		//print("ROTATE   Angle="+angle + " Value="+lerpFloat.Value + " Target="+lerpFloat.Target);
		iTween.RotateAdd(g, iTween.Hash ("z", angle, "time", 0.4f, "easetype", iTween.EaseType.easeOutExpo, "oncomplete", "OnRotationCompleteHandler"));
		rotationAngle = angle;
	}


	private void OnRotationCompleteHandler ()
	{
		if (isRotationQueued)
		{
			RotateMenu(this.gameObject, rotationAngle);
			isRotationQueued = false;
		}
	}

	private List<T> GetComponentsInChildren<T>(Transform trans) where T : Component {
		// Exception
		if(trans == null) return null;
		if(trans.childCount <= 0) return null;

		List<T> list = new List<T>();
		foreach(Transform e in trans) {
			T t = e.GetComponent<T>();
			if(t != null) list.Add(t);
		}

		fragmentCount = list.Count;
		fragmentOffset = 360.0f / fragmentCount * 0.5f;
		print("GetComponentsInChildren COUNT="+fragmentCount);
		if(list.Count <= 0) {
			return null;
		} else {
			return list;
		}
	}

	private int GetChildDistance(Transform child) {
		int distance = 0;
		while(child != transform) {
			child = child.parent;
			if(child == null) return -1;
			distance++;
		}
		return distance;
	}

	/// <summary>
	/// Show fragments
	/// </summary>
	private void VisibleFragment(List<UICircleFragment> frags, int depth) {

		// Caliculate params
		float deltaAngle = (endAngle - startAngle) / frags.Count;
		float halfInterval = trackInterval * 0.5f;
		float startOffset = deltaAngle > 0f ? halfInterval : -halfInterval;
		float endOffset = -startOffset;
		float overOuter = sectorInterval / radiusRange;
		float clickOuter = overOuter * 0.5f;

		// Show
		for(int i = 0; i < frags.Count; ++i) {
			frags[i].SetManager(this);
			float start = deltaAngle * i + startAngle + startOffset;
			float end = start + deltaAngle + endOffset;
			float inner = innerRadius + (radiusRange + sectorInterval) * depth;
			float outer = inner + radiusRange;
			frags[i].gameObject.SetActive(true);
			frags[i].Visible(start, end, inner, outer);
			frags[i].SetOuterScale(overOuter + 1f, clickOuter + 1f);
		}
	}

	public bool Visible(Transform trans) {
		if(stack.Count == 0) {
			if(trans == transform) {
				List<UICircleFragment> list = GetComponentsInChildren<UICircleFragment>(trans);
				if(list == null) return false;
				VisibleFragment(list, stack.Count);
				stack.Push(trans);
				return true;
			}
		} else {
			int distance = GetChildDistance(trans);
			int adjust = distance - stack.Count;
			if(adjust >= 0) {
				List<UICircleFragment> list = GetComponentsInChildren<UICircleFragment>(trans);
				if(list == null) return false;
				VisibleFragment(list, stack.Count);
				stack.Push(trans);
				return true;
			} else if(adjust < 0) {
				for(int i = adjust; i < 0; ++i) {
					Hide(stack.Pop());
				}
				return Visible(trans);
			}
		}
		return false;
	}

	public void Visible() {
		Visible(transform);
	}

	public void Visible(Vector2 point) {
		transform.localPosition = point;
		Visible(transform);
	}

	private void HideFragment(List<UICircleFragment> frags) {
		for(int i = 0; i < frags.Count; ++i) {
			frags[i].Hide();
		}
	}

	public void Hide(Transform trans) {
		List<UICircleFragment> list = GetComponentsInChildren<UICircleFragment>(trans);
		if(list == null) return;
		HideFragment(list);
		UICircleFragment frag = trans.GetComponent<UICircleFragment>();
		if(frag != null) {
			frag.ResetParentMode();
			focusPointer.transform.position = focusOrigin;
		}
	}

	public void Hide() {
		while(stack.Count > 0) {
			Hide(stack.Pop());
		}
	}

	public void AddClickCallback(string path, Action<GameObject> callback) {
		if(clickCallbackDic.ContainsKey(path)) {
			clickCallbackDic[path] += callback;
		} else {
			clickCallbackDic.Add(path, callback);
		}
	}
	public void FragmentClicked(GameObject gObj) {
		StringBuilder sb = new StringBuilder();
		int i = 0;
		foreach(var e in stack.Reverse()) {
			if(i == 0) {
				++i;
				continue;
			}
			sb.Append(e.name);
			sb.Append(".");
		}
		sb.Append(gObj.name);
		string path = sb.ToString();
		print("CALLBACK=" + path);

		if(clickCallbackDic.ContainsKey(path)) {
			clickCallbackDic[path](gObj);
		} 
	}
	#endregion
}