var rotationSpeedX:float=90;
var rotationSpeedY:float=0;
var rotationSpeedZ:float=0;
private var rotationVector:Vector3=Vector3(rotationSpeedX,rotationSpeedY,rotationSpeedZ);



function Update () {

transform.Rotate(Vector3(rotationSpeedX,rotationSpeedY,rotationSpeedZ)*Time.deltaTime);

}