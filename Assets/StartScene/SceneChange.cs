using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void MoveScene()
    {
        click.Play();
        SceneManager.LoadScene("SettingScene");
    }
}
