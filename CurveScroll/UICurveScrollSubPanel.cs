using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UICurveScrollSubPanel : UIListItem 
{
	public float fPos;
	public UILuaNode nodeBtn;
	public int nPanelStartDepth = 1;
	UIPanel pl = null;

	void Awake( )
	{
		pl = GetComponent<UIPanel>();
	}

	virtual public void SetShowParams( int nRenderOrder, float fVisibility )
	{
		if (pl)
		{
			pl.depth = nPanelStartDepth + nRenderOrder;
		}

		if(nodeBtn)
		{
			nodeBtn.mScrObj.CallMember( "SetShowParams", new object[]{fVisibility} );
		}
	}
}
