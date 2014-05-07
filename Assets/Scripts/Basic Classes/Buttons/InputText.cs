using UnityEngine;
using TVR.Helpers;

namespace TVR.Button
{	
	public class InputText : TextButton {
		private bool mSelected;
		private bool mDrawIndicator;
		private float mTime;
		private Texture2D mIndicatorTexture;
		private Rect mIndicatorRect;
		private string mEmptyText;
		private int mMaxLength;
		//#if UNITY_IPHONE || UNITY_ANDROID
		private TouchScreenKeyboard mKeyboard;
		private string mOldInput;
		//#endif
		private int mIndicatorPos;
		private char[] mSpecialCharacters = new char[0]; //{ ' ', '-', '_', '.' };
		public DelegateButton selectedCallBack;
		public DelegateButton unSelectedCallBack;
				
		public int maxLength {
			get { return mMaxLength; }
			set { mMaxLength = value; }
		}
		public bool selected {
			get { return mSelected; }
			set {
				/*if(mSelected != value) {
					if(value && selectedCallBack != null)
						selectedCallBack(this);
					else if(!value && unSelectedCallBack != null)
							unSelectedCallBack(this);
					mSelected = value;
					disableOnMouseMove = !value;
					mDrawIndicator = value;
				#if UNITY_IPHONE || UNITY_ANDROID
					if(mSelected) {
						mIndicatorPos = Text.Length;
						if(mKeyboard == null) {
							TouchScreenKeyboard.hideInput = true;
							mKeyboard = TouchScreenKeyboard.Open(Text, TouchScreenKeyboardType.Default, false);
							mOldInput = Text;
						} else {
							mKeyboard.active = true;
							mKeyboard.text = Text;
							mOldInput = Text;
						}

					} else
						mKeyboard.active = false;
				} else if(mSelected == true && mKeyboard != null && mKeyboard.active == false) {
					mKeyboard.active = true;
					mKeyboard.text = Text;
					mOldInput = Text;
				#else
					if(mSelected)
						mIndicatorPos = Text.Length;
				#endif
				}*/
				/*if(mSelected != value) {
					if(value && selectedCallBack != null)
						selectedCallBack(this);
					else if(!value && unSelectedCallBack != null)
							unSelectedCallBack(this);
					mSelected = value;
					disableOnMouseMove = !value;
					mDrawIndicator = value;
				#if UNITY_IPHONE || UNITY_ANDROID
					if(mSelected) {
						mIndicatorPos = Text.Length;
						if(Application.platform == RuntimePlatform.IPhonePlayer) {
							if(mKeyboard == null) {
								TouchScreenKeyboard.hideInput = true;
								mKeyboard = TouchScreenKeyboard.Open(Text, TouchScreenKeyboardType.Default, false);
								mOldInput = Text;
							} else {
								mKeyboard.active = true;
								mKeyboard.text = Text;
								mOldInput = Text;
							}
						} else if(mKeyboard != null)
							mKeyboard.active = false;

					} else if(Application.platform == RuntimePlatform.IPhonePlayer)
						mKeyboard.active = false;
				} else if(mSelected == true && Application.platform == RuntimePlatform.IPhonePlayer && mKeyboard != null && mKeyboard.active == false) {
					mKeyboard.active = true;
					mKeyboard.text = Text;
					mOldInput = Text;
				#else
					if(mSelected)
						mIndicatorPos = Text.Length;
				#endif
				}*/
				if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) {
					if(mSelected != value) {
						if(value && selectedCallBack != null)
							selectedCallBack(this);
						else if(!value && unSelectedCallBack != null)
								unSelectedCallBack(this);
						mSelected = value;
						disableOnMouseMove = !value;
						mDrawIndicator = value;
						if(mSelected) {
							mIndicatorPos = Text.Length;
							if(mKeyboard == null) {
								TouchScreenKeyboard.hideInput = true;
								mKeyboard = TouchScreenKeyboard.Open(Text, TouchScreenKeyboardType.Default, false);
								mOldInput = Text;
							} else {
								mKeyboard.active = true;
								mKeyboard.text = Text;
								mOldInput = Text;
							}

						} else
								mKeyboard.active = false;
					} else if(mSelected == true && mKeyboard != null && mKeyboard.active == false) {
							mKeyboard.active = true;
							mKeyboard.text = Text;
							mOldInput = Text;
						}
				} else {
					if(mSelected != value) {
						if(value && selectedCallBack != null)
							selectedCallBack(this);
						else if(!value && unSelectedCallBack != null)
								unSelectedCallBack(this);
						mSelected = value;
						disableOnMouseMove = !value;
						mDrawIndicator = value;
						if(mSelected)
							mIndicatorPos = Text.Length;
					}
				}
			}
		}
		public char[] specialCharacters {
			get { return mSpecialCharacters; }
			set { mSpecialCharacters = value; }
		}

