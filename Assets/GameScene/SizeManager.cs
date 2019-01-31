using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeManager : MonoBehaviour
{
    public static int mapSize = 8;
    public static int colorNum = 5;

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
            case "Size":
                mapSize = n + 8;
                Debug.Log(mapSize);
                break;
            case "ColorNum":
                colorNum = n + 5;
                Debug.Log(colorNum);
                break;
        }
    }

    public int GetSize()
    {
        return (mapSize);
    }

    public int GetColorNum()
    {
        return (colorNum);
    }

    public void ChangeSetting()
    {

    }
}
