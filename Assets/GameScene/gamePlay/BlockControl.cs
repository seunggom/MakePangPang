using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Block
{
    public static float COLLISION_SIZE = 0.7f; // 블록의 충돌크기
    public static float VANISH_TIME = 0.3f; // 사라질 때까지의 시간

    public struct iPosition
    {
        public int x, y;
    }

    public enum COLOR
    {
        NONE = -1,
        red = 0, blue, yellow, green, magenta, cyan, gray,
    };

    public enum DIR4
    {
        NONE = -1,
        RIGHT, LEFT, UP, DOWN,
        NUM,
    };

    public enum STEP
    {
        NONE = -1,
        idle = 0, grabbed, released, slide, vacant, respawn, fall,
        // 대기 중, 잡혀있음, 떨어진 순간, 슬라이드 중, 소멸 중, 재생성 중, 낙하 중,
        NUM,
    }

    public static int BLOCK_NUM_X = SizeManager.mapSize;
    public static int BLOCK_NUM_Y = SizeManager.mapSize;
}

public class BlockControl : MonoBehaviour
{
    private struct StepFall
    {
        public float velocity; // 낙하속도
    }
    private StepFall fall;

    public Block.COLOR color = (Block.COLOR)0;
    public BlockRoot block_root = null;
    public Block.iPosition i_pos;

    public Block.STEP step = Block.STEP.NONE;
    public Block.STEP next_step = Block.STEP.NONE;
    private Vector2 position_offset_initial = Vector2.zero; // 교체전 위치
    public Vector2 position_offset = Vector2.zero; // 교체후 위치

    public float vanish_timer = -1.0f; // 블록이 사라질 때 까지의 시간
    public Block.DIR4 slide_dir = Block.DIR4.NONE; // 슬라이드 된 방향
    public float step_timer = 0.0f; // 블록 교체된 때의 이동시간 등..

    public Material opague_material; // 불투명
    public Material transparent_material; // 반투명