		public InputText(Rect r, Texture down, Texture up, Texture disableDown, Texture disableUp, Font font, Texture Indicator, string emptyText, bool bKeepSt = false) : base (r,down,up,disableDown,disableUp,font,bKeepSt) {
			mSelected = false;
			mDrawIndicator = false;
			mIndicatorTexture = (Texture2D)Indicator; 
			mIndicatorRect = new Rect(0, 0, 2, 0);
			UpdateIndicatorSize();
			UpdateIndicatorPos();
			mEmptyText = emptyText;
			mMaxLength = -1;
			mStyle.wordWrap = false;
			mIndicatorPos = 0;
			//#if UNITY_IPHONE || UNITY_ANDROID
			mOldInput = "";
			//#endif 
		}
		
		public InputText(Rect r, Texture down, Texture up, Texture disableDown, Texture disableUp, DelegateButton delega, Font font, Texture Indicator, string emptyText, bool bKeepSt = false) : base (r,down,up,disableDown,disableUp, delega, font, bKeepSt) {
			mSelected = false;
			mDrawIndicator = false;
			mIndicatorTexture = (Texture2D)Indicator;
			mIndicatorRect = new Rect(0, 0, 2, 0);
			UpdateIndicatorSize();
			UpdateIndicatorPos();
			mEmptyText = emptyText;
			mMaxLength = -1;
			mStyle.wordWrap = false;
			mIndicatorPos = 0;
			//#if UNITY_IPHONE || UNITY_ANDROID
			mOldInput = "";
			//#endif 
		}
		
		public override void Update() {
			//base.Text=mTempText;
			base.Update();
			if(justReleased) {
				selected = true;
				mTime = 0;
			} else if(InputHelp.GetMouseButtonUp(0) && mSelected) {
				selected = false;
			}
			
			if(mSelected) {
				if(pressed && Text.Length > 0) {
					mTime = 0;
					mDrawIndicator = true;
					float mousePos = InputHelp.mousePosition.x;
					mIndicatorPos = 0;
					float posTemp = 0;
					for(int i = 1; i <= Text.Length; ++i) {
						posTemp = calculateIndicatorPos(i).x;
						if(posTemp < mousePos)
							mIndicatorPos = i;
						else
							break;
					}
					if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) {
						//#if UNITY_IPHONE || UNITY_ANDROID
						mKeyboard.text = Text.Substring(0, mIndicatorPos);
						mOldInput = mKeyboard.text;
						//#endif
					}
				}
				
				string input;
				if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) {
				//#if UNITY_IPHONE || UNITY_ANDROID
					input = "";
					int count = 0;
					for(int i = 0; i < Mathf.Min(mOldInput.Length, mKeyboard.text.Length); ++i) {
						if(mOldInput[i] == mKeyboard.text[i])
							count++;
						else
							break;
					}
					for(int i = 0; i < mOldInput.Length - count; ++i) {
						input += '\b';
					}
					if(count < mKeyboard.text.Length)
						input += mKeyboard.text.Substring(count);
					mOldInput = mKeyboard.text;
				} else {
				//#else
					input = Input.inputString;
				//#endif
				}
				
				string temp = Text.Substring(0, mIndicatorPos);
				string temp2 = Text.Substring(mIndicatorPos);
				foreach(char c in input) {
					if((c >= 'a' && c <= 'z') ||
					   (c >= 'A' && c <= 'Z') ||
					   (c >= '0' && c <= '9') || System.Array.IndexOf(mSpecialCharacters, c) > -1) {
						if(Text.Length < mMaxLength || mMaxLength < 0) {
							temp += c;
							Text = temp + temp2;
							mIndicatorPos++;
						}
					} else if(c == '\b' && temp.Length > 0) {
						temp = temp.Substring(0, temp.Length - 1);
						Text = temp + temp2;
						mIndicatorPos--;
					}
				}
				
