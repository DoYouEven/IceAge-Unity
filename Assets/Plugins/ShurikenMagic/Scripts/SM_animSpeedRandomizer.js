
var minSpeed:float=0.7;
var maxSpeed:float=1.5;

function Start () {
animation[animation.clip.name].speed = Random.Range(minSpeed, maxSpeed);
}