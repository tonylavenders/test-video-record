using UnityEngine;
using TVR.Utils;
using TVR.Helpers;
using TVR.Button;
using System.Threading;

namespace TVR.Utils
{	
	public static class Message {
		public enum States {
			Hide,
			Showing,
			Running,
			Hiding
		}
		public enum Type {
			YesNo,
			Accept,
			NoButtons
		}
		public enum ButtonClicked {
			Yes,
			No,
			Accept
		}
		
		private const int BUTTONS_SEPARATION = 28 / 2;
		private const int TEXT_MARGIN = 5;
		private const int TEXT_BUTTON_MARGIN = 30;
		private const int DIALOG_W = 890 / 2;
		private const int DIALOG_H = 278 / 2;
		private const int TITLE_FONT_SIZE = 30;
		private const int BUTTON_FONT_SIZE = 23;
		private const int DIALOG_Y = 160;

		private static int mBUTTONS_SEPARATION;
		private static int mTEXT_MARGIN;
		private static int mTEXT_BUTTON_MARGIN;
		private static int mDIALOG_W;
		private static int mDIALOG_H;
		private static int mTITLE_FONT_SIZE;
		private static int mBUTTON_FONT_SIZE;
		private static int mDIALOG_Y;
		private static int mBUTTONS_Y;

		private static TextButton mYesButton;
		private static TextButton mNoButton;
		private static GUIStyle mStyle1, mStyle2;
		private static string mCaption="";
		private static string mMessage="";
		private static float mAnimationDuration;
		private static int mIdentifier;
		
		//private static Texture2D mBlack;
		private static Texture2D mTexDialog;
		//private static RectScreen.width - 280 * 2 mRBlack;
		private static Rect mRTexDialog;
		private static Rect mRCaption;
		private static Rect mRMessage;
		
		private static States mState;
		private static SmoothStep mAlpha;
		
		private static Type mType;
		
		public delegate void Clicked(ButtonClicked buttonClicked, int Identifier);

		private static Clicked mClicked;

		private static Thread mThread;
		private static float mMinTime;
		private static float mCount;

		public static bool yesJustReleased {
			get {
				return mYesButton.justReleased && mType == Type.YesNo;
			}
		}

		public static bool noJustReleased {
			get {
				return mType == Type.YesNo && mNoButton.justReleased;
			}
		}

		public static bool acceptJustReleased {
			get {
				return mYesButton.justReleased && mType == Type.Accept;
			}
		}

		public static float Alpha {
			get { return mAlpha.Value; }
			set {
				mAlpha.Value = value;
				mAlpha.Enable = false;
			}
		}

