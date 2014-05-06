using UnityEngine;
using TVR.Helpers;
using TVR.Utils;

namespace TVR.Button
{	
	public delegate void DelegateButton(ExtendedButton sender);

	public class ExtendedButton : BasicButton {
		private const float TIMEDOUBLECLICK = 0.5f;
		public int Identifier = -1;
		public bool shadow = false;
		private float mTimePressed = 0f;
		private float mTimeDoubleClick = 0f;
		private bool mDoubleClick=false;

		private bool mReactivate;

		protected SmoothStep mMoveX;
		protected SmoothStep mMoveY;
		
		protected SmoothStep mFade;
		//protected float mAlpha;
		public float LimitAlphaUpdate;
		
		protected SmoothStep mSizeX;
		protected SmoothStep mSizeY;
		
		protected DelegateButton mDelegate;

		public float x {
			get { return rec.x; }
		}
		public float y {
			get { return rec.y; }
		}
		public float width {
			get { return rec.width; }
		}
		public float height {
			get { return rec.height; }
		}
		public bool Moving {
			get { return (!(mMoveX.Ended && !mMoveX.Enable) || !(mMoveY.Ended && !mMoveY.Enable)); }
		}
		public bool Resizing {
			get { return (!(mSizeX.Ended && !mSizeX.Enable) || !(mSizeY.Ended && !mSizeY.Enable)); }
		}
		public bool Fading {
			get { return !(mFade.Ended && !mFade.Enable); }
		}
		public bool doubleClick {
			get { return mDoubleClick; }
		}
		public float Alpha {
			get { return mFade.Value; }
			set { mFade.Reset (value,value,1f,false); }
		}
		public float timePressed {
			get { return mTimePressed; }
			set { mTimePressed = value; }
		}
			
		public ExtendedButton(Rect r, Texture down, Texture up, Texture disableDown, Texture disableUp, bool bKeepSt = true) : base (r,down,up,disableDown,disableUp,bKeepSt) {
			mMoveX = new SmoothStep(r.x, r.x, 1.0f, false);
			mMoveY = new SmoothStep(r.y, r.y, 1.0f, false);
			//mAlpha=1.0f;
			mFade = new SmoothStep(1.0f, 1.0f, 1.0f, false);
			mSizeX = new SmoothStep(r.width, r.width, 1.0f, false);
			mSizeY = new SmoothStep(r.height, r.height, 1.0f, false);
			LimitAlphaUpdate = -1;
		}
		
		public ExtendedButton(Rect r, Texture down, Texture up, Texture disableDown, Texture disableUp, DelegateButton delega, bool bKeepSt = true) : base (r,down,up,disableDown,disableUp,bKeepSt) {
			mMoveX = new SmoothStep(r.x, r.x, 1.0f, false);
			mMoveY = new SmoothStep(r.y, r.y, 1.0f, false);
			//mAlpha=1.0f;
			mFade = new SmoothStep(1.0f, 1.0f, 1.0f, false);
			mSizeX = new SmoothStep(r.width, r.width, 1.0f, false);
			mSizeY = new SmoothStep(r.height, r.height, 1.0f, false);
			mDelegate = delega;
			LimitAlphaUpdate = -1;
		}		
		
		public bool containsMouse() {
			return rec.Contains(InputHelp.mousePosition);
		}
		
		public void Position (float left, float top, float width, float height) {
			Position(new Rect(left, top, width, height));
		}
		
		public void Position (float left, float top) {
			Position(new Rect(left, top, rec.width, rec.height));
		}

		public void Position (Rect newRec, float offSetX, float offSetY) {
			newRec.x -= offSetX;
			newRec.y -= offSetY;
			Position(newRec);
		}

		public virtual void Position (Rect newRec) {
			rec = newRec;
			mMoveX.Reset(rec.x, rec.x, 1f, false);
			mMoveY.Reset(rec.y, rec.y, 1f, false);
			mSizeX.Reset(rec.width, rec.width, 1f, false);
			mSizeY.Reset(rec.height, rec.height, 1f, false);
		}
		
		public override void Reset() {
			mTimePressed = 0;
			mTimeDoubleClick = 0;
			mFade.End();
			if(!mMoveX.Ended) {
				mMoveX.End();
				rec.x = mMoveX.Value;
			}
			if(!mMoveY.Ended) {
				mMoveY.End();
				rec.y = mMoveY.Value;
			}
			if(!mSizeX.Ended) {
				mSizeX.End();
				float diff = rec.width - mSizeY.Value;
				rec.x += diff / 2f;
				rec.width = mSizeX.Value;
			}
			if(!mSizeY.Ended) {
				mSizeY.End();
				float diff = rec.height - mSizeY.Value;
				rec.y += diff / 2f;
				rec.height = mSizeY.Value;
			}
			base.Reset();
		}

		public virtual void Update (float left, float top, float width, float height) {
			Update(new Rect(left, top, width, height));
		}
		
		public virtual void Update (float left, float top) {
			Update(new Rect(left, top, rec.width, rec.height));
		}

		public virtual void Update (Rect newRec, float offSetX, float offSetY) {
			newRec.x -= offSetX;
			newRec.y -= offSetY;
			Update(newRec);
		}

