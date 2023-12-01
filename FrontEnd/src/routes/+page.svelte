<script>
	import { fail, redirect } from "@sveltejs/kit";
	import { CreateDiffDataStructure, getConnection, processFileChunks, resetStores, sliceFileIntoChunks } from "$lib";
	import { HubConnectionState } from "@microsoft/signalr";
	import { goto } from "$app/navigation";
	import { diffStore, errorMessageStore, progressInformationMessageStore } from "../store.js";

	const FILE_SIZE_LIMIT_IN_BYTES = 1000 * 1000 * 1000; // 1GB


    /**
     * Calculates line numbers for each line in a given diff.
     *
     * @param {Array.<{id: number, newValue: undefined | string | Array, oldValue: string | Array}>} diff - The diff to calculate line numbers for.
     * @returns {Array.<{id: number,
     * newValue: Array.<{oldLineNumber: number, newLineNumber: number, selected: undefined | string, value: string}>,
     * oldValue: Array.<{oldLineNumber: number, newLineNumber: number, selected: undefined | string, value: string}>}>} - An array of line objects with line numbers, content, and added/removed flags.
     */
	const calculateLineNumbers = (diff) => {
		let oldLineNumber = 0;
		let newLineNumber = 0;
		const diffCopy = diff.map(chunk => ({ ...chunk })); // Copy for debugging purposes
		return diffCopy.map((chunk) => {
			if (chunk.oldValue) {
				chunk.oldValue = chunk.oldValue.trimEnd('\n').split('\n').map(line => {
					oldLineNumber++;
					if (!chunk.newValue) newLineNumber++;
					return {
						oldLineNumber: oldLineNumber,
						newLineNumber: newLineNumber,
						value: line,
						selected: undefined
					}
				});
			}
			if (chunk.newValue) {
				chunk.newValue = chunk.newValue.trimEnd('\n').split('\n').map(line => {
					newLineNumber++;
					return {
						oldLineNumber: oldLineNumber,
						newLineNumber: newLineNumber,
						value: line,
						selected: undefined
					};
				});
			}
			return chunk;
		});
	};

	async function removeSignalRCallbacks() {
		const connection = await getConnection();

		connection.off('ReceiveProgressInformation');
		connection.off('ReceiveError');
		connection.off('ReceiveLlmResponse');
	}

	async function registerSignalRCallbacks() {
		const connection = await getConnection();

		connection.on('ReceiveProgressInformation', (_, message) => {
			console.log(`Success: ${message}`);
			progressInformationMessageStore.set(message);
		});

		connection.on('ReceiveError', (_, message) => {
			console.log(`ServerError: ${message}`);
			errorMessageStore.set(message);
		});

		connection.on('ReceiveLlmResponse', (_, fileName, contentType, fileContent, oldFileContent) => {
			let diffDataStructure = CreateDiffDataStructure(oldFileContent, fileContent, { ignoreWhitespace: false });
			let calculated = calculateLineNumbers(diffDataStructure);

			diffStore.update((value) => {
				const diff = {
					id: value ? value.length : 0,
					fileName: fileName,
					diffs: calculated
				};

				if (value) return [...value, diff];
				return [diff];
			});

			progressInformationMessageStore.set(null);
			errorMessageStore.set(null);
		});
	}

	async function submitForm(event) {
		event.preventDefault();
		await removeSignalRCallbacks();
		resetStores();
		const formData = new FormData(event.target);
		const file = formData.get('file');

		if (!(file instanceof File)) return;
		if (file.size <= 0) return fail(400, { error: "No file received or file is empty." });
		if (file.size > FILE_SIZE_LIMIT_IN_BYTES) return fail(413, { error: `File size of file "${file.name}" exceeded the limit of ${FILE_SIZE_LIMIT_IN_BYTES / 1000 / 1000} MB.` });

		const connection = await getConnection();
		await registerSignalRCallbacks();

		if (connection.state === HubConnectionState.Connected) {
			const connectionId = await connection.invoke("GetConnectionId");
			const fileChunks = sliceFileIntoChunks(file);
			await processFileChunks(connectionId, fileChunks, file.name, file.type);
			await goto('/differences');
		}
	}

</script>

<h1 class="mb-4 text-4xl font-extrabold leading-none tracking-tight text-gray-900">ZIP-file upload</h1>

<div class="w-fit mt-5">
	<form on:submit={submitForm} enctype="multipart/form-data">
		<label class="block mb-2 text-sm font-medium text-gray-900" for="file">Currently, only TypeScript files within the ZIP file are being processed by the AI.</label>
		<input class="block w-full text-sm text-gray-900 border border-gray-300 rounded-lg cursor-pointer bg-gray-50 focus:outline-none"
					 aria-describedby="file_input_help"
					 id="file"
					 name="file"
					 type="file"
					 accept=".zip"
					 required>
		<p class="mt-1 text-sm text-gray-500" id="file">Only .zip allowed</p>

		<button class="text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 mt-5 focus:outline-none"
						type="submit"
						on:click={() => redirect(300, '/differences')}>
			Upload
		</button>
	</form>
</div>