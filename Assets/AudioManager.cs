using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	// Use this for initialization
    public  AudioSource[] audiolist;
   
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlayAudio(int index)
    {
        audiolist[index].Play();
    }

}
