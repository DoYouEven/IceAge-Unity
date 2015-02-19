
var startStr:float=2;
var speed:float=3;
private var timeGoes:float=0;
private var currStr:float=0;



function Update () {

timeGoes+=Time.deltaTime*speed*startStr;

currStr=startStr-timeGoes;

renderer.material.SetFloat( "_AllPower", currStr );


}

