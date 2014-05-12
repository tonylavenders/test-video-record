using UnityEngine;
using System.Collections;

public class CameraBlur : MonoBehaviour {
	public static iBlur Blur;

	// Use this for initialization
	void Start () {
		if(transform.GetComponent<cBlur>().isSupported())
			Blur = transform.GetComponent<cBlur>();
		else
			Blur = transform.GetComponent<cBlur2>();
		Blur.enabled = true;
	}
}