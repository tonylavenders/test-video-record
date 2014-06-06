#pragma strict

function Start () {

}

function Update () {
	renderer.material.SetTextureOffset("_MainTex", Vector2(-Time.time * .1, Mathf.Cos(Time.time * .1)));
}