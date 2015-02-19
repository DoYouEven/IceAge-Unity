
var targetMaterialSlot:int=0;
//var scrollThis:Material;
var speedY:float=0.5;
var speedX:float=0.0;
private var timeWentX:float=0;
private var timeWentY:float=0;
function Start () {

}

function Update () {
timeWentY += Time.deltaTime*speedY;
timeWentX += Time.deltaTime*speedX;


renderer.materials[targetMaterialSlot].SetTextureOffset ("_MainTex", Vector2(timeWentX, timeWentY));


}