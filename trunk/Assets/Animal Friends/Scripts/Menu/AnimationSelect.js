#pragma strict
#pragma downcast

var sceneCamera : Camera;

private var currentAnimation : String;

var dog : GameObject;
var cat : GameObject;
var turtle : GameObject;
var owl : GameObject;

var idleButton : Transform;
var talkButton : Transform;
var rollingButton : Transform;
var successButton : Transform;
var jumpButton : Transform;
var idle2Button : Transform;
var runButton : Transform;
var failureButton : Transform;
var sleepButton : Transform;
var walkButton : Transform;

var selectedButton : Transform;
private var selectedButtonScript : SelectedButton;

function Start () {
	dog.animation.Play("Walk Dog");
	cat.animation.Play("Walk Cat");
	turtle.animation.Play("Walk Turtle");
	owl.animation.Play("Walk Owl");
	
	selectedButtonScript = selectedButton.GetComponent(SelectedButton);
}

function Update () {
	if(Input.GetMouseButtonDown(0)){
		var ray = sceneCamera.ScreenPointToRay(Input.mousePosition);
		var hit : RaycastHit;
		if(Physics.Raycast(ray, hit)){;
			switch(hit.transform){
				case idleButton:
					if(currentAnimation != "Idle"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Idle Dog");
						cat.animation.Blend("Idle Cat");
						turtle.animation.Blend("Idle Turtle");
						owl.animation.Blend("Idle Owl");
						selectedButton.position = idleButton.position;
						currentAnimation = "Idle";
					}
					break;
				case talkButton:
					if(currentAnimation != "Talk"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Talk Dog");
						cat.animation.Blend("Talk Cat");
						turtle.animation.Blend("Talk Turtle");
						owl.animation.Blend("Talk Owl");
						selectedButton.position = talkButton.position;
						currentAnimation = "Talk";
					}
					break;
				case rollingButton:
					if(currentAnimation != "Rolling"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Rolling Dog");
						cat.animation.Blend("Rolling Cat");
						turtle.animation.Blend("Rolling Turtle");
						owl.animation.Blend("Rolling Owl");
						selectedButton.position = rollingButton.position;
						currentAnimation = "Rolling";
					}
					break;
				case successButton:
					if(currentAnimation != "Success"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Success Dog");
						cat.animation.Blend("Success Cat");
						turtle.animation.Blend("Success Turtle");
						owl.animation.Blend("Success Owl");
						selectedButton.position = successButton.position;
						currentAnimation = "Success";
					}
					break;
				case jumpButton:
					if(currentAnimation != "Jump"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.GetComponent(JumpTest).enabled = true;
						cat.GetComponent(JumpTest).enabled = true;
						turtle.GetComponent(JumpTest).enabled = true;
						owl.GetComponent(JumpTest).enabled = true;
						selectedButton.position = jumpButton.position;
						currentAnimation = "Jump";
					}
					break;
				case idle2Button:
					if(currentAnimation != "Idle 2"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Idle 2 Dog");
						cat.animation.Blend("Idle 2 Cat");
						turtle.animation.Blend("Idle 2 Turtle");
						owl.animation.Blend("Idle 2 Owl");
						selectedButton.position = idle2Button.position;
						currentAnimation = "Idle 2";
					}
					break;
				case runButton:
					if(currentAnimation != "Run"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Run Dog");
						cat.animation.Blend("Run Cat");
						turtle.animation.Blend("Run Turtle");
						owl.animation.Blend("Run Owl");
						selectedButton.position = runButton.position;
						currentAnimation = "Run";
					}
					break;
				case failureButton:
					if(currentAnimation != "Failure"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Failure Dog");
						cat.animation.Blend("Failure Cat");
						turtle.animation.Blend("Failure Turtle");
						owl.animation.Blend("Failure Owl");
						selectedButton.position = failureButton.position;
						currentAnimation = "Failure";
					}
					break;
				case sleepButton:
					if(currentAnimation != "Sleep"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Sleep Dog");
						cat.animation.Blend("Sleep Cat");
						turtle.animation.Blend("Sleep Turtle");
						owl.animation.Blend("Sleep Owl");
						selectedButton.position = sleepButton.position;
						currentAnimation = "Sleep";
					}
					break;
				case walkButton:
					if(currentAnimation != "Walk"){
						BlendAllCharactersToZero();
						selectedButtonScript.SetAnimationStart(Time.time);
						dog.animation.Blend("Walk Dog");
						cat.animation.Blend("Walk Cat");
						turtle.animation.Blend("Walk Turtle");
						owl.animation.Blend("Walk Owl");
						selectedButton.position = walkButton.position;
						currentAnimation = "Walk";
					}
					break;
			}
		}
	}
}

function BlendAllCharactersToZero(){
	BlendAllToZero(dog);
	BlendAllToZero(cat);
	BlendAllToZero(turtle);
	BlendAllToZero(owl);
	
	dog.GetComponent(JumpTest).enabled = false;
	cat.GetComponent(JumpTest).enabled = false;
	turtle.GetComponent(JumpTest).enabled = false;
	owl.GetComponent(JumpTest).enabled = false;
}

function BlendAllToZero(character : GameObject){
	var animation = character.animation;
	for(var state : AnimationState in animation){
		animation.Blend(state.name, 0);
	}
}