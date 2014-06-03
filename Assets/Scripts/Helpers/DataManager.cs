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

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		mMesh = transform.Find("mesh");
		mActivesAnims = new List<activeAnim>();
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.loop = true;
		audioSource.playOnAwake = false;
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
		if(mMesh!=null && mMesh.animation!=null && mMesh.animation.GetClip("idle")!=null){
			PlayAnimation("idle", false);
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

	public void SetExpression(string idExpr)
	{
		Component[] children = GetComponentsInChildren<Component>(true);

		foreach(Component child in children){
			if(child.name.StartsWith("exp_")){
				if(child.name == idExpr){
					child.gameObject.SetActive(true);
				}else{
					child.gameObject.SetActive(false);
				}
			}
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

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private class activeAnim
	{
		public int layer;
		public string name;
		public float weight;
		public int startFrame;
	}
}




