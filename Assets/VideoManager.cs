using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    // Start is called before the first frame update
    
    public VideoPlayer videoPlayer;

    
    void Start()
    {
        
    }

    
    public void TogglePlayPause()
    {
        
        Debug.Log("TogglePlayPause");
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
