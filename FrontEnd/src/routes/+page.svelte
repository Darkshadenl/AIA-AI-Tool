<script>
	import { fail, redirect } from "@sveltejs/kit";
	import {getConnection, sliceFileIntoChunks, processFileChunks, CreateDiffDataStructure} from '$lib';
	import { HubConnectionState } from "@microsoft/signalr";
	import { goto } from "$app/navigation";
	import { oldCodeStore, newCodeStore, progressInformationMessageStore, errorMessageStore, diffStore } from "../store.js";
	import { resetStores } from "$lib";
	import { diffLines } from "diff";

	const FILE_SIZE_LIMIT_IN_BYTES = 1000 * 1000 * 1000; // 1GB

	const calculateLineNumbers = (diff) => {
		let lineNumber = 1;
		return diff.map((chunk) => {
			return chunk.value.trimEnd('\n').split('\n').map((line) => ({
				line: lineNumber++,
				content: line,
				added: chunk.added,
				removed: chunk.removed,
			}));
		}).flat();
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
			const differences = diffLines(oldFileContent, fileContent, { ignoreWhitespace: true });
			let diffDataStructure = CreateDiffDataStructure(oldFileContent, fileContent, { ignoreWhitespace: true });
			console.log('diffDataStructure in routes/page')

			diffStore.update((value) => {
				const diff = {
					id: value ? value.length : 0,
					fileName: fileName,
					diffs: diffDataStructure
				};

				if (value) return [...value, diff];
				return [diff];
			});

			oldCodeStore.update((value) => {
				const oldCode = { fileName: fileName, code: oldFileContent, diff: calculateLineNumbers(differences.filter(diff => !diff.added)) };
				console.log('inside oldCodeStore update');
				if (value) return [...value, oldCode];
				return [oldCode];
			});
			newCodeStore.update((value) => {
				const newCode = { fileName: fileName, code: fileContent, diff: calculateLineNumbers(differences.filter(diff => !diff.removed)) };
				console.log('inside newCodeStore update');
				if (value) return [...value, newCode];
				return [newCode];
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
