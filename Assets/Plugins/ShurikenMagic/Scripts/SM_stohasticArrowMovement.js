

var rotSpeed:float=3;
var rotRandomPlus:float=0.5;
var rotTreshold:float=50;

var changeRotMin:float=1;
var changeRotMax:float=2;

var minSpeed:float=0.5;
var maxSpeed:float=2;

var changeSpeedMin:float=0.5;
var changeSpeedMax:float=2;


private var speed:float=0;
private var timeGoesX:float=0;
private var timeGoesY:float=0;
private var timeGoesSpeed:float=0;

private var timeToChangeX:float=0.1;
private var timeToChangeY:float=0.1;
private var timeToChangeSpeed:float=0.1;

private var xDir:boolean=true;
private var yDir:boolean=true;

private var curRotSpeedX:float=0;
private var curRotSpeedY:float=0;

private var lendX:float=0;
private var lendY:float=0;


//********CUSTOM
function RandomizeSpeed ()
{
speed=Random.Range(minSpeed, maxSpeed);
}

function RandomizeXRot () {
var rnd=Random.value*rotRandomPlus;
curRotSpeedX=rotSpeed*rnd;
}

function RandomizeYRot () {
var rnd=Random.value*rotRandomPlus;
curRotSpeedY=rotSpeed*rnd;
}



//********START
function Start () {
RandomizeSpeed ();
if (Random.value>0.5) xDir=!xDir;
if (Random.value>0.5) yDir=!yDir;

timeToChangeY=Random.Range(changeRotMin, changeRotMax);
timeToChangeX=Random.Range(changeRotMin, changeRotMax);
timeToChangeSpeed=Random.Range(changeSpeedMin, changeSpeedMax);

curRotSpeedX=Random.Range((rotSpeed),(rotSpeed+rotRandomPlus));
curRotSpeedY=Random.Range((rotSpeed),(rotSpeed+rotRandomPlus));


}


//********UPDATE
function Update () {



if (xDir==true) lendX+=Time.deltaTime*curRotSpeedX;
if (xDir==false) lendX-=Time.deltaTime*curRotSpeedX;
if (yDir==true) lendY+=Time.deltaTime*curRotSpeedY;
if (yDir==false) lendY-=Time.deltaTime*curRotSpeedY;

if (lendX>rotTreshold)
{
lendX=rotTreshold;
xDir=!xDir;
}

if (lendX>rotTreshold)
{
lendX=-rotTreshold;
xDir=!xDir;
}
if (lendY>rotTreshold)
{
lendY=rotTreshold;
yDir=!yDir;
}


if (lendY>rotTreshold)
{
lendY=-rotTreshold;
yDir=!yDir;
}


transform.Rotate(lendX*Time.deltaTime, lendY*Time.deltaTime, 0);
transform.Translate(0, speed*Time.deltaTime, 0);


//**************
timeGoesX+=Time.deltaTime;
timeGoesY+=Time.deltaTime;
timeGoesSpeed+=Time.deltaTime;

if (timeGoesX>timeToChangeX)
{
xDir=!xDir;
timeGoesX-=timeGoesX;
timeToChangeX=Random.Range(changeRotMin, changeRotMax);
curRotSpeedX=Random.Range((rotSpeed),(rotSpeed+rotRandomPlus));
}

if (timeGoesY>timeToChangeY)
{
yDir=!yDir;
timeGoesY-=timeGoesY;
timeToChangeY=Random.Range(changeRotMin, changeRotMax);
curRotSpeedY=Random.Range((rotSpeed),(rotSpeed+rotRandomPlus));
}


if (timeGoesSpeed>timeToChangeSpeed)
{
RandomizeSpeed();
timeGoesSpeed-=timeGoesSpeed;
timeToChangeSpeed=Random.Range(changeSpeedMin, changeSpeedMax);
Debug.Log("hejj");
}



}