    // Start is called before the first frame update
    void Start()
    {
        this.SetColor(this.color);
        next_step = Block.STEP.idle; // 다음 블록을 대기중으로
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouse_position;
        block_root.unprojectMousePosition(out mouse_position, Input.mousePosition);

        
        if (this.vanish_timer >= 0.0f)
        {
            this.vanish_timer -= Time.deltaTime;
            if (this.vanish_timer < 0.0f)
            {
                if (this.step != Block.STEP.slide)
                {
                    this.vanish_timer = -1.0f;
                    this.next_step = Block.STEP.vacant;
                }
                else
                {
                    this.vanish_timer = 0.0f;
                }
            }
        }
        
        this.step_timer += Time.deltaTime;
        float slide_time = 0.2f;

        if (this.next_step == Block.STEP.NONE)
        {
            switch (this.step)
            {
                case Block.STEP.slide:
                    if (this.step_timer >= slide_time)
                    {
                        // 슬라이드 중인 블록이 소멸되면 vacant 상태로 이행
                        if (this.vanish_timer == 0.0f)
                        {
                            this.next_step = Block.STEP.vacant;
                            // vanish_timer가 0 이 아니면 idle 상태로 이행
                        }
                        else
                        {
                            this.next_step = Block.STEP.idle;
                        }
                    }
                    break;

                case Block.STEP.idle:
                    this.GetComponent<Renderer>().enabled = true;
                    break;

                case Block.STEP.fall:
                    if (this.position_offset.y <= 0.0f)
                    {
                        this.next_step = Block.STEP.idle;
                        this.position_offset.y = 0.0f;
                    }
                    break;
            }
        }
        
        while (this.next_step != Block.STEP.NONE)
        {
            this.step = this.next_step;
            this.next_step = Block.STEP.NONE;

            switch (this.step)
            {
                case Block.STEP.idle:
                    this.position_offset = Vector3.zero;
                    this.transform.localScale = Vector3.one * 0.7f;
                    break;
                case Block.STEP.grabbed:
                    this.transform.localScale = Vector3.one * 1.0f;
                    break;
                case Block.STEP.released:
                    this.position_offset = Vector3.zero;
                    this.transform.localScale = Vector3.one * 0.7f;
                    break;
                case Block.STEP.vacant:
                    this.position_offset = Vector3.zero;
                    this.setVisible(false);
                    break;

                case Block.STEP.respawn:
                    // 색을 랜덤하게 선택하여 블록을 선택한 색으로 설정.
                    int color_index = Random.Range(0, SizeManager.colorNum);
                    this.SetColor((Block.COLOR)color_index);
                    this.next_step = Block.STEP.idle;
                    break;
                case Block.STEP.fall:
                    this.setVisible(true); // 블록을 표시.
                    this.fall.velocity = 0.0f; // 낙하 속도를 리셋.
                    break;

            }
            this.step_timer = 0.0f;
        }

        switch (this.step)
        {
            case Block.STEP.grabbed:
                // 잡힌 상태일 때는 항상 슬라이드 방향을 체크
                this.slide_dir = this.calcSlideDir(mouse_position);
                break;
            case Block.STEP.slide:
                // 블록을 서서히 이동하는 처리
                float rate = this.step_timer / slide_time;
                rate = Mathf.Min(rate, 1.0f);
                rate = Mathf.Sin(rate * Mathf.PI / 2.0f);
                this.position_offset = Vector3.Lerp(this.position_offset_initial, Vector3.zero, rate);
                break;

            case Block.STEP.fall:
                // 속도에 중력의 영향을 준다.
                this.fall.velocity += Physics.gravity.y * Time.deltaTime * 1.5f;
                // 세로 방향 위치를 계산.
                this.position_offset.y += this.fall.velocity * Time.deltaTime;
                if (this.position_offset.y < 0.0f)
                { // 다 내려왔다면.
                    this.position_offset.y = 0.0f; // 그 자리에 머문다.
                }
                break;

        }

        // 그리드 좌표를 실제 좌표로 변환하고 position_offset을 추가한다
        Vector3 position = BlockRoot.calcBlockPosition(this.i_pos) + this.position_offset;

        // 실제 위치를 새로운 위치로 변경한다
        this.transform.position = position;

        this.SetColor(this.color);

        if (this.vanish_timer >= 0.0f)
        {
            Renderer rd = this.GetComponent<Renderer>();
            Color color0 = Color.Lerp(rd.material.color, Color.white, 0.5f);
            Color color1 = Color.Lerp(rd.material.color, Color.black, 0.5f);

            // 불붙는 연출 시간이 절반을 지났다면,
            if (this.vanish_timer < Block.VANISH_TIME / 2.0f)
            {
                // 투명도(a)를 설정
                color0.a = this.vanish_timer / (Block.VANISH_TIME / 2.0f);
                color1.a = color0.a;

                rd.material = this.transparent_material;
            }
            // vanish_timer가 줄어들수록 1에 가까워진다
            float rate = 1.0f - this.vanish_timer / Block.VANISH_TIME;
            //서서히 색을 바꾼다
            rd.material.color = Color.Lerp(color0, color1, rate);
        }
    }

    public void SetColor(Block.COLOR color)
    {
        this.color = color;
        Color color_value;

        switch (this.color)
        {
            default:
            case Block.COLOR.red:
                color_value = Color.red;
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
            case Block.COLOR.cyan:
                color_value = Color.cyan;
                break;
            case Block.COLOR.gray:
                color_value = Color.gray;
                break;
        }
        Renderer rd = GetComponent<Renderer>();
        rd.material.color = color_value;
    }

    public void beginGrab()
    {
        this.next_step = Block.STEP.grabbed;
    }

    public void endGrab()
    {
        this.next_step = Block.STEP.idle;
    }

    public bool isGrabbable()
    {
        bool is_grabbable = false;
        switch (this.step)
        {
            case Block.STEP.idle: // 대기 상태일때만 잡을 수 있음
                is_grabbable = true;
                break;
        }
        return (is_grabbable);
    }

