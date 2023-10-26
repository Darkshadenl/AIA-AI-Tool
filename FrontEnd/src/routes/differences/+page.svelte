<script>
  import Code from "$lib/+code.svelte";
  import { invalidateAll } from "$app/navigation";

  export let data;
  setInterval(invalidateAll, 500);
</script>

<!--Line 6 can only exist when the onclick in this button is present, so DO NOT REMOVE IT!-->
<button style="visibility: hidden" on:click={async () => await invalidateAll()}>Refresh</button>

{#if data.successMessage}
  <p>{data.successMessage}</p>
  <p>The AI is currently analysing the code and generating a response. This could take a while, please wait.</p>
{/if}

{#if data.errorMessage}
  <p>{data.errorMessage}</p>
  <p>Please try again.</p>
{/if}

<div class="differences-container">
  <Code title="Old Code" code="{data.oldCode}" />
  <Code title="New Code" code="{data.newCode}" />
</div>

<style>
    .differences-container {
        display: flex;
        justify-content: space-between;
    }
</style>