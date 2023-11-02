//@ts-check

import { fail } from '@sveltejs/kit';
import { uploadChunk, getConnection } from '../SignalRServer.js';
import * as stores from '../store.js';

const FILE_SIZE_LIMIT_IN_BYTES = 1000 * 1000 * 1000; // 1GB

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
    await establishSignalRConnection();

    const formData = await request.formData();
    const file = formData.get('file');

    if (!(file instanceof File)) return;
    if (file.size <= 0) return fail(400, { error: "No file received or file is empty." });
    if (file.size > FILE_SIZE_LIMIT_IN_BYTES) return fail(413, { error: `File size of file "${file.name}" exceeded the limit of ${FILE_SIZE_LIMIT_IN_BYTES / 1000 / 1000} MB.` });

    let connection = await getConnection();
    console.log(connection.state)
    let connectionId = await connection.invoke("GetConnectionId")
    console.log("id=" + connectionId);

    const fileChunks = sliceFileIntoChunks(file);
    await processFileChunks(connectionId, fileChunks, file.name, file.type);
    // let message;
    // successMessage.subscribe((value) => {
		// 	message = value;
		// });
    // return { success: message }
	}
}

/**
 * Creates and starts a connection with the SignalR server, as well as closing an earlier connection if it exists.
 */
async function establishSignalRConnection() {
  await getConnection();
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
 * @param {string} connectionId - The id of the connection with the SignalR server.
 * @param {Blob[]} fileChunks - The list of chunks.
 * @param {string} fileName - The name of the file before it was sliced into chunks.
 * @param {string} contentType - The content type of the file before it was sliced into chunks.
 */
async function processFileChunks(connectionId, fileChunks, fileName, contentType) {
  for (let i = 0; i < fileChunks.length; i++) {
    const byteArray = await chunkToByteArray(fileChunks[i]);
    const base64 = Buffer.from(byteArray).toString('base64');
    uploadChunk(connectionId, base64, fileName, contentType, i, fileChunks.length).catch((err) => {
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
      stores[storeName].update(() => null);
    }
  }
}