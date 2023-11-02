<script>
	import { redirect } from "@sveltejs/kit";
	import { getConnection } from "../SignalRServer.js";
	import { onMount } from "svelte";

	export let form;

	setTimeout(() => {
		if (form?.success) form.success = "";
	}, 5000);

	function resetMessages() {
		if (form?.success) form.success = "";
		if (form?.error) form.error = "";
	}

	//TODO: Fix that connectionId is equal to the connectionId invoked to the SignalR server.
	onMount(async () => {
		const connection = await getConnection();

		connection.on('ReturnLLMResponse', (connectionId, fileName, contentType, fileContent, oldFileContent) => {
			console.log("Message received:", connectionId);
			throw redirect(307, '/differences');
		});
	})
</script>

<h1>ZIP File upload</h1>

{#if form?.error}
	<p class="error" style="color: red;">{form.error}</p>
{/if}

{#if form?.success}
	<p class="success">{form.success}</p>
{/if}

<a href="/differences">Newly Generated Comments</a>

<p>Currently, only TypeScript files within the ZIP file are being processed by the AI.</p>
<form method="POST" action="?/uploadFile" enctype="multipart/form-data">
	<p>
		<label>
			ZIP File:
			<input on:click={() => resetMessages()} name="file" type="file" accept=".zip" required>
		</label>
	</p>
	<button on:click={() => redirect(300, '/differences')}>Upload</button>
</form>