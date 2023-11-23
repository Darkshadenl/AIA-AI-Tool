<script>
  import Code from '$lib/+code.svelte';
  import Loading from '$lib/+loading.svelte';
  import { errorMessageStore, newCodeStore, oldCodeStore, progressInformationMessageStore } from "../../store.js";

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

<div class="differences-container">
{#if oldCode && newCode && oldCode.length === newCode.length}
  {#each oldCode as _, index (index)}
    <!--Header of each file-->
    <div class="column-container">
      <div class="column">
        <div class="fileName-container"><h3>{oldCode[index].fileName}</h3></div>
      </div>
      <div class="column">
        <div class="fileName-container"><h3>{oldCode[index].fileName}</h3></div>
      </div>
    </div>


    <!--Content of each file-->
    {#if oldCode[index].diff && newCode[index].diff}
      {#each oldCode[index].diff as _, innerIndex (innerIndex)}
        <div class="column-container">
          <div class="column">
            <Code code="{oldCode[index].diff[innerIndex]}" />
          </div>
          <div class="column">
            <Code code="{newCode[index].diff[innerIndex]}" />
          </div>
        </div>
      {/each}
    {/if}
  {/each}
{:else}
  <Loading />
{/if}
</div>

<style>
    .column-container {
        column-count: 2;
        column-gap: 20px;
    }

    .column {
        margin: 0;
    }
    .column .fileName-container {
        display: flex;
    }
</style>