using UnityEngine;
using System.Collections;
using TVR;
using System;

public class SoundRecorder : MonoBehaviour
{
	public BasicButton mVoicePlayButton;
	public BasicButton mVoiceRecButton;
	public BasicButton mVoiceFxButton;
	public BasicButton mVoiceSaveButton;

	public GUIManagerBlocks guiManagerBlocks;

	AudioClip audioClip;
	AudioClip audioClipMonster;
	AudioClip audioClipSmurf;
	AudioSource audioSource;

	float mCurrentTime;
	float CurrentTime{
		get{ return mCurrentTime;}
		set{ 
			mCurrentTime=value;
			guiManagerBlocks.SetTime((int)value);
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

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		mMode = Modes.Idle;

		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.loop = false;
		audioSource.playOnAwake = false;
		audioSource.clip = null;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void InitButtons()
	{
		//Voice: Play button
		float pos_x = ButtonProperties.buttonBarScaleX*2.0f+ButtonProperties.buttonMargin/2.0f+ButtonProperties.buttonSize/2.0f;
		float pos_y = 4*(ButtonProperties.buttonSize/2+ButtonProperties.buttonMargin/2) + Screen.height/2;
		pos_y -= ButtonProperties.buttonMargin+ButtonProperties.buttonSize; 
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
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

	public void SetAudioClip()
	{
		if(Data.selChapter.selBlock.Sound!=null){
			audioClip = Data.selChapter.selBlock.Sound;
			audioSource.clip = audioClip;
			mCurrentTime = (int)Data.selChapter.selBlock.Sound.length;
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
			if(CurrentTime < audioClip.length){
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
			mVoicePlayButton.Show(0, Globals.ANIMATIONDURATION, audioClip!=null);
			mVoiceRecButton.Show();
			mVoiceFxButton.Show(0, Globals.ANIMATIONDURATION, audioClip!=null);
			mVoiceSaveButton.Show(0, Globals.ANIMATIONDURATION, audioClip!=null);
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
		float[] samples = new float[audioClip.samples * audioClip.channels];
		audioClip.GetData(samples, 0);
		
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
		
		audioClip = AudioClip.Create("user_clip", samplesTrim.Length, channels, frequency, false, false);
		audioClip.SetData(samplesTrim, 0);
		
		samples = null;
		samplesTrim = null;
		
		audioSource.clip = audioClip;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//PLAY
	public void OnButtonTimeVoicePlayPressed(BasicButton sender)
	{
		//Start playing
		if(mMode==Modes.Idle){
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
			guiManagerBlocks.SetTime((int)audioClip.length);
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
			audioClip = Microphone.Start(null, false, totalTime, frequency);
			mVoicePlayButton.Show(0, Globals.ANIMATIONDURATION, false);
			mVoiceFxButton.Show(0, Globals.ANIMATIONDURATION, false);
			mVoiceSaveButton.Show(0, Globals.ANIMATIONDURATION, false);
			guiManagerBlocks.SetColor(Color.red);
			CurrentTime=0;
			mMode=Modes.Recording;
		}
		//Stop recording
		else if(mMode==Modes.Recording){
			if(audioClip != null) {
				Microphone.End(null);
				RemoveEmptyData();
			}
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
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//SAVE
	public void OnButtonTimeVoiceSavePressed(BasicButton sender)
	{
		if(audioClip!=null){
			Data.selChapter.selBlock.BlockType = Data.Chapter.Block.blockTypes.Voice;
			Data.selChapter.selBlock.Frames = (int)audioClip.length*Globals.FRAMESPERSECOND;
			Data.selChapter.selBlock.Sound = audioClip;
			Data.selChapter.selBlock.OriginalSound = audioClip;
			Data.selChapter.selBlock.Save();
		}
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



