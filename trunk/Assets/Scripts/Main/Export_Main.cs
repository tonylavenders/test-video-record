//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://trac.ffmpeg.org/wiki/EncodeforYouTube
//http://stackoverflow.com/questions/13294919/can-you-stream-images-to-ffmpeg-to-construct-a-video-instead-of-saving-them-t
//http://stackoverflow.com/questions/15280722/android-camera-capture-using-ffmpeg
//http://stackoverflow.com/questions/13068327/capture-android-screen-as-a-video-file-using-ffmpeg
//https://github.com/sjitech/sji-android-screen-capture
//https://github.com/google/grafika/blob/master/src/com/android/grafika/VideoEncoderCore.java
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Debug=UnityEngine.Debug;
using System.Runtime.InteropServices;
using TVR;
using TVR.Helpers;

public class Export_Main : GUIManager
{
	//string mName;
	//List<Data.Episode.Scene.Stage> mStages;
	//int mCurrentScene;
	float mOriginalShadowDistance;
	float mTime;
	//int mTotalDurationFrames;
	
	bool render;
	bool bHaveAudio=false;

	Texture texRendering;

	//TabBackUndo mTabBackUndo;

	//static Data.Episode mEpisode;
	//static Data.Episode.Scene mScene;
	//static Data.Episode.Scene.Stage mStage;

	GUIStyle mStyleLabelWhite;
	GUIStyle mStyleLabelBlack;

	//GameObject sceneCamera;
	//Camera mainCam;
	public RenderTexture renderTex;
	Texture2D myTexture2D;
	int mCountCurrentFrame = 0;
	int mCountTotalFrames = 0;
	private System.Threading.Thread mThreadMixingAudio;

	enum States {
		INIT,
		EXPORTING,
		ENCODING,
		ENCODING_IOS,
		AUDIO,
		END
	}
	States state=States.INIT;

	//const int video_w = 1280;
	//const int video_h = 720;

	int video_w = Screen.width;
	int video_h = Screen.height;

	string mCurrentPath;
	Rect rectGUI;
	private System.IO.StreamWriter mLog;
	private bool mAudioProcessed;

	public bool Processing;
	public short HighestSample;
	private bool mAbort;

	public enum CaptureAudio {
		/// <summary>
		/// Audio will not be recorded.
		/// </summary>
		No_Audio = 0,

		/// <summary>
		/// Audio from the scene will be recorded in addition to video.
		/// </summary>
		Audio = 1,

		/// <summary>
		/// Audio from the scene and microphone will be recorded in addition to video.
		/// </summary>
		Audio_Plus_Mic = 2
	}

	/// <summary>
	/// Specifies whether or not time in Unity is locked to the video framerate. 
	/// </summary>
	public enum CaptureFramerateLock {

		/// <summary>
		/// Unity time is free running.  Use this option when you need to record video 
		/// in real time, e.g. when capturing a demo or game play session. 
		/// 
		/// If the rendering framerate exceeds the ability of the video encoder to process
		/// frames, then frames will be dropped from the video.  To prevent this, use the
		/// Throttled mode and reduce the target framerate to a level at which frame drops
		/// do not occur.  See GetNumberDroppedFrames() for more details.
		/// </summary>
		Unlocked = 0,

		/// <summary>
		/// Unity time advancement is locked to the recording framerate.  This ensures that the
		/// video will always be smooth, but may negatively affect framerate in the app.  This is a good
		/// choice when you're app needs to perform non-real time rendering of a video.
		/// Video frames will not be dropped in Locked mode.
		///
		/// Note that audio recorded from the scene will not maintain synchronization with the video
		/// when recorded in this mode.  This is because locking the framerate in Unity affects 
		/// rendering but does not apply to audio play back.  As long as your audio does not need to
		/// be precisely synchronized with the video (e.g. an ambient music track) this is not an issue.
		/// If you need synchronized video and audio, use the Unlocked capture type.  Alternatively,
		/// you can provide as many as two pre-recorded audio files to be mixed with the video.  Do this
		/// by using the EndRecordingSessionWithAudioFiles() method.
		/// </summary>
		Locked = 1,

		/// <summary>
		/// Unity time is free running.  Recording of frames will occur at the interval specified by
		/// the framerate parameter to BeginRecordingSession.  A typical use for this setting would be 
		/// to specify it in conjunction with a small value for framerate in order to achieve a 
		/// slideshow effect. For example, setting framerate to 0.25 in conjunction
		/// with Throttled will yield a video which displays a new frame every 4 seconds.
		/// 
		/// Throttled mode may also be used to prevent dropped video frames.  Set the target
		/// video framerate to a value (e.g. 30 fps) lower than the rendering framerate.
		/// </summary>
		Throttled = 2
	}

	/// <summary>
	/// Specifies what to do with the video when ending the recording session.
	/// </summary>
	public enum VideoDisposition {
		/// <summary>
		/// The video is placed in the Photo Album/Camera Roll
		///	on the device.  The name specified for the video when BeginRecordingSession()
		/// is called is used only as a temporary name while the video is being 
		/// recorded.
		/// </summary>
		Save_Video_To_Album = 0,