    public bool isContainedPosition(Vector2 position) // 지정된 마우스 좌표가 자신에게 겹치는지 반환한다
    {
        bool ret = false;
        Vector2 center = this.transform.position;
        float h = Block.COLLISION_SIZE / 2.0f;

        do
        {
            if (position.x < center.x - h || center.x + h < position.x) break;
            if (position.y < center.y - h || center.y + h < position.y) break;

            ret = true;
        } while (false);

        return (ret);
    }

    public Block.DIR4 calcSlideDir(Vector2 mouse_position)
    {
        Block.DIR4 dir = Block.DIR4.NONE;
        // 지정된 mouse_position과 현재 위치의 차를 나타내는 벡터
        Vector2 v = mouse_position - new Vector2(this.transform.position.x, this.transform.position.y);

        // 벡터크기가 0.1보다 크면 (그보다 작으면 슬라이드하지 않은 것으로 간주)
        if (v.magnitude > 0.1f)
        {
            if (v.y > v.x)
            {
                if (v.y > -v.x)
                {
                    dir = Block.DIR4.UP;
                }
                else
                {
                    dir = Block.DIR4.LEFT;
                }
            }
            else
            {
                if (v.y > -v.x)
                {
                    dir = Block.DIR4.RIGHT;
                }
                else
                {
                    dir = Block.DIR4.DOWN;
                }
            }
        }
        return (dir);
    }

    public float calcDirOffset(Vector2 position, Block.DIR4 dir)
    {
        float offset = 0.0f;

        Vector2 v = position - new Vector2(this.transform.position.x, this.transform.position.y);
        switch (dir)
        {
            case Block.DIR4.RIGHT:
                offset = v.x;
                break;
            case Block.DIR4.LEFT:
                offset = -v.x;
                break;
            case Block.DIR4.UP:
                offset = v.y;
                break;
            case Block.DIR4.DOWN:
                offset = -v.y;
                break;
        }
        return (offset);
    }

    public void beginSlide(Vector3 offset)
    {
        this.position_offset_initial = offset;
        this.position_offset = this.position_offset_initial;

        this.next_step = Block.STEP.slide;
    }

    public bool isVisible()
    {
        // 그리기 가능(renderer.enabled가 true)이라면.
        // 표시된다. 
        bool is_visible = this.GetComponent<Renderer>().enabled;
        return (is_visible);
    }

    public void setVisible(bool is_visible)
    {
        // 그리기 가능 설정에 인수를 대입한다.
        this.GetComponent<Renderer>().enabled = is_visible;
    }

    public bool isIdle()
    {
        bool is_idle = false;
        // 현재 블록 상태가 '대기 중'이고.
        // 다음 블록 상태가 '없음'이면.
        if (this.step == Block.STEP.idle &&
           this.next_step == Block.STEP.NONE)
        {
            is_idle = true;
        }
        return (is_idle);
    }

    public void beginFall(BlockControl start)
    {
        this.next_step = Block.STEP.fall;
        // 지정된 블록에서 좌표를 계산해 낸다.
        this.position_offset.y = (float)(start.i_pos.y - this.i_pos.y) * Block.COLLISION_SIZE;
    }

    public void beginRespawn(int start_ipos_y)
    {
        // 지정 위치까지 y좌표를 이동.
        this.position_offset.y = (float)(start_ipos_y - this.i_pos.y) * Block.COLLISION_SIZE;
        this.next_step = Block.STEP.fall;
        int color_index = Random.Range(0, SizeManager.colorNum);
        this.SetColor((Block.COLOR)color_index);
    }

    public bool isVacant()
    {
        bool is_vacant = false;
        if (this.step == Block.STEP.vacant && this.next_step == Block.STEP.NONE)
        {
            is_vacant = true;
        }
        return (is_vacant);
    }

    public bool isSliding()
    {
        bool is_sliding = (this.position_offset.x != 0.0f);
        return (is_sliding);
    }

    public void toVanishing()
    {
        // 사라질때까지 걸리는 시간을 규정값으로 리셋
        this.vanish_timer = Block.VANISH_TIME;
    }

    public bool isVanishing()
    {
        bool is_vanishing = (this.vanish_timer > 0.0f);
        return (is_vanishing);
    }



}
