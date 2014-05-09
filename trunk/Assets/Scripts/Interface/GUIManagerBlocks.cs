//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;
using TVR;

//Script attached to the GUICamera object
public class GUIManagerBlocks : GUIManager
{
	public ButtonBar mAnimationsButtonBar;
	public ButtonBar mExpressionsButtonBar;
	public ButtonBar mTimeButtonBar;

	public BasicButton mDecreaseTimeButton;
	public BasicButton mIncreaseTimeButton;
	public BasicButton mSaveTimeButton;

	public BasicButton mVoicePlayButton;
	public BasicButton mVoiceRecButton;
	public BasicButton mVoiceFxButton;
	public BasicButton mVoiceSaveButton;

	public GUIText mTextTime;
	public GUIText mTextTimeShadow;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Start()
	{
		base.Start();

		float screenRatio = 800.0f/Screen.width;
		mTextTime.fontSize = Mathf.RoundToInt(110.0f*screenRatio);
		mTextTimeShadow.fontSize = Mathf.RoundToInt(110.0f*screenRatio);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void InitButtons()
	{
		base.InitButtons();

		mEditButton.Show();

		//TIME

		//Time: Decrease button
		float pos_x = ButtonProperties.buttonBarScaleX*2.0f+ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize/2.0f;
		float pos_y = 4*(ButtonProperties.buttonSize/2+ButtonProperties.buttonMargin/2) + Screen.height/2;
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		mDecreaseTimeButton.Init(pos, scale);

		//Time: Increase button
		pos_x += ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mIncreaseTimeButton.Init(pos, scale);

		//Time: Save button
		pos_x += ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mSaveTimeButton.Init(pos, scale);

		//VOICE

		//Voice: Play button
		pos_x = ButtonProperties.buttonBarScaleX*2.0f+ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize/2.0f;
		pos_y -= ButtonProperties.buttonMargin+ButtonProperties.buttonSize; 
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoicePlayButton.Init(pos, scale);

		//Voice: Rec button
		pos_x += ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceRecButton.Init(pos, scale);

		//Voice: Fx button
		pos_x += ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceFxButton.Init(pos, scale);

		//Voice: Save button
		pos_x += ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceSaveButton.Init(pos, scale);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public override void HideAllButtonBars()
	{
		mAnimationsButtonBar.Hide();
		mExpressionsButtonBar.Hide();
		mTimeButtonBar.Hide();
		
		mAnimationsButtonBar.UncheckButtons();
		mExpressionsButtonBar.UncheckButtons();
		mTimeButtonBar.UncheckButtons();
		
		mLeftButtonBar.UncheckButtons();
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Animations button
	public void OnButtonAnimationsPressed(BasicButton sender)
	{
		if(sender.Checked){
			mAnimationsButtonBar.Show();
		}else{
			mAnimationsButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Expressions button
	public void OnButtonExpressionsPressed(BasicButton sender)
	{
		if(sender.Checked){
			mExpressionsButtonBar.Show();
		}else{
			mExpressionsButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	string TimeToString(int seconds)
	{
		return "00:"+seconds.ToString("00");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void ChangeButtonState(bool bTime, bool bVoice)
	{
		if(bTime){
			mDecreaseTimeButton.Show();
			mIncreaseTimeButton.Show();
			mSaveTimeButton.Show();
		}else{
			mDecreaseTimeButton.Hide();
			mIncreaseTimeButton.Hide();
			mSaveTimeButton.Hide();
		}
		if(bVoice){
			mVoicePlayButton.Show();
			mVoiceRecButton.Show();
			mVoiceFxButton.Show();
			mVoiceSaveButton.Show();
		}else{
			mVoicePlayButton.Hide();
			mVoiceRecButton.Hide();
			mVoiceFxButton.Hide();
			mVoiceSaveButton.Hide();
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Time button
	public void OnButtonTimePressed(BasicButton sender)
	{
		if(sender.Checked){
			mTimeButtonBar.Show();
			if(mTimeButtonBar.listButtons[0].GetComponent<BasicButton>().Checked){
				ChangeButtonState(true, false);
			}
			else if(mTimeButtonBar.listButtons[1].GetComponent<BasicButton>().Checked){
				ChangeButtonState(false, true);
			}
			int seconds = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
			mTextTime.text = TimeToString(seconds);
			mTextTimeShadow.text = TimeToString(seconds);
			mTextTime.GetComponent<GUITextController>().Show();
			mTextTimeShadow.GetComponent<GUITextController>().Show();
		}else{
			mTimeButtonBar.Hide();
			ChangeButtonState(false, false);
			mTextTime.GetComponent<GUITextController>().Hide();
			mTextTimeShadow.GetComponent<GUITextController>().Hide();
		}
		Count(sender.Checked);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Time-Time button
	public void OnButtonTimeTimePressed(BasicButton sender)
	{
		ChangeButtonState(true, false);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Time-Voice button
	public void OnButtonTimeVoicePressed(BasicButton sender)
	{
		ChangeButtonState(false, true);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeDecrPressed(BasicButton sender)
	{
		int time = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
		if(time>Globals.MIN_SEC_BLOCK){
			time--;
			Data.selChapter.selBlock.Frames=time*Globals.FRAMESPERSECOND;
			mTextTime.text = TimeToString(time);
			mTextTimeShadow.text = TimeToString(time);
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeIncrPressed(BasicButton sender)
	{
		int time = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
		if(time<Globals.MAX_SEC_BLOCK){
			time++;
			Data.selChapter.selBlock.Frames = time*Globals.FRAMESPERSECOND;
			mTextTime.text = TimeToString(time);
			mTextTimeShadow.text = TimeToString(time);
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeSavePressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoicePlayPressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoiceRecPressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoiceFxPressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoiceSavePressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Camera button
	public void OnButtonCamerasPressed(BasicButton sender)
	{
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonBlockPressed(BasicButton sender)
	{
		HideAllButtonBars();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Edit button
	public override void OnButtonEditPressed(BasicButton sender)
	{
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
		SceneMgr.Get.SwitchTo("ChapterMgr");

		if(Data.selChapter!=null){
			Data.selChapter.unloadBlocks();
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public override void OnButtonPlayPressed(BasicButton sender)
	{
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
		Debug.Log("play blocks");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected override void OnApplicationPause(bool pauseStatus)
	{
		base.OnApplicationPause(pauseStatus);
		
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
	}
}






