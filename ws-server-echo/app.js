'use strict';

const
  WebSocket     = require("ws"),
  PORT          = 8080,
  PING_INTERVAL = 30000,
  DATA_INTERVAL = 5000;

const server = new WebSocket.Server({ port: PORT });

const getUnixTs = () =>
{
  return Math.round((new Date()).getTime() / 1000);
}

const onMessage = (ws, data) =>
{
  console.log(`received: ${data}`);
  ws.send(`echo: ${data}`); // sending back
}

const onConnection = ws =>
{
  ws.on("message", data => { onMessage(ws, data); });
  setInterval(() => { ws.ping(); }, PING_INTERVAL); // establish ping
  setInterval(() => { ws.send(getUnixTs()); }, DATA_INTERVAL); // sending some data
}

server.on("connection", onConnection);
console.log(`Started listening on port ${PORT} ...`);

