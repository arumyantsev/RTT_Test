using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpinner : MonoBehaviour
{
    [SerializeField]
    private float spinSpeed = 100f;
    [SerializeField]
    private Vector3 spinDirection = Vector3.up;
    [SerializeField]
    private bool isSpinning = true;

    private void Update()
    {
        if(isSpinning)
        {
            transform.Rotate(spinDirection * spinSpeed * Time.deltaTime, Space.World);
        }
    }
}
