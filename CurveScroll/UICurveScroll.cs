using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UICurveScroll : UICurveScrollBase 
{
	override protected void Awake()
	{
		smPos.StartFresh(0);

		fVisibleRangeMin = 0;
		fVisibleRangeMax = 0;

		float min = 0;
		float max = 0;
		GameUtils.GetCurveTimeBound(cvX, out min, out max);
		fVisibleRangeMin = Mathf.Min(fVisibleRangeMin, min);
		fVisibleRangeMax = Mathf.Max(fVisibleRangeMax, max);

		GameUtils.GetCurveTimeBound(cvY, out min, out max);
		fVisibleRangeMin = Mathf.Min(fVisibleRangeMin, min);
		fVisibleRangeMax = Mathf.Max(fVisibleRangeMax, max);

		GameUtils.GetCurveTimeBound(cvZ, out min, out max);
		fVisibleRangeMin = Mathf.Min(fVisibleRangeMin, min);
		fVisibleRangeMax = Mathf.Max(fVisibleRangeMax, max);

		GameUtils.GetCurveTimeBound(cvScale, out min, out max);
		fVisibleRangeMin = Mathf.Min(fVisibleRangeMin, min);
		fVisibleRangeMax = Mathf.Max(fVisibleRangeMax, max);

		GameUtils.GetCurveTimeBound(cvVisibility, out min, out max);
		fVisibleRangeMin = Mathf.Min(fVisibleRangeMin, min);
		fVisibleRangeMax = Mathf.Max(fVisibleRangeMax, max);
	}


	public void OnDragStart() { }

	public void OnDraging(Vector2 vDrag)
	{
		float fDestPos = smPos.fCur + vDrag.x * 0.0003f;
		smPos.StartFresh(fDestPos);
	}

	public void OnDragEnd()
	{
		int nItem = _GetNearestItemByPos(0);
		if (nItem >= 0)
		{
			ScrollToItem(nItem);
		}
	}
}
