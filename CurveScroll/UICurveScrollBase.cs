using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UICurveScrollBase: MonoBehaviour 
{
	
	public UIList mList;	//用来复制子节点
	
	public AnimationCurve cvX = new AnimationCurve( new Keyframe(0,0), new Keyframe(1,0) );
	public AnimationCurve cvY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve cvZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve cvScale = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public AnimationCurve cvVisibility = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

	public delegate void CBScrollEvent(UICurveScrollBase scr);

	public UILuaNode mLuaListener;
	public CBScrollEvent cbOnStart;
	public CBScrollEvent cbOnFinish;

        public float fSpace;
	public float fVisibleRangeMin = 0;
	public float fVisibleRangeMax = 1;
	public int nSubPanelCount	{get; private set;}

	public SmoothValueFloat smPos = new SmoothValueFloat();
	protected List<UICurveScrollSubPanel> lstPanelCached = new List<UICurveScrollSubPanel>();
	protected List<UICurveScrollSubPanel> lstOrdered = new List<UICurveScrollSubPanel>();
	protected float fOldPos = -999;
	protected bool bChangedLastFrame = false;

	virtual protected void Awake() {}

	virtual protected void Update()
	{
		smPos.DoUpdateFrame();
		float fCurPos = smPos.fCur;
		bool bChanged = fOldPos != fCurPos;
		if(bChanged)
		{
			_UpdateSubPanelPos();
		}

		bool bTriggerStart = (bChangedLastFrame==false && bChanged);
		bool bTriggerFinish = bChangedLastFrame && !bChanged;

		if (bTriggerStart)
		{
			_OnMoveStart();
		}
		if(bTriggerFinish)
		{
			_OnMoveFinish();
		}

		bChangedLastFrame = bChanged;
		fOldPos = fCurPos;
	}
	
	public void Duplicate( int n )
	{
		lstPanelCached.Clear();

		mList.Duplicate(n);

		for( int i=0; i<mList.ActiveCount; i++)
		{
			UICurveScrollSubPanel pl = mList.GetByIndex(i) as UICurveScrollSubPanel;
			if(pl)
				lstPanelCached.Add(pl);
		}

		lstOrdered.Clear();
		lstOrdered.AddRange(lstPanelCached);


		_SetPos(0, true);
	}

	protected void _SetPos( float pos, bool bImmediate )
	{
		if(bImmediate)
		{
			smPos.StartFresh(pos);
			_UpdateSubPanelPos();
		}
		else
		{
			smPos.Start(pos);
		}
	}


	virtual protected void _UpdateSubPanelPos()
	{
		float fPos = smPos.fCur;
		for( int i=0; i<lstPanelCached.Count; i++ )
		{
			bool bVisible = (fPos >= fVisibleRangeMin && fPos <= fVisibleRangeMax);
			float x = cvX.Evaluate(fPos);
			float y = cvY.Evaluate(fPos);
			float z = cvZ.Evaluate(fPos);
			float scale = cvScale.Evaluate(fPos);

			UICurveScrollSubPanel sp = lstPanelCached[i];
			sp.gameObject.SetActive(bVisible);
			sp.fPos = fPos;
			sp.transform.localPosition = new Vector3(x,y,z);
			sp.transform.localScale = new Vector3(scale, scale, scale);


			fPos += fSpace;
		}

		lstOrdered.Sort(_SortByZ);
		for (int i = 0; i < lstOrdered.Count; i++)
		{
			UICurveScrollSubPanel sp = lstOrdered[i];
			if(!sp.gameObject.activeInHierarchy)
				continue;

			float fVis = cvVisibility.Evaluate(sp.fPos);
			sp.SetShowParams( i, fVis );
		}
	}
  
	virtual public void ScrollToItem(int nIndex)
	{
		float fDestPos = -nIndex * fSpace;
		_SetPos(fDestPos, false);
	}

	static public int _SortByZ(UICurveScrollSubPanel a, UICurveScrollSubPanel b) 
	{
		return -a.transform.localPosition.z.CompareTo( b.transform.localPosition.z );
	}



	virtual protected void _OnMoveStart()
	{
		if(mLuaListener)
		{
			mLuaListener.mScrObj.CallMember("OnScrollStart", new object[] {this} );
		}

		if(cbOnStart != null)
		{
			cbOnStart(this);
		}
	}

	virtual protected void _OnMoveFinish()
	{
		if (mLuaListener)
		{
			mLuaListener.mScrObj.CallMember("OnScrollFinish", new object[] { this });
		}

		if (cbOnFinish != null)
		{
			cbOnFinish(this);
		}
	}

	virtual protected int _GetNearestItemByPos(float pos)
	{
		int ret = -1;
		float fMinDist = 999;
		for (int i = 0; i < lstPanelCached.Count; i++)
		{
			UICurveScrollSubPanel sp = lstPanelCached[i];
			float posi = sp.fPos;
			float dist = Mathf.Abs(pos - posi);
			if(dist < fMinDist)
			{
				fMinDist = dist;
				ret = i;
			}
		}
		return ret;
	}

	//连接UIList
	public int ActiveCount
	{
		get
		{
			return lstPanelCached.Count;
		}
	}
  
	public UICurveScrollSubPanel GetByIndex( int i) 
	{
		return lstPanelCached[i];
	}
}
