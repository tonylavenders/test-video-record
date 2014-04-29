using UnityEngine;
using System.Collections;
using TVR;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Blur/Blur")]
public class cBlur : iBlur {	
	/// Blur iterations - larger number means more blur.
	public int iterations = 3;

	/// Blur spread for each iteration. Lower values
	/// give better looking blur, but require more iterations to
	/// get large blurs. Value is usually between 0.5 and 1.0.
	public float blurSpread = 0.6f;

	// --------------------------------------------------------
	// The blur iteration shader.
	// Basically it just takes 4 texture samples and averages them.
	// By applying it repeatedly and spreading out sample locations
	// we get a Gaussian blur approximation.
	public Shader blurShader = null;

	Material m_Material = null;
	protected Material material {
		get {
			if(m_Material == null) {
				m_Material = new Material(blurShader);
				m_Material.hideFlags = HideFlags.DontSave;
			}
			return m_Material;
		} 
	}

	public override bool isSupported() {
		if (!SystemInfo.supportsImageEffects || !blurShader || !material.shader.isSupported) {
			if(m_Material)
				DestroyImmediate(m_Material);
			return false;
		}
		return true;
	}

	// Performs one blur iteration.
	public void FourTapCone (RenderTexture source, RenderTexture dest, int iteration) {
		float off = 0.5f + iteration * blurSpread;
		Graphics.BlitMultiTap(source, dest, material,
			new Vector2(-off, -off),
			new Vector2(-off, off),
			new Vector2(off, off),
			new Vector2(off, -off)
		);
	}

	// Downsamples the texture to a quarter resolution.
	private void DownSample4x (RenderTexture source, RenderTexture dest) {
		float off = 1.0f;
		Graphics.BlitMultiTap(source, dest, material,
			new Vector2(-off, -off),
			new Vector2(-off, off),
			new Vector2(off, off),
			new Vector2(off, -off)
		);
	}

	// Called by the camera to apply the image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		if(base.Enable && mTextureBlurred == null) {
			RenderTexture rt = new RenderTexture(source.width, source.height, 16, RenderTextureFormat.RGB565);
			Graphics.Blit(source, rt);
			mTexture = rt;

			int rtW = source.width / 4;
			int rtH = source.height / 4;
			RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

			// Copy source to the 4x4 smaller texture.
			DownSample4x(source, buffer);

			// Blur the small texture
			for(int i = 0; i < iterations; i++) {
				RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
				FourTapCone(buffer, buffer2, i);
				RenderTexture.ReleaseTemporary(buffer);
				buffer = buffer2;
			}
			DestroyImmediate(m_Material);

			rt = new RenderTexture(source.width, source.height, 16, RenderTextureFormat.RGB565);
			Graphics.Blit(buffer, rt);
			mTextureBlurred = rt;

			Graphics.Blit(buffer, destination);
			RenderTexture.ReleaseTemporary(buffer);
			enableCameras(false);

			/*Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			tex.Apply();
			mTexture = tex;*/
		} else
			Graphics.Blit(source, destination);
	}
}