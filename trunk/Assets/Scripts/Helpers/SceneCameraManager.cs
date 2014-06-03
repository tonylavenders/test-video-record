using UnityEngine;
using System.Collections;
using TVR;

public class SceneCameraManager : MonoBehaviour
{
	public void PlayAudio(int idMusic, float sec)
	{
		audio.clip = TVR.Helpers.ResourcesManager.LoadResource(ResourcesLibrary.getMusic(idMusic).ResourceName, "Music") as AudioClip;
		audio.time = sec;
		audio.Play();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void StopAudio()
	{
		audio.Stop();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetParams(int shotType)
	{
		ResourcesLibrary.CameraParams mCameraParams = ResourcesLibrary.getCamera(shotType);
		transform.position = mCameraParams.Position;
		transform.eulerAngles = mCameraParams.EulerAngles;
		camera.fieldOfView = mCameraParams.DoF;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void OnDestroy()
	{
		TVR.Helpers.ResourcesManager.UnloadScene("Music");
	}
}