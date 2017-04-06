using UnityEngine;
using System.Collections;

public class UIMixtureControl : MonoBehaviour
{
	public UIProgressBar pb;
	public UISprite pic;
	public UILabel txt;

	public delegate void CBProgressBarEnd(UIMixtureControl obj);
	public CBProgressBarEnd cbPBEnd;
        public CBProgressBarEnd cbPBRoundEnd;

	//lua event
	public UILuaNode mNodeEvent;
	public string sPBRoundEndEvent;
	public string sPBEndEvent;
	public string sAudio;

	private float fInterval = .0f;
	private float fAudioInterval = .0f;
	private float fTotalTime = .0f;
	private string sFormat = "{0}";
	private int nDes = 0;
	private float fDes = .0f;
	private int nCurValue = 0;
	private float fCurValue = .0f;
	private int nRecRound = 0;

	public void SetText( string sfmt, int nCur, int nRet )
	{
		sFormat = sfmt;
		nCur = nCur > 0 ? nCur : 0;
		nCurValue = nCur;
		nDes = nRet;

		_UpdateTxt(nCur);
	}

	//强制要求cur在[0~1)中
	public void SetProgressBar( float fCur, float fRet )
	{
		fCurValue = fCur;
		fDes = fRet;

		_UpdateProg(fCur);
	}

	//强制要求cur在[0~1)中
	public void SetSprite( float fCur, float fRet )
	{
		fCurValue = fCur;
		fDes = fRet;

		_UpdatePic(fCur);
	}

	public void Play( float fTime )
	{
		fTotalTime = fTime;
		fInterval = .0f;
		nRecRound = 0;
	}

	void Update( )
	{
		if ( fInterval >= fTotalTime )
			return;

		//播放AudioClip
		fAudioInterval += Time.deltaTime;
		if ( !string.IsNullOrEmpty( sAudio ) && fAudioInterval >= 0.2f ) {
			fAudioInterval = .0f;
			_PlayAudio( );
		}

		fInterval += Time.deltaTime;
		if ( fInterval >= fTotalTime ) 
		{
			_UpdateProg(fDes);
			_UpdatePic(fDes);
			_UpdateTxt(nDes);
			
			_NotifyDone( );
			return;
		}


		float fVal = Mathf.Lerp(fCurValue, fDes, fInterval / fTotalTime);
		int nRound = Mathf.FloorToInt(fVal);

		_UpdateProg(fVal);
		_UpdatePic(fVal);

		int nVal = (int)Mathf.Lerp(nCurValue, nDes, fInterval / fTotalTime);
		_UpdateTxt(nVal);
		
		_NotifyRoundDone( nRound );
		nRecRound = nRound;
	}

	void _UpdateProg( float v )
	{
		if (pb)
		{
			int nRound = Mathf.FloorToInt(v);
			pb.value = v - nRound;
		}
	}

	void _UpdatePic(float v)
	{
		
		if (pic)
		{
			int nRound = Mathf.FloorToInt(v);
			pic.fillAmount = v - nRound;
		}
	}

	void _UpdateTxt( int n )
	{
		if (txt)
		{
			txt.text = string.Format(sFormat, n);
		}
	}

	void _PlayAudio( )
	{
		if( PubAudioPlayer.GetCurrent() )	
		{
			PubAudioPlayer.GetCurrent().PlaySound( sAudio, 1, 1, PubAudioPlayer.ePubAudioType.ui1 );
		}
	}

	void _NotifyRoundDone( int nCurRound )
	{
		if ( nRecRound == nCurRound || nCurRound == 0 )
			return;
		
		if( cbPBRoundEnd != null )
            cbPBRoundEnd(this);
		
		gameObject.SendMessage( "OnProgressBarRoundEnd", this, SendMessageOptions.DontRequireReceiver );
		if ( mNodeEvent && !string.IsNullOrEmpty(sPBRoundEndEvent) )
		{
			mNodeEvent.OnEvent( sPBRoundEndEvent );
		}
	}

	void _NotifyDone( )
	{
		if( cbPBEnd != null )
			cbPBEnd( this );

		gameObject.SendMessage( "OnProgressBarEnd", this, SendMessageOptions.DontRequireReceiver );
		if ( mNodeEvent && !string.IsNullOrEmpty(sPBEndEvent) )
		{
			mNodeEvent.OnEvent( sPBEndEvent );
		}
	}
}
