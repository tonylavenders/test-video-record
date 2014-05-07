using UnityEngine;

namespace TVR.Utils {	
	public class SmoothStep {
		public enum State {
			inDelay,
			inFade,
			justEnd,
			End,
			Disable,
		}
		private float mValue;
		private float mValue0;
		private float mValue1;
		private float mDelta;
		private float mTime;
		private float mDuration;
		private float mDelay;
		private bool mEnabled;
		
		public float Value {
			get { return mValue; }
			set {
				mDelta = mValue - value;
				mValue = value;
				mValue0=value;
				mValue1=value;
				mEnabled=false;
			}
		}
		
		public bool Enable {
			get { return mEnabled; }
			set {
				mEnabled = false;
				if(mTime < mDuration) {
					mEnabled = value;
				}
			}
		}
		
		public float initValue {
			get { return mValue0; }
		}
		
		public float finalValue {
			get { return mValue1; }
		}

		public float duration {
			get { return mDuration; }
		}
		
		public float delay {
			get { return mDelay; }
		}

		public bool Ended {
			get { return (Mathf.Approximately(mValue, mValue1) && mDelay >= 0); }
		}

		public float Delta {
			get { return mDelta; }
		}
		
		public SmoothStep(float initValue, float finalValue, float duration, bool enable = true, float delay = 0f) {
			Reset(initValue, finalValue, duration, enable, delay);
		}
		
		public void End() {
			mEnabled = false;
			mDelta = mValue - mValue1;
			mValue = mValue1;
		}
		
		public void Reset(bool enable, float time = 0f) {
			mDelta = 0;
			mTime = time;
			if(mDuration == 0)
				mValue = mValue0;
			else {
				if(mDelay > 0)
					mValue = Mathf.SmoothStep(mValue0, mValue1, (mTime - mDelay) / mDuration);
				else
					mValue = Mathf.SmoothStep(mValue0, mValue1, mTime / mDuration);
			}
			mEnabled = enable;
		}
		
		public void Reset(float finalValue, float duration, bool enable = true, float delay = 0f, float time = 0f) {
			/*mValue0 = mValue;
			mValue1 = finalValue;
			mDuration = duration;
			mDelay = delay;*/
			Reset(mValue, finalValue, duration, enable, delay, time);
		}

		public void Reset(float initValue, float finalValue, float duration, bool enable = true, float delay = 0f, float time = 0f) {
			mValue0 = initValue;
			mValue1 = finalValue;
			mDuration = duration;
			mDelay = delay;
			Reset(enable, time);
		}
		
		public State Update() {
			if(mEnabled) {
				State ret;
				float old = mValue;
				if(mDelay < 0) {
					mDelay++;
					ret = State.inDelay;
				} else {
					mTime += Time.deltaTime;
					if(Ended) {
						mValue = mValue1;
						mEnabled = false;
						ret = State.justEnd;
					} else if(mTime > mDelay) {
						mValue = Mathf.SmoothStep(mValue0, mValue1, (mTime - mDelay) / mDuration);
						ret = State.inFade;
					} else
						ret = State.inDelay;
					mDelta = mValue - old;
				}
				return ret;
			} else if(Ended) {
				mDelta = 0;
				return State.End;
			} else {
				mDelta = 0;
				return State.Disable;
			}
		}
	}
}