using System.Collections;
using UnityEngine;
using NetworkAPI; // Ensure your NetworkComm class is within this namespace

public class MoveCubes : MonoBehaviour
{
    NetworkComm networkComm;

    // Keep track of this client's ID and the remote client's ID
    private int localClientId = -1;
    private int remoteClientId = -1;

    public GameObject localCube, remoteCube;
    public Vector3 localCubePos = new Vector3(4.0f, 1.0f, -0.5f);
    public Vector3 remoteCubePos = new Vector3(-4.0f, 1.0f, -0.5f);

    void Start()
    {
        networkComm = new NetworkComm();
        networkComm.MsgReceived += ProcessMsg;

        StartCoroutine(ConnectToWebSocketServer());

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
        UpdateCubePositions();
    }

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

    private Coroutine moveCoroutine; // To keep track of the coroutine

    void ProcessMsg(string message)
    {
        Debug.Log($"Message Received: {message}"); // Check if messages are being received

        string[] msgParts = message.Split(';');
        if (msgParts[0] == "SETID")
        {
            // ... ID setting logic ...
        }
        else if (msgParts[0] == "UPDATE")
        {
            Debug.Log("UPDATE message received."); // Confirm UPDATE messages are received

            int senderId;
            if (int.TryParse(msgParts[1], out senderId))
            {
                Debug.Log($"Parsed sender ID: {senderId}"); // Check if sender ID is parsed

                if (senderId == remoteClientId)
                {
                    string[] coords = msgParts[2].Split(',');
                    if (coords.Length == 3)
                    {
                        float x, y, z;
                        if (float.TryParse(coords[0], out x) && float.TryParse(coords[1], out y) && float.TryParse(coords[2], out z))
                        {
                            Debug.Log($"Parsed coordinates: {x}, {y}, {z}"); // Check if coordinates are parsed

                            remoteCubePos = new Vector3(x, y, z);
                            if (remoteCube != null)
                            {
                                Debug.Log("Attempting to start coroutine."); // Check before starting the coroutine
                                moveCoroutine = StartCoroutine(MoveRemoteCubeToPosition(remoteCubePos));
                            }
                            else
                            {
                                Debug.LogError("remoteCube reference is null.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to parse coordinates.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Incorrect number of coordinates in message.");
                    }
                }
                else if (remoteClientId == -1 && senderId != localClientId)
                {
                    remoteClientId = senderId;
                    Debug.Log($"Remote client ID set to {remoteClientId}");
                }
                else
                {
                    Debug.Log("Message sender ID does not match remote client ID and is not a new ID.");
                }
            }
            else
            {
                Debug.LogError("Failed to parse sender ID.");
            }
        }
        else
        {
            Debug.LogError("Unrecognized message type.");
        }
    }


    IEnumerator MoveRemoteCubeToPosition(Vector3 targetPosition)
    {
        Debug.Log("Moving remote cube to new position."); // For debugging

        float timeToMove = 0.5f; // Duration of the movement in seconds
        float elapsedTime = 0f;

        while (elapsedTime < timeToMove)
        {
            remoteCube.transform.position = Vector3.Lerp(remoteCube.transform.position, targetPosition, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        remoteCube.transform.position = targetPosition; // Ensure the target position is set at the end
    }
}

