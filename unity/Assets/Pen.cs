using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pen : MonoBehaviour
{
    [SerializeField] private char penAxis = 'X';
    [SerializeField] private int restingPenPosition = 0;
    // Start is called before the first frame update

    [SerializeField] private float minInputValue = 0f;
    [SerializeField] private float maxInputValue = 180f;
    [SerializeField] private float minRotationDegree = 0f;
    [SerializeField] private float maxRotationDegree = 180f;


    private float initialRotationX;
    private float initialRotationY;
    private float initialRotationZ;

    private void Awake()
    {
        initialRotationX = transform.localEulerAngles.x;
        initialRotationY = transform.localEulerAngles.y;
        initialRotationZ = transform.localEulerAngles.z;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RotateObject(UDPManager.Instance.penAngle);
    }

    private void RotateObject(float value) {
        float mappedRotation = MapValue(value, minInputValue, maxInputValue, minRotationDegree, maxRotationDegree);
        Vector3 rotationVector = new Vector3(initialRotationX, initialRotationY, initialRotationZ);
        switch (penAxis) {
            case 'X':
                rotationVector = new Vector3(mappedRotation, initialRotationY, initialRotationZ);
                break;
            case 'Y':
                rotationVector = new Vector3(initialRotationX, mappedRotation, initialRotationZ);
                break;
            case 'Z':
                rotationVector = new Vector3(initialRotationX, initialRotationY, mappedRotation);
                break;
            default:
                Debug.LogError("Invalid pen axis");
                break;
        }
        
        transform.localRotation = Quaternion.Euler(rotationVector);
    }

    private float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
    }
}