		public virtual void Update (Rect newRec) {
			Position(newRec);
			Update();
		}
		
		public bool isVisible() {
			float top = rec.y;
			float botton = top + rec.height;
			float left = rec.x;
			float right = left + rec.width;
			
			if(shadow) {
				botton += 10;
				right += 10;
			}
			
			if(left < Screen.width && right > 0 && top < Screen.height && botton > 0)
				return true;
			
			return false;
		}
		
		public Vector2 offset() {
			Vector2 res = Vector2.zero;
			Rect rect = new Rect(rec);
			if(shadow) {
				rect.width += 10;
				rect.height += 10;
			}
			if(rect.x < 0 /*&& rect.xMax>0*/) {
				res.x = rect.x;
			} else if(/*rect.x<Screen.width &&*/ rect.xMax > Screen.width) {
					res.x = rect.xMax - Screen.width;
				}

			if(rect.y < 0 /*&& rect.yMax>0*/) {
				res.y = rect.y;
			} else if(/*rect.y<Screen.height &&*/ rect.yMax > Screen.height) {
					res.y = rect.yMax - Screen.height;
				}

			return res;
		}
		
		public override void Update() {
			mDoubleClick = false;
			if(mMoveX.Enable == true) {
				mMoveX.Update();
				rec.x = mMoveX.Value;
			}
			if(mMoveY.Enable == true) {
				mMoveY.Update();
				rec.y = mMoveY.Value;
			}
			if(mFade.Enable) {
				mFade.Update();
				//mAlpha=mFade.Value;
			}
			if(mSizeX.Enable == true) {
				mSizeX.Update();
				float diff = rec.width - mSizeY.Value;
				rec.x += diff / 2f;
				rec.width = mSizeX.Value;
			}
			if(mSizeY.Enable == true) {
				mSizeY.Update();
				float diff = rec.height - mSizeY.Value;
				rec.y += diff / 2f;
				rec.height = mSizeY.Value;
			}
 			
			if(mReactivate && mMoveX.Ended && mMoveY.Ended && mFade.Ended && mSizeX.Ended && mSizeY.Ended) {
				enable = true;
				mReactivate = false;
			}
			
			if(isVisible() && Alpha > LimitAlphaUpdate)
				base.Update();
			else {
				mJustPressed = false;
				mJustReleased = false;
			}
			
			if(pressed)
				mTimePressed += Time.deltaTime;
			else if(justReleased)
					mTimePressed = 0;
			
			if(justReleased) {
				if(mTimeDoubleClick >= 0) {
					mDoubleClick = true;
				}
				mTimeDoubleClick = TIMEDOUBLECLICK;
			}
			if(mTimeDoubleClick > 0) {
				mTimeDoubleClick -= Time.deltaTime;
			}
		}
		
		public override void OnButtonReleased() {
			mTimePressed = 0;
			if(mDelegate != null)
				mDelegate(this);
		}
		
		public override void OnButtonReleasedNotContained() {
			mTimePressed = 0;
		}
		
		public void OnGUI(Color color, bool useGUIColor = true) {
			if(Event.current.type != EventType.Repaint)
				return;
			if(mFade.Value != 0f && isVisible()) {
				renderShadow(color.a);
				color.a *= mFade.Value;
				Color preColor = GUI.color;
				if(useGUIColor)
					GUI.color = color * preColor;
				else
					GUI.color = color;
				base.OnGUI();
				GUI.color = preColor;
			}
		}
		
		public new void OnGUI() {
			OnGUI(Color.white, true);
		}

		private void renderShadow(float alpha) {
			if(shadow) {
				Color preColor = GUI.color;
				GUI.color = preColor * new Color(0, 0, 0, 0.12f * mFade.Value * alpha);
				GUI.DrawTexture(new Rect(rec.x + 10, rec.y + 10, rec.width, rec.height), TexDown, scaleMode, true);
				GUI.color = preColor;
			}
		}
		
		public void Animate(Vector2 Destination, float Duration,bool Disable, bool reactivateOnEnd, float Delay = 0f) {
			mMoveX.Reset(rec.x, Destination.x, Duration, true, Delay);
			mMoveY.Reset(rec.y, Destination.y, Duration, true, Delay);
			mReactivate = reactivateOnEnd;
			if(Disable)
				enable = false;
		}
		
		public void Fade(float Alpha0, float Alpha1,float Duration,bool Disable, bool reactivateOnEnd, float Delay = 0f) {
			mFade.Value = Alpha0;
			mFade.Reset(Alpha0, Alpha1, Duration, true, Delay);
			mReactivate = reactivateOnEnd;
			if(Disable)
				enable = false;
		}
		
		public void Fade(float Alpha1,float Duration,bool Disable, bool reactivateOnEnd, float Delay = 0f) {
			Fade(mFade.Value, Alpha1, Duration, Disable, reactivateOnEnd, Delay);
		}
		
		public void Resize(float Height, float Width,float Duration,bool Disable, bool reactivateOnEnd, float Delay = 0f) {
			mSizeX.Reset(rec.height, Height, Duration, true, Delay);
			mSizeY.Reset(rec.width, Width, Duration, true, Delay);
			mReactivate = reactivateOnEnd;
			if(Disable)
				enable = false;
		}
	}
}