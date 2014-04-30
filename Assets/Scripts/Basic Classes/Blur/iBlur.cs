using UnityEngine;
using TVR.Utils;
using TVR;

public abstract class iBlur : MonoBehaviour {
	public Texture mTexture;
	public Texture mTextureBlurred;
	public Camera[] Cameras;
	public Color Tint = Color.white;
	protected SmoothStep mAlpha;
	bool mEnable;
	public virtual bool Enable {
		get { return mEnable; }
		set {
			if(mEnable != value) {
				mEnable = value;
				if(mEnable)
					mAlpha.Reset(1, Globals.ANIMATIONDURATION, true, -2);
				else
					mAlpha.Reset(0, Globals.ANIMATIONDURATION, true);
			}
		}
	}

	public abstract bool isSupported();

	protected virtual void Start() {
		mEnable = false;
		mAlpha = new SmoothStep(0, 1, Globals.ANIMATIONDURATION, false, 0);
		Tint.a = 0;
	}

	protected virtual void Update() {
		SmoothStep.State SSState = mAlpha.Update();
		if(SSState == SmoothStep.State.inFade || SSState == SmoothStep.State.justEnd) {
			if(SSState == SmoothStep.State.justEnd && mAlpha.Value == 0f) {
				enableCameras(true);
				if(mTextureBlurred != null) {
					if(mTextureBlurred is RenderTexture)
						((RenderTexture)mTextureBlurred).Release();
					DestroyImmediate(mTextureBlurred);
					mTextureBlurred = null;
				}
				if(mTexture != null) {
					if(mTextureBlurred is RenderTexture)
						((RenderTexture)mTextureBlurred).Release();
					DestroyImmediate(mTexture);
					mTexture = null;
				}
			}
			Tint.a = mAlpha.Value;
		}
	}

	protected virtual void enableCameras(bool value) {
		if(Cameras != null) {
			foreach(Camera c in Cameras) {
				c.enabled = value;
			}
		}
	}

	protected virtual void OnGUI() {
		if(Event.current.type == EventType.Repaint) {
			if(mTexture != null)
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mTexture);

			if(mTextureBlurred != null) {
				Color current = GUI.color;
				GUI.color = Tint;
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mTextureBlurred);
				GUI.color = current;
			}
		}
	}
}