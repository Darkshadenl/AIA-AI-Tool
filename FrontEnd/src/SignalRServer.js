//@ts-check

import { error } from '@sveltejs/kit';
import { HubConnectionBuilder, HttpTransportType, HubConnection, HubConnectionState } from '@microsoft/signalr';

const API_URL = "http://localhost:5195/uploadZip";

/** @type {HubConnection} */
let connection;

/**
 * Start the SignalR connection if it does not already exist.
 * @async
 * @returns {HubConnection} - The current connection to the SignalR server.
 */
export async function getConnection() {
  if (!connection) {
    console.log("No connection found. Creating new connection...")
    connection = new HubConnectionBuilder()
      .withUrl(API_URL, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([5000, 5000, 5000, 5000, 5000])
      .build();
  }

  if (connection && connection.state === HubConnectionState.Disconnected) {
    await connection.start().catch(err => console.log(err.toString()));
  }

  return connection;
}

/**
 * Upload single chunk of a zip file to the API using the SignalR server.
 * @param {string} connectionId - The id of the connection with the SignalR server
 * @param {string} chunk - The base64 encoded chunk of the zip file.
 * @param {string} fileName - Name of the zip file.
 * @param {string} contentType - The type of the file that is uploaded.
 * @param {number} index - Current index of the total amount of uploaded chunks.
 * @param {number} totalChunks - Total amount of uploaded chunks.
 * @returns {Promise} Returns a string error message if there's no connection or if the connection is not active, otherwise returns nothing.
 * @throws {Error}
 */
export function uploadChunk(connectionId, chunk, fileName, contentType, index, totalChunks) {
  if (!connection) throw error(503, "No SignalR connection found");
  if (connection.state !== HubConnectionState.Connected) throw error(503, "Not connected to SignalR server");

  console.log(`Chunk ${index} send to API`);
  return connection.invoke('UploadChunk', connectionId, fileName, contentType, chunk, index, totalChunks);
}
