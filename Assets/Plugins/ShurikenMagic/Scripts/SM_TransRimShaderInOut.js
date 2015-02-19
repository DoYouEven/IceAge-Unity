
var str:float=1;

var fadeIn:float=1;
var stay:float=1;
var fadeOut:float=1;

private var timeGoes:float=0;
private var currStr:float=0;

function Start () {
renderer.material.SetFloat( "_AllPower", currStr );
}

function Update () {

timeGoes+=Time.deltaTime;

if(timeGoes<fadeIn)
{
currStr=timeGoes*str*(1/fadeIn);
}

if((timeGoes>fadeIn)&&(timeGoes<fadeIn+stay))
{
currStr=str;
}

if(timeGoes>fadeIn+stay)
{
currStr=str-((timeGoes-(fadeIn+stay))*(1/fadeOut));
}



//currStr=startStr-timeGoes;

renderer.material.SetFloat( "_AllPower", currStr );


}

