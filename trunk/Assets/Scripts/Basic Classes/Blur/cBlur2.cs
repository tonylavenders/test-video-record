using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Blur/Blur")]
public class cBlur2 : iBlur {	
	public override bool isSupported() {
		return true;
	}

	protected void OnEnable() {
		StartCoroutine(Capture());
	}

	IEnumerator Capture() {
		yield return new WaitForEndOfFrame();
		enableCameras(false);
		Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		tex.Apply();
		mTexture = tex;
	}
}