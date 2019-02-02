using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    public GameObject timer_text;
    public GameObject score_text;

    public int count = 0;
    private static float total_score;
    private Text Timer;
    private Text Score;
    public float not_move_time = 0;
    private float timer = 60.0f;
    private bool is_timeover = false;

    // Start is called before the first frame update
    void Start()
    {
        Timer = timer_text.GetComponent<Text>();
        Score = score_text.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        Timer.text = string.Format("Timer: {0:N1}", Calc_timer());
        Score.text = string.Format("Score: {0}", Calc_total_score());

    }

    public void PlusCount()
    {
        count++;
    }

    public float Calc_total_score()
    {
        if (!is_timeover)
        {
            not_move_time += (Time.deltaTime * 10);
            total_score = (float)count * 100 - not_move_time;
            total_score -= total_score % 10;
        }
        // 아무것도 안하고 있으면 1초당 10점 깎임

        return total_score;
    }

    public static float get_score()
    {
        float r = total_score;
        return r;
    }

    public float Calc_timer()
    {
        if (timer > 0.0f)
        {
            timer -= Time.deltaTime;
        }
        else is_timeover = true;

        return timer;
    }

    public bool Is_gameover()
    {
        return is_timeover;
    }
    
}
