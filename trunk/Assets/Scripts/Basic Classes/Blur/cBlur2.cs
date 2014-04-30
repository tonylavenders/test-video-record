using UnityEngine;
using System.Collections;
using TVR;

public class cBlur2 : iBlur {
	public bool softwareBlur = true;
	public int mipLevel = 1;
	public int iterations = 1;
	public int brushSizeHorizontal = 1;
	public int brushSizeVertical = 1;
	public override bool isSupported() {
		return true;
	}

	protected virtual void OnApplicationPause(bool pauseStatus) {
		if(Enable && !pauseStatus) {
			DestroyImmediate(mTexture);
			mTexture = null;

			DestroyImmediate(mTextureBlurred);
			mTextureBlurred = null;

			enableCameras(true);
		}
	}

	void OnPostRender() {
		if(base.Enable) {
			enableCameras(false);
			Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
			tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			tex.Apply();

			mTexture = tex;
			mTextureBlurred = Blur(tex);
		}
	}

	private Texture2D Blur(Texture2D image) {
		if(mipLevel >= image.mipmapCount)
			mipLevel = image.mipmapCount - 1;

		int mipWidth = Mathf.Max(1, image.width >> mipLevel);
		int mipHeight = Mathf.Max(1, image.height >> mipLevel);
		Texture2D blurred = new Texture2D(mipWidth, mipHeight, TextureFormat.RGB24, false);
		Color[] pixels = image.GetPixels(mipLevel);

		if(softwareBlur) {
			for(int iter = 0; iter < iterations; ++iter) {
				for(int x = 0; x < mipWidth; ++x) {
					for(int y = 0; y < mipHeight; ++y) {
						Color color = new Color(0, 0, 0, 0);
						int count = 0;
						for(int i = x - 1 * brushSizeHorizontal; i <= x + 1 * brushSizeHorizontal; ++i) {
							for(int j = y - 1 * brushSizeVertical; j <= y + 1 * brushSizeVertical; ++j) {
								if(i > 0 && i < mipWidth && j > 0 && j < mipHeight) {
									color += pixels[i + j * mipWidth];
									count++;
								}
							}
						}
						color /= count;
						pixels[x + y * mipWidth] = color; 
					}
				}
			}
		}

		blurred.SetPixels(pixels);
		blurred.Apply();
		pixels = null;
		return blurred;
	}
}