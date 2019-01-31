using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeManager : MonoBehaviour
{
    private int mapSize;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChange(Dropdown drop)
    {
        int n = drop.value;

        switch (drop.name)
        {
            case "MapSize":
                mapSize = n;
                break;
            case "ColorNum":
                break;
        }
    }

    public int getSize()
    {
        return (mapSize);
    }
}
