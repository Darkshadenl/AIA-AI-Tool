<script>
  export let codeFile;
</script>

<div class="code">
    <h2>{codeFile.fileName}</h2>

    {#each codeFile.diff as diff}
      <div class={diff.added ? 'added' : diff.removed ? 'removed' : 'unchanged'}>
        <pre>{diff.value}</pre>
      </div>
    {/each}
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