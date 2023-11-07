<script>
  import { diffLines } from "diff";

  export let oldCode;
  export let newCode;

  let differences;
  console.log(oldCode)
  console.log(newCode.code)
  if (oldCode && newCode && oldCode.code && newCode.code) {
    differences = diffLines(oldCode.code, newCode.code);
  }
</script>

<div class={$$props.class}>
  <h2>Old Code</h2>

  {#if differences}
    {#each differences as code}
      <h4>{code.fileName}</h4>
      <pre>{code.code}</pre>
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