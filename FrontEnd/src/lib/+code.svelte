<script>
  export let code;
</script>

<div class="code">
  {#if code}
    {#each code as codeFile}
      <h2>{codeFile.fileName}</h2>

      {#each codeFile.diff as diff}
        {#if !diff.added}
          <div class={diff.removed ? 'removed' : 'unchanged'}>
            <pre>{diff.value}</pre>
          </div>
        {/if}
      {/each}
    {/each}
  {/if}
</div>

<style>
    pre {
        white-space: pre-wrap;
        margin: 0;
    }

    .code {
        width: 100%;
        margin: 30px;
    }

    .removed {
        background-color: #ffeef0;
        color: #24292e;
        text-decoration: line-through;
    }
</style>