using UnityEngine;  
using System.Collections;  
using System.Collections.Generic;  
  
public class UITextImage : UIWidget   
{
    /// <summary>
    /// public variable
    /// </summary>
    public UIAtlas  mAtlas;
    public Font     mFont;

    /// <summary>
    /// private variable
    /// </summary>
    float           fCurHeight;
    float           fCurLineLength;

    string          mContent;
    int             nCallTimes;

    List<UILabel>   mTextCache;
    List<UISprite>  mSpriteCache;
    List<UILabel>   mPartTextCache;

    /// <summary>
    /// private const
    /// </summary>
    const float fLineHeight = 30.0f;

    void Awake( )
    {
        nCallTimes = 0;
        fCurHeight = fLineHeight;
        mPivot = UIWidget.Pivot.Center;
        mTextCache = new List<UILabel>();
        mSpriteCache = new List<UISprite>();
        mPartTextCache = new List<UILabel>();

        if (!mAtlas)
            mAtlas = AtlasCache.Instance.GetAtlas("setting");

        if (!mFont)
            mFont = UIFormRes.Instance.mDefaultFont2.dynamicFont;
    }
    
    public string content
    {
        get
        {
            return mContent;
        }
        set
        {
            if (mContent == value) return;

            if ( string.IsNullOrEmpty( value ) )
            {
                if ( !string.IsNullOrEmpty(mContent) )
                {
                    mContent = string.Empty;
                    MarkAsChanged( );
                }
            }
            else if (mContent != value)
            {
                mContent = value;
                MarkAsChanged( );
            }
        }
    }

    public override void MarkAsChanged( )
    {
        base.MarkAsChanged();
        ProcessContent( );
    }

    public void ProcessContent( )
    {
        if ( string.IsNullOrEmpty(mContent) )
            return;

        int i = 0;
        while ( i < mContent.Length )
        {
            if (mContent[i] == '/')
            {
                if (mContent.Length > (i+1) && mContent[i+1] == 'n')
                {
                    WrapLine( );
                    i = i + 2;
                }
                else
                {
                    ProcessText(mContent[i].ToString());
                    i++;
                }
            }
            else if (mContent[i] == '{')
            {
                int nLast = mContent.IndexOf("}", i);
                if (nLast > -1)
                {
                    string sSprite = mContent.Substring(i + 1, nLast - i - 1);
                    if (string.IsNullOrEmpty(sSprite))
                    {
                        ProcessText(mContent.Substring(i, nLast - i + 1));
                    }
                    else
                    {
                        PartSort( );
                        ProcessImage(sSprite);
                    }
                    i = nLast + 1;
                }
                else
                {
                    ProcessText(mContent[i].ToString());
                    i++;
                }
            }
            else
            {
                ProcessText(mContent[i].ToString());
                i++;
            }
        }

        WrapLine( );
    }

    public UILabel GenUILabel( string sText )
    {
        nCallTimes++;
        GameObject goLabel = NGUITools.AddChild( gameObject );
        if ( goLabel == null )
            return null;

        goLabel.name = "label" + nCallTimes;
        UILabel label = goLabel.AddComponent<UILabel>( );
        label.trueTypeFont = mFont;
        label.fontSize = Mathf.RoundToInt( fLineHeight );
        label.overflowMethod = UILabel.Overflow.ResizeFreely;
        label.depth = nCallTimes;
        label.maxLineCount = 1;
        label.pivot = UIWidget.Pivot.BottomLeft;
        label.text = sText;

        return label;
    }

    public void ProcessText( string sText )
    {
        UILabel label = GenUILabel(sText);
        if ( label == null )
            return;
                
        fCurLineLength += label.width;
        if ( fCurLineLength > (float)mWidth )
        {
            WrapLine( );
            if ( label )
            {
                mPartTextCache.Add( label );
                label.transform.localPosition = new Vector3( 0 - (mWidth / 2), _CalcPostionY( ), 0 );
            }
        }
        else
        {
            mPartTextCache.Add( label );
            label.transform.localPosition = new Vector3( fCurLineLength - (float)label.width - (mWidth / 2), _CalcPostionY(), 0 );
        }
    }

    public void ProcessImage( string sImage )
    {
        GameObject goImage = new GameObject();
        if ( goImage == null )
            return;

        nCallTimes++;
        GameObject obj = NGUITools.AddChild(gameObject, goImage);
        obj.name = "sprite" + nCallTimes;
        UISprite sprite = obj.AddComponent<UISprite>( );
        sprite.pivot = UIWidget.Pivot.BottomLeft;
        sprite.atlas = mAtlas;
        sprite.spriteName = sImage;
        sprite.depth = nCallTimes;
        UISpriteData sp = mAtlas.GetSprite( sImage );
        if ( sp != null )
        {
            sprite.width = sp.width;
            sprite.height = sp.height;
        }
                                
        fCurLineLength += sprite.width;
        if ( fCurLineLength > (float)mWidth )
        {
            WrapLine( );
            fCurHeight = sprite.height >= fLineHeight ? fCurHeight + sprite.height - fLineHeight : fCurHeight;
            sprite.transform.localPosition = new Vector3( 0 - (mWidth / 2), _CalcPostionY(), 0 );
        }
        else
        {
            mSpriteCache.Add( sprite );
            sprite.transform.localPosition = new Vector3( fCurLineLength - (float)sprite.width - (mWidth / 2), _CalcPostionY(), 0 );
        }
    }

    public void PartSort( )
    {
        if (mPartTextCache.Count > 0)
        {
            string sPartText = string.Empty;
            float fPosX = mPartTextCache[0].transform.localPosition.x;
            for (int i = 0; i < mPartTextCache.Count; ++i)
            {
                sPartText += mPartTextCache[i].text;
                GameObject.Destroy( mPartTextCache[i].gameObject );
            }
            UILabel label = GenUILabel( sPartText );
            if ( label )
            {
                mTextCache.Add( label );
                label.transform.localPosition = new Vector3( fPosX, _CalcPostionY( ), 0 );
            }

            mPartTextCache.Clear( );
        }
    }

    public void WrapLine( )
    {
        PartSort( );
        if ( mSpriteCache.Count > 0 )
        {
            mSpriteCache.Sort( (a, b) => (b.height.CompareTo(a.height)) );
            if ( mSpriteCache[0].height > Mathf.RoundToInt(fLineHeight) )
            {
                fCurHeight = fCurHeight - fLineHeight + mSpriteCache[0].height;
                for (int i = 0; i < mSpriteCache.Count; ++i)
                {
                    mSpriteCache[i].transform.localPosition = new Vector3( mSpriteCache[i].transform.localPosition.x, _CalcPostionY(), 0 );
                }

                for (int i = 0; i < mTextCache.Count; ++i)
                {
                    mTextCache[i].transform.localPosition = new Vector3( mTextCache[i].transform.localPosition.x, _CalcPostionY(), 0 );
                }
            }
        }
        
        if ( mTextCache != null )
            mTextCache.Clear( );
        if ( mSpriteCache != null )
            mSpriteCache.Clear( );

        fCurLineLength = .0f;
        fCurHeight += fLineHeight;
    }
    
    float _CalcPostionY( )
    {
        if ( fCurHeight > (float)mHeight / 2 )
            return 0 - ( fCurHeight - (float)mHeight / 2 );
        else
            return (float)mHeight / 2 - fCurHeight;
    }
}