		/// <summary>
		/// The video is placed in the application's "Documents" 
		/// folder.  This is the location that is used by iTunes file sharing.  The file
		/// will be saved with the name specified when BeginRecordingSession() was called.
		/// </summary>
		Save_Video_To_Documents = 1,

		/// <summary>
		/// This video is deleted.  A typical use for this would be to
		///	respond appropriately to a "Cancel Recording" action by the user.
		/// </summary>
		Discard_Video = 2
	}

	/// <summary>
	// Indicates the current status of the recording session. 
	/// </summary>	
	public enum SessionStatusCode {

		/// <summary>
		/// The recording session has encountered no errors.
		/// </summary>
		OK = 0,

		/// <summary>
		/// The plugin failed when writing a video frame
		///	to the output stream.  This is usually caused by switching apps while
		/// the recording is in progress.
		/// </summary>
		Failed_FrameCapture = -1,

		/// <summary>
		/// A memory warning was received during the recording session.
		/// This requires a change to Application.mm to add a call to ivcp_Abort.  
		/// Use of ivcp_Abort is optional and you may instead elect to just let 
		/// iOS handle low memory conditions.  The trade-off is that iOS may also kill
		/// your app if enough memory cannot be recovered by killing backgrounded
		/// apps.
		/// </summary>
		Failed_Memory = -2,

		/// <summary>
		/// A failure occurred while attempting to copy the video the Camera Roll/Photo Album
		/// on the device.  A possible reason for this is that storage on the device is full.
		/// </summary>
		Failed_CopyToAlbum = -3,

		/// <summary>
		/// When the video was checked for compatibility with the Camera Roll/Photo Album it was
		/// discovered to be incompatible.
		/// </summary>
		Failed_VideoIncompatible = -4,

		/// <summary>
		/// A failure occurred during the mixing of the audio and video tracks into the finished
		/// video.
		/// </summary>
		Failed_SessionExport = -5,

		/// <summary>
		/// A failure occurred for reasons unknown.
		/// </summary>
		Failed_Unknown = -6,

		/// <summary>
		/// A plugin method was called without first calling BeginRecordingSession().
		/// </summary>
		Failed_SessionNotInitialized = -7,

		/// <summary>
		/// No camera was found to perform video recording and no custom rendertexture was
		/// specified.  You must specify one or the other before calling BeginRecordingSession().
		/// One or more cameras may be specified by setting the VideoCameras property.
		/// A custom rendertexture is specified by calling SetCustomRenderTexture.
		/// </summary>
		Failed_CameraNotFound = -8,

		/// <summary>
		/// Audio recording was requested but the Save Audio property was not specified in
		/// the inspector.  The Save Audio property must be set to reference a game object that 
		/// has an iVidCapProAudio component.
		/// </summary>
		Failed_AudioSourceNotFound = -9,

		/// <summary>
		/// The requested resolution is not supported.
		/// The maximum resolution currently supported is 1920x1080.
		/// </summary>
		Failed_ResolutionNotSupported = -10
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/*[DllImport ("__Internal")]
	private static extern void ivcp_Log (string message);*/

	[DllImport ("__Internal")]
	private static extern void ivcp_BeginRecordingSession(string videoName, int frameWidth, int frameHeight, int frameRate, uint glTextureID, CaptureAudio captureAudio, CaptureFramerateLock captureFramerateLock, int bitsPerSecond, int keyFrameInterval, float gamma, string commObjectName, bool showDebug);

	[DllImport ("__Internal")]
	private static extern SessionStatusCode ivcp_EndRecordingSession(VideoDisposition action);

	[DllImport ("__Internal")]
	private static extern SessionStatusCode ivcp_EndRecordingSessionWithAudioFiles(VideoDisposition action, string audioFile1, string audioFile2);

	[DllImport ("__Internal")]
	private static extern void ivcp_Release();

	[DllImport ("__Internal")]
	private static extern void ivcp_CaptureFrameFromRenderTexture();

	[DllImport ("__Internal")]
	private static extern SessionStatusCode ivcp_GetSessionStatusCode();

