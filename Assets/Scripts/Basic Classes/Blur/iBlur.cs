using UnityEngine;
using TVR.Utils;
using TVR;

public abstract class iBlur : MonoBehaviour {
	//TODO: Prueba con 2 texturas, una procesada la otra no (fade).
	public Texture mTexture;
	public Camera[] Cameras;
	public Color Tint = Color.white;
	Color mTint = Color.white;
	protected SmoothStep mAlpha;
	bool mEnable;
	public virtual bool Enable {
		get { return mEnable; }
		set {
			if(mEnable != value) {
				mEnable = value;
				if(mEnable)
					mAlpha.Reset(Tint.r, Globals.ANIMATIONDURATION, true, -2);
				else
					mAlpha.Reset(1, Globals.ANIMATIONDURATION, true);
			}
		}
	}

	void Start() {
		mEnable = false;
		mAlpha = new SmoothStep(1, Tint.r, Globals.ANIMATIONDURATION, false, 0);
	}

	void Update() {
		SmoothStep.State SSState = mAlpha.Update();
		if(SSState == SmoothStep.State.inFade || SSState == SmoothStep.State.justEnd) {
			if(SSState == SmoothStep.State.justEnd && mAlpha.Value == 1f) {
				enableCameras(true);
				if(mTexture is RenderTexture)
					((RenderTexture)mTexture).Release();
				DestroyImmediate(mTexture);
				mTexture = null;
			}
			mTint.r = mAlpha.Value;
			mTint.g = mAlpha.Value;
			mTint.b = mAlpha.Value;
		}
	}

	public abstract bool isSupported();

	protected void enableCameras(bool value) {
		if(Cameras != null) {
			foreach(Camera c in Cameras)
				c.enabled = value;
		}
	}

	public void OnGUI() {
		if(Event.current.type == EventType.Repaint & mTexture != null) {
			Color current = GUI.color;
			GUI.color = mTint;
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mTexture);
			GUI.color = current;
		}
	}
}