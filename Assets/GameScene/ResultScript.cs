using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ResultScript : MonoBehaviour
{
    public GameObject text;
    private Text t;
    // Start is called before the first frame update
    void Start()
    {
        t = text.gameObject.GetComponent<Text>();
        t.text = "Score: <color=#ffffffff>" + ScoreCounter.get_score() + "</color>";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
