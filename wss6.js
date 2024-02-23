const WebSocket = require("ws");
const wss = new WebSocket.Server({ port: 3000 });

let clients = {}; // Use an object to store clients by ID

// Define these outside the setInterval so their values are maintained across calls
let x = 0;
let y = 1; // Keep y constant
let z = 0;

wss.on("connection", function connection(ws) {
  let currentClientId = Object.keys(clients).length + 1;
  console.log(`Client ${currentClientId} connected`);
  clients[currentClientId] = ws;

  // ws.send(`SETID;${currentClientId}`);

  ws.on("message", function incoming(message) {
    console.log(`Received from client ${currentClientId}: ${message}`);
    // You can handle messages from the client here if needed
  });

  ws.on("close", () => {
    console.log(`Client ${currentClientId} disconnected`);
    delete clients[currentClientId];
  });
});

// Assuming you want to simulate the movement for the remote cube
setInterval(() => {
  // Simulate as if it's for client 2
  let simulatedClientId = 2;

  // Increment x and z coordinates
  x += 1;
  z += 1;

  // Create the message
  let message = `ID=2;${x.toFixed(2)},${y.toFixed(2)},${z.toFixed(2)}`;

  // Send the message to the simulated remote cube
  // Note: This will send the message to all connected clients
  // You would need to add logic to send it to a specific client if you have more than one client connected
  Object.values(clients).forEach((client) => {
    if (client.readyState === WebSocket.OPEN) {
      console.log(`Sending to simulated remote cube (ID 2): ${message}`);
      client.send(message);
    }
  });
}, 1000); // Update every 1000 milliseconds (1 second)
