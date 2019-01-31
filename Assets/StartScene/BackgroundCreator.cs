using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundCreator : MonoBehaviour
{
    private CloudCreator cloud;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 7; i++)
        {
            cloud = this.gameObject.GetComponent<CloudCreator>();
            Vector2 v = new Vector2(Random.Range(-20.0f, 15.0f), Random.Range(-3.0f, 5.0f));
            cloud.CreateCloud(v);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
