//@ts-check

import { error } from '@sveltejs/kit';
import { HubConnectionBuilder, HttpTransportType, HubConnection } from '@microsoft/signalr';
import { oldCodeStore, newCodeStore, errorMessage, successMessage } from "./store.js";

const API_URL = process.env.API_URL || "http://localhost:5195/uploadZip";

export class SignalRService {
  /** @type {SignalRService} */
  static instance;

  /** @type {HubConnection} */
  static connection;


  /**
   * Get the singleton instance of the SignalRService class.
   * @static
   * @returns {SignalRService} The singleton instance of the SignalRService class.
   */
  static getInstance() {
      if (!this.instance) {
          this.instance = new SignalRService();
      }
      return this.instance;
  }

  /**
   * Start the SignalR connection if it does not already exist.
   * @async
   */
  async startConnection() {
    if (this.connection) return console.log("Connection already exist.");

    this.connection = new HubConnectionBuilder()
                  .withUrl(API_URL, {
                    skipNegotiation: true,
                    transport: HttpTransportType.WebSockets
                  })
                  .withAutomaticReconnect([5000, 5000, 5000, 5000, 5000])
                  .build();

    this.RegisterSignalRCallbacks();

    await this.connection.start().catch(err => console.log(err.toString()));
  }

  async stopConnection() {
    if (!this.connection) return console.log("No connection found to stop.");

    await this.connection.stop().then(() => this.connection = undefined).catch(err => console.log(err.toString()));
  }

  getConnection() {
    return this.connection;
  }

  /**
   * Upload single chunk of a zip file to the API using the SignalR server.
   * @param {string} chunk - The base64 encoded chunk of the zip file.
   * @param {string} fileName - Name of the zip file.
   * @param {string} contentType - The type of the file that is uploaded.
   * @param {number} index - Current index of the total amount of uploaded chunks.
   * @param {number} totalChunks - Total amount of uploaded chunks.
   * @returns {Promise} Returns a string error message if there's no connection or if the connection is not active, otherwise returns nothing.
   * @throws {Error}
   */
  uploadChunk(chunk, fileName, contentType, index, totalChunks) {
    if (!this.connection) throw error(500, "No connection found");
    if (this.connection.state !== "Connected") throw error(500, "Not connected to server");
    
    console.log(`Chunk ${index} send to API`);
    return this.connection.invoke('UploadChunk', fileName, contentType, chunk, index, totalChunks);
  }

  RegisterSignalRCallbacks() {
    if (!this.connection) throw error(500, "No connection found");

    this.connection.on('UploadSuccess', (message) => {
      successMessage.set(message);
      console.log(`Success: ${message}`);
    });

    this.connection.on('ReceiveError', (message) => {
      errorMessage.set(message);
      console.log(`ServerError: ${message}`);
    });

    this.connection.on("ReturnLLMResponse", (fileName, contentType, fileContent, oldFileContent) => {
      oldCodeStore.update((value) => {
        if (value) return [...value, { fileName: fileName, code: oldFileContent }];
        return [{ fileName: fileName, code: oldFileContent }];
      });
      newCodeStore.update((value) => {
        if (value) return [...value, { fileName: fileName, code: fileContent }];
        return [{ fileName: fileName, code: oldFileContent }];
      });
    });
  }
}
