﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneControl : MonoBehaviour
{
    private BlockRoot block_root = null;

    // Start is called before the first frame update
    void Start()
    {
        // BlockRoot스크립트 가져오기.
        this.block_root = this.gameObject.GetComponent<BlockRoot>();
        // BlockRoot스크립트의 initialSetUp()을 호출한다.
        this.block_root.InitialSetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
