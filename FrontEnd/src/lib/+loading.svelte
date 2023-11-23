<script>
  import mrBean from "$lib/assets/mr-bean-waiting.gif";

  let jokeSetup;
  let jokeDelivery;
  let showDelivery = false;

  async function getJoke() {
    showDelivery = false;
    try {
      const response = await fetch('https://v2.jokeapi.dev/joke/Programming?type=twopart');
      if (!response.ok) return;

      const data = await response.json();
      jokeSetup = data.setup;
      jokeDelivery = data.delivery;
    } catch (error) {
      console.log(error.message);
    }

    document.querySelector('#get-joke-button').textContent = 'ANOTHER!';
  }

  function displayDelivery() {
    showDelivery = true;
  }
</script>

<div class="loading-container">
  <div class="img-joke-container">
    <img src="{mrBean}" alt="mr-bean-waiting-gif" />

    <div class="joke-container">
      <p>Want to hear a joke in the meantime?</p>
      <button id="get-joke-button" on:click="{getJoke}">YES!</button>

      {#if jokeSetup && jokeDelivery}
        <p>{jokeSetup}</p>
        {#if jokeSetup.endsWith('?')}
          <button on:click={displayDelivery}>I don't know, tell me!</button>
          {:else}
          <button on:click={displayDelivery}>Why?!</button>
        {/if}

        {#if showDelivery}
          <p>{jokeDelivery}</p>
        {/if}
      {/if}
    </div>
  </div>
</div>

<style>
    .loading-container {
        display: flex;
        justify-content: center;
    }

    .img-joke-container { width: 640px; }

    .joke-container { word-break: break-word; }
</style>