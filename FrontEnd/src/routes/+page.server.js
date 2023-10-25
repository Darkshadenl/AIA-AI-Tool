//@ts-check

import { fail, error } from '@sveltejs/kit';
import { SignalRService } from '../SignalRServer.js';
import { HubConnection } from '@microsoft/signalr';
import { newCodeStore, oldCodeStore } from "../store.js";
import * as stores from '../store.js';

const FILE_SIZE_LIMIT_IN_BYTES = 1000 * 1000 * 1000; // 1GB

/**
 * Create API connection using SignalR
 * @type {import('@sveltejs/kit').Load}
 */
export const load = (async () => {
  const signalRService = SignalRService.getInstance();
  await signalRService.startConnection();
  oldCodeStore.set([]);
  newCodeStore.set([]);
});

/** @type {Object} */
export const actions = {
  /**
	 * Uploads a file to the API using the SignalR server
	 * @async
	 * @function
	 * @param {Object} params - An object containing request details
	 * @param {Request} params.request - The incoming request containing form data
	 * @returns {Promise<Object>} - A promise resolving to either a success or error response
	 */
	uploadFile: async ({ request }) => {
        resetStores();
        const signalRService = SignalRService.getInstance();

        const formData = await request.formData();
        const file = formData.get('file');
        
        if (!(file instanceof File)) return;
        if (file.size <= 0) return fail(400, { error: "No file received or file is empty." });
        if (file.size > FILE_SIZE_LIMIT_IN_BYTES) return fail(413, { error: `File size of file "${file.name}" exceeded the limit of ${FILE_SIZE_LIMIT_IN_BYTES / 1000 / 1000} MB.` });

        const fileChunks = sliceFileIntoChunks(file);
        await processFileChunks(fileChunks, file.name, file.type);
        return await showResult(signalRService);
	}
}

/**
 * Fetches the result of the file upload operation from the SignalR service
 * @async
 * @function
 * @param {SignalRService} signalRService
 * @returns {Promise<Object>} - A promise resolving to either a success or error response
 */
async function showResult(signalRService) {
  let connection = signalRService.getConnection();
  if (!connection) throw error(500, "No connection found");

  return receiveMessage(connection)
  .then((successMessage) => {
    return { success: successMessage }
  })
  .catch((errorMessage) => {
    return fail(500, { error: errorMessage });
  });
}

/**
 * Listens for success or error messages from the server
 * @function
 * @param {HubConnection} connection - The SignalR connection to the server
 * @returns {Promise} - A promise resolving with a success message or rejecting with an error message
 */
function receiveMessage(connection) {
  return new Promise((resolve, reject) => {
    connection.on('UploadSuccess', (message) => {
      console.log(`Success: ${message}`);
      resolve(message);
    });

    connection.on('ReceiveError', (message) => {
      console.log(`ServerError: ${message}`);
      reject(message);
    });
  });
}

/**
 * Slices the zip file into mutiple smaller chunks.
 * @param {File} file - Zip file to slice.
 * @returns {Blob[]} Returns a list of chunks.
 */
function sliceFileIntoChunks(file) {
  const fileChunks = [];
  const chunkSize = 1024 * 1024;
  const totalChunks = Math.ceil(file.size / chunkSize);
  console.log(`Total amount of chunks being created: ${totalChunks}`);

  for (let i = 0; i < totalChunks; i++) {
    const start = i * chunkSize;
    const end = (i + 1) * chunkSize;
    const chunk = file.slice(start, end);
    fileChunks.push(chunk);
  }
  
  return fileChunks;
}

/**
 * Upload each chunk of the zip file to the API using SignalR.
 * @async
 * @param {Blob[]} fileChunks - The list of chunks.
 * @param {string} fileName - The name of the file before it was sliced into chunks
 * @param {string} contentType - The content type of the file before it was sliced into chunks.
 */
async function processFileChunks(fileChunks, fileName, contentType) {
  const signalRService = SignalRService.getInstance();

  for (let i = 0; i < fileChunks.length; i++) {
    const byteArray = await chunkToByteArray(fileChunks[i]);
    const base64 = Buffer.from(byteArray).toString('base64');
    signalRService.uploadChunk(base64, fileName, contentType, i, fileChunks.length).catch((err) => {
      console.error(err);
    });
  }
}

/**
 * Convert chunk of the zip file into a ByteArray (Uint8Array).
 * @async
 * @param {Blob} chunk - The chunk to convert.
 */
async function chunkToByteArray(chunk) {
  const arrayBuffer = await chunk.arrayBuffer();
  return new Uint8Array(arrayBuffer);
}

/**
 *
 */
function resetStores() {
  for (const storeName in stores) {
    if (Object.hasOwnProperty.call(stores, storeName)) {
      stores[storeName].set([]);
    }
  }
}