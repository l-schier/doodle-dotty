using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPManager : MonoBehaviour
{
    // Static variable that holds the instance
    public static UDPManager Instance { get; private set; }

    // UDP Settings
    [Header("UDP Settings")]
    [SerializeField] private int UDPPort = 50195;
    [SerializeField] private bool displayUDPMessages = false;
    [SerializeField] private string ipAddress = "10.126.128.155";
    [SerializeField] private int speed = 15;
    [SerializeField] private int stepsPerPress = 100;
    private UdpClient udpClient;
    private IPEndPoint endPoint;

    // ESP32 Sensor
    public int motorXValue { get; private set; } = 0; 
    public bool motorXChange { get; set; } = false;
    public int motorYValue { get; private set; } = 0;
    public bool motorYChange { get; set; } = false;

    public bool motorXConfigured { get; set; } = false;
    public bool motorYConfigured { get; set; } = false;

    private int penState { get; set; } = 0;

    public float penAngle { get; private set; } = 0;

    private int trueMotorXValue = 0;
    private int trueMotorYValue = 0;

    public bool debug;

    void Awake()
    {
        // Assign the instance to this instance, if it is the first one
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        if(debug){
            return;
        }
        //Get IP Address
        DisplayIPAddress();

        // UDP begin
        endPoint = new IPEndPoint(IPAddress.Any, UDPPort);
        udpClient = new UdpClient(endPoint);
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    // Update is called once per frame
    void Update()
    {

        if(debug){
            onlyUnity();
            return;
        }

        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && trueMotorYValue + stepsPerPress <=5200)
        {
            Debug.Log("trueMotorYValue: " + trueMotorYValue);
            String message = "Y|" + stepsPerPress.ToString() + "|" + speed.ToString();
            SendUDPMessage(message, ipAddress, 3002);
        }

        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && trueMotorYValue - stepsPerPress >=0)
        {   
            Debug.Log("trueMotorYValue: " + trueMotorYValue);
            String message = "Y|-" + stepsPerPress.ToString() + "|" + speed.ToString();
            SendUDPMessage(message, ipAddress, 3002);
        }

        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && trueMotorXValue - stepsPerPress >=0)
        {
            Debug.Log("Left trueMotorXValue: " + trueMotorXValue);
            String message = "X|" + stepsPerPress.ToString() + "|" + speed.ToString();
            SendUDPMessage(message, ipAddress, 3002);
        }

        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && trueMotorXValue + stepsPerPress <=5200)
        {
            Debug.Log(" Right trueMotorXValue: " + trueMotorXValue);
            String message = "X|-" + stepsPerPress.ToString() + "|" + speed.ToString();
            SendUDPMessage(message, ipAddress, 3002);
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            String message = "CALIBRATE|1";
            SendUDPMessage(message, ipAddress, 3002);
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            string message = "";
            if (penState == 0) {
                penState = 1;
                message = "PEN|1";
            } else {
                penState = 0;
                message = "PEN|0";
                
            }
            SendUDPMessage(message, ipAddress, 3002);
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        byte[] receivedBytes = udpClient.EndReceive(result, ref endPoint);
        string receivedData = Encoding.UTF8.GetString(receivedBytes);

        // Log UDP message
        if (displayUDPMessages)
        {
            Debug.Log("Received data from " + endPoint.Address.ToString() + ": " + receivedData);
        }

        // Splitting the receivedData string by the '|' character
        string[] parts = receivedData.Split('|');

        string ID = parts[0];
        int value;
        if (int.TryParse(parts[1], out value))
        {
            if (ID == "X0") {
                motorXValue = 0;
                trueMotorXValue = 0;
                motorXChange = true;
                motorXConfigured = true;
                Debug.Log("Calibrated Motor X");
            } else if (ID == "Y0") {
                motorYValue = 0;
                trueMotorYValue = 0;
                motorYChange = true;
                motorYConfigured = true;
                Debug.Log("Calibrated Motor Y");
            } else if (ID == "X") {
                motorXValue = value;
                trueMotorXValue += -value;
                motorXChange = true;
                Debug.Log("Motor " + parts[0] +  " Value: " + motorXValue);
            } else if (ID == "Y") {
                motorYValue = value;
                trueMotorYValue += value;
                motorYChange = true;
                Debug.Log("Motor " + parts[0] +  " Value: " + motorYValue);
            } else if (ID == "PEN") {
                if(value == 1) {
                    penState = 1;
                    penAngle = 30;
                } else {
                    penState = 0;
                    penAngle = 0;
                }
                Debug.Log("Pen state: " + value + " resulting in pen angle: " + penAngle);
            } else {
                Debug.Log("Did not recognize message with ID: " + parts[0] +  " and Value: " + motorXValue);
            }
        }
        else
        {
            Debug.LogError("Failed to parse the value as an integer.");
        }
        
        Debug.Log("Beginning to receive again.");
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    // Function to send UDP message
    public void SendUDPMessage(string message, string ipAddress, int port)
    {
        UdpClient client = new UdpClient();
        try
        {
            // Convert the message string to bytes
            byte[] data = Encoding.UTF8.GetBytes(message);

            // Send the UDP message
            client.Send(data, data.Length, ipAddress, port);
            Debug.Log("UDP message sent: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending UDP message: " + e.Message);
        }
        finally
        {
            client.Close();
        }
    }

    void DisplayIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    Debug.Log(ip.ToString());
                    break;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching local IP address: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }

    private void onlyUnity(){

        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && trueMotorYValue + stepsPerPress <=5200)
        {
            Debug.Log("trueMotorYValue: " + trueMotorYValue);
            String message = "Y|" + stepsPerPress.ToString() + "|" + speed.ToString();
         

            motorYValue = stepsPerPress;
            trueMotorYValue += stepsPerPress;
            motorYChange = true;
            Debug.Log("Calibrated Motor Y");
        }

        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && trueMotorYValue - stepsPerPress >=0)
        {   
            Debug.Log("trueMotorYValue: " + trueMotorYValue);
            String message = "Y|-" + stepsPerPress.ToString() + "|" + speed.ToString();
            
            motorYValue = -stepsPerPress;
            trueMotorYValue += -stepsPerPress;
            motorYChange = true;
            Debug.Log("Motor y" +  " Value: " + motorYValue);
        }

        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && trueMotorXValue - stepsPerPress >=0)
        {
            Debug.Log("Left trueMotorXValue: " + trueMotorXValue);
            String message = "X|" + stepsPerPress.ToString() + "|" + speed.ToString();
            
            motorXValue = -stepsPerPress;
            trueMotorXValue += -stepsPerPress;
            motorXChange = true;
            Debug.Log("Calibrated Motor X");
        }

        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && trueMotorXValue + stepsPerPress <=5200)
        {
            Debug.Log(" Right trueMotorXValue: " + trueMotorXValue);
            String message = "X|-" + stepsPerPress.ToString() + "|" + speed.ToString();
            
            motorXValue = stepsPerPress;
            trueMotorXValue += stepsPerPress;
            motorXChange = true;
            Debug.Log("Motor X" +  " Value: " + motorXValue);
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            String message = "CALIBRATE|1";
            if(penAngle == 30)
            {
                penAngle = 0;
            } else
            {
                penAngle = 30;
            }
            
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            motorXConfigured = true;
            motorYConfigured = true;
            
        }

    }
}
