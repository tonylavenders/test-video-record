using UnityEngine;
using System.Collections;

public class cBlur : PostEffectsBase {
	[Range(0, 2)]
	public int downsample = 1;

	public enum BlurType {
		StandardGauss = 0,
		SgxGauss = 1,
	}

	[Range(0.0f, 10.0f)]
	public float blurSize  = 2.5f;

	[Range(1, 4)]
	public int blurIterations = 2;
	public BlurType blurType = BlurType.StandardGauss;
	public Shader blurShader;
	public Color Tint = Color.white;
	private Material mBlurMaterial = null;
	RenderTexture mRT;
	private Camera[] mCameras;

	public override bool CheckResources() {
		CheckSupport(false);

		if(!isSupported)
			ReportAutoDisable();
		return isSupported;				
	}

	public void clear() {
		if(mBlurMaterial) {
			RenderTexture.active = null;
			DestroyImmediate(mBlurMaterial);
			mRT.Release();
			mRT = null;
			foreach(Camera c in mCameras) {
				c.targetTexture = null;
				c.enabled = true;
			}
			mCameras = null;
		}
	}

	public void proccess(Camera[] Cameras) {	
		if(Cameras == null || CheckResources() == false) {
			return;
		}
		mBlurMaterial = CheckShaderAndCreateMaterial(blurShader, mBlurMaterial);
		mRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.RGB565);
		mRT.Create();
		mCameras = Cameras;
		foreach(Camera c in mCameras) {
			c.enabled = false;
			c.targetTexture = mRT;
			c.Render();
		}

		float widthMod = 1.0f / (1.0f * (1 << downsample));
		mBlurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
		mRT.filterMode = FilterMode.Bilinear;

		int rtW = mRT.width >> downsample;
		int rtH = mRT.height >> downsample;

		// downsample
		RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, mRT.format);
		rt.filterMode = FilterMode.Bilinear;
		Graphics.Blit(mRT, rt, mBlurMaterial, 0);

		int passOffs = blurType == BlurType.StandardGauss ? 0 : 2;
		for(int i = 0; i < blurIterations; i++) {
			float iterationOffs = (i * 1.0f);
			mBlurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

			// vertical blur
			RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, mRT.format);
			rt2.filterMode = FilterMode.Bilinear;
			Graphics.Blit(rt, rt2, mBlurMaterial, 1 + passOffs);
			RenderTexture.ReleaseTemporary(rt);
			rt = rt2;

			// horizontal blur
			rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, mRT.format);
			rt2.filterMode = FilterMode.Bilinear;
			Graphics.Blit(rt, rt2, mBlurMaterial, 2 + passOffs);
			RenderTexture.ReleaseTemporary(rt);
			rt = rt2;
		}
		mRT.Release();
		mRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.RGB565);
		mRT.Create();
		Graphics.Blit(rt, mRT);

		RenderTexture.ReleaseTemporary(rt);
	}

	public void render() {
		if(enabled && mRT != null) {
			Color current = GUI.color;
			GUI.color = Tint;
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), mRT);
			GUI.color = current;
		}
	}
}
