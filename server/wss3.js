const WebSocket = require("ws");
const wss = new WebSocket.Server({ port: 3000 });

let clientId = 0;
let clients = {};

wss.on("connection", function connection(ws) {
  const currentClientId = ++clientId;
  clients[currentClientId] = ws;
  console.log(`Client ${currentClientId} connected`);

  // Send the client ID back to the client
  ws.send(`SETID;${currentClientId}`);

  ws.on("message", function incoming(message) {
    console.log(`Received from client ${currentClientId}: ${message}`);
    // Broadcast the message to all other clients
    for (let id in clients) {
      if (clients.hasOwnProperty(id) && Number(id) !== currentClientId) {
        clients[id].send(`UPDATE;${currentClientId};${message}`);
      }
    }
  });

  ws.on("close", () => {
    console.log(`Client ${currentClientId} disconnected`);
    delete clients[currentClientId]; // Remove client from the object
  });
});
