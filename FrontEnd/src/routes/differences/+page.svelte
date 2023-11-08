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
    <table>
      <thead>
        <tr>
          <td><h3>{oldCode[index].fileName}</h3></td>
          <td><h3>{oldCode[index].fileName}</h3></td>
        </tr>
      </thead>
      <tbody>
        {#each oldCode[index].diff as _, innerIndex (innerIndex)}
          <tr>
            <td>
                <div class={oldCode[index].diff[innerIndex].added ? 'added' : oldCode[index].diff[innerIndex].removed ? 'removed' : 'unchanged'}>
                  <pre>{oldCode[index].diff[innerIndex].value}</pre>
                </div>
            </td>
            <td>
              <div class={newCode[index].diff[innerIndex].added ? 'added' : newCode[index].diff[innerIndex].removed ? 'removed' : 'unchanged'}>
                <pre>{newCode[index].diff[innerIndex].value}</pre>
              </div>
            </td>
          </tr>
        {/each}
      </tbody>
    </table>
  {/each}
{/if}

<style>
    pre {
        white-space: pre-wrap;
        margin: 0;
    }

    td {
        vertical-align: bottom;
    }

    .added {
        background-color: #e6ffed;
        color: #24292e;
        text-decoration: none;
    }

    .removed {
        background-color: #ffeef0;
        color: #24292e;
        text-decoration: line-through;
    }
</style>