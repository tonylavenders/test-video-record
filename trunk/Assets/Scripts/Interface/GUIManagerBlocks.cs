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
	public ButtonBar mVoiceFxButtonBar;

	public BasicButton mDecreaseTimeButton;
	public BasicButton mIncreaseTimeButton;
	public BasicButton mSaveTimeButton;

	public GUIText mTextTime;
	public GUIText mTextTimeShadow;

	int mLastBlockTime=-1;
	int mCurrentBlockTime=-1;
	Data.Chapter.Block mPreviousBlock=null;
	BasicButton mPreviousButton=null;

	public SoundRecorder soundRecorder;

	enum Modes{
		TimeMode,
		VoiceMode
	}
	Modes Mode;

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
	//If time has changed and block isn't saved yet then show warning message
	public override void SaveWarning(Data.Chapter.Block previousBlock, BasicButton previousButton)
	{
		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Showing){
			return;
		}
		if((mCurrentBlockTime!=-1 && mLastBlockTime!=-1 && mCurrentBlockTime!=mLastBlockTime) || !soundRecorder.bLastSaved){
			TVR.Utils.Message.Show(1, "AVISO", "No ha guardado los cambios. \u00BFDesea guardar?", TVR.Utils.Message.Type.YesNo, "S\u00ED", "No", Message_Save);
			mPreviousBlock = previousBlock;
			mPreviousButton = previousButton;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Message to choice whether save the block or not
	void Message_Save(TVR.Utils.Message.ButtonClicked buttonClicked, int Identifier)
	{
		if(buttonClicked == TVR.Utils.Message.ButtonClicked.Yes){
			if(!soundRecorder.bLastSaved){
				soundRecorder.SaveAudioData(mPreviousBlock, mPreviousButton);
			}else{
				SaveBlockTime();
			}
		}else{
			mCurrentBlockTime=mLastBlockTime;
			soundRecorder.bLastSaved=true;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SaveBlockTime()
	{
		if(mPreviousBlock!=null){
			SaveBlock(mPreviousBlock);
		}else{
			SaveBlock(Data.selChapter.selBlock);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SaveBlock(Data.Chapter.Block block)
	{
		block.Frames = mCurrentBlockTime*Globals.FRAMESPERSECOND;

		if(Mode==Modes.TimeMode){
			block.BlockType = Data.Chapter.Block.blockTypes.Time;
		}else if(Mode==Modes.VoiceMode){
			block.BlockType = Data.Chapter.Block.blockTypes.Voice;
		}
		block.Save();
		mPreviousBlock=null;
	
		if(mPreviousButton!=null){
			mPreviousButton.SetTextTime();
		}else{
			RightButtonBar.currentSelected.SetTextTime();
		}
	
		mLastBlockTime = mCurrentBlockTime;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void InitButtons()
	{
		base.InitButtons();
		soundRecorder.InitButtons();

		EditButton.Show();

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
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public override void HideAllButtonBars()
	{
		mAnimationsButtonBar.Hide();
		mExpressionsButtonBar.Hide();
		mTimeButtonBar.Hide();
		mVoiceFxButtonBar.Hide();
		
		mAnimationsButtonBar.UncheckButtons();
		mExpressionsButtonBar.UncheckButtons();
		mTimeButtonBar.UncheckButtons();
		mVoiceFxButtonBar.UncheckButtons();
		
		LeftButtonBar.UncheckButtons();

		ChangeButtonState(false,false);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void SetTime(int seconds)
	{
		mTextTime.text = "00:"+seconds.ToString("00");
		mTextTimeShadow.text = mTextTime.text;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void SetColor(Color color)
	{
		mTextTime.color = color;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void ChangeButtonState(bool bTime, bool bVoice)
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
		
		soundRecorder.ChangeButtonState(bVoice);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Animations button
	public void OnButtonAnimationsPressed(BasicButton sender)
	{
		if(sender.Checked){
			mAnimationsButtonBar.Show(true);
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
			mExpressionsButtonBar.Show(true);
		}else{
			mExpressionsButtonBar.Hide();
		}
		Count(sender.Checked);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Time button
	public void OnButtonTimePressed(BasicButton sender)
	{
		if(sender.Checked){
			mTimeButtonBar.Show(true);
			//TIME
			if(Data.selChapter.selBlock.BlockType==Data.Chapter.Block.blockTypes.Time){
				ChangeButtonState(true, false);
				mTimeButtonBar.listButtons[0].GetComponent<BasicButton>().Checked=true;
				Mode=Modes.TimeMode;
			}
			//VOICE
			else if(Data.selChapter.selBlock.BlockType==Data.Chapter.Block.blockTypes.Voice){
				soundRecorder.SetAudioClip();
				ChangeButtonState(false, true);
				mTimeButtonBar.listButtons[1].GetComponent<BasicButton>().Checked=true;
				Mode=Modes.VoiceMode;
			}
			mCurrentBlockTime = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
			SetTime(mCurrentBlockTime);
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
			Mode=Modes.TimeMode;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Time-Voice button
	public void OnButtonTimeVoicePressed(BasicButton sender)
	{
		if(sender.Checked){
			soundRecorder.SetAudioClip();
			ChangeButtonState(false, true);
			mLastBlockTime = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
			mCurrentBlockTime = mLastBlockTime;
			Mode=Modes.VoiceMode;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeDecrPressed(BasicButton sender)
	{
		if(mCurrentBlockTime > Globals.MIN_SEC_BLOCK){
			mCurrentBlockTime--;
			SetTime(mCurrentBlockTime);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeIncrPressed(BasicButton sender)
	{
		if(mCurrentBlockTime < Globals.MAX_SEC_BLOCK){
			mCurrentBlockTime++;
			SetTime(mCurrentBlockTime);
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeSavePressed(BasicButton sender)
	{
		SaveBlockTime();
		HideAllButtonBars();
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
		
		if(pauseStatus && Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected override void OnApplicationQuit()
	{
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
	}
}






