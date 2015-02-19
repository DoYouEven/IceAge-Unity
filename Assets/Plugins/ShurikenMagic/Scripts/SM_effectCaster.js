var moveThis : GameObject; //this is an invisible "cursor" that is always there where the mouse points
var hit : RaycastHit;
var createThis : GameObject[];
var cooldown : float;
var changeCooldown : float;
var selected:int=0;
var writeThis:GUIText;
private var rndNr:float;
private var effect:GameObject;


function Start () {

selected=createThis.length-1;
writeThis.text=selected.ToString()+" "+createThis[selected].name;

}

function Update () {

if(cooldown>0){cooldown-=Time.deltaTime;}
if(changeCooldown>0){changeCooldown-=Time.deltaTime;}

var ray = Camera.main.ScreenPointToRay (Input.mousePosition);

if (Physics.Raycast (ray, hit)) {
// Create a prefab if hit something

moveThis.transform.position=hit.point;


if(Input.GetMouseButton(0)&&cooldown<=0){
effect=Instantiate(createThis[selected], moveThis.transform.position, moveThis.transform.rotation);

//For showcasing reasons: some effects don't look good if they appear half underground. I move these effects a bit upward.
if(effect.tag=="explosion"||effect.tag=="missile"||effect.tag=="breath"){
effect.transform.position.y+=1.5;
}

if(effect.tag=="shield"){
effect.transform.position.y+=0.5;
}

cooldown=0.15;
}



}


if (Input.GetKeyDown(KeyCode.UpArrow) && changeCooldown<=0)
{
	selected+=1;
		if(selected>(createThis.length-1)) {selected=0;}
	
	writeThis.text=selected.ToString()+" "+createThis[selected].name;
	changeCooldown=0.1;
}

if (Input.GetKeyDown(KeyCode.DownArrow) && changeCooldown<=0)
{
	selected-=1;
		if(selected<0) {selected=createThis.length-1;}
	
		writeThis.text=selected.ToString()+" "+createThis[selected].name;
	changeCooldown=0.1;
}




}