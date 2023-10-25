<script>
  import Code from "$lib/+code.svelte";

  export let data;
  let oldCode = data.oldCode;
  let newCode = data.newCode;
</script>

<div class="differences-container">
  <Code title="Old Code" code="{oldCode}" />
  <Code title="New Code" code="{newCode}" />
</div>

<style>
    .differences-container {
        display: flex;
        justify-content: space-between;
    }
</style>