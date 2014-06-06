#pragma strict

function Start () {
	Screen.showCursor = false;
}

function Update () {
	if(Input.GetKeyDown(KeyCode.Escape)){
		Screen.showCursor = true;
	}
}