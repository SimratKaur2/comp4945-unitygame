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
    public Vector3 remoteCubePos = new Vector3(-4.0f, 1.0f, -0.5f);

    void Start()
    {
        //(new Thread(new ThreadStart(ThreadFunction))).Start();
        networkComm = new NetworkComm();
        networkComm.MsgReceived += ProcessMsg;

        StartCoroutine(ConnectToWebSocketServer());
        remoteCube = GameObject.Find("RemoteCube");
        localCube = GameObject.Find("LocalCube");
        localCube.transform.position = localCubePos;
        remoteCube.transform.position = remoteCubePos;
    }

    IEnumerator ConnectToWebSocketServer()
    {
        // Replace "ws://yourwebsocketserver.com" with your actual WebSocket server URI
        var connectTask = networkComm.ConnectToServer("ws://localhost:3000");
        yield return new WaitUntil(() => connectTask.IsCompleted);

        // Once connected, start receiving messages
        StartCoroutine(StartReceivingMessages());
    }

    IEnumerator StartReceivingMessages()
    {
        var receiveTask = networkComm.ReceiveMessages();
        yield return new WaitUntil(() => receiveTask.IsCompleted);
    }
    void Update()
    {
        HandleInput();
        //MoveRemoteCube();
        UpdateCubePositions();
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
                string message = $"POS;{localCubePos.x.ToString()},{localCubePos.y.ToString()},{localCubePos.z.ToString()}";

                // Send the updated position to the server
                networkComm.SendMessage(message);

                // Optionally, revert the cube's position if the message fails to send
                // localCubePos = previousPosition;
            }
        }
    }


    void UpdateCubePositions()
    {
        localCube.transform.position = localCubePos;
        remoteCube.transform.position = remoteCubePos;
    }


    //void ProcessMsg(string message)
    //{
    //    Debug.Log($"Message Received: {message}");

    //    // Split the received message by semicolon
    //    string[] msgParts = message.Split(';');

    //    // Handling SETID message
    //    if (msgParts[0] == "SETID")
    //    {
    //        // Parse and set the local client ID
    //        localClientId = int.Parse(msgParts[1]);
    //        Debug.Log($"Local client ID set to {localClientId}");
    //    }
    //    // Handling UPDATE message
    //    else if (msgParts[0] == "UPDATE")
    //    {
    //        // Parse the sender ID from the message
    //        int senderId = int.Parse(msgParts[1]);

    //        // Check if the sender ID is 2, indicating the message is for the remote cube
    //        if (senderId == 2) // Assuming ID 2 is for the remote cube
    //        {
    //            // Split the coordinate part of the message and parse each coordinate
    //            string[] coords = msgParts[2].Split(',');
    //            float x = float.Parse(coords[0]);
    //            float y = float.Parse(coords[1]);
    //            float z = float.Parse(coords[2]);

    //            // Update the remote cube's position on the main thread
    //            UnityMainThreadDispatcher.Instance().Enqueue(() =>
    //            {
    //                if (remoteCube != null)
    //                {
    //                    remoteCube.transform.position = new Vector3(x, y, z);
    //                }
    //            });
    //        }
    //    }
    //}

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
            remoteCube.transform.position = new Vector3(x, y, z); // Update the position

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                //remoteCube.transform.position = new Vector3(x, y, z); // Update the position
                Debug.Log($"Remote cube moved to: {remoteCube.transform.position}"); // Confirm the cube's position
                Debug.Log($"Is the cube visible? {remoteCube.GetComponent<Renderer>().isVisible}");

            });
        }
    }



    //public void ThreadFunction()
    //{
    //    float x = 1.0f, y = 1.0f, z = 1.0f;
    //    while (true)
    //    {
    //        Thread.Sleep(1000); // Simulate receiving position updates
    //        string simulatedMessage = $"ID=2;{x},{y},{z}";
    //        ProcessMsg(simulatedMessage);
    //        x += 0.1f; y += 0.1f; z += 0.1f; // Simulate changing positions
    //    }
    //}
}