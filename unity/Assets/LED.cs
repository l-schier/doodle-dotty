using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LED : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UDPManager.Instance.SendUDPMessage("LED|1", "10.126.128.155", 3002);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            UDPManager.Instance.SendUDPMessage("LED|0", "10.126.128.155", 3002);
        }
    }
}
