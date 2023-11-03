<script>
	import { fail, redirect } from "@sveltejs/kit";
	import { uploadChunk, getConnection } from '../SignalRServer.js';
	import { HubConnectionState } from "@microsoft/signalr";
	import { goto } from "$app/navigation";
	import { oldCodeStore, newCodeStore, progressInformationMessageStore, errorMessageStore } from "../store.js";
	import * as stores from '../store.js';

	const FILE_SIZE_LIMIT_IN_BYTES = 1000 * 1000 * 1000; // 1GB

	function registerSignalRCallbacks(connection) {
		connection.on('ReceiveProgressInformation', (_, message) => {
			console.log(`Success: ${message}`);
			progressInformationMessageStore.set(message);
		});

		connection.on('ReceiveError', (message) => {
			console.log(`ServerError: ${message}`);
			errorMessageStore.set(message);
		});

		connection.on('ReceiveLlmResponse', (_, fileName, contentType, fileContent, oldFileContent) => {
			oldCodeStore.update((value) => {
				if (value) return [...value, { fileName: fileName, code: oldFileContent }];
				return [{ fileName: fileName, code: oldFileContent }];
			});
			newCodeStore.update((value) => {
				if (value) return [...value, { fileName: fileName, code: fileContent }];
				return [{ fileName: fileName, code: fileContent }];
			});

			progressInformationMessageStore.set(null);
			errorMessageStore.set(null);
		});
	}

	async function submitForm(event) {
		event.preventDefault();
		resetStores();
		const formData = new FormData(event.target);
		const file = formData.get('file');

		if (!(file instanceof File)) return;
		if (file.size <= 0) return fail(400, { error: "No file received or file is empty." });
		if (file.size > FILE_SIZE_LIMIT_IN_BYTES) return fail(413, { error: `File size of file "${file.name}" exceeded the limit of ${FILE_SIZE_LIMIT_IN_BYTES / 1000 / 1000} MB.` });

		const connection = await getConnection();
		registerSignalRCallbacks(connection);

		if (connection.state === HubConnectionState.Connected) {
			const connectionId = await connection.invoke("GetConnectionId");
			const fileChunks = sliceFileIntoChunks(file);
			await processFileChunks(connectionId, fileChunks, file.name, file.type);
			await goto('/differences');
		}
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
			const base64 = toBase64(byteArray);
			uploadChunk(connectionId, base64, fileName, contentType, i, fileChunks.length).catch((err) => {
				console.error(err);
			});
		}
	}

	function toBase64(byteArray) {
		let binary = '';
		const len = byteArray.byteLength;

		for (let i = 0; i < len; i++) {
			binary += String.fromCharCode(byteArray[i]);
		}

		return window.btoa(binary);
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
				stores[storeName].set(null);
			}
		}
	}
</script>

<h1>ZIP File upload</h1>

<a href="/differences">Newly Generated Comments</a>

<p>Currently, only TypeScript files within the ZIP file are being processed by the AI.</p>
<form on:submit={submitForm} enctype="multipart/form-data">
	<p>
		<label>
			ZIP File:
			<input name="file" type="file" accept=".zip" required>
		</label>
	</p>
	<button on:click={() => redirect(300, '/differences')}>Upload</button>
</form>