		public static States State {
			get { return mState; }
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Init(float animationDuration) {
			mBUTTONS_SEPARATION = Mathf.RoundToInt(BUTTONS_SEPARATION * ButtonProperties.scaleCorrectionX);
			mTEXT_MARGIN = Mathf.RoundToInt(TEXT_MARGIN  * ButtonProperties.scaleCorrectionX);
			mTEXT_BUTTON_MARGIN = Mathf.RoundToInt(TEXT_BUTTON_MARGIN * ButtonProperties.scaleCorrectionX);
			mDIALOG_W = Mathf.RoundToInt(DIALOG_W * ButtonProperties.scaleCorrectionX);
			mDIALOG_H = Mathf.RoundToInt(DIALOG_H * ButtonProperties.scaleCorrectionX);
			mTITLE_FONT_SIZE = Mathf.RoundToInt(TITLE_FONT_SIZE * ButtonProperties.scaleCorrectionX);
			mBUTTON_FONT_SIZE = Mathf.RoundToInt(BUTTON_FONT_SIZE * ButtonProperties.scaleCorrectionX);
			mDIALOG_Y = Mathf.RoundToInt(DIALOG_Y * ButtonProperties.scaleCorrectionY);
			mBUTTONS_Y = Mathf.RoundToInt(mDIALOG_H + mDIALOG_Y + ButtonProperties.buttonMargin);

			Font font = (Font)ResourcesManager.LoadResource("Interface/Fonts/Futura Oblique", "Message");
			Texture buttonUp = (Texture)ResourcesManager.LoadResource("Interface/Textures/Warning/button", "Message");
			Texture buttonDown = (Texture)ResourcesManager.LoadResource("Interface/Textures/Warning/button_pressed", "Message");
			mTexDialog = (Texture2D)ResourcesManager.LoadResource("Interface/Textures/Warning/window", "Message");

			mStyle1 = new GUIStyle();
			mStyle1.font = font;
			mStyle1.clipping = TextClipping.Clip;
			mStyle1.wordWrap = true;
			mStyle1.fontSize = mTITLE_FONT_SIZE;
			mStyle1.alignment = TextAnchor.UpperCenter;
			mStyle1.fontStyle = FontStyle.Bold;
			mStyle1.normal.textColor = Color.black;
		
			mStyle2 = new GUIStyle();
			mStyle2.font = font;
			mStyle2.clipping = TextClipping.Clip;
			mStyle2.wordWrap = true;
			mStyle2.fontSize = mBUTTON_FONT_SIZE;
			mStyle2.alignment = TextAnchor.UpperCenter;
			mStyle2.fontStyle = FontStyle.Normal;
			mStyle2.normal.textColor = new Color(0f, 0f, 0f, 0.5f);
		
			mAnimationDuration = animationDuration;
		
			mYesButton = new TextButton(new Rect(0, 0, 0, 0), buttonDown, buttonUp, buttonDown, buttonUp, font, false);
			mYesButton.TextSize = mBUTTON_FONT_SIZE;
			mYesButton.TextStyle = FontStyle.Normal;
			mYesButton.TextPosition = TextAnchor.MiddleCenter;
			mYesButton.shadow = true;
			mYesButton.TextColor = Color.white;
			mYesButton.scaleMode = ScaleMode.StretchToFill;
			mYesButton.enable = false;

			mNoButton = new TextButton(new Rect(0, 0, 0, 0), buttonDown, buttonUp, buttonDown, buttonUp, font, false);
			mNoButton.TextSize = mBUTTON_FONT_SIZE;
			mNoButton.TextStyle = FontStyle.Normal;
			mNoButton.TextPosition = TextAnchor.MiddleCenter;
			mNoButton.Text = "No";
			mNoButton.shadow = true;
			mNoButton.TextColor = Color.white;
			mNoButton.scaleMode = ScaleMode.StretchToFill;
			mNoButton.enable = false;

			mNoButton.Position(new Rect((Screen.width + mBUTTONS_SEPARATION) / 2, mBUTTONS_Y, ButtonProperties.buttonSize, ButtonProperties.buttonSize));
		
			mState = States.Hide;
			mAlpha = new SmoothStep(0, 0, 0, false, 0);
		
			//mBlack = (Texture2D)ResourcesManager.LoadResource("SceneMgr/black_pixel", "Message");
			//mRBlack = new Rect(0, 0, Screen.width, Screen.height);
		
			mRTexDialog = new Rect((Screen.width - mDIALOG_W) / 2, mDIALOG_Y, mDIALOG_W, mDIALOG_H);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		public static void update() {
			if(mState != States.Hide) {
				mYesButton.Update();
				mNoButton.Update();
			}
			switch(mState) {
			case States.Showing:
				mAlpha.Update();
				if(mAlpha.Ended) {
					mState = States.Running;
					mYesButton.enable = true;
					mNoButton.enable = true;
				}
				break;
			case States.Running:
				if(mThread != null) {
					if(!mThread.IsAlive && mCount > mMinTime)
						Hide();
					else
						mCount += Time.deltaTime;
				} else if(mYesButton.justReleased || mNoButton.justReleased) {
					mAlpha.Reset(0, mAnimationDuration, true, 0);
					mState = States.Hiding;
					mYesButton.enable = false;
					mNoButton.enable = false;
					if(mClicked != null) {
						if(mYesButton.justReleased) {
							if(mType == Type.YesNo)
								mClicked(ButtonClicked.Yes, mIdentifier);
							else
								mClicked(ButtonClicked.Accept, mIdentifier);
						} else if(mNoButton.justReleased)
							mClicked(ButtonClicked.No, mIdentifier);
					}
				}
				break;
			case States.Hiding:
				mAlpha.Update();
				if(mAlpha.Ended) {
					mState = States.Hide;
				}
				break;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		public static void Show(int identifier, string caption, string message, Type type, string button1Text, string button2Text, Clicked clicked) {
			mCaption = caption;
			mMessage = message;
			mType = type;
		
			int caption_h, message_h;
		
			CalcContentY(caption, message, out caption_h, out message_h);
		
			if(mType == Type.YesNo) {
				mYesButton.Position(new Rect(((Screen.width - mBUTTONS_SEPARATION) / 2) - ButtonProperties.buttonSize, mBUTTONS_Y, ButtonProperties.buttonSize, ButtonProperties.buttonSize));
			} else {
				mYesButton.Position(new Rect((Screen.width - ButtonProperties.buttonSize) / 2, mBUTTONS_Y, ButtonProperties.buttonSize, ButtonProperties.buttonSize));
			}
		
			mYesButton.Text = button1Text;
			mYesButton.enable = false;
			mNoButton.Text = button2Text;
			mNoButton.enable = false;
		
			mAlpha.Reset(1, mAnimationDuration, true, 0);
			mState = States.Showing;
			mIdentifier = identifier;
			mClicked = clicked;
			mThread = null;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		public static void ShowNow(int identifier, string caption, string message, Type type, string button1Text, string button2Text, Clicked clicked) {
			Show(identifier, caption, message, type, button1Text, button2Text, clicked);
			mAlpha.Value = 1;
			mState = States.Running;
			mYesButton.enable = true;
			mNoButton.enable = true;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Show(int identifier, string caption, string message, Type type, string button1Text, string button2Text, Clicked clicked, ThreadStart tStart, float minTime) {
			Show(identifier, caption, message, type, button1Text, button2Text, clicked);
			mMinTime = minTime;
			mCount = 0;
			mThread = new Thread(tStart);
			mThread.Start();
			while (!mThread.IsAlive);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void HideNow() {
			mAlpha.Value = 0;
			mState = States.Hide;
			mYesButton.enable = false;
			mNoButton.enable = false;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Hide() {
			mAlpha.Reset(0, mAnimationDuration, true, 0);
			mState = States.Hiding;
			mYesButton.enable = false;
			mNoButton.enable = false;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		static void CalcContentY(string caption, string message, out int caption_h, out int message_h) {
			int contentY;
			int caption_H;
			int h;
			int buttons_h = 0;
			int text_margin_h = 0;
			int text_button_margin_h = 0;
		
			caption_h = 0;
			message_h = 0;
		
			float w = mDIALOG_W - (mTEXT_BUTTON_MARGIN * 2);
			caption_h = (int)(mStyle1.CalcSize(new GUIContent(caption)).y);
			caption_H = caption_h;
		
			if(message.Length > 0) {
				message_h = (int)(mStyle2.CalcHeight(new GUIContent(message), w));
				text_margin_h = mTEXT_MARGIN;
			}
		
			if(mType != Type.NoButtons) {
				buttons_h = Mathf.RoundToInt(ButtonProperties.buttonSize);
				text_button_margin_h = mTEXT_BUTTON_MARGIN;
			}
		
			h = caption_h + text_margin_h + message_h + text_button_margin_h + buttons_h;
		
			contentY = mDIALOG_Y + (mDIALOG_H - h) / 2;

			mRCaption = new Rect((Screen.width - w) / 2, contentY, w, 35);
			mRMessage = new Rect((Screen.width - w) / 2, contentY+ mTEXT_MARGIN + caption_H, w, 55);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		public static void OnGUI() {
			if(Event.current.type != EventType.Repaint)
				return;

			if(mState != States.Hide) {
				//Fondo oscuro
				Color colorGUI = GUI.color;
				Color color = Color.white;
				color.a *= mAlpha.Value * 0.75f;
				GUI.color = color;
				//GUI.DrawTexture(mRBlack, mBlack);
				//GUI.color = colorGUI;
			
				//Diálogo
				color.a = mAlpha.Value * colorGUI.a;
				GUI.color = color;
				GUI.DrawTexture(mRTexDialog, mTexDialog);
			
				//Texto1
				GUI.Label(mRCaption, mCaption, mStyle1);
			
				//Texto2
				if(mMessage.Length > 0)
					GUI.Label(mRMessage, mMessage, mStyle2);
			
				//Botones
				if(mType != Type.NoButtons) {
					mYesButton.OnGUI(color, false);
					if(mType == Type.YesNo)
						mNoButton.OnGUI(color, false);
				}
			
				GUI.color = colorGUI;
			}
		}
	}
}