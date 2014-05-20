using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;
using System;

public class SoundRecorder : MonoBehaviour
{
	public BasicButton mVoicePlayButton;
	public BasicButton mVoiceRecButton;
	public BasicButton mVoiceFxButton;
	public BasicButton mVoiceSaveButton;

	public GUIManagerBlocks guiManagerBlocks;

	AudioFilters filter;
	AudioClip[] audioClips;
	AudioSource audioSource;
	int mCurrentFilter=0;

	float mCurrentTime;
	float CurrentTime{
		get{ return mCurrentTime;}
		set{ 
			mCurrentTime=value;
			guiManagerBlocks.SetTime((int)Mathf.Min(value,15.0f));
		}
	}

	const int totalTime = 15;
	const int frequency = Globals.OUTPUTRATEPERSECOND;
	const int channels = 1;

	public enum Modes{
		Idle,
		Playing,
		Recording
	}
	public Modes mMode;

	public bool bLastSaved=true;

	string[] filterNames = new string[]{"<none>", "Monster", "Smurf", "Echo", "M-Pro", "S-Pro", "Robot", "Dist", "Noise", "Compress"};

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		mMode = Modes.Idle;

		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.loop = false;
		audioSource.playOnAwake = false;
		audioSource.clip = null;

		audioClips = new AudioClip[Enum.GetNames(typeof(Data.Chapter.Block.filterType)).Length];
		filter = new AudioFilters();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void InitButtons()
	{
		//Voice: Play button
		float pos_x = (ButtonProperties.buttonBarScaleX/2.0f) + ButtonProperties.buttonBarScaleX*2.0f;
		float pos_y = 4*(ButtonProperties.buttonSize/2+ButtonProperties.buttonMargin/2) + Screen.height/2;
		pos_y -= ButtonProperties.buttonMargin+ButtonProperties.buttonSize; 
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		mVoicePlayButton.Init(pos, scale);
		
		//Voice: Rec button
		pos_x += ButtonProperties.buttonBarScaleX;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceRecButton.Init(pos, scale);
		
		//Voice: Fx button
		pos_x += ButtonProperties.buttonBarScaleX;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceFxButton.Init(pos, scale);
		
		//Voice: Save button
		pos_x += ButtonProperties.buttonBarScaleX;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceSaveButton.Init(pos, scale);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetAudioClip()
	{
		mVoiceFxButton.SetTextBottom(filterNames[mCurrentFilter]);

		if(Data.selChapter.selBlock.BlockType==Data.Chapter.Block.blockTypes.Voice)
		{
			mCurrentFilter=(int)Data.selChapter.selBlock.FilterType;
			audioClips[mCurrentFilter] = Data.selChapter.selBlock.Sound;
			audioSource.clip = Data.selChapter.selBlock.Sound;
			CurrentTime = (int)Data.selChapter.selBlock.Sound.length;
			guiManagerBlocks.mVoiceFxButtonBar.SetCurrentButton(mCurrentFilter);

			if(Data.selChapter.selBlock.FilterType!=Data.Chapter.Block.filterType.Off){
				audioClips[0] = Data.selChapter.selBlock.OriginalSound;
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		if(TVR.Utils.Message.State!=TVR.Utils.Message.States.Hide)
			return;

		if(mMode==Modes.Recording){
			//Recording
			if(Microphone.IsRecording(null)){
				CurrentTime += Time.deltaTime;
			}
			//Max.time reached
			else{
				mVoicePlayButton.Show();
				mVoiceFxButton.Show();
				mVoiceSaveButton.Show();
				mVoiceRecButton.Checked=false;
				guiManagerBlocks.SetColor(Color.white);
				mMode=Modes.Idle;
			}
		}
		else if(mMode==Modes.Playing){
			//Playing
			if(CurrentTime < audioSource.clip.length){
				CurrentTime += Time.deltaTime;
			}
			//End playing
			else {
				mVoiceRecButton.Show();
				mVoiceFxButton.Show();
				mVoiceSaveButton.Show();
				mVoicePlayButton.Checked=false;
				audioSource.Stop();
				mMode=Modes.Idle;
			}
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void ChangeButtonState(bool bShow)
	{
		if(bShow){
			mVoicePlayButton.Show(0, Globals.ANIMATIONDURATION, audioClips[mCurrentFilter]!=null);
			mVoiceRecButton.Show();
			mVoiceFxButton.Show(0, Globals.ANIMATIONDURATION, audioClips[mCurrentFilter]!=null);
			mVoiceSaveButton.Show(0, Globals.ANIMATIONDURATION, audioClips[mCurrentFilter]!=null);
		}
		else{
			mVoicePlayButton.Hide();
			mVoiceRecButton.Hide();
			mVoiceFxButton.Hide();
			mVoiceSaveButton.Hide();
			guiManagerBlocks.mVoiceFxButtonBar.Hide();

			if(audioSource.isPlaying)
				audioSource.Stop();
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Remove empty sound at the end of the file
	void RemoveEmptyData()
	{
		float[] samples = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(samples, 0);
		
		float recordTime = Mathf.Max(CurrentTime, 0.5f); //0.5sec minimum record time
		
		int count = 0;
		int trim = Mathf.CeilToInt(recordTime * frequency);
		if(trim > samples.Length)
			trim = samples.Length;
		for(int i = trim - 1; i > 0; --i) {
			if(samples[i] != 0f)
				break;
			else
				++count;
		}
		
		int size = Mathf.Max(trim - count, (int)(frequency * 0.5f));
		float[] samplesTrim = new float[size];
		
		Buffer.BlockCopy(samples, 0, samplesTrim, 0, samplesTrim.Length * sizeof(float));
		
		audioClips[0] = AudioClip.Create("user_clip", samplesTrim.Length, channels, frequency, false, false);
		audioClips[0].SetData(samplesTrim, 0);
		
		samples = null;
		samplesTrim = null;
		
		audioSource.clip = audioClips[0];
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SaveAudioData(Data.Chapter.Block block, BasicButton button)
	{
		if(audioClips[0]!=null){
			if(block==null){
				block=Data.selChapter.selBlock;
			}
			block.BlockType = Data.Chapter.Block.blockTypes.Voice;
			block.Frames = (int)(audioClips[0].length*Globals.FRAMESPERSECOND);
			block.Sound = audioClips[mCurrentFilter];
			block.OriginalSound = audioClips[0];
			block.FilterType = (Data.Chapter.Block.filterType)mCurrentFilter;
			block.Save();
			if(button==null){
				button=guiManagerBlocks.RightButtonBar.currentSelected;
			}
			button.SetTextBottom();
			bLastSaved=true;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void ResetFilters()
	{
		for(int i=1;i<audioClips.Length;i++){
			audioClips[i]=null;
		}
		mCurrentFilter=0;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void CloseButtonBar()
	{
		guiManagerBlocks.mVoiceFxButtonBar.Hide();
		mVoiceFxButton.Show(0.2f,0.2f,true);
		mVoiceFxButton.Checked=false;
		mVoiceFxButton.SetTextBottom(filterNames[mCurrentFilter]);
		guiManagerBlocks.ShowTime();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//PLAY
	public void OnButtonTimeVoicePlayPressed(BasicButton sender)
	{
		//Start playing
		if(mMode==Modes.Idle){
			audioSource.clip=audioClips[mCurrentFilter];
			audioSource.Play();
			mVoiceRecButton.Show(0, Globals.ANIMATIONDURATION, false);
			mVoiceFxButton.Show(0, Globals.ANIMATIONDURATION, false);
			mVoiceSaveButton.Show(0, Globals.ANIMATIONDURATION, false);
			CurrentTime=0;
			mMode=Modes.Playing;
		}
		//Stop playing
		else if(mMode==Modes.Playing){
			audioSource.Stop();
			guiManagerBlocks.SetTime((int)audioClips[mCurrentFilter].length);
			mVoiceRecButton.Show();
			mVoiceFxButton.Show();
			mVoiceSaveButton.Show();
			mMode=Modes.Idle;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//REC
	public void OnButtonTimeVoiceRecPressed(BasicButton sender)
	{
		//Start recording
		if(mMode==Modes.Idle){
			ResetFilters();
			audioClips[0] = Microphone.Start(null, false, totalTime, frequency);
			mVoicePlayButton.Show(0, Globals.ANIMATIONDURATION, false);
			mVoiceFxButton.Show(0, Globals.ANIMATIONDURATION, false);
			mVoiceSaveButton.Show(0, Globals.ANIMATIONDURATION, false);
			guiManagerBlocks.SetColor(Color.red);
			CurrentTime=0;
			mMode=Modes.Recording;
			bLastSaved=false;
		}
		//Stop recording
		else if(mMode==Modes.Recording){
			if(audioClips[0] != null) {
				Microphone.End(null);
				RemoveEmptyData();
			}
			guiManagerBlocks.SetTime((int)Mathf.Max(1.0f,CurrentTime));
			guiManagerBlocks.SetColor(Color.white);
			mVoicePlayButton.Show();
			mVoiceFxButton.Show();
			mVoiceSaveButton.Show();
			mMode=Modes.Idle;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//FX
	public void OnButtonTimeVoiceFxPressed(BasicButton sender)
	{
		if(sender.Checked){
			guiManagerBlocks.mVoiceFxButtonBar.Show(true);
			guiManagerBlocks.HideTime();
			sender.Hide(0,0);
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//APPLY EFFECT
	public void OnButtonTimeVoiceFxEffectPressed(BasicButton sender)
	{
		/*if(mCurrentFilter==sender.ID)
			return;*/

		mCurrentFilter = sender.ID;

		if(audioClips[sender.ID] == null) {
			float[] indata, outdata;
			indata = new float[audioClips[0].samples * audioClips[0].channels];
			outdata = new float[audioClips[0].samples * audioClips[0].channels];
			audioClips[0].GetData(indata, 0);

			switch((Data.Chapter.Block.filterType)sender.ID) {
			case Data.Chapter.Block.filterType.Monster:
				filter.Monster(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.Mosquito:
				filter.Mosquito(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.MonsterPro:
				filter.MonsterPro(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.MosquitoPro:
				filter.MosquitoPro(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.Echo:
				filter.Echo(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.Compression:
				filter.Compression(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.Distorsion:
				filter.Distorsion(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.Robot:
				filter.Robot(indata, out outdata);
				break;
			case Data.Chapter.Block.filterType.Noise:
				filter.Noise(indata, out outdata);
				break;
			}
			/*if(sender.ID == (int)Data.Chapter.Block.filterType.Monster)
				filter.Monster(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.Mosquito)
				filter.Mosquito(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.MonsterPro)
				filter.MonsterPro(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.MosquitoPro)
				filter.MosquitoPro(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.Echo)
				filter.Echo(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.Compression)
				filter.Compression(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.Distorsion)
				filter.Distorsion(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.Robot)
				filter.Robot(indata, out outdata);
			else if(sender.ID == (int)Data.Chapter.Block.filterType.Noise)
				filter.Noise(indata, out outdata);*/

			//if(sender.ID != (int)Data.Chapter.Block.filterType.Off) {
				audioClips[mCurrentFilter] = AudioClip.Create("sound", outdata.Length, channels, frequency, false, false);
				audioClips[mCurrentFilter].SetData(outdata, 0);
			//}
		}

		CloseButtonBar();
		audioSource.clip = audioClips[mCurrentFilter];
		CurrentTime = (int)audioSource.clip.length;
		bLastSaved=false;
	}
	/*
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//OFF
	public void OnButtonVoiceFxOffPressed(BasicButton sender)
	{
		if(mCurrentFilter==(int)Data.Chapter.Block.filterType.Off)
			return;

		guiManagerBlocks.mVoiceFxButtonBar.Hide();
		mVoiceFxButton.Show(0.2f,0.2f,true);
		mVoiceFxButton.Checked=false;
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Off;
		SetTextBottom();
	}
	*/
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//SAVE
	public void OnButtonTimeVoiceSavePressed(BasicButton sender)
	{
		SaveAudioData(null,null);
		guiManagerBlocks.HideAllButtonBars();
		bLastSaved=true;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnApplicationPause(bool pauseStatus)
	{
		if(Microphone.IsRecording(null) && pauseStatus){
			Microphone.End(null);	
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnApplicationQuit()
	{
		if(Microphone.IsRecording(null)) {
			Microphone.End(null);	
		}
	}
}



