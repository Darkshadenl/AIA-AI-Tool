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

  const calculateLineNumbers = (diff) => {
    let lineNumber = 1;
    return diff.map((chunk) => {
      return chunk.value.split('\n').map((line) => ({
        line: lineNumber++,
        content: line,
        added: chunk.added,
        removed: chunk.removed,
      }));
    }).flat();
  };

  if (oldCode && newCode) {
    oldCode.forEach(code => {
      code.diffWithLineNumbers = calculateLineNumbers(code.diff);
    });
    newCode.forEach(code => {
      code.diffWithLineNumbers = calculateLineNumbers(code.diff);
    });
  }
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
        {#each oldCode[index].diffWithLineNumbers as _, innerIndex (innerIndex)}
          <tr>
            <td>
              <Code code="{oldCode[index].diffWithLineNumbers[innerIndex]}" />
            </td>
            <td>
              <Code code="{newCode[index].diffWithLineNumbers[innerIndex]}" />
            </td>
          </tr>
        {/each}
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