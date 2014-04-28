using UnityEngine;

public abstract class iBlur : MonoBehaviour {
	protected Texture mTexture;
	public Camera[] Cameras;
	public Color Tint = Color.white;

	public abstract bool isSupported();

	protected void OnDisable() {
		enableCameras(true);
		DestroyImmediate(mTexture);
		mTexture = null;
	}

	protected void enableCameras(bool value) {
		if(Cameras != null) {
			foreach(Camera c in Cameras)
				c.enabled = value;
		}
	}

	public void render() {
		if(mTexture != null) {
			Color current = GUI.color;
			GUI.color = Tint;
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mTexture);
			GUI.color = current;
		}
	}
}