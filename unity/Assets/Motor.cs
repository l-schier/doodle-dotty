using UnityEngine;

public class Motor : MonoBehaviour
{
    [SerializeField] private char motorAxis = 'X';
    [SerializeField] private char motorDirection = 'X';
    private Vector3 motorAxisVector;
    //[SerializeField] private float degreesPerSecond = 360f
    
    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        switch (motorAxis)
        {
            case 'X':
                motorAxisVector = Vector3.right;
                break;
            case 'Y':
                motorAxisVector = Vector3.up;
                break;
            case 'Z':
                motorAxisVector = Vector3.forward;
                break;
            default:
                Debug.LogError("Invalid motor axis");
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        if (motorDirection == 'X' && UDPManager.Instance.motorXChange) {
            RotateObject(UDPManager.Instance.motorXValue, motorAxisVector, 100);
            UDPManager.Instance.motorXChange = false;
        }
        if (motorDirection == 'Y' && UDPManager.Instance.motorYChange) {
            RotateObject(UDPManager.Instance.motorYValue, motorAxisVector, 100);
            UDPManager.Instance.motorYChange = false;
        }
    }

    private void RotateObject(float steps, Vector3 axis, float inTime)
    {
        float angle = steps / 5.625f;
        float anglePerSecond = angle / inTime;

        Quaternion startRotation = transform.rotation;

        float deltaAngle = 0;
        if (angle > 0)
        {
            while (deltaAngle < angle)
            {
                deltaAngle +=  anglePerSecond * Time.deltaTime;
                deltaAngle = Mathf.Min(deltaAngle, angle);
                transform.rotation = startRotation * Quaternion.AngleAxis(deltaAngle, axis);
            }
        } else {
            while (deltaAngle > angle)
            {
                deltaAngle +=  anglePerSecond * Time.deltaTime;
                deltaAngle = Mathf.Max(deltaAngle, angle);
                transform.rotation = startRotation * Quaternion.AngleAxis(deltaAngle, axis);
            }
        }
    }
}