				mTime += Time.deltaTime;
				if(mTime > 1) {
					mDrawIndicator = true;
					mTime = mTime % 1;
				} else if(mTime > 0.5)
					mDrawIndicator = false;
				if(mDrawIndicator) {
					UpdateIndicatorSize();
					UpdateIndicatorPos();
				}
			}
		}
		
		public override void Update (float left, float top, float width, float height) {
			Update(new Rect(left, top, width, height));
		}
		
		public override void Update (float left, float top) {
			Update(new Rect(left, top, rec.width, rec.height));
		}

		public override void Update (Rect newRec, float offSetX, float offSetY) {
			newRec.x -= offSetX;
			newRec.y -= offSetY;
			Update(newRec);
		}

		public override void Update (Rect newRec) {
			Position(newRec);
			Update();
		}
		
		private void UpdateIndicatorPos() {
			Vector2 pos = calculateIndicatorPos(mIndicatorPos);
			mIndicatorRect.x = pos.x;
			mIndicatorRect.y = pos.y;
		}
		
		private Vector2 calculateIndicatorPos(int indicatorPos) {
			Vector2 pos = Vector2.zero;
			string tex = mText;
			Vector2 size = mStyle.CalcSize(new GUIContent(tex));
			float textOffsetSpaces = 0;
			
			if(tex.Trim().Length != tex.Length) {
				float spaceWidth = mStyle.CalcSize(new GUIContent(". .")).x - mStyle.CalcSize(new GUIContent("..")).x;
				textOffsetSpaces = spaceWidth * ((tex.Length - tex.TrimStart(' ').Length) + (tex.Length - tex.TrimEnd(' ').Length));
			}
			switch(TextPosition) {
			case TextAnchor.LowerCenter:
			case TextAnchor.LowerLeft:
			case TextAnchor.LowerRight:
				pos.y = rec.yMax - mIndicatorRect.height;
				break;
			case TextAnchor.MiddleCenter:
			case TextAnchor.MiddleLeft:
			case TextAnchor.MiddleRight:
				pos.y = rec.y + ((rec.height - mIndicatorRect.height) / 2f);
				break;
			case TextAnchor.UpperCenter:
			case TextAnchor.UpperLeft:
			case TextAnchor.UpperRight:
				pos.y = rec.y;
				break;
			}
			switch(TextPosition) {
			case TextAnchor.LowerCenter:
			case TextAnchor.MiddleCenter:
			case TextAnchor.UpperCenter:
				textOffsetSpaces /= 2f;
				pos.x = rec.x + ((rec.width + size.x) / 2f);
				break;
			case TextAnchor.LowerLeft:
			case TextAnchor.MiddleLeft:
			case TextAnchor.UpperLeft:
				pos.x = rec.x + size.x;
				break;
			case TextAnchor.LowerRight:
			case TextAnchor.MiddleRight:
			case TextAnchor.UpperRight:
				textOffsetSpaces = 0;
				pos.x = rec.xMax;
				break;
			}
			pos.x += TextOffset.x + textOffsetSpaces;
			pos.y += TextOffset.y;

			tex = tex.Substring(indicatorPos, tex.Length - indicatorPos);
			size = mStyle.CalcSize(new GUIContent(tex));
			pos.x -= size.x;
			return pos;
		}
		
		private void UpdateIndicatorSize() {
			mIndicatorRect.height = mStyle.CalcSize(new GUIContent("Hg")).y;
		}
		
		public void OnGUIAllEvents() {
			if(mSelected) {
				if(Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android) {
				//#if !UNITY_IPHONE && UNITY_ANDROID
					Event e = Event.current;
					if(e.type == EventType.KeyDown) {
						if(e.isKey) {
							switch(e.keyCode) {
							case KeyCode.UpArrow:
								break;
							case KeyCode.DownArrow:
								break;
							case KeyCode.Delete:
								if(mIndicatorPos < Text.Length)
									Text = Text.Substring(0, mIndicatorPos) + Text.Substring(mIndicatorPos + 1);
								break;
							case KeyCode.LeftArrow:
								if(mIndicatorPos > 0)
									mIndicatorPos--;
								break;
							case KeyCode.RightArrow:
								if(mIndicatorPos < Text.Length)
									mIndicatorPos++;
								break;
							case KeyCode.Backspace:
								break;
							default:
								break;
							}
						}
					}
				//#endif
				}
			}
		}
		
		public new void OnGUI() {
			OnGUI(Color.white, Color.white, true);
		}
		
		public new void OnGUI(Color colorTexture, Color colorText, bool useGUIColor = true) {
			if(Event.current.type != EventType.Repaint)
				return;
			if(Text == "" && !mSelected && enable) {
				Text = mEmptyText;
				base.OnGUI(colorTexture, colorText * 0.7f, useGUIColor);
				Text = "";
			} else
				base.OnGUI(colorTexture, colorText, useGUIColor);
			if(mDrawIndicator) {
				Color guiCol = GUI.color;
				GUI.color = TextColor;
				GUI.DrawTexture(mIndicatorRect, mIndicatorTexture);
				GUI.color = guiCol;
			}
		}
		
		public new void OnGUI(Color colorTexture, bool useGUIColor = true) {
			OnGUI(colorTexture, colorTexture, useGUIColor);
		}
	}
}