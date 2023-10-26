<script>
	import { redirect } from "@sveltejs/kit";

	export let form;

	setTimeout(() => {
		if (form?.success) form.success = "";
	}, 5000);

	function resetMessages() {
		if (form?.success) form.success = "";
		if (form?.error) form.error = "";
	}
</script>

<h1>ZIP File upload</h1>

{#if form?.error}
	<p class="error" style="color: red;">{form.error}</p>
{/if}

{#if form?.success}
	<p class="success">{form.success}</p>
{/if}

<a href="/differences">Newly Generated Comments</a>

<form method="POST" action="?/uploadFile" enctype="multipart/form-data">
	<p>
		<label>
			ZIP File:
			<input on:click={() => resetMessages()} name="file" type="file" accept=".zip" required>
		</label>
	</p>
	<button on:click={() => redirect(300, '/differences')}>Upload</button>
</form>