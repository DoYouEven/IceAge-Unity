var translationSpeedX:float=0;
var translationSpeedY:float=1;
var translationSpeedZ:float=0;

var local:boolean=true;






function Update () {


if (local==true){
transform.Translate(Vector3(translationSpeedX,translationSpeedY,translationSpeedZ)*Time.deltaTime);
}

if (local==false){
transform.Translate(Vector3(translationSpeedX,translationSpeedY,translationSpeedZ)*Time.deltaTime, Space.World);
}
 

}