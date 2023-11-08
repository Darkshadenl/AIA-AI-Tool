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

<div class="differences-container">
    <Code code="{oldCode}" />
    <Code code="{newCode}" />

<!--  <div class="code">-->
<!--    {#if newCode}-->
<!--      {#each newCode as code}-->
<!--        <h2>{code.fileName}</h2>-->

<!--          {#each code.diff as diff}-->
<!--            {#if !diff.removed}-->
<!--              <div class={diff.added ? 'added' : 'unchanged'}>-->
<!--                <pre>{diff.value}</pre>-->
<!--              </div>-->
<!--            {/if}-->
<!--          {/each}-->
<!--      {/each}-->
<!--    {/if}-->
<!--  </div>-->
</div>

<style>
    .differences-container {
        display: flex;
        justify-content: space-between;
    }

    /*.code {*/
    /*    width: 48%;*/
    /*}*/

    .added {
        background-color: #e6ffed;
        color: #24292e;
        text-decoration: none;
    }
</style>