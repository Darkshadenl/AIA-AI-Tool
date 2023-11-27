<script>
  export let code;
</script>

{#if code}
  <div class={code.added ? 'added' : code.removed ? 'removed' : 'unchanged'}>
    <pre>{code.line} {code.content}</pre>
  </div>
{/if}

<style>
    pre {
        white-space: pre-wrap;
        margin: 0;
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