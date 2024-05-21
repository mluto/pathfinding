using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private int maxZoom = 5;
    [SerializeField] private int minZoom = 100;
    private int maxH;
    private int maxW;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.x > 0)
        {
            transform.position += new Vector3(-1, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && transform.position.x < maxW)
        {
            transform.position += new Vector3(1, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && transform.position.z < maxH)
        {
            transform.position += new Vector3(0, 0, 1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && transform.position.z > 0)
        {
            transform.position += new Vector3(0, 0, -1);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            if ((transform.position.y > maxZoom && Input.mouseScrollDelta.y > 0)
                || (transform.position.y < minZoom && Input.mouseScrollDelta.y < 0))
            {
                transform.position -= new Vector3(0, Input.mouseScrollDelta.y, 0);
            }
        }
    }

    public void SetMax(int width, int height)
    {
        maxH = height;
        maxW = width;
    }
}
