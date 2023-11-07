<script>
  export let oldCode;
  export let newCode;
</script>

<div class={$$props.class}>
  <h2>New Code</h2>

  {#if newCode}
    {#each newCode as code}
      <h4>{code.fileName}</h4>
      <pre>
        {code.code}
      </pre>
    {/each}
  {/if}
</div>

<style>
  div {
      width: 50%;
  }

  pre {
      white-space: pre-wrap;
  }
</style>