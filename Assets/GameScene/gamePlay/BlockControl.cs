using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public static float COLLISION_SIZE = 1.0f; // 블록의 충돌크기
    public static float VANISH_TIME = 3.0f; // 불 붙고 사라질 때까지의 시간

    public struct iPosition
    {
        public int x, y;
    }

    public enum COLOR
    {
        NONE = -1,
        pink = 0, blue, yellow, green, magenta, orange, gray,
        NUM,
        FIRST = pink,
        LAST = orange,
        NORMAL_COLOR_NUM = gray,
    };

    public enum DIR4
    {
        NONE = -1,
        right, left, up, down,
        NUM,
    };

    public enum STEP
    {
        NONE = -1,
        idle = 0, grabbed, released, slide, vacant, respawn, fall, long_slide,
        // 대기 중, 잡혀있음, 떨어진 순간, 슬라이드 중, 소멸 중, 재생성 중, 낙하 중, 크게 슬라이드 중
        NUM,
    }

    public static int BLOCK_NUM_X = 9;
    public static int BLOCK_NUM_Y = 9;
}

public class BlockControl : MonoBehaviour
{
    public Block.COLOR color = (Block.COLOR)0;
    public BlockRoot block_root = null;
    public Block.iPosition i_pos;

    public Block.STEP step = Block.STEP.NONE;
    public Block.STEP next_step = Block.STEP.NONE;
    private Vector3 position_offset_initial = Vector3.zero; // 교체전 위치
    public Vector3 position_offset = Vector3.zero; // 교체후 위치

    public float vanish_timer = -1.0f; // 블록이 사라질 때 까지의 시간
    public Block.DIR4 slide_dir = Block.DIR4.NONE; // 슬라이드 된 방향
    public float step_timer = 0.0f; // 블록 교체된 때의 이동시간 등..

    // Start is called before the first frame update
    void Start()
    {
        this.SetColor(this.color);
        next_step = Block.STEP.idle; // 다음 블록을 대기중으로
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Block.COLOR color)
    {
        this.color = color;
        Color color_value;

        switch (this.color)
        {
            default:
            case Block.COLOR.pink:
                color_value = new Color(1.0f, 0.5f, 0.5f);
                break;
            case Block.COLOR.blue:
                color_value = Color.blue;
                break;
            case Block.COLOR.yellow:
                color_value = Color.yellow;
                break;
            case Block.COLOR.green:
                color_value = Color.green;
                break;
            case Block.COLOR.magenta:
                color_value = Color.magenta;
                break;
            case Block.COLOR.orange:
                color_value = new Color(1.0f, 0.46f, 0.0f);
                break;
        }
        Renderer rd = GetComponent<Renderer>();
        rd.material.color = color_value;
    }
}
