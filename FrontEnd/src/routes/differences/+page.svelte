<script>
  import { diffLines } from 'diff';
  import OldCode from "$lib/+oldCode.svelte";
  import NewCode from "$lib/+newCode.svelte";
  import { oldCodeStore, newCodeStore, progressInformationMessageStore, errorMessageStore } from "../../store.js";

  let progressInformationMessage;
  let errorMessage;
  progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
  errorMessageStore.subscribe((value) => errorMessage = value);


  let oldCode;
  let newCode;
  oldCodeStore.subscribe((value) => oldCode = value);
  newCodeStore.subscribe((value) => newCode = value);


  let differences = [];
  if (oldCode && newCode) {
    for (let i = 0; i < oldCode.length; i++) {
      const diff = diffLines(oldCode[i].code, newCode[i].code);
      differences.push(diff);
    }
  }
</script>

<h1>Differences</h1>

{#if progressInformationMessage && errorMessage === null}
  <p>{progressInformationMessage}</p>
{/if}

{#if errorMessage}
  <p>An exception occurred: {errorMessage}</p>
{/if}

<!--<div class="differences-container">-->
<!--  <OldCode title="Old Code" code="{oldCode}" />-->
<!--  <NewCode title="New Code" code="{newCode}" />-->
<!--</div>-->
<div class="differences-container">
  <div class="code">
    {#if oldCode}
      {#each oldCode as code, index (index)}
        <h2>{code.fileName}</h2>

        {#if differences}
          {#each differences[index] as diff}
            {#if !diff.added}
              <div class={diff.removed ? 'removed' : 'unchanged'}>
                <pre>{diff.value}</pre>
              </div>
            {/if}
          {/each}
        {/if}
      {/each}
    {/if}
  </div>

  <div class="code">
  {#if newCode}
    {#each newCode as code, index (index)}
      <h2>{code.fileName}</h2>

      {#if differences}
        {#each differences[index] as diff}
          {#if !diff.removed}
            <div class={diff.added ? 'added' : 'unchanged'}>
              <pre>{diff.value}</pre>
            </div>
          {/if}
        {/each}
      {/if}
    {/each}
  {/if}
  </div>
</div>

<style>
    .differences-container {
        display: flex;
        justify-content: space-between;
    }

    .code {
        width: 48%;
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
    .unchanged {
        color: #24292e;
    }

    pre {
        white-space: pre-wrap;
    }
</style>