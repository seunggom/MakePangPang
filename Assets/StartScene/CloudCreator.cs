using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudCreator : MonoBehaviour
{
    public GameObject[] CloudPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateCloud(Vector2 cloud_pos)
    {
        int cloud_type = Random.Range(0, CloudPrefabs.Length);
        GameObject go = GameObject.Instantiate(CloudPrefabs[cloud_type]) as GameObject;
        go.transform.position = cloud_pos;
        float rnd = Random.Range(0.7f, 1.7f);
        go.transform.localScale = new Vector2(rnd, rnd);
        float rnd2 = (cloud_type+1) * 400;
        go.transform.GetComponent<Rigidbody2D>().AddForce(Vector2.right * rnd2 * Time.deltaTime);
    }
}
