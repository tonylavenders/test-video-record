using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;

public class GUITextController : MonoBehaviour
{
	Transform mParent;
	SmoothStep mFade;
	public bool bIsTime=false;
	Data.Chapter.Block mBlock;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		mFade = new SmoothStep(0.0f,1.0f,1.0f,false);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		mParent = transform.parent;
		if(bIsTime){
			mBlock = mParent.GetComponent<BasicButton>().iObj as Data.Chapter.Block;
		}

		if(mParent!=null){
			if(!bIsTime){
				guiText.fontSize = (int)(mParent.lossyScale.x*(26.0f/90.0f)); //para button.scale=90 ==> font_size=26
			}else{
				guiText.fontSize = (int)(mParent.lossyScale.x*(15.0f/90.0f)); 
			}
		}
		Color c = guiText.color;
		guiText.color = new Color(c.r, c.g, c.b, mFade.Value);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		SmoothStep.State state = mFade.Update();

		if(state == SmoothStep.State.inFade || state == SmoothStep.State.justEnd){
			Color c = guiText.color;
			guiText.color = new Color(c.r, c.g, c.b, mFade.Value);
		}

		//position
		if(mParent!=null){
			float pos_x = mParent.position.x/Screen.width;
			float pos_y;
			if(!bIsTime){
				pos_y = mParent.position.y/Screen.height;
			}else{
				pos_y = (mParent.position.y-mParent.lossyScale.x * 0.3f)/Screen.height;
				guiText.text = "00:"+Mathf.RoundToInt(mBlock.Frames*Globals.MILISPERFRAME).ToString("00");
			}
			guiText.transform.position = new Vector3(pos_x, pos_y, 0.0f);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show(float delay = 0, float duration = Globals.ANIMATIONDURATION, float fAlpha=1.0f)
	{
		if(mParent!=null){
			mFade.Reset(fAlpha, duration, true, delay);
		}
		//Time GUIText
		else{
			mFade.Reset(1.0f, duration, true, delay);
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(0.0f, duration, true, delay);
	}
}



