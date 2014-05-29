using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;

public class GUITextController : MonoBehaviour
{
	Transform mParent;
	SmoothStep mFade;
	public bool bIsTime=false;
	public bool bIsFX=false;
	Data.Chapter.Block mBlock;
	BasicButton mButton;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		mFade = new SmoothStep(0.0f,1.0f,1.0f,false);
		mParent = transform.parent;
		if(mParent!=null){
			mButton = mParent.GetComponent<BasicButton>();
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		SetTextBottom();

		if(mParent!=null){
			if(bIsTime || bIsFX){
				guiText.fontSize = (int)(mParent.lossyScale.x*(15.0f/90.0f)); 
			}else{
				guiText.fontSize = (int)(mParent.lossyScale.x*(26.0f/90.0f)); //para button.scale=90 ==> font_size=26
			}
		}
		Color c = guiText.color;
		guiText.color = new Color(c.r, c.g, c.b, mFade.Value);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetTextBottom(string text="")
	{
		if(bIsTime){
			Data.Chapter.Block mBlock = mButton.iObj as Data.Chapter.Block;
			int seconds = Mathf.RoundToInt(mBlock.Frames*Globals.MILISPERFRAME);
			seconds = Mathf.Max(seconds,1);
			guiText.text = "00:"+seconds.ToString("00");
		}
		else if(bIsFX){
			guiText.text = text;
		}
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

			//time or fx label
			if(bIsTime || bIsFX){
				pos_y = (mParent.position.y-mParent.lossyScale.x * 0.3f)/Screen.height;
			//chapter or block number label
			}else{
				pos_y = mParent.position.y/Screen.height;
			}
			guiText.transform.position = new Vector3(pos_x, pos_y, 0.0f);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show(float delay = 0, float duration = Globals.ANIMATIONDURATION, float fAlpha=1.0f)
	{
		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Running)
			return;

		mFade.Reset(fAlpha, duration, true, delay);
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Running)
			return;

		mFade.Reset(0.0f, duration, true, delay);
	}
}



