using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;

public class Text3DController : MonoBehaviour {
	void Start() {
		TextMesh text3D = transform.GetComponent<TextMesh>();
		text3D.fontSize = (int)(transform.parent.lossyScale.x * (29.0f / 90.0f)); //para button.scale=90 ==> font_size=26
		float scale = 90f * (0.1f / transform.parent.lossyScale.x);
		transform.localScale = new Vector3(scale, scale, 1);
	}
}