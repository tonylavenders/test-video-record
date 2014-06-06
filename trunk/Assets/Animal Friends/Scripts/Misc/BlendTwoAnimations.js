#pragma strict

var firstAnimation : AnimationClip;
var secondAnimation : AnimationClip;

var blend : float;

function Start () {

}

function Update () {
	blend = Mathf.Clamp01(blend);
	
	animation.Blend(firstAnimation.name, blend, 0.05);
	animation.Blend(secondAnimation.name, 1 - blend, .05);
}