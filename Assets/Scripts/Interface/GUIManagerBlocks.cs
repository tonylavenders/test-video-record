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
	public ButtonBar mCamerasButtonBar;
	public ButtonBar mTimeButtonBar;
	public ButtonBar mVoiceFxButtonBar;

	public BasicButton mDecreaseTimeButton;
	public BasicButton mIncreaseTimeButton;
	public BasicButton mSaveTimeButton;

	bool mPlay;
	float mTime;

	public GUIText mTextTime;
	public GUIText mTextTimeShadow;

	public bool bLastSaved=true;
	public bool LastSaved{
		get { return (bLastSaved && soundRecorder.bLastSaved); }
		set{}
	}
	int mLastBlockTime=-1;
	int mCurrentBlockTime=-1;
	int CurrentBlockTime{
		get { return mCurrentBlockTime; }
		set {
			mCurrentBlockTime = value;
			if(mLastBlockTime!=-1 && mCurrentBlockTime!=-1){
				bLastSaved = (mCurrentBlockTime==mLastBlockTime) && soundRecorder.bLastSaved;
			}
		}
	}

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
		mPlay=false;
		mTime=0;

		base.Start();

		SampleAnimation();

		mTextTime.fontSize = Mathf.RoundToInt(ButtonProperties.buttonSize);
		mTextTimeShadow.fontSize = Mathf.RoundToInt(ButtonProperties.buttonSize);

		float y_pos = (ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2.0f)/Screen.height;
		mTextTime.transform.position = new Vector3(mTextTime.transform.position.x, y_pos, mTextTime.transform.position.z);
		mTextTimeShadow.transform.position = new Vector3(mTextTimeShadow.transform.position.x, y_pos, mTextTimeShadow.transform.position.z);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		base.Update();

		if(Data.selChapter.selBlock==null)
			return;

		if(mPlay){
			mTime += Time.deltaTime;
			if(mTime>Data.selChapter.selBlock.EndTime){
				StopPlayBlock();
			}else{
				Data.selChapter.Frame(mTime,true);
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//If time has changed and block isn't saved yet then show warning message
	public override void SaveWarning(Data.Chapter.Block previousBlock, BasicButton previousButton)
	{
		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Running)
			return;

		TVR.Utils.Message.Show(1, "AVISO", "No ha guardado los cambios. \u00BFDesea guardar?", TVR.Utils.Message.Type.YesNo, "S\u00ED", "No", Message_Save);
		mPreviousBlock = previousBlock;
		mPreviousButton = previousButton;
		blur = true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Message to choice whether save the block or not
	void Message_Save(TVR.Utils.Message.ButtonClicked buttonClicked, int Identifier)
	{
		if(buttonClicked == TVR.Utils.Message.ButtonClicked.Yes){
			if(Mode==Modes.VoiceMode){
				soundRecorder.SaveAudioData(mPreviousBlock, mPreviousButton);
			}else{
				SaveBlockTime();
			}
		}else{
			CurrentBlockTime=mLastBlockTime;
			if(Mode==Modes.VoiceMode){
				soundRecorder.ResetAudio((int)Data.selChapter.selBlock.FilterType);
			}
		}
		bLastSaved=true;
		soundRecorder.bLastSaved=true;
		blur = false;
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
		block.Frames = CurrentBlockTime*Globals.FRAMESPERSECOND;

		if(Mode==Modes.TimeMode){
			block.BlockType = Data.Chapter.Block.blockTypes.Time;
		}else if(Mode==Modes.VoiceMode){
			block.BlockType = Data.Chapter.Block.blockTypes.Voice;
		}
		block.Save();
		mPreviousBlock=null;
	
		if(mPreviousButton!=null){
			mPreviousButton.SetTextBottom();
		}else{
			RightButtonBar.currentSelected.SetTextBottom();
		}
	
		mLastBlockTime = CurrentBlockTime;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void InitButtons()
	{
		base.InitButtons();
		soundRecorder.InitButtons();

		//EditButton.Show();

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
		mCamerasButtonBar.Hide();
		mTimeButtonBar.Hide();
		mVoiceFxButtonBar.Hide();
		
		//mAnimationsButtonBar.UncheckButtons();
		//mExpressionsButtonBar.UncheckButtons();
		//mCamerasButtonBar.UncheckButtons();
		mTimeButtonBar.UncheckButtons();
		mVoiceFxButtonBar.UncheckButtons();
		
		LeftButtonBar.UncheckButtons();

		ChangeButtonState(false,false);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void HideTime()
	{
		mTextTime.GetComponent<GUITextController>().Hide();
		mTextTimeShadow.GetComponent<GUITextController>().Hide();
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void ShowTime()
	{
		mTextTime.GetComponent<GUITextController>().Show();
		mTextTimeShadow.GetComponent<GUITextController>().Show();
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

	void SetExpression(string sExpression)
	{
		Component[] children = CurrentCharacter.GetComponentsInChildren<Component>(true);

		foreach(Component child in children){
			if(child.name.StartsWith("exp_")){
				if(child.name == "exp_"+sExpression)
					child.gameObject.SetActive(true);
				else if(child.name == "exp_"+sExpression+"_m")
					child.gameObject.SetActive(true);
				else
					child.gameObject.SetActive(false);
			}
		}
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

	void SetCurrentBlockElements()
	{
		if(Data.selChapter==null || Data.selChapter.selBlock==null)
			return;

		//Animation
		if(Data.selChapter.selBlock.IdAnimation!=-1){
			mAnimationsButtonBar.SetCurrentButton(Data.selChapter.selBlock.IdAnimation);
			if(CurrentCharacter!=null){
				//CurrentCharacter.transform.Find("mesh").animation.Stop();
				StopAnimation();
			}
		}else{
			mAnimationsButtonBar.SetCurrentButton(1);
		}/*
		//Expression
		if(Data.selChapter.selBlock.IdExpression!=-1){
			mExpressionsButtonBar.SetCurrentButton(Data.selChapter.selBlock.IdExpression);
			//if(CurrentCharacter!=null){
				//SetExpression(ResourcesLibrary.getExpression(Data.selChapter.selBlock.IdExpression).Name);
			//}
		}else{
			mExpressionsButtonBar.SetCurrentButton(1);
		}*/
		//Camera
		if((int)Data.selChapter.selBlock.ShotType!=-1){
			mCamerasButtonBar.SetCurrentButton((int)Data.selChapter.selBlock.ShotType);
			mCamera.GetComponent<SceneCameraManager>().SetParams((int)Data.selChapter.selBlock.ShotType);
		}else{
			mCamerasButtonBar.SetCurrentButton(1);
		}

		//Data.selChapter.Frame(Data.selChapter.selBlock.StartTime,false);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/*
	void StopAnimation()
	{
		Transform mMesh = CurrentCharacter.transform.Find("mesh");
		mMesh.animation[ResourcesLibrary.getAnimation(mAnimationsButtonBar.currentSelected.ID).Name].enabled = false;
		mMesh.animation["Idle"].time = 0.0f;
		mMesh.animation["Idle"].weight = 1.0f;
		mMesh.animation["Idle"].enabled = true;
		mMesh.animation.Sample();
		mMesh.animation["Idle"].enabled = false;
	}
*/
	void StopAnimation()
	{
		string sAnimName = ResourcesLibrary.getAnimation(Data.selChapter.selBlock.IdAnimation).Name;
		Transform mMesh = CurrentCharacter.transform.Find("mesh");
		mMesh.animation[ResourcesLibrary.getAnimation(mAnimationsButtonBar.currentSelected.ID).Name].enabled = false;
		mMesh.animation[sAnimName].time = 0.0f;
		mMesh.animation[sAnimName].weight = 1.0f;
		mMesh.animation[sAnimName].enabled = true;
		mMesh.animation.Sample();
		mMesh.animation[sAnimName].enabled = false;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Animations button
	public void OnButtonAnimationsChecked(BasicButton sender)
	{
		if(sender.Checked){
			mAnimationsButtonBar.Show(true);
			CurrentCharacter.transform.Find("mesh").animation.Stop();
			CurrentCharacter.transform.Find("mesh").animation.Play(ResourcesLibrary.getAnimation(mAnimationsButtonBar.currentSelected.ID).Name);
		}else{
			mAnimationsButtonBar.Hide();
			StopAnimation();
		}
		Count(sender.Checked);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Expressions button
	public void OnButtonExpressionsChecked(BasicButton sender)
	{
		if(sender.Checked){
			mExpressionsButtonBar.Show(true);
		}else{
			mExpressionsButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Cameras button
	public void OnButtonCamerasChecked(BasicButton sender)
	{
		if(sender.Checked){
			mCamerasButtonBar.Show(true);
		}else{
			mCamerasButtonBar.Hide();
		}
		Count(sender.Checked);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonAnimationChecked(BasicButton sender)
	{
		if(sender.Checked){
			if(CurrentCharacter!=null){
				CurrentCharacter.transform.Find("mesh").animation.Stop();
				CurrentCharacter.transform.Find("mesh").animation.Play(ResourcesLibrary.getAnimation(sender.ID).Name);
			}
			Data.selChapter.selBlock.IdAnimation=sender.ID;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonExpressionChecked(BasicButton sender)
	{
		if(sender.Checked){
			if(CurrentCharacter!=null){
				SetExpression(ResourcesLibrary.getExpression(sender.ID).Name);
			}
			Data.selChapter.selBlock.IdExpression=sender.ID;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonCameraChecked(BasicButton sender)
	{
		if(sender.Checked){
			Data.selChapter.selBlock.ShotType=(Data.Chapter.Block.shotTypes)sender.ID;
			mCamera.GetComponent<SceneCameraManager>().SetParams((int)Data.selChapter.selBlock.ShotType);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Time button
	public void OnButtonTimeChecked(BasicButton sender)
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
			CurrentBlockTime = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
			mLastBlockTime = CurrentBlockTime;
			SetTime(CurrentBlockTime);
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
	public void OnButtonTimeTimeChecked(BasicButton sender)
	{
		if(sender.Checked){
			ChangeButtonState(true, false);
			CurrentBlockTime = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
			mLastBlockTime = CurrentBlockTime;
			Mode=Modes.TimeMode;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Time-Voice button
	public void OnButtonTimeVoiceChecked(BasicButton sender)
	{
		if(sender.Checked){
			soundRecorder.SetAudioClip();
			ChangeButtonState(false, true);
			CurrentBlockTime = Mathf.RoundToInt(Data.selChapter.selBlock.Frames*Globals.MILISPERFRAME);
			mLastBlockTime = CurrentBlockTime;
			Mode=Modes.VoiceMode;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeDecrClicked(BasicButton sender)
	{
		if(CurrentBlockTime > Globals.MIN_SEC_BLOCK){
			CurrentBlockTime--;
			SetTime(CurrentBlockTime);
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeIncrClicked(BasicButton sender)
	{
		if(CurrentBlockTime < Globals.MAX_SEC_BLOCK){
			CurrentBlockTime++;
			SetTime(CurrentBlockTime);
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonTimeTimeSaveClicked(BasicButton sender)
	{
		SaveBlockTime();
		HideAllButtonBars();
		bLastSaved=true;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonBlockChecked(BasicButton sender)
	{
		if(sender.Checked){
			if(Data.selChapter.selBlock!=null){
				if(Data.selChapter.selBlock.BlockType==Data.Chapter.Block.blockTypes.Voice){
					soundRecorder.ResetAudio((int)Data.selChapter.selBlock.FilterType);
				}else{
					soundRecorder.ResetAudio();
				}
			}
			Data.selChapter.selBlock = sender.iObj as Data.Chapter.Block;
			SetCurrentBlockElements();
			HideAllButtonBars();
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Edit button
	public override void OnButtonEditClicked(BasicButton sender)
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
	public override void OnButtonPlayClicked(BasicButton sender)
	{
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
			SetDataGameObjects();

			//Stop and rewind
			if(mPlay){
				StopPlayBlock();
			}
			//Start playing from the beginning
			else{
				StartPlayBlock();
			}
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void StartPlayBlock()
	{
		mTime=Data.selChapter.selBlock.StartTime;
		Data.selChapter.Reset();
		Data.selChapter.Frame(Data.selChapter.selBlock.StartTime,true);
		mPlay=true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void StopPlayBlock()
	{
		Data.selChapter.Stop();
		Data.selChapter.Reset();
		Data.selChapter.Frame(Data.selChapter.selBlock.StartTime,false);
		mPlay=false;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void OnApplicationPause(bool pauseStatus)
	{
		if(pauseStatus && Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void OnApplicationQuit()
	{
		if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
			Data.selChapter.selBlock.Save();
		}
	}
}






