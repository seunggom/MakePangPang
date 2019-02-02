using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChange : MonoBehaviour
{
    private AudioSource click;
    public AudioClip sound;

    // Start is called before the first frame update
    void Start()
    {
        click = gameObject.AddComponent<AudioSource>();
        click.clip = sound;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveScene(Button b)
    {
        click.Play();
        if (b.name == "Game Start!")
        {
            SceneManager.LoadScene("SettingScene");
        }
        else if (b.name == "Play!")
        {
            SceneManager.LoadScene("PlayScene");
        }
        else if(b.name == "init")
        {
            SceneManager.LoadScene("StartScene");
        }
        else if(b.name == "exit")
        {
            Application.Quit();
        }
    }
}
