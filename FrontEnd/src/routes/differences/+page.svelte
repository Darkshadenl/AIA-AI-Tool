<script>
  import Code from '$lib/+code.svelte';
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

{#if oldCode && newCode && oldCode.length === newCode.length}
  {#each oldCode as _, index (index)}
    <table>
      <thead>
        <tr>
          <td><h3>{oldCode[index].fileName}</h3></td>
          <td><h3>{newCode[index].fileName}</h3></td>
        </tr>
      </thead>
      <tbody>
        {#if oldCode[index].diff && newCode[index].diff}
          {#each oldCode[index].diff as _, innerIndex (innerIndex)}
            <tr>
              <td>
                <Code code="{oldCode[index].diff[innerIndex]}" />
              </td>
              <td>
                <Code code="{newCode[index].diff[innerIndex]}" />
              </td>
            </tr>
          {/each}
        {/if}
      </tbody>
    </table>
  {/each}
{/if}

<style>
    table {
        width: 100%;
    }

    td {
        vertical-align: bottom;
    }
</style>