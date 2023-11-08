<script>
  import Code from '$lib/+code.svelte';
  import { oldCodeStore, newCodeStore, progressInformationMessageStore, errorMessageStore } from '../../store.js';

  let progressInformationMessage;
  let errorMessage;
  progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
  errorMessageStore.subscribe((value) => errorMessage = value);


  let oldCode;
  let newCode;
  oldCodeStore.subscribe((value) => oldCode = value);
  newCodeStore.subscribe((value) => newCode = value);
</script>

<h1>Differences</h1>

{#if progressInformationMessage && errorMessage === null}
  <p>{progressInformationMessage}</p>
{/if}

{#if errorMessage}
  <p>An exception occurred: {errorMessage}</p>
{/if}

{#if oldCode && newCode}
  {#each oldCode as _, index (index)}
    <div class="differences-container">
      <Code codeFile="{oldCode[index]}" />
      <Code codeFile="{newCode[index]}" />
    </div>
  {/each}
{/if}

<style>
    .differences-container {
        display: flex;
        justify-content: space-between;
    }
</style>