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
		
		private const int BUTTONS_SEPARATION = 30 / 2;
		private const int BUTTONS_WIDTH = 182 / 2;
		private const int BUTTONS_HEIGHT = 182 / 2;
		private const int TITLE_FONT_SIZE = 30 / 2;
		private const int BUTTON_FONT_SIZE = 23 / 2;
		private const int TEXT_MARGIN = 5 / 2;
		private const int TEXT_BUTTON_MARGIN = 30 / 2;
		private const int DIALOG_Y = 160;
		private const int DIALOG_W = 890 / 2;
		private const int DIALOG_H = 278 / 2;

		private static TextButton mYesButton;
		private static TextButton mNoButton;
		private static GUIStyle mStyle1, mStyle2;
		private static string mMessage1="";
		private static string mMessage2="";
		private static float mAnimationDuration;
		private static int mIdentifier;
		
		private static int mContentY;
		private static int mMessage1_H;
		
		//private static Texture2D mBlack;
		private static Texture2D mTexDialog;
		//private static Rect mRBlack;
		private static Rect mRTexDialog;
		
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
			Font font = (Font)ResourcesManager.LoadResource("Interface/Fonts/Futura Oblique", "Message");
			Texture buttonUp = (Texture)ResourcesManager.LoadResource("Interface/Textures/Warning/button", "Message");
			Texture buttonDown = (Texture)ResourcesManager.LoadResource("Interface/Textures/Warning/button_pressed", "Message");
			mTexDialog = (Texture2D)ResourcesManager.LoadResource("Interface/Textures/Warning/window", "Message");

			mStyle1 = new GUIStyle();
			mStyle1.font = font;
			mStyle1.clipping = TextClipping.Clip;
			mStyle1.wordWrap = true;
			mStyle1.fontSize = TITLE_FONT_SIZE;
			mStyle1.alignment = TextAnchor.UpperCenter;
			mStyle1.fontStyle = FontStyle.Normal;
			mStyle1.normal.textColor = Color.black;
		
			mStyle2 = new GUIStyle();
			mStyle2.font = font;
			mStyle2.clipping = TextClipping.Clip;
			mStyle2.wordWrap = true;
			mStyle2.fontSize = BUTTON_FONT_SIZE;
			mStyle2.alignment = TextAnchor.UpperCenter;
			mStyle2.fontStyle = FontStyle.Normal;
			mStyle2.normal.textColor = new Color(0f, 0f, 0f, 0.5f);
		
			mAnimationDuration = animationDuration;
		
			mYesButton = new TextButton(new Rect(0, 0, 0, 0), buttonDown, buttonUp, buttonDown, buttonUp, font, false);
			mYesButton.TextSize = BUTTON_FONT_SIZE;
			mYesButton.TextStyle = FontStyle.Normal;
			mYesButton.TextPosition = TextAnchor.MiddleCenter;
			mYesButton.shadow = true;
			mYesButton.TextColor = Color.white;
			mYesButton.scaleMode = ScaleMode.StretchToFill;
			mYesButton.enable = false;

			mNoButton = new TextButton(new Rect(0, 0, 0, 0), buttonDown, buttonUp, buttonDown, buttonUp, font, false);
			mNoButton.TextSize = BUTTON_FONT_SIZE;
			mNoButton.TextStyle = FontStyle.Normal;
			mNoButton.TextPosition = TextAnchor.MiddleCenter;
			mNoButton.Text = "No";
			mNoButton.shadow = true;
			mNoButton.TextColor = Color.white;
			mNoButton.scaleMode = ScaleMode.StretchToFill;
			mNoButton.enable = false;
		
			mState = States.Hide;
			mAlpha = new SmoothStep(0, 0, 0, false, 0);
		
			//mBlack = (Texture2D)ResourcesManager.LoadResource("SceneMgr/black_pixel", "Message");
			//mRBlack = new Rect(0, 0, Screen.width, Screen.height);
		
			mRTexDialog = new Rect((Screen.width - DIALOG_W) / 2, DIALOG_Y, DIALOG_W, DIALOG_H);
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
	
		public static void Show(int identifier, string message1, string message2, Type type, string button1Text, string button2Text, Clicked clicked) {
			mMessage1 = message1;
			mMessage2 = message2;
			mType = type;
		
			int message1_h, message2_h;
		
			CalcContentY(message1, message2, out message1_h, out message2_h);
		
			int buttons_pos_y = mContentY + message1_h + TEXT_MARGIN + message2_h + TEXT_BUTTON_MARGIN;
		
			if(mType == Type.YesNo) {
				mYesButton.Position(new Rect((Screen.width / 2) - (BUTTONS_SEPARATION + BUTTONS_WIDTH), buttons_pos_y, BUTTONS_WIDTH, BUTTONS_HEIGHT));
			} else {
				mYesButton.Position(new Rect((Screen.width - BUTTONS_WIDTH) / 2, buttons_pos_y, BUTTONS_WIDTH, BUTTONS_HEIGHT));
			}
		
			mYesButton.Text = button1Text;
			mYesButton.enable = false;
			mNoButton.Text = button2Text;
			mNoButton.enable = false;
		
			mNoButton.Position(new Rect((Screen.width / 2) + BUTTONS_SEPARATION, buttons_pos_y, BUTTONS_WIDTH, BUTTONS_HEIGHT));
		
			mAlpha.Reset(1, mAnimationDuration, true, 0);
			mState = States.Showing;
			mIdentifier = identifier;
			mClicked = clicked;
			mThread = null;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		public static void ShowNow(int identifier, string message1, string message2, Type type, string button1Text, string button2Text, Clicked clicked) {
			Show(identifier, message1, message2, type, button1Text, button2Text, clicked);
			mAlpha.Value = 1;
			mState = States.Running;
			mYesButton.enable = true;
			mNoButton.enable = true;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Show(int identifier, string message1, string message2, Type type, string button1Text, string button2Text, Clicked clicked, ThreadStart tStart, float minTime) {
			Show(identifier, message1, message2, type, button1Text, button2Text, clicked);
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
	
		static void CalcContentY(string message1, string message2, out int message1_h, out int message2_h) {
			int h;
			int buttons_h = 0;
			int text_margin_h = 0;
			int text_button_margin_h = 0;
		
			message1_h = 0;
			message2_h = 0;
		
			float w = Screen.width - 280 * 2;
			message1_h = (int)(mStyle1.CalcSize(new GUIContent(message1)).y);
			mMessage1_H = message1_h;
		
			if(message2.Length > 0) {
				message2_h = (int)(mStyle2.CalcHeight(new GUIContent(message2), w));
				text_margin_h = TEXT_MARGIN;
			}
		
			if(mType != Type.NoButtons) {
				buttons_h = BUTTONS_HEIGHT;
				text_button_margin_h = TEXT_BUTTON_MARGIN;
			}
		
			h = message1_h + text_margin_h + message2_h + text_button_margin_h + buttons_h;
		
			mContentY = DIALOG_Y + (DIALOG_H - h) / 2;
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
				GUI.Label(new Rect(280, mContentY, Screen.width - 280 * 2, 35), mMessage1, mStyle1);
			
				//Texto2
				if(mMessage2.Length > 0)
					GUI.Label(new Rect(280, mContentY + TEXT_MARGIN + mMessage1_H, Screen.width - 280 * 2, 55), mMessage2, mStyle2);
			
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