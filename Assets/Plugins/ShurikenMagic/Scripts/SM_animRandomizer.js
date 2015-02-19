var animList : AnimationClip[];
var actualAnim:AnimationClip;

var minSpeed:float=0.7;
var maxSpeed:float=1.5;

function Start () {
var rnd=Mathf.Round(Random.Range(0,animList.length));
actualAnim = animList[rnd];
animation.Play(actualAnim.name);
animation[actualAnim.name].speed = Random.Range(minSpeed, maxSpeed);

}