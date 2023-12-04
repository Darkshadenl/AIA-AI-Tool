<script>
  import '../app.css';
  import logo from '$lib/assets/logo.jpeg';
</script>

<div class="flex flex-col h-screen">
  <header class="bg-white m-5">
    <nav class="flex items-center">
      <div class="flex">
        <a href="/" class="-m-1.5 p-1.5">
          <img class="h-20 w-auto" src="{logo}" alt="">
        </a>
      </div>
      <div class="ml-5">
        <a href="/" class="text-sm font-semibold leading-6 text-gray-900">Upload File</a>
      </div>
      <div class="ml-5">
        <a href="/differences" class="text-sm font-semibold leading-6 text-gray-900">Differences</a>
      </div>
    </nav>
  </header>

  <div class="flex flex-grow justify-center items-center">
    <slot />
  </div>
</div>
