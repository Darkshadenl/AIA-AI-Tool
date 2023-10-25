<script>
  export let title;
  export let code;
</script>

<div class={$$props.class}>
  <h1>{title}</h1>

  {#each code as codeBlock}
    <pre>
      {codeBlock}
    </pre>
  {/each}
</div>

<style>
  div {
      width: 50%;
  }

  pre {
      white-space: pre-wrap;
  }
</style>