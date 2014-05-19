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

	enum Modes{
		Idle,
		Playing,
		Recording
	}
	Modes mMode;

	public bool bLastSaved=true;

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
		//float pos_x = ButtonProperties.buttonBarScaleX*2.0f+ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize/2.0f;
		float pos_x = (ButtonProperties.buttonBarScaleX/2.0f) + ButtonProperties.buttonBarScaleX*2.0f;
		float pos_y = 4*(ButtonProperties.buttonSize/2+ButtonProperties.buttonMargin/2) + Screen.height/2;
		pos_y -= ButtonProperties.buttonMargin+ButtonProperties.buttonSize; 
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		mVoicePlayButton.Init(pos, scale);
		
		//Voice: Rec button
		//pos_x += ButtonProperties.buttonMargin+ButtonProperties.buttonSize;
		pos_x += ButtonProperties.buttonBarScaleX;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceRecButton.Init(pos, scale);
		
		//Voice: Fx button
		//pos_x += ButtonProperties.buttonMargin+ButtonProperties.buttonSize;
		pos_x += ButtonProperties.buttonBarScaleX;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceFxButton.Init(pos, scale);
		
		//Voice: Save button
		//pos_x += ButtonProperties.buttonMargin+ButtonProperties.buttonSize;
		pos_x += ButtonProperties.buttonBarScaleX;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		mVoiceSaveButton.Init(pos, scale);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetAudioClip()
	{
		if(Data.selChapter.selBlock.BlockType==Data.Chapter.Block.blockTypes.Voice)
		{
			audioClips[(int)Data.selChapter.selBlock.FilterType] = Data.selChapter.selBlock.Sound;
			audioSource.clip = Data.selChapter.selBlock.Sound;
			CurrentTime = (int)Data.selChapter.selBlock.Sound.length;

			if(Data.selChapter.selBlock.FilterType!=Data.Chapter.Block.filterType.Off){
				audioClips[0] = Data.selChapter.selBlock.OriginalSound;
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
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
				guiManagerBlocks.SetColor(Color.red);
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
			block.Save();
			if(button==null){
				button=guiManagerBlocks.RightButtonBar.currentSelected;
			}
			button.SetTextBottom();
			bLastSaved=true;
		}
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

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void ApplyFilter(string filterName, float[] outdata)
	{
		guiManagerBlocks.mVoiceFxButtonBar.Hide();
		mVoiceFxButton.SetTextBottom(filterName);
		mVoiceFxButton.Show(0.2f,0.2f,true);
		mVoiceFxButton.Checked=false;

		audioClips[mCurrentFilter]= AudioClip.Create("sound", outdata.Length, channels, frequency, false, false);
		audioClips[mCurrentFilter].SetData(outdata,0);
		audioSource.clip = audioClips[mCurrentFilter];
		CurrentTime = (int)audioSource.clip.length;

		guiManagerBlocks.ShowTime();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//MONSTER
	public void OnButtonVoiceFxMonsterPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.Monster(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Monster;
		ApplyFilter("Monster", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//SMURF
	public void OnButtonVoiceFxSmurfPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.Mosquito(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Mosquito;
		ApplyFilter("Smurf", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//ECHO
	public void OnButtonVoiceFxEchoPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.Echo(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Echo;
		ApplyFilter("Echo", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//MONSTER PRO
	public void OnButtonVoiceFxMonsterProPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.MonsterPro(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.MonsterPro;
		ApplyFilter("M-Pro", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//SMURF PRO
	public void OnButtonVoiceFxSmurfProPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.MosquitoPro(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.MosquitoPro;
		ApplyFilter("S-Pro", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//ROBOT
	public void OnButtonVoiceFxRobotPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.Robot(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Robot;
		ApplyFilter("Robot", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//DISTORTION
	public void OnButtonVoiceFxDistortionPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.Distorsion(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Distorsion;
		ApplyFilter("Dist", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//NOISE
	public void OnButtonVoiceFxNoisePressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.Noise(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Noise;
		ApplyFilter("Noise", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//COMPRESSION
	public void OnButtonVoiceFxCompressionPressed(BasicButton sender)
	{
		float[] indata, outdata;
		indata = new float[audioClips[0].samples * audioClips[0].channels];
		audioClips[0].GetData(indata, 0);
		filter.Compression(indata, out outdata);
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Compression;
		ApplyFilter("Compress", outdata);
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//OFF
	public void OnButtonVoiceFxOffPressed(BasicButton sender)
	{
		guiManagerBlocks.mVoiceFxButtonBar.Hide();
		mVoiceFxButton.SetTextBottom("<none>");
		mVoiceFxButton.Show(0.2f,0.2f,true);
		mVoiceFxButton.Checked=false;
		mCurrentFilter = (int)Data.Chapter.Block.filterType.Off;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//SAVE
	public void OnButtonTimeVoiceSavePressed(BasicButton sender)
	{
		SaveAudioData(null,null);
		guiManagerBlocks.HideAllButtonBars();
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



