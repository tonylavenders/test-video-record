using UnityEngine;
using System.Collections;
using TVR;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
	private const float CROSSTIME = 0.2f;
	Transform mMesh;
	AudioSource audioSource;
	List<activeAnim> mActivesAnims;
	public const int NUMFRAMESLIPSYNC = 3; //Número de frames para el cambio de boca

	private bool mLipSync;
	private string mExpression;
	private bool mMouthClose;
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		mMesh = transform.Find("mesh");
		mActivesAnims = new List<activeAnim>();
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.loop = false;
		audioSource.playOnAwake = false;

		mLipSync = false;
		mMouthClose = false;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void PlayAnimation(string sAnim, int startFrame, float seconds, bool play)
	{
		float sec = seconds - (startFrame * Globals.MILISPERFRAME);

		if(!play)
			StopAnimation();
		
		activeAnim anim;
		if(play) {
			for(int i = 0; i < mActivesAnims.Count; ++i) {
				anim = mActivesAnims[i];
				if(anim.startFrame == startFrame || anim.name == sAnim) {
					--i;
					mActivesAnims.Remove(anim);
				}
			}
		
			mMesh.animation[sAnim].time = sec;
			mMesh.animation[sAnim].speed = 1f;

			mActivesAnims.Add(new activeAnim() { layer = 0, name = sAnim, weight = 1, startFrame = startFrame });
			mMesh.animation.CrossFade(sAnim, CROSSTIME, PlayMode.StopAll);
		} else {
			for(int i = 0; i < mActivesAnims.Count; ++i) {
				anim = mActivesAnims[i];
				float time = seconds - (anim.startFrame * Globals.MILISPERFRAME);
				if(time < 0 || sec >= CROSSTIME || anim.startFrame == startFrame || anim.name == sAnim) {
					mMesh.animation[anim.name].weight = 0;
					mMesh.animation[anim.name].enabled = false;
					--i;
					mActivesAnims.Remove(anim);
				} else {
					anim.weight = Mathf.Min(time / CROSSTIME, 1);
				}
			}
			if(mActivesAnims.Count == 0)
				mActivesAnims.Add(new activeAnim() { layer = 0, name = sAnim, weight = 1, startFrame = startFrame });
			else
				mActivesAnims.Add(new activeAnim() { layer = 0, name = sAnim, weight = Mathf.Min(sec / CROSSTIME, 1), startFrame = startFrame });
			
			for(int i = 0; i < mActivesAnims.Count; ++i) {
				anim = mActivesAnims[i];
				mMesh.animation[anim.name].layer = i;

				mMesh.animation[sAnim].time = seconds - (anim.startFrame * Globals.MILISPERFRAME);
				mMesh.animation[sAnim].speed = 1f;
				
				mMesh.animation[anim.name].weight = anim.weight;
				mMesh.animation[anim.name].enabled = true;
			}
			mMesh.animation.Sample();
			foreach(activeAnim anim1 in mActivesAnims) {
				mMesh.animation[anim1.name].enabled = false;
			}
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/*
	public void IdleAnimation()
	{
		if(mMesh!=null && mMesh.animation!=null && mMesh.animation.GetClip("Idle")!=null){
			PlayAnimation("Idle", false);
		}
	}
	*/
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void StopAnimation()
	{
		foreach(activeAnim anim in mActivesAnims) {
			mMesh.animation[anim.name].enabled = false;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/*public void SetExpression(string idExpr)
	{
		Component[] children = GetComponentsInChildren<Component>(true);

		foreach(Component child in children){
			if(child.name.StartsWith("exp_")){
				if(child.name == "exp_"+idExpr){
					child.gameObject.SetActive(true);
				}else if(child.name == "exp_"+idExpr+"_m"){
						child.gameObject.SetActive(true);
				}else{
					child.gameObject.SetActive(false);
				}
			}
		}
	}*/

	public void SetExpression(string idExpr) {
		mExpression = "exp_" + idExpr;

		if(mExpression.EndsWith("_c"))
			mMouthClose = true;
		else
			mMouthClose = false;

		Component[] children = GetComponentsInChildren<Component>(true);
		foreach(Component child in children) {
			if(child.name.StartsWith("exp_")) {
				if(child.name == mExpression)
					child.gameObject.SetActive(true);
				else if(child.name == mExpression + "_m" && !mLipSync)
					child.gameObject.SetActive(true);
				else
					child.gameObject.SetActive(false);
			} else if(child.name.StartsWith("lipsync"))
				child.gameObject.SetActive(false);
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void PlayAudio(AudioClip audioClip, float sec)
	{
		audioSource.clip = audioClip;
		audioSource.time = sec;
		audioSource.Play();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	 
	public void StopAudio()
	{
		audioSource.Stop();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//LIPSYNC
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void startLipSync() {
		mLipSync = true;
		StartCoroutine("changeMounth");
		/*max=0;
		min=1;*/
	}

	public void stopLipSync() {
		mLipSync = false;
		StopCoroutine("changeMounth");
		Component[] children = GetComponentsInChildren<Component>(true);
		foreach(Component child in children) {
			if(child.name.StartsWith("lipsync"))
				child.gameObject.SetActive(false);
			else if(child.name == mExpression + "_m")
				child.gameObject.SetActive(true);
		}
		//Debug.Log("Min " + min + " Max " + max);
	}

	IEnumerator changeMounth() {
		while(audioSource.isPlaying) {
			lipSync(audioSource.clip, audioSource.time);
			yield return new WaitForSeconds(Globals.MILISPERFRAME * NUMFRAMESLIPSYNC);
		}
	}

	public void lipSync(AudioClip clip, int currentFrame, int startFrame) {
		mLipSync = true;
		lipSync(clip, currentFrame - startFrame);
	}

	private void lipSync(AudioClip clip, int frame) {
		frame = ((int)(frame / NUMFRAMESLIPSYNC)) * NUMFRAMESLIPSYNC;
		int init = Mathf.CeilToInt(Globals.MILISPERFRAME * clip.frequency);
		float[] samples = new float[init * clip.channels * 3];
		init *= frame % (int)(clip.length * Globals.FRAMESPERSECOND);

		clip.GetData(samples, init);
		float midSamples = 0;
		for(int i = 0; i < samples.Length; ++i) {
			midSamples += Mathf.Abs(samples[i]);
		}
		midSamples /= samples.Length;

		Component[] children = GetComponentsInChildren<Component>(true);
		foreach(Component child in children) {
			if((child.name.StartsWith("exp_") && child.name.EndsWith("_m")) || child.name.StartsWith("lipsync_"))
				child.gameObject.SetActive(false);
			if(midSamples < 0.05f && mMouthClose && child.name == mExpression + "_m")
				child.gameObject.SetActive(true);
			else if(midSamples < 0.05f && !mMouthClose && child.name == "lipsync_00")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.05f && midSamples < 0.1f && child.name == "lipsync_06")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.1f && midSamples < 0.15f && child.name == "lipsync_07")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.15f && midSamples < 0.2f && child.name == "lipsync_01")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.2f && midSamples < 0.25f && child.name == "lipsync_04")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.25f && midSamples < 0.3f && child.name == "lipsync_05")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.3f && midSamples < 0.35f && child.name == "lipsync_06")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.35f && midSamples < 0.4f && child.name == "lipsync_07")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.4f && midSamples < 0.45f && child.name == "lipsync_08")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.45f && child.name == "lipsync_09")
				child.gameObject.SetActive(true);

			/*if(midSamples < 0.05f && mMouthClose && child.name == mExpression + "_m")
				child.gameObject.SetActive(true);
			else if(midSamples < 0.05f && !mMouthClose && child.name == "lipsync_00")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.05f && midSamples < 0.1f && child.name == "lipsync_01")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.1f && midSamples < 0.15f && child.name == "lipsync_02")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.15f && midSamples < 0.2f && child.name == "lipsync_03")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.2f && midSamples < 0.25f && child.name == "lipsync_04")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.25f && midSamples < 0.3f && child.name == "lipsync_05")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.3f && midSamples < 0.35f && child.name == "lipsync_06")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.35f && midSamples < 0.4f && child.name == "lipsync_07")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.4f && midSamples < 0.45f && child.name == "lipsync_08")
				child.gameObject.SetActive(true);
			else if(midSamples >= 0.45f && child.name == "lipsync_09")
				child.gameObject.SetActive(true);*/

			/*if(midSamples<0.01f && child.name==mExpression+"_m")
			child.gameObject.SetActive(true);
			else if(midSamples>=0.01f && midSamples<0.05f && child.name=="lipsync_01")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.05f && midSamples<0.1f && child.name=="lipsync_02")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.1f && midSamples<0.15f && child.name=="lipsync_03")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.15f && midSamples<0.2f && child.name=="lipsync_04")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.2f && midSamples<0.25f && child.name=="lipsync_05")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.25f && midSamples<0.3f && child.name=="lipsync_06")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.3f && midSamples<0.35f && child.name=="lipsync_07")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.35f && midSamples<0.4f && child.name=="lipsync_08")
				child.gameObject.SetActive(true);
			else if(midSamples>=0.4f && child.name=="lipsync_09")
				child.gameObject.SetActive(true);*/
		}
	}

	private void lipSync(AudioClip clip, float time) {
		int frame = (int)((time * Globals.FRAMESPERSECOND + 0.1f));
		lipSync(clip, frame);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private class activeAnim
	{
		public int layer;
		public string name;
		public float weight;
		public int startFrame;
	}
}