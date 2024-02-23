using System.Collections;
using UnityEngine;
using NetworkAPI; // Ensure your NetworkComm class is within this namespace
using System.Threading;
using System.Globalization;

public class MoveCubes : MonoBehaviour
{
    NetworkComm networkComm;

    public GameObject localCube, remoteCube;
    public Vector3 localCubePos = new Vector3(4.0f, 1.0f, -0.5f);
    //public Vector3 remoteCubePos = new Vector3(-4.0f, 1.0f, -0.5f);

    void Start()
    {
        //(new Thread(new ThreadStart(ThreadFunction))).Start();
        networkComm = new NetworkComm();
        networkComm.MsgReceived += ProcessMsg;

        StartCoroutine(ConnectToWebSocketServer());
        remoteCube = GameObject.Find("RemoteCube");
        localCube = GameObject.Find("LocalCube");
        localCube.transform.position = localCubePos;
        //remoteCube.transform.position = remoteCubePos;
    }

    IEnumerator ConnectToWebSocketServer()
    {
        // Replace "ws://yourwebsocketserver.com" with your actual WebSocket server URI
        var connectTask = networkComm.ConnectToServer("ws://172.20.10.2:3000");
        yield return new WaitUntil(() => connectTask.IsCompleted);

        // Once connected, start receiving messages
        StartCoroutine(StartReceivingMessages());
    }

    private bool hasNewPosition = false;
    private Vector3 pendingRemoteCubePosition;
    public float speed = 5.0f; // You can adjust this value as needed


    IEnumerator StartReceivingMessages()
    {
        var receiveTask = networkComm.ReceiveMessages();
        yield return new WaitUntil(() => receiveTask.IsCompleted);
    }
    void Update()
    {
        HandleInput();
        //MoveRemoteCube();
        //UpdateCubePositions();

        // Smoothly move the remote cube towards the new position
        if (hasNewPosition)
        {
            remoteCube.transform.position = Vector3.MoveTowards(remoteCube.transform.position, pendingRemoteCubePosition, speed * Time.deltaTime);
            if (remoteCube.transform.position == pendingRemoteCubePosition)
            {
                // Once the cube has reached the target position, reset the flag
                hasNewPosition = false;
            }
        }
    }
    //void MoveRemoteCube()
    //{
    //    // This will increment the position of the remote cube every frame
    //    remoteCubePos.x += 0.1f * Time.deltaTime; // Multiply by Time.deltaTime for frame-independent movement
    //    remoteCubePos.z += 0.1f * Time.deltaTime;
    //    remoteCube.transform.position = remoteCubePos;
    //}

    void HandleInput()
    {
        if (Input.anyKey)
        {
            bool positionChanged = false;
            Vector3 previousPosition = localCubePos;

            if (Input.GetKey(KeyCode.RightArrow)) { localCubePos.x += 0.05f; positionChanged = true; }
            if (Input.GetKey(KeyCode.LeftArrow)) { localCubePos.x -= 0.05f; positionChanged = true; }
            if (Input.GetKey(KeyCode.UpArrow)) { localCubePos.z += 0.05f; positionChanged = true; }
            if (Input.GetKey(KeyCode.DownArrow)) { localCubePos.z -= 0.05f; positionChanged = true; }

            if (positionChanged)
            {
                localCube.transform.position = localCubePos;

                // Serialize the position to a string format "POS;x,y,z"
                string message = $"ID=3;{localCubePos.x.ToString()},{localCubePos.y.ToString()},{localCubePos.z.ToString()}";

                // Send the updated position to the server
                networkComm.SendMessage(message);

                // Optionally, revert the cube's position if the message fails to send
                // localCubePos = previousPosition;
            }
        }
    }
    void ProcessMsg(string message)
    {
        Debug.Log("Received message: " + message); // Confirm the message is received
        string[] msgParts = message.Split(';');

        if (msgParts[0] == "ID=2") // Make sure the ID matches your remote cube
        {
            string[] coords = msgParts[1].Split(',');
            float x = float.Parse(coords[0], CultureInfo.InvariantCulture);
            float y = float.Parse(coords[1], CultureInfo.InvariantCulture);
            float z = float.Parse(coords[2], CultureInfo.InvariantCulture);

            // Store the received position
            pendingRemoteCubePosition = new Vector3(x, y, z);
            hasNewPosition = true; // Indicate that there's a new target position

        }
    }
}