	[DllImport ("__Internal")]
	private static extern int ivcp_GetNumDroppedFrames();

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool Abort{
		get { return mAbort; }
		set {
			if(value == false && mAbort)
				throw new System.Exception("The abort proccess couldn't be aborted.");
			if(value == true && mAbort != value) {
				mAbort = value;
				QueueManager.add(new QueueManager.QueueManagerAction("Export_Cancel", () => preCancelRecording(), "Export_Main.preCancelRecording"), QueueManager.Priorities.Highest);
				QueueManager.clear("Export");
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Start()
	{
		texRendering = (Texture)ResourcesManager.LoadResource("Interface/Textures/renderizando", "Export");

		mOriginalShadowDistance = QualitySettings.shadowDistance;
		QualitySettings.shadowDistance = 20;

		//Para que las GUITextures tengan el tamaño correcto en pantalla
		//CameraManagerSmall.pixelInset = new Rect(-video_w / 2, -video_h / 2, video_w, video_h);

		//LetterboxManager.Start();
		//LetterboxManager.Init();
		//rectGUI = LetterboxManager.GetRectPercent();

		//SetGUICamera();
		//OnFinishedFadeOut();
		myTexture2D = new Texture2D(video_w, video_h, TextureFormat.RGB24, false);
		LoadChapterElements();
		Data.selChapter.Reset();
		//SetButtons();

		//SceneMgr.Get().OnFinished = OnFinishedFadeOut;
		//mCurrentScene = 0;
		/*
		mStages = new List<Data.Episode.Scene.Stage>();
		if(mEpisode != null) {
			LoadEpisode();
		} else if(mScene != null) {
			LoadScene(mScene);
		} else if(mStage != null) {
			LoadStage(mStage);
		} else {
			throw new System.Exception("Nothing to load.");
		}

		MirrorReflection.renderOnSceneCamera = true;
		*/
		//if(mStages.Count > 0) {
		if(Data.selChapter.Blocks.Count>0){
			//Data.selStage = mStages[0];
			CreateFolders();
			CleanFolder("*.*");
		} else {
			TVR.Utils.Message.Show(1, Texts.WARNING, Texts.NOTHING_TO_REPRODUCE, TVR.Utils.Message.Type.Accept, Texts.ACCEPT, "", NothingToReproduce);
			state = States.END;
		}
		mAudioProcessed = false;
		mAbort = false;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetButtons()
	{
		float pos_x = Screen.width - ButtonProperties.buttonSize/2.0f - ButtonProperties.buttonMargin*2;
		float pos_y = ButtonProperties.buttonSize/2.0f + ButtonProperties.buttonMargin*2;
		
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		
		EditButton.Init(pos, scale);
		EditButton.Show(0, Globals.ANIMATIONDURATION, true);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	 
	public void OnButtonExportEditClicked(BasicButton sender)
	{
		Data.selChapter.Stop();
		SceneMgr.Get.SwitchTo("ChapterMgr");
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void LoadChapterElements()
	{
		mCamera = GameObject.Find("CameraMain").transform;
		mCamera.gameObject.AddComponent<SceneCameraManager>();

		//mCamera.camera.enabled = false;
		//mCamera.camera.targetTexture = renderTex;
		Data.selChapter.Camera = mCamera.gameObject;
		
		CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("Player");
		CurrentCharacter.AddComponent<DataManager>();
		Data.selChapter.Character = CurrentCharacter;
		
		CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("Player");
		CurrentBackground.AddComponent<DataManager>();
		Data.selChapter.BackGround = CurrentBackground;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private void ExportAudioFiles()
	{
		/*mLog.WriteLine("Exportando audio.");
		mLog.WriteLine("-----------------");*/
		//QueueManager.processQueue();
		QueueManager.framesDelay = 1;
		/*
		if(mEpisode != null)
			mEpisode.saveAudioClips(mCurrentPath, mLog, this);
		else if(mScene != null)
			mScene.saveAudioClips(mCurrentPath, mLog, this);
		else if(mStage != null)
			mStage.saveAudioClips(mCurrentPath, mLog, this);
		*/

		//FALTA QUE XAVI LO IMPLEMENTE--> Data.selChapter.saveAudioClips(mCurrentPath, mLog, this);

		mThreadMixingAudio = new System.Threading.Thread(() => mixAudio());
		QueueManager.add(new QueueManager.QueueManagerAction("Export", mThreadMixingAudio.Start, "Export_Main.mixAudio"), QueueManager.Priorities.Normal);
		//QueueManager.add(new QueueManager.QueueManagerAction("Export", () => mixAudio()), QueueManager.Priorities.Low);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	 
	private void mixAudio()
	{
		Processing = true;
		mLog.WriteLine("");
		mLog.WriteLine("Mezclando audio.");
		DirectoryInfo dir = new DirectoryInfo(mCurrentPath);
		FileInfo[] info = dir.GetFiles("*.wav");
		mAudioProcessed = true;
		if(info.Length > 0) {
			float[] output;
			float multiplier = 0.0f;
			float highest2 = 0;
			QueueManager.addGCCollectionMode(QueueManager.Priorities.Normal);
			int filesBytes = (int)info[0].Length - Globals.WAVHEADERSIZE;
			if(filesBytes / (Globals.OUTPUTRATEPERSECOND * Globals.NUMCHANNELS * 2) < 15) {
				output = new float[filesBytes / 2];
				short sample = 0;
				for(int i = 0; i < info.Length; ++i) {
					if(mAbort)
						break;
					using(BinaryReader reader = new BinaryReader(File.Open(info[i].FullName, FileMode.Open, FileAccess.Read))) {
						reader.BaseStream.Seek(Globals.WAVHEADERSIZE, SeekOrigin.Begin);
						for(int j = 0; j < filesBytes / 2; ++j) {
							sample = reader.ReadInt16();
							output[j] += sample;
						}
						reader.Close();
					}
				}
				if(!mAbort) {
					multiplier = 1.0f - ((HighestSample - (short.MaxValue / info.Length)) / (float)HighestSample);
					for(int i = 0; i < filesBytes / 2; ++i) {
						if(multiplier != 1.0f)
							output[i] = (output[i] * multiplier) / info.Length;
						else
							output[i] = output[i] / info.Length;

						if(highest2 < Mathf.Abs(output[i]))
							highest2 = Mathf.Abs(output[i]);
					}
					multiplier = short.MaxValue / highest2;
				}
				if(!mAbort) {
					using(System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(Path.Combine(mCurrentPath, "AudioOutput.wav"), System.IO.FileMode.CreateNew, FileAccess.Write))) {
						writer.Write("RIFF".ToCharArray());
						writer.Write(filesBytes + Globals.WAVHEADERSIZE - 8);
						writer.Write("WAVE".ToCharArray());
						writer.Write("fmt ".ToCharArray());
						writer.Write(16);
						writer.Write((short)1);
						writer.Write(Globals.NUMCHANNELS);
						writer.Write(Globals.OUTPUTRATEPERSECOND);
						writer.Write(Globals.OUTPUTRATEPERSECOND * 2 * Globals.NUMCHANNELS);
						writer.Write((short)(2 * Globals.NUMCHANNELS));
						writer.Write((short)16);
						writer.Write("data".ToCharArray());
						writer.Write(filesBytes);

						short[] shortSamples = new short[filesBytes / 2];
						for(int i = 0; i < filesBytes / 2; ++i) {
							if(multiplier != 1.0f)
								shortSamples[i] = (short)Mathf.RoundToInt((output[i] * multiplier));
							else
								shortSamples[i] = (short)Mathf.RoundToInt(output[i]);
						}
						if(!mAbort) {
							byte[] bytesSamples = new byte[shortSamples.Length * sizeof(short)];
							System.Buffer.BlockCopy(shortSamples, 0, bytesSamples, 0, bytesSamples.Length);
							writer.Write(bytesSamples);
						}
						writer.Close();
					}
				}
			} else {
				//Si dura más de 15 segundos escribir byte a byte.
				const int maxSamples = 15 * Globals.OUTPUTRATEPERSECOND * Globals.NUMCHANNELS;
				output = new float[maxSamples];
				multiplier = 1.0f - ((HighestSample - (short.MaxValue / info.Length)) / (float)HighestSample);
				using(System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(Path.Combine(mCurrentPath, "AudioOutput.tmp"), System.IO.FileMode.CreateNew, FileAccess.Write))) {
					for(int h = 0; h < (filesBytes / 2f) / maxSamples; ++h) {
						if(mAbort)
							break;
						if(maxSamples > (filesBytes / 2) - (maxSamples * h))
							output = new float[(filesBytes / 2) - (maxSamples * h)];
						for(int i = 0; i < info.Length; ++i) {
							if(mAbort)
								break;
							using(BinaryReader reader = new BinaryReader(File.Open(info[i].FullName, FileMode.Open, FileAccess.Read))) {
								reader.BaseStream.Seek(Globals.WAVHEADERSIZE + (maxSamples * h * 2), SeekOrigin.Begin);
								for(int j = 0; j < output.Length; ++j) {
									output[j] += reader.ReadInt16();
								}
								reader.Close();
							}
						}
						for(int i = 0; i < output.Length; ++i) {
							if(i % maxSamples == 0 && i > 0) {
								if(mAbort)
									break;
								else
									writer.Flush();
							}
							if(multiplier != 1.0f)
								output[i] = (output[i] * multiplier) / info.Length;
							else
								output[i] = output[i] / info.Length;

							if(highest2 < Mathf.Abs(output[i]))
								highest2 = Mathf.Abs(output[i]);
							writer.Write(output[i]);
						}
					}
					writer.Close();
				}
				if(!mAbort) {
					multiplier = short.MaxValue / highest2;
					using(System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(Path.Combine(mCurrentPath, "AudioOutput.wav"), System.IO.FileMode.CreateNew, FileAccess.Write))) {
						writer.Write("RIFF".ToCharArray());
						writer.Write(filesBytes + Globals.WAVHEADERSIZE - 8);
						writer.Write("WAVE".ToCharArray());
						writer.Write("fmt ".ToCharArray());
						writer.Write(16);
						writer.Write((short)1);
						writer.Write(Globals.NUMCHANNELS);
						writer.Write(Globals.OUTPUTRATEPERSECOND);
						writer.Write(Globals.OUTPUTRATEPERSECOND * 2 * Globals.NUMCHANNELS);
						writer.Write((short)(2 * Globals.NUMCHANNELS));
						writer.Write((short)16);
						writer.Write("data".ToCharArray());
						writer.Write(filesBytes);

						using(BinaryReader reader = new BinaryReader(System.IO.File.Open(Path.Combine(mCurrentPath, "AudioOutput.tmp"), System.IO.FileMode.Open, FileAccess.Read))) {
							float output2;
							short shortSample;
							for(int i = 0; i < filesBytes / 2; ++i) {
								if(i % maxSamples == 0 && i > 0) {
									if(mAbort)
										break;
									else
										writer.Flush();
								}
								output2 = reader.ReadSingle();
								if(multiplier != 1.0f)
									shortSample = (short)Mathf.RoundToInt((output2 * multiplier));
								else
									shortSample = (short)Mathf.RoundToInt(output2);
								writer.Write(shortSample);
							}
							reader.Close();
						}
						writer.Close();
					}
				}
				CleanFolder("AudioOutput.tmp");
			}
			bHaveAudio = true;
			if(!mAbort)
				mLog.WriteLine("Audio exportado y mezclado en AudioOutput.wav.");
		} else {
			if(!mAbort)
				mLog.WriteLine("No tiene audio.");
		}
		Processing = false;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void CreateFolders()
	{
		if(!Directory.Exists(Globals.RendersPath))
			Directory.CreateDirectory(Globals.RendersPath);

		mCurrentPath = Globals.RendersPath;
		Debug.Log (mCurrentPath);

		/*
		string path;

		//Episode
		path = Path.Combine(Globals.RendersPath, "EP" + Data.selStage.episodeNumber.ToString("D2"));
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);

		if(mEpisode != null) {
			mCurrentPath = path;
		} else { //Scene
			path = Path.Combine(path, "SEQ" + Data.selStage.sceneNumber.ToString("D3"));
			if(!Directory.Exists(path))
				Directory.CreateDirectory(path);

			if(mScene != null) {
				mCurrentPath = path;
			} else { //Stage
				path = Path.Combine(path, "SCE" + Data.selStage.Number.ToString("D3"));
				if(!Directory.Exists(path))
					Directory.CreateDirectory(path);

				if(mStage != null)
					mCurrentPath = path;
			}
		}*/
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void CleanFolder(string searchPattern)
	{
		DirectoryInfo dir = new DirectoryInfo(mCurrentPath);
		FileInfo[] info = dir.GetFiles(searchPattern);
		foreach(FileInfo f in info) {
			f.Delete();
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void EncodeVideo()
	{
		//if(/*QueueManager.isEmpty && */!mThreadMixingAudio.IsAlive && mAudioProcessed) {
			Processing = true;
			mLog.WriteLine("Codificando vídeo.");

			if(Application.platform != RuntimePlatform.IPhonePlayer)
			{
				Process myProcess = new Process();
				myProcess.StartInfo.WorkingDirectory = mCurrentPath;

				if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor){
					myProcess.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath,"ffmpeg.exe");
				}else{
					myProcess.StartInfo.FileName = Path.Combine(Application.streamingAssetsPath, "ffmpeg");
				}
				if(bHaveAudio)
					myProcess.StartInfo.Arguments = "-i " + Path.Combine(mCurrentPath, "screenshot_%04d.png") + " -i " + Path.Combine(mCurrentPath, "AudioOutput.wav") + " -c:v libx264 -c:a aac -strict experimental " + GetVideoName();
				else
					myProcess.StartInfo.Arguments = "-i " + Path.Combine(mCurrentPath, "screenshot_%04d.png") + " -c:v libx264 -strict experimental " + GetVideoName();
				myProcess.StartInfo.UseShellExecute = false;
				myProcess.StartInfo.CreateNoWindow = true;
				myProcess.Start();
				myProcess.WaitForExit();
				int ExitCode = myProcess.ExitCode;
				if(ExitCode != 0) {
					mLog.WriteLine("Exit code: " + ExitCode);
					Debug.Log("Exit code: " + ExitCode);
				} else {
					CleanFolder("*.png");
					CleanFolder("*.wav");
					//Process.Start(mCurrentPath); //show folder with files
					//Process.Start(Path.Combine(mCurrentPath, GetVideoName())); //play encoded video
					mLog.WriteLine("Vídeo codificado (" + Path.Combine(mCurrentPath, GetVideoName()) + ").");
					mLog.WriteLine("");
					mLog.WriteLine("Eliminando archivos temporales.");
					mLog.WriteLine("Archivos temporales eliminados.");
					mLog.WriteLine("Finalizada la exportación.");
				}
				state = States.END;
				//mTabBackUndo.Show();
			} else {
				SessionStatusCode result;
				if(bHaveAudio) {
					result = ivcp_EndRecordingSessionWithAudioFiles(VideoDisposition.Save_Video_To_Album, Path.Combine(mCurrentPath, "AudioOutput.wav"), null);
					Debug.Log("Encoding " + (int)result);
				} else {
					result = ivcp_EndRecordingSession(VideoDisposition.Save_Video_To_Album);
					Debug.Log("Encoding " + (int)result);
				}
				mLog.WriteLine("Vídeo codificado (" + (int)result + " frames codificados).");
				mLog.WriteLine("");
				mLog.WriteLine("Eliminando archivos temporales.");
				mLog.WriteLine("Archivos temporales eliminados.");
				mLog.WriteLine("Finalizada la exportación.");
				state = States.ENCODING_IOS;
			}

			mLog.Close();
			mLog.Dispose();
			//renderTex.Release();
			Processing = false;
		//}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	string GetVideoName() 
	{
		string sVideoName = "VIDEO_" + Data.selChapter.Number.ToString("D2") + ".mp4";

		return sVideoName;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	IEnumerator SaveImage()
	{
		yield return new WaitForEndOfFrame();
		//mainCam.Render();
		//mCamera.camera.Render();

		if(Application.platform!=RuntimePlatform.IPhonePlayer){
			//RenderTexture.active = renderTex;
			myTexture2D.ReadPixels(new Rect(0, 0, video_w, video_h), 0, 0);
			myTexture2D.Apply();

			byte[] bytes = myTexture2D.EncodeToPNG();
			string path = Path.Combine(mCurrentPath, "screenshot_" + mCountTotalFrames.ToString("D4") + ".png");
			File.WriteAllBytes(path, bytes);
			
			myTexture2D.filterMode=FilterMode.Bilinear;
		}
		else{
			ivcp_CaptureFrameFromRenderTexture();
		}

		//renderTex.DiscardContents();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void CancelRecording(TVR.Utils.Message.ButtonClicked buttonClicked, int Identifier)
	{
		if(buttonClicked == TVR.Utils.Message.ButtonClicked.Yes) {
			QueueManager.clear("Export");
			preCancelRecording();
		} else {
			TVR.Utils.Message.Hide();
			QueueManager.pauseOnButtonDown = true;
		}
		QueueManager.pause = false;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private void preCancelRecording()
	{
		//Data.selStage.Stop(mTime, true);
		Data.selChapter.Stop();
		state = States.END;
		TVR.Utils.Message.Show(-1, Texts.WARNING, Texts.CANCEL_ENCODING1, TVR.Utils.Message.Type.NoButtons, "", "", null, new System.Threading.ThreadStart(CancelRecording), 0.5f);
		mAbort = true;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private void CancelRecording()
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer)
			ivcp_EndRecordingSession(VideoDisposition.Discard_Video);
		System.Threading.Thread.Sleep(500);
		while(Processing)
			System.Threading.Thread.Sleep(100);

		CleanFolder("*.wav");
		CleanFolder("*.png");
		if(mLog.BaseStream.CanWrite) {
			mLog.WriteLine("Exportación cancelada, " + (mCountTotalFrames - 1) + " frames exportados.");
			mLog.Close();
			mLog.Dispose();
		}
		QueueManager.add(new QueueManager.QueueManagerAction("Export", () => CancelRecording2(), "Export_Main.CancelRecording2"), QueueManager.Priorities.Highest);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private void CancelRecording2()
	{
		//SceneMgr.Get().SwitchTo("Menus");
		SceneMgr.Get.SwitchTo("ChapterMgr");
		QueueManager.pauseOnButtonDown = true;
		if(Application.platform == RuntimePlatform.IPhonePlayer)
			ivcp_Release();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnGUI()
	{
		InitStyles();

		if(Event.current.type!=EventType.Repaint)
			return;
		/*
		if(Application.platform==RuntimePlatform.IPhonePlayer){
			GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), texRendering);
		}
		else{
			//LetterboxManager.OnGUI();
			if(myTexture2D != null) {
				GUI.DrawTexture(new Rect(rectGUI.x * Screen.width, rectGUI.y * Screen.height, rectGUI.width * Screen.width, rectGUI.height * Screen.height), myTexture2D, ScaleMode.StretchToFill);
			}
		}
		*/
		if(state == States.EXPORTING) {
			GUI.Box(new Rect(-2, -2, 220, 120), "");
			GUILabelWithShadows(new Rect(10, 10, 200, 25), "VIDEO CREATION PROCESS");
			GUILabelWithShadows(new Rect(10, 30, 200, 40), "STEP 1/3: Exporting video frames");
			GUILabelWithShadows(new Rect(10, 50, 200, 25), "Current video frames: " + mCountCurrentFrame + "/" + Data.selChapter.totalFrames);
			//GUILabelWithShadows(new Rect(10, 70, 200, 25), "Total video frames: " + mCountTotalFrames + "/" + mTotalDurationFrames);
		}
		else if(state == States.ENCODING || state == States.ENCODING_IOS) {
			if(!mAudioProcessed || mThreadMixingAudio.IsAlive) {
				GUI.Box(new Rect(-2, -2, 220, 70), "");
				GUILabelWithShadows(new Rect(10, 10, 200, 25), "VIDEO CREATION PROCESS");
				GUILabelWithShadows(new Rect(10, 30, 200, 40), "STEP 2/3: Mixing audio");
			} else {
				GUI.Box(new Rect(-2, -2, 300, 130), "");
				GUILabelWithShadows(new Rect(10, 10, 200, 25), "VIDEO CREATION PROCESS");
				GUILabelWithShadows(new Rect(10, 30, 200, 40), "STEP 3/3: Encoding video");
				GUILabelWithShadows(new Rect(10, 50, 300, 25), "Encoding video: " + GetVideoName());
				GUILabelWithShadows(new Rect(10, 70, 200, 25), "Codec: H.264");
				GUILabelWithShadows(new Rect(10, 90, 200, 25), "Size: " + video_w + "x" + video_h);
			}
		} else if(state == States.AUDIO) {
			GUI.Box(new Rect(-2, -2, 220, 70), "");
			GUILabelWithShadows(new Rect(10, 10, 200, 25), "VIDEO CREATION PROCESS");
			//GUILabelWithShadows(new Rect(10, 30, 200, 40), "STEP 1/3: Capturing audio");
		}
		else if(state == States.END/* && mStages.Count > 0*/) {
			GUI.Box(new Rect(-2, -2, 220, 70), "");
			GUILabelWithShadows(new Rect(10, 10, 200, 25), "VIDEO CREATION PROCESS");
			GUILabelWithShadows(new Rect(10, 30, 200, 40), "Video file created.");
		}

		//mTabBackUndo.OnGUI();
		//Message.OnGUI();
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void InitStyles()
	{
		mStyleLabelWhite = new GUIStyle("Label");
		mStyleLabelWhite.normal.textColor = Color.white;
		
		mStyleLabelBlack = new GUIStyle("Label");
		mStyleLabelBlack.normal.textColor = Color.black;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void GUILabelWithShadows(Rect r, string text)
	{
		GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, mStyleLabelBlack);
		GUI.Label(r, text, mStyleLabelWhite);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		base.Update();

		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Running)
			return;

		//Message.update();

		//if(Message.State != Message.States.Hide)
		//	return;

		if(InputHelp.GetMouseButton(0) && state == States.EXPORTING && mCountTotalFrames > 0) {
			QueueManager.pause = true;
			QueueManager.pauseOnButtonDown = false;
			TVR.Utils.Message.Show(0, Texts.WARNING, Texts.CANCEL_ENCODING, TVR.Utils.Message.Type.YesNo, Texts.YES, Texts.NO, CancelRecording);
		}
		if(state == States.INIT) {
			Application.runInBackground = true;
			state = States.AUDIO;
			bHaveAudio = false;
			mLog = new System.IO.StreamWriter(System.IO.Path.Combine(mCurrentPath, "Export.log"), false);
			mLog.AutoFlush = true;

			mLog.WriteLine("Empieza la exportación del capítulo: " + Data.selChapter.Number + ".");
			/*
			if(mEpisode != null)
				mLog.WriteLine("Empieza la exportación del episodio " + mEpisode.Number + ".");
			else if(mScene != null)
				mLog.WriteLine("Empieza la exportación de la secuencia " + mScene.Number + ".");
			else if(mStage != null)
				mLog.WriteLine("Empieza la exportación de la escena " + mStage.Number + ".");
			*/
		}
		else if(state == States.AUDIO) {
			//ExportAudioFiles();
			state = States.EXPORTING;
			/*mLog.WriteLine("");
			mLog.WriteLine("Exportando Frames.");
			mLog.WriteLine("------------------");*/
			//StartStage();
			//mLog.WriteLine("Exportando frames escena " + Data.selStage.Number + " de la secuencia " + Data.selStage.sceneNumber + " del episodio " + Data.selStage.episodeNumber + " (" + Data.selStage.Frames + ").");
			mLog.WriteLine("Exportando frames capitulo: " + Data.selChapter.Number + " (" + Data.selChapter.totalFrames + ").");

			if(Application.platform == RuntimePlatform.IPhonePlayer)
				ivcp_BeginRecordingSession("BRB", video_w, video_h, 25, (uint)renderTex.GetNativeTextureID(), CaptureAudio.No_Audio, CaptureFramerateLock.Locked, -1, -1, -1, this.gameObject.name, false);
		}
		else if(state == States.EXPORTING) {
			UpdateExportFrames();
		}
		else if(state == States.ENCODING) {
			EncodeVideo();
		}
		else if(state == States.ENCODING_IOS) {

		}
		else if(state == States.END) {
			//mTabBackUndo.update();
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Create one PNG file per frame, then call to encode H.264 video 
	void UpdateExportFrames()
	{
		if(mTime < Data.selChapter.totalTime){
			//First pass: Update scene
			if(!render){
				render = true;
				Data.selChapter.Frame(mTime,false);
				mTime += Globals.MILISPERFRAME;
			}
			//Second pass: Save image to disk
			else {
				StartCoroutine(SaveImage());
				if(mCountCurrentFrame < Data.selChapter.totalFrames) {
					mCountCurrentFrame++;
					mCountTotalFrames++;
				}
				render = false;
			}
		}else{
			mLog.WriteLine("Frames exportados (" + mCountTotalFrames + ").");
			state = States.ENCODING;
			mTime = Data.selChapter.totalTime;
			Data.selChapter.Stop();
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnDestroy()
	{
		QueueManager.framesDelay = 5;
		QualitySettings.shadowDistance = mOriginalShadowDistance;
		
		//mEpisode = null;
		//mScene = null;
		//mStage = null;
		//mStages.Clear();
		//BRB.Helpers.ResourcesManager.UnloadScene("Scene");
		//BRB.Helpers.ResourcesManager.UnloadScene("Export");
		ResourcesManager.UnloadScene("Export");
		QueueManager.addGCCollectionMode(QueueManager.Priorities.Low);
		Application.runInBackground = false;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void OnFinishedFadeOut()
	{
		if(Data.selChapter.Blocks.Count > 0)
		{
			//renderTex = new RenderTexture(video_w, video_h, 24, RenderTextureFormat.ARGB32);

			//if(Application.platform!=RuntimePlatform.OSXEditor && Application.platform!=RuntimePlatform.OSXPlayer){
			//	renderTex.antiAliasing=8; //En MAC no funciona de momento
			//}
				
			//renderTex.Create();
			myTexture2D = new Texture2D(video_w, video_h, TextureFormat.RGB24, false);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/*
	public void BackPressed()
	{
		mTabBackUndo.Hide();
		Data.selStage.Stop(mTime, true);
		SceneMgr.Get().SwitchTo("Menus");
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void LoadEpisode()
	{
		mEpisode.loadScenes();
		List<Data.Episode.Scene> tempList = new List<Data.Episode.Scene>();
		foreach(Data.Episode.Scene scene in mEpisode.Scenes.Values) {
			tempList.Add(scene);
		}
		tempList.Sort();
		
		foreach(Data.Episode.Scene scene in tempList) {
			LoadScene(scene);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void LoadScene(Data.Episode.Scene scene)
	{
		scene.loadStages();
		List<Data.Episode.Scene.Stage> tempList = new List<Data.Episode.Scene.Stage>();
		foreach(Data.Episode.Scene.Stage stage in scene.Stages.Values) {
			tempList.Add(stage);
		}
		tempList.Sort();
		
		foreach(Data.Episode.Scene.Stage stage in tempList) {
			LoadStage(stage);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void LoadStage(Data.Episode.Scene.Stage stage)
	{
		stage.loadStage();
		if(stage.Frames > 0) {
			mStages.Add(stage);
			//stage.Background.getInstance("Scene");
			//stage.Background.destroyInstancePlayer();
			mTotalDurationFrames += stage.Frames;
		}
	}
	*/
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void NothingToReproduce(TVR.Utils.Message.ButtonClicked buttonClicked, int Identifier)
	{
		//SceneMgr.Get().SwitchTo("Menus");
		SceneMgr.Get.SwitchTo("ChapterMgr");
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/*
	void StartStage()
	{
		//Background
		Data.selStage = mStages[mCurrentScene];
		GameObject currentBackground = Data.selStage.Background.getInstance("Scene");
		GameObject backgroundCam = currentBackground.transform.Find("Camera").gameObject;

		//Camera
		sceneCamera = Data.selStage.SceneCamera.getInstance("Scene");
		sceneCamera.transform.Find("mesh").gameObject.SetActive(false);
		mainCam = sceneCamera.transform.Find("Camera").camera;
		mainCam.enabled = false;
		mainCam.targetTexture = renderTex;

		if(backgroundCam!=null){
			Color backgroundColor = backgroundCam.camera.backgroundColor;
			mainCam.backgroundColor = backgroundColor;
		}else{
			Debug.Log("¡El escenario deberia tener un objecto Camera!");
		}

		//Stage objects
		foreach(Data.Episode.Scene.Stage.StageObject stageObject in Data.selStage.Objects.Values)
			stageObject.getInstance("Scene");

		Data.selStage.Reset(0);
		Data.selStage.Frame(0, false, false, true);
		mTime = 0;
	}*/

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/*
	void EndStage()
	{
		Data.selStage.Stop(Data.selStage.DurationInSeconds, true);
		Data.selStage.Background.destroyInstancePlayer();
		//Data.selStage.SceneCamera.destroyInstancePlayer();
		foreach(Data.Episode.Scene.Stage.StageObject stageObject in Data.selStage.Objects.Values)
			stageObject.destroyInstancePlayer();
		BRB.Helpers.ResourcesManager.UnloadScene("Scene");
		Resources.UnloadUnusedAssets();
	}
	*/
	/* ------------------------------------------------------------------------
	   -- PluginCompletionHandler --
	   
	   This method will be called by the plugin when all processing is complete
	   for the current recording session.
	   
	   ------------------------------------------------------------------------ */
	private void PluginCompletionHandler(string message)
	{
		CleanFolder("*.wav");
		ivcp_Release();
		//mTabBackUndo.Show();
		state = States.END;
	}

	/* ------------------------------------------------------------------------
	   -- PluginErrorHandler --
	   
	   This method will be called by the plugin when an error has occurred 
	   during recording.
	   
	   ------------------------------------------------------------------------ */
	private void PluginErrorHandler(string message)
	{
		SessionStatusCode sessionStatus = ivcp_GetSessionStatusCode();
		Debug.Log(sessionStatus);
		//Data.selStage.Stop(mTime, true);
		Data.selChapter.Stop();
		state = States.END;
		if(mLog.BaseStream.CanWrite)
			mLog.WriteLine("Error durante la exportación, " + sessionStatus + ".");
		TVR.Utils.Message.Show(-1, Texts.WARNING, Texts.ERROR_EXPORT, TVR.Utils.Message.Type.NoButtons, "", "", null, new System.Threading.ThreadStart(CancelRecording), 0.5f);
		mAbort = true;
		ivcp_Release();
	}
}



