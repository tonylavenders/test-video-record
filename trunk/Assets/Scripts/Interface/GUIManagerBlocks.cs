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

	int mLastBlockTime;
	int mCurrentBlockTime;
	Data.Chapter.Block mPreviousBlock=null;

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Start()
	{
		base.Start();

		mTextTime.fontSize = Mathf.RoundToInt(ButtonProperties.buttonSize);
		mTextTimeShadow.fontSize = Mathf.RoundToInt(ButtonProperties.buttonSize);

		float y_pos = (ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2.0f)/Screen.height;
		mTextTime.transform.position = new Vector3(mTextTime.transform.position.x, y_pos, mTextTime.transform.position.z);
		mTextTimeShadow.transform.position = new Vector3(mTextTimeShadow.transform.position.x, y_pos, mTextTimeShadow.transform.position.z);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public override void CurrentBlockChanged(Data.Chapter.Block previousBlock)
	{
		if(mCurrentBlockTime!=mLastBlockTime){
			TVR.Utils.Message.Show(1, "AVISO", "No ha guardado los cambios. \u00BFDesea guardar?", TVR.Utils.Message.Type.YesNo, "S\u00ED", "No", Message_Save);
			mPreviousBlock = previousBlock;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Message_Save(TVR.Utils.Message.ButtonClicked buttonClicked, int Identifier)
	{
		if(buttonClicked == TVR.Utils.Message.ButtonClicked.Yes){
			SaveBlockTime();
		}else{
			mCurrentBlockTime=mLastBlockTime;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SaveBlockTime()
	{
		if(mPreviousBlock!=null){
			mPreviousBlock.Frames = mCurrentBlockTime*Globals.FRAMESPERSECOND;
			mPreviousBlock=null;
		}else{
			Data.selChapter.selBlock.Frames = mCurrentBlockTime*Globals.FRAMESPERSECOND;
		}
		mLastBlockTime = mCurrentBlockTime;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
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
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	string TimeToString(int seconds)
	{
		return "00:"+seconds.ToString("00");
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
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

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Time-Time button
	public void OnButtonTimeTimePressed(BasicButton sender)
	{
		if(sender.Checked){
			ChangeButtonState(true, false);
			mLastBlockTime = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
			mCurrentBlockTime = mLastBlockTime;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Time-Voice button
	public void OnButtonTimeVoicePressed(BasicButton sender)
	{
		ChangeButtonState(false, true);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeDecrPressed(BasicButton sender)
	{
		if(mCurrentBlockTime > Globals.MIN_SEC_BLOCK){
			mCurrentBlockTime--;
			mTextTime.text = TimeToString(mCurrentBlockTime);
			mTextTimeShadow.text = TimeToString(mCurrentBlockTime);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeIncrPressed(BasicButton sender)
	{
		if(mCurrentBlockTime < Globals.MAX_SEC_BLOCK){
			mCurrentBlockTime++;
			mTextTime.text = TimeToString(mCurrentBlockTime);
			mTextTimeShadow.text = TimeToString(mCurrentBlockTime);
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeSavePressed(BasicButton sender)
	{
		SaveBlockTime();
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoicePlayPressed(BasicButton sender)
	{
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoiceRecPressed(BasicButton sender)
	{
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoiceFxPressed(BasicButton sender)
	{
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeVoiceSavePressed(BasicButton sender)
	{
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Camera button
	public void OnButtonCamerasPressed(BasicButton sender)
	{
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonBlockPressed(BasicButton sender)
	{
		HideAllButtonBars();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public override void OnButtonPlayPressed(BasicButton sender)
	{
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
		Debug.Log("play blocks");
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected override void OnApplicationPause(bool pauseStatus)
	{
		base.OnApplicationPause(pauseStatus);
		
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
	}
}






