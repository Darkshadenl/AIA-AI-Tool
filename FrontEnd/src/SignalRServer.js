//@ts-check

import { error } from '@sveltejs/kit';
import { HubConnectionBuilder, HttpTransportType, HubConnection } from '@microsoft/signalr';
import { oldCodeStore, newCodeStore, errorMessage, successMessage } from "./store.js";

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
    connection = new HubConnectionBuilder()
      .withUrl(API_URL, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([5000, 5000, 5000, 5000, 5000])
      .build();

    // RegisterSignalRCallbacks();
    await connection.start().then(() => console.log("Connected!")).catch(err => console.log(err.toString()));
  }

  return connection;
}

async function stopConnection() {
  if (!connection) return console.log("No connection found to stop.");

  await connection.stop().then(() => connection = undefined).catch(err => console.log(err.toString()));
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
  if (!connection) throw error(500, "No connection found");
  if (connection.state !== "Connected") throw error(500, "Not connected to server");

  console.log(`Chunk ${index} send to API`);
  return connection.invoke('UploadChunk', connectionId, fileName, contentType, chunk, index, totalChunks);
}

function RegisterSignalRCallbacks() {
  if (!connection) throw error(500, "No connection found");

  connection.on('UploadSuccess', (message) => {
    successMessage.set(message);
    console.log(`Success: ${message}`);
  });

  connection.on('ReceiveError', (message) => {
    errorMessage.set(message);
    console.log(`ServerError: ${message}`);
  });

  connection.on("ReturnLLMResponse", (fileName, contentType, fileContent, oldFileContent) => {
    oldCodeStore.update((value) => {
      if (value) return [...value, { fileName: fileName, code: oldFileContent }];
      return [{ fileName: fileName, code: oldFileContent }];
    });
    newCodeStore.update((value) => {
      if (value) return [...value, { fileName: fileName, code: fileContent }];
      return [{ fileName: fileName, code: fileContent }];
    });
  });
}
