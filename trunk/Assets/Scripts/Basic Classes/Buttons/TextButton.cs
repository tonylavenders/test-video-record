using UnityEngine;

namespace TVR.Button
{	
	public class TextButton : ExtendedButton {
		protected string mText;
		protected GUIStyle mStyle;
		protected Vector2 mTextOffset;
		protected float mTextOffsetSpaces;
		private Color mTextColor;
		public bool onlyTextShadow;
		
		public string Text {
			get { return mText; }
			set {
				if(mText == value)
					return;
				mText = value;
				calculateOffsetSpaces();
			}
		}
		public int TextSize {
			get { return mStyle.fontSize; }
			set {
				if(value > 0 && value != mStyle.fontSize) {
					mStyle.fontSize = value;
					calculateOffsetSpaces();
				}
			}
		}
		public TextAnchor TextPosition {
			get { return mStyle.alignment; }
			set {
				if(mStyle.alignment == value)
					return;
				mStyle.alignment = value;
				calculateOffsetSpaces();
			}
		}
		public FontStyle TextStyle {
			get { return mStyle.fontStyle; }
			set {
				if(mStyle.fontStyle == value)
					return;
				mStyle.fontStyle = value;
				calculateOffsetSpaces();
			}
		}
		public Color TextColor {
			get { return mTextColor; }
			set { mTextColor = value; }
		}
		public Vector2 TextOffset {
			get { return mTextOffset; }
			set { mTextOffset = value; }
		}
		public TextClipping textClipping {
			get { return mStyle.clipping; }
			set { mStyle.clipping = value; }
		}
		
		public TextButton(Rect r, Texture down, Texture up, Texture disableDown, Texture disableUp, Font font, bool bKeepSt = true) : base (r,down,up,disableDown,disableUp,bKeepSt) {
			mStyle = new GUIStyle();
			mStyle.font = font;
			mStyle.clipping = TextClipping.Clip;
			mStyle.wordWrap = true;
			mStyle.fontSize = 10;
			mStyle.alignment = TextAnchor.MiddleCenter;
			mStyle.fontStyle = FontStyle.Normal;
			mStyle.normal.textColor = Color.black;
			mTextColor = Color.black;
			mTextOffset = Vector2.zero;
			mText = "";
			mTextOffsetSpaces = 0;
		}
		
		public TextButton(Rect r, Texture down, Texture up, Texture disableDown, Texture disableUp, DelegateButton delega, Font font, bool bKeepSt = true) : base (r,down,up,disableDown,disableUp, delega, bKeepSt) {
			mStyle = new GUIStyle();
			mStyle.font = font;
			mStyle.clipping = TextClipping.Clip;
			mStyle.wordWrap = true;
			mStyle.fontSize = 10;
			mStyle.alignment = TextAnchor.MiddleCenter;
			mStyle.fontStyle = FontStyle.Normal;
			mStyle.normal.textColor = Color.black;
			mTextColor = Color.black;
			mTextOffset = Vector2.zero;
			mText = "";
			mTextOffsetSpaces = 0;
		}
		
		private void calculateOffsetSpaces() {
			if(mText.Trim().Length != mText.Length) {
				float spaceWidth = mStyle.CalcSize(new GUIContent(". .")).x - mStyle.CalcSize(new GUIContent("..")).x;
				if(mStyle.alignment == TextAnchor.LowerCenter || mStyle.alignment == TextAnchor.MiddleCenter || mStyle.alignment == TextAnchor.UpperCenter) {
					mTextOffsetSpaces = spaceWidth * ((mText.Length - mText.TrimStart(' ').Length) + (mText.TrimEnd(' ').Length - mText.Length));
					mTextOffsetSpaces /= 2f;
				} else if(mStyle.alignment == TextAnchor.LowerRight || mStyle.alignment == TextAnchor.MiddleRight || mStyle.alignment == TextAnchor.UpperRight) {
						mTextOffsetSpaces = spaceWidth * (mText.TrimEnd(' ').Length - mText.Length);
				} else if(mStyle.alignment == TextAnchor.LowerLeft || mStyle.alignment == TextAnchor.MiddleLeft || mStyle.alignment == TextAnchor.UpperLeft) {
					mTextOffsetSpaces = spaceWidth * (mText.Length - mText.TrimStart(' ').Length);
				}
			} else
				mTextOffsetSpaces = 0;
		}
		
		public new void OnGUI() {
			OnGUI(Color.white, Color.white, true);
		}
		
		public void OnGUI(Color colorTexture, Color colorText, bool useGUIColor = true) {
			if(Event.current.type != EventType.Repaint)
				return;
			renderShadow(colorText.a);
			base.OnGUI(colorTexture, useGUIColor);
			writeText(colorText);
		}
		
		public new void OnGUI(Color colorTexture, bool useGUIColor = true) {
			OnGUI(colorTexture, colorTexture, useGUIColor);
		}
		
		public new bool isVisible() {
			return isVisibleText() || base.isVisible();
		}
		
		private bool isVisibleText() {
			Vector2 Size;
			if(mStyle.clipping == TextClipping.Clip) {
				Size.x = rec.width;
				Size.y = rec.height;
			} else {
				Size = mStyle.CalcSize(new GUIContent(mText));
			}
				
			float top = rec.y + mTextOffset.y;
			float botton = top + Size.y;
			float left = rec.x + mTextOffset.x;
			float right = left + Size.x;
			
			if(shadow) {
				botton += 10;
				right += 10;
			}
			
			/*if(((top >= 0 && top <= Screen.height) || (botton >= 0 && botton <= Screen.height)) && 
				((left >= 0 && left <= Screen.width) || (right >= 0 && right <= Screen.width))) {
				return true;
			}*/
			if(left < Screen.width && right > 0 && top < Screen.height && botton > 0)
				return true;
			
			return false;
		}

		protected void writeText(Color color) {
			if(mFade.Value != 0f && isVisibleText()) {
				if(mFade.Value < 1.0f) {
					Color color1 = color * mTextColor;
					color1.a *= mFade.Value;
					mStyle.normal.textColor = color1;
				} else {
					mStyle.normal.textColor = color * mTextColor;
				}
				Rect recText = new Rect(base.rec.x + mTextOffset.x + mTextOffsetSpaces, base.rec.y + mTextOffset.y, base.rec.width, base.rec.height);
				GUI.Label(recText, mText, mStyle);
			}
		}
		
		private void renderShadow(float alpha) {
			if((shadow || onlyTextShadow) && mFade.Value != 0f && isVisibleText()) {
				mStyle.normal.textColor = new Color(0, 0, 0, 0.12f * mFade.Value * alpha);
				Rect recText = new Rect(base.rec.x + mTextOffset.x + 10 + mTextOffsetSpaces, base.rec.y + mTextOffset.y + 10, base.rec.width, base.rec.height);
				GUI.Label(recText, mText, mStyle);
				mStyle.normal.textColor = mTextColor;
			}
		}
	}
}