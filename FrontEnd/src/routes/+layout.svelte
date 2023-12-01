<script>
  import '../app.css';
  import logo from '$lib/assets/logo.jpeg'
</script>

<header class="bg-white m-5">
  <nav class="flex items-center" aria-label="Global">
    <div class="flex ">
      <a href="/" class="-m-1.5 p-1.5">
        <span class="sr-only">AIA</span>
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

<body class="m-5">
<slot />
</body>
