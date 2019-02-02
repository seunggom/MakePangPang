﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRoot : MonoBehaviour
{
    public GameObject BlockPrefab = null;
    public BlockControl[,] blocks;
    private GameObject main_camera = null; // 메인카메라
    private BlockControl grabbed_block = null; // 잡은 블록

    private ScoreCounter score_counter = null; // 점수 카운터 ScoreCounter.
    protected bool is_vanishing_prev = false; // 앞에서 발화했는가?.


    // Start is called before the first frame update
    void Start()
    {
        this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        this.score_counter = this.gameObject.GetComponent<ScoreCounter>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouse_position;
        this.unprojectMousePosition(out mouse_position, Input.mousePosition);

        if (this.grabbed_block == null)
        { // 잡은 블록이 비어있으면.
            if (!this.is_has_falling_block())
            {
                if (Input.GetMouseButtonDown(0))
                { // 마우스 버튼이 눌렸다면.
                  // blocks 배열의 모든 요소를 차례로 처리한다.
                    foreach (BlockControl block in this.blocks)
                    {
                        if (!block.isGrabbable())
                        { // 블록을 잡을 수 없다면.
                            continue; // 루프의 맨 앞으로 점프.
                        }
                        // 마우스 위치가 블록 영역 안에 없으면.
                        if (!block.isContainedPosition(mouse_position))
                        {
                            continue; // 루프의 맨 앞으로 점프.
                        }
                        // 처리 중인 블록을 grabbed_block에 등록.
                        this.grabbed_block = block;
                        // 잡았을 때의 처리를 실행.
                        this.grabbed_block.beginGrab();
                        break;
                    }
                }
            }
        }
        else
        { // 잡은 블록이 비어있지 않으면.
            do
            {
                // 슬라이드할 곳의 블록을 획득.
                BlockControl swap_target = this.getNextBlock(grabbed_block, grabbed_block.slide_dir);
                // 슬라이드할 곳의 블록이 비었으면.
                if (swap_target == null)
                {
                    break; // 루프 탈출.
                }
                // 슬라이드할 곳의 블록이 잡을 수 있는 상태가 아니면.
                if (!swap_target.isGrabbable())
                {
                    break; // 루프 탈출.
                }
                // 현재 위치에서 슬라이드할 곳까지의 거리를 구한다.
                float offset = this.grabbed_block.calcDirOffset(mouse_position, this.grabbed_block.slide_dir);
                // 이동 거리가 블록 크기의 절반보다 작으면.
                if (offset < Block.COLLISION_SIZE / 2.0f)
                {
                    break; // 루프 탈출.
                }
                // 블록을 교체한다.
                this.swapBlock(grabbed_block, grabbed_block.slide_dir, swap_target);
                this.grabbed_block = null; // 지금은 블록을 잡고 있지 않다.
            } while (false);


            if (!Input.GetMouseButton(0))
            { // 마우스 버튼이 눌려있지 않으면.
                this.grabbed_block.endGrab(); // 블록을 놓았을 때 처리를 실행.
                this.grabbed_block = null; // grabbed_block를 비어 있게 설정.
            }
        }

        
        // 낙하 중 또는 슬라이드 중이면.
        if (this.is_has_falling_block() || this.is_has_sliding_block())
        {
            // 아무것도 하지 않는다.
            // 낙하 중도 슬라이드 중도 아니면.
        }
        else
        {
                                  // 그리드 안의 모든 블록에 대해서 처리.
            foreach (BlockControl block in this.blocks)
            {
                if (!block.isIdle())
                { // 대기 중이면 루프의 처음으로 점프하고,.
                    continue; // 다음 블록을 처리한다.
                }
                // 세로 또는 가로에 같은 색 블록이 세 개 이상 나열했다면.
                if (this.checkConnection(block))
                {

                }

            }
            
        }

        // 하나라도 연소 중인 블록이 있는가?.
        bool is_vanishing = this.is_has_vanishing_block();
        // 조건이 충족되면 블록을 떨어뜨리고 싶다.
        do
        {
            if (is_vanishing)
            { // 연소 중인 블록이 있으면.
                break; // 낙하 처리를 하지 않는다.
            }
            if (this.is_has_sliding_block())
            { // 교체 중인 블록이 있으면.
                break; // 낙하 처리는 하지 않는다.
            }
            for (int x = 0; x < Block.BLOCK_NUM_X; x++)
            {
                // 열에 교체 중인 블록이 있으면 그 열은 처리하지 않고 다음 열로 넘어간다.
                if (this.is_has_sliding_block_in_column(x))
                {
                    continue;
                }
                // 그 열에 있는 블록을 위에서부터 검사.
                for (int y = 0; y < Block.BLOCK_NUM_Y - 1; y++)
                {
                    // 지정 중인 블록이 비표시면 다음 블록으로.
                    if (!this.blocks[x, y].isVacant())
                    {
                        continue;
                    }
                    // 지정 중인 블록 아래에 있는 블록을 검사.
                    for (int y1 = y + 1; y1 < Block.BLOCK_NUM_Y; y1++)
                    {
                        // 아래에 있는 블록이 비표시면 다음 블록으로.
                        if (this.blocks[x, y1].isVacant())
                        {
                            continue;
                        }
                        // 블록을 교체한다.
                        this.fallBlock(this.blocks[x, y], Block.DIR4.UP,
                                       this.blocks[x, y1]);
                        break;
                    }
                }
            }
            // 보충 처리.
            for (int x = 0; x < Block.BLOCK_NUM_X; x++)
            {
                int fall_start_y = Block.BLOCK_NUM_Y;
                for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
                {
                    // 비표시 블록이 아니면 다음 블록으로.
                    if (!this.blocks[x, y].isVacant())
                    {
                        continue;
                    }
                    this.blocks[x, y].beginRespawn(fall_start_y); // 블록 부활.
                    fall_start_y++;
                }
            }
        } while (false);
        this.is_vanishing_prev = is_vanishing;
    }

    public void InitialSetUp()
    {
        this.blocks = new BlockControl[Block.BLOCK_NUM_X, Block.BLOCK_NUM_Y];
        int color_index = 0;

        for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
        {
            for (int x = 0; x < Block.BLOCK_NUM_X; x++)
            {
                // BlockPrefab의 인스턴스를 씬에 만든다
                GameObject game_object = Instantiate(this.BlockPrefab) as GameObject;
                BlockControl block = game_object.GetComponent<BlockControl>();
                this.blocks[x, y] = block;

                block.i_pos.x = x;
                block.i_pos.y = y;
                block.block_root = this;

                Vector2 position = BlockRoot.calcBlockPosition(block.i_pos);
                block.transform.position = position;
                block.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                block.SetColor((Block.COLOR)color_index);
                block.name = "block(" + block.i_pos.x.ToString() + ", " + block.i_pos.y.ToString() + ")";

                color_index = Random.Range(0, SizeManager.colorNum);
            }
        }
    }

    public static Vector2 calcBlockPosition(Block.iPosition i_pos)
    {
        Vector2 position = new Vector2(-(Block.BLOCK_NUM_X * 0.7f / 2.0f - 0.35f), -(Block.BLOCK_NUM_Y * 0.7f / 2.0f - 0.35f) + 1.0f);

        position.x += (float)i_pos.x * Block.COLLISION_SIZE;
        position.y += (float)i_pos.y * Block.COLLISION_SIZE;

        return (position);
    }

    public bool unprojectMousePosition(out Vector2 world_position, Vector3 mouse_position)
    {
        bool ret;

        // 판을 작성한다. 이 판은 카메라에 대해서 뒤를 향해서 블록의 절반 크기만큼 앞에 둔다
        Plane plane = new Plane(Vector3.back, new Vector3(0.0f, 0.0f, -Block.COLLISION_SIZE / 2.0f));
        // 카메라와 마우스를 통과하는 빛을 만든다
        Ray ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

        float depth;

        // 광선이 판에 닿았다면,
        if (plane.Raycast(ray, out depth))
        {
            world_position = ray.origin + ray.direction * depth;
            ret = true;
        }
        else // 닿지 않았다면,
        {
            world_position = Vector2.zero;
            ret = false;
        }
        return (ret);
    }

    public BlockControl getNextBlock(BlockControl block, Block.DIR4 dir)
    {
        BlockControl next_block = null; // 슬라이드할 곳의 블록을 여기에 저장

        switch (dir)
        {
            case Block.DIR4.RIGHT:
                if (block.i_pos.x < Block.BLOCK_NUM_X - 1)
                {
                    next_block = this.blocks[block.i_pos.x + 1, block.i_pos.y];
                }
                break;

            case Block.DIR4.LEFT:
                if (block.i_pos.x > 0)
                {
                    next_block = this.blocks[block.i_pos.x - 1, block.i_pos.y];
                }
                break;

            case Block.DIR4.UP:
                if (block.i_pos.y < Block.BLOCK_NUM_Y - 1)
                {
                    next_block = this.blocks[block.i_pos.x, block.i_pos.y + 1];
                }
                break;

            case Block.DIR4.DOWN:
                if (block.i_pos.y > 0)
                {
                    next_block = this.blocks[block.i_pos.x, block.i_pos.y - 1];
                }
                break;
        }
        return (next_block);
    }

    public static Vector3 getDirVector(Block.DIR4 dir)
    {
        Vector3 v = Vector3.zero;

        switch (dir)
        {
            case Block.DIR4.RIGHT:
                v = Vector3.right;
                break;
            case Block.DIR4.LEFT:
                v = Vector3.left;
                break;
            case Block.DIR4.UP:
                v = Vector3.up;
                break;
            case Block.DIR4.DOWN:
                v = Vector3.down;
                break;
        }
        v *= Block.COLLISION_SIZE;
        return (v);
    }

    public static Block.DIR4 getOppositDir(Block.DIR4 dir)
    {
        Block.DIR4 opposit = dir;
        switch (dir)
        {
            case Block.DIR4.RIGHT:
                opposit = Block.DIR4.LEFT;
                break;
            case Block.DIR4.LEFT:
                opposit = Block.DIR4.RIGHT;
                break;
            case Block.DIR4.UP:
                opposit = Block.DIR4.DOWN;
                break;
            case Block.DIR4.DOWN:
                opposit = Block.DIR4.UP;
                break;
        }
        return (opposit);
    }

    public void swapBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
    {
        // 각각의 블록 색을 기억해둔다
        Block.COLOR color0 = block0.color;
        Block.COLOR color1 = block1.color;

        // 각각의 블록의 확대율을 기억해둔다
        Vector3 scale0 = block0.transform.localScale;
        Vector3 scale1 = block1.transform.localScale;

        // 각각의 블록의 사라지는 시간을 기억해둔다
        float vanish_timer0 = block0.vanish_timer;
        float vanish_timer1 = block1.vanish_timer;

        // 각각의 블록의 이동할 곳을 구한다
        Vector3 offset0 = BlockRoot.getDirVector(dir);
        Vector3 offset1 = BlockRoot.getDirVector(BlockRoot.getOppositDir(dir));

        // 색을 교체한다
        block0.SetColor(color1);
        block1.SetColor(color0);

        // 확대율 교체
        block0.transform.localScale = scale1;
        block1.transform.localScale = scale0;

        // 사라지는 시간 교체
        block0.vanish_timer = vanish_timer1;
        block1.vanish_timer = vanish_timer0;

        block0.beginSlide(offset0); // 원래 블록 이동 시작
        block1.beginSlide(offset1); // 이동할 위치의 블록 이동을 시작
    }

    public bool checkConnection(BlockControl start)
    {
        bool ret = false;
        int normal_block_num = 0;
        // 인수인 블록이 착화 후가 아니면.
        if (!start.isVanishing())
        {
            normal_block_num = 1;
        }
        // 그리드 좌표를 기억해 둔다.
        int rx = start.i_pos.x;
        int lx = start.i_pos.x;
        // 블록의 왼쪽을 검사.
        for (int x = lx - 1; x > 0; x--)
        {
            BlockControl next_block = this.blocks[x, start.i_pos.y];
            if (next_block.color != start.color)
            { // 색이 다르면.
                break; // 루프를 빠져나간다.
            }
            if (next_block.step == Block.STEP.fall || // 낙하 중이면.
               next_block.next_step == Block.STEP.fall)
            {
                break; // 루프를 빠져나간다.
            }
            if (next_block.step == Block.STEP.slide || // 슬라이드 중이면.
               next_block.next_step == Block.STEP.slide)
            {
                break; // 루프를 빠져나간다.
            }
            if (!next_block.isVanishing())
            { // 발화 중이 아니면.
                normal_block_num++; // 검사용 카운터를 증가.
            }
            lx = x;
        }
        // 블록의 오른쪽을 검사.
        for (int x = rx + 1; x < Block.BLOCK_NUM_X; x++)
        {
            BlockControl next_block = this.blocks[x, start.i_pos.y];
            if (next_block.color != start.color)
            {
                break;
            }
            if (next_block.step == Block.STEP.fall ||
               next_block.next_step == Block.STEP.fall)
            {
                break;
            }
            if (next_block.step == Block.STEP.slide ||
               next_block.next_step == Block.STEP.slide)
            {
                break;
            }
            if (!next_block.isVanishing())
            {
                normal_block_num++;
            }
            rx = x;
        }
        do
        {
            // 오른쪽 블록의 그리드 번호 - 왼쪽 블록의 그리드 번호 +.
            // 중앙 블록(1)을 더한 수가 3미만 이면.
            if (rx - lx + 1 < 3)
            {
                break; // 루프 탈출.
            }
            if (normal_block_num == 0)
            { // 착화 중이 아닌 블록이 하나도 없으면.
                break; // 루프 탈출.
            }
            for (int x = lx; x < rx + 1; x++)
            {
                // 나열된 같은 색 블록을 착화 상태로.
                this.blocks[x, start.i_pos.y].toVanishing();
                score_counter.PlusCount();
                ret = true;
            }
        } while (false);
        normal_block_num = 0;
        if (!start.isVanishing())
        {
            normal_block_num = 1;
        }
        int uy = start.i_pos.y;
        int dy = start.i_pos.y;
        // 블록의 위쪽을 검사.
        for (int y = dy - 1; y > 0; y--)
        {
            BlockControl next_block = this.blocks[start.i_pos.x, y];
            if (next_block.color != start.color)
            {
                break;
            }
            if (next_block.step == Block.STEP.fall || next_block.next_step == Block.STEP.fall)
            {
                break;
            }
            if (next_block.step == Block.STEP.slide || next_block.next_step == Block.STEP.slide)
            {
                break;
            }
            if (!next_block.isVanishing())
            {
                normal_block_num++;
            }
            dy = y;
        }
        // 블록의 아래쪽을 검사.
        for (int y = uy + 1; y < Block.BLOCK_NUM_Y; y++)
        {
            BlockControl next_block = this.blocks[start.i_pos.x, y];
            if (next_block.color != start.color)
            {
                break;
            }
            if (next_block.step == Block.STEP.fall || next_block.next_step == Block.STEP.fall)
            {
                break;
            }
            if (next_block.step == Block.STEP.slide || next_block.next_step == Block.STEP.slide)
            {
                break;
            }
            if (!next_block.isVanishing())
            {
                normal_block_num++;
            }
            uy = y;
        }
        do
        {
            if (uy - dy + 1 < 3)
            {
                break;
            }
            if (normal_block_num == 0)
            {
                break;
            }
            for (int y = dy; y < uy + 1; y++)
            {
                this.blocks[start.i_pos.x, y].toVanishing();
                score_counter.PlusCount();
                ret = true;
            }
        } while (false);
        return (ret);
    }

    private bool is_has_vanishing_block()
    {
        bool ret = false;
        foreach (BlockControl block in this.blocks)
        {
            if (block.vanish_timer > 0.0f)
            {
                ret = true;
                break;
            }
        }
        return (ret);
    }

    private bool is_has_sliding_block()
    {
        bool ret = false;
        foreach (BlockControl block in this.blocks)
        {
            if (block.step == Block.STEP.slide)
            {
                ret = true;
                break;
            }
        }
        return (ret);
    }

    private bool is_has_falling_block()
    {
        bool ret = false;
        foreach (BlockControl block in this.blocks)
        {
            if (block.step == Block.STEP.fall)
            {
                ret = true;
                break;
            }
        }
        return (ret);
    }

    public void fallBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
    {
        // block0과 block1의 색, 크기, 사라질 때까지 걸리는 시간, 표시, 비표시, 상태를 기록.
        Block.COLOR color0 = block0.color;
        Block.COLOR color1 = block1.color;
        Vector3 scale0 = block0.transform.localScale;
        Vector3 scale1 = block1.transform.localScale;
        float vanish_timer0 = block0.vanish_timer;
        float vanish_timer1 = block1.vanish_timer;
        bool visible0 = block0.isVisible();
        bool visible1 = block1.isVisible();
        Block.STEP step0 = block0.step;
        Block.STEP step1 = block1.step;
        // block0과 block1의 각종 속성을 교체한다.
        block0.SetColor(color1);
        block1.SetColor(color0);
        block0.transform.localScale = scale1;
        block1.transform.localScale = scale0;
        block0.vanish_timer = vanish_timer1;
        block1.vanish_timer = vanish_timer0;
        block0.setVisible(visible1);
        block1.setVisible(visible0);
        block0.step = step1;
        block1.step = step0;
        block0.beginFall(block1);
    }

    private bool is_has_sliding_block_in_column(int x)
    {
        bool ret = false;
        for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
        {
            if (this.blocks[x, y].isSliding())
            { // 슬라이드 중인 블록이 있으면.
                ret = true; // true를 반환한다. 
                break;
            }
        }
        return (ret);
    }
}

