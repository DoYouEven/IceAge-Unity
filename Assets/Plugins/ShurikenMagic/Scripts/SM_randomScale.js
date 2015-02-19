var minScale:float=1;
var maxScale:float=2;

function Start ()
{
var actualRandom=Random.Range(minScale, maxScale);
transform.localScale=Vector3(actualRandom, actualRandom, actualRandom);

}


function Update () {


}