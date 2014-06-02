using UnityEngine;
using System.Collections;
using TVR;

public class SceneCameraManager : MonoBehaviour
{

	public void SetParams(int shotType)
	{
		ResourcesLibrary.CameraParams mCameraParams = ResourcesLibrary.getCamera(shotType);
		transform.position = mCameraParams.Position;
		transform.eulerAngles = mCameraParams.EulerAngles;
		camera.fieldOfView = mCameraParams.DoF;
	}
}
