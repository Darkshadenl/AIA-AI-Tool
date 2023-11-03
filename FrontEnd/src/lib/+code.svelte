<script>
  export let title;
  export let code;
</script>

<div class={$$props.class}>
  <h2>{title}</h2>

  {#if code}
    {#each code as codeBlock}
      <h4>{codeBlock.fileName}</h4>
      <pre>
        {codeBlock.code}
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