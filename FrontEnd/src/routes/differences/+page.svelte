<script>
    import Code from '$lib/components/+code.svelte'
    import Loading from '$lib/components/+loading.svelte';
    import {diffStore, errorMessageStore, progressInformationMessageStore} from "../../store.js";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    /** @type {DiffData} */
    let selectedDiffItem;
    /** @type {number} */
    let selectedDiffItemIndex;

    /** @type {Array<DiffData>}*/
    let diffDataStruct;
    diffStore.subscribe((value) => {
        diffDataStruct = value;

        if (diffDataStruct && diffDataStruct.length > 0) {
            selectedDiffItem = diffDataStruct[0];
            selectedDiffItemIndex = 0;
        }
    });

    function setDiffItemsOnClick(index) {
        selectedDiffItem = diffDataStruct[index];
        selectedDiffItemIndex = index;
    }
    </script>

<div class="m-5">
    <div>
        <h1 class="mb-4 text-4xl font-extrabold leading-none tracking-tight text-gray-900">Differences</h1>
    </div>

    {#if progressInformationMessage && errorMessage === null}
        <p>{progressInformationMessage}</p>
    {/if}

    {#if errorMessage}
        <p>An exception occurred: {errorMessage}</p>
    {/if}

    {#if diffDataStruct}
        <div class="flex">
            <div class="flex flex-col mr-2">
                <h4 class="m-1 text-xl">Files</h4>
                {#each diffDataStruct as diffItem, index}
                    <button class="bg-blue-400 hover:bg-blue-800 text-white font-bold py-2 px-4 m-1 shadow-lg h-fit w-56 break-words"
                            on:click={() => setDiffItemsOnClick(index)}>
                        {diffItem.fileName}
                    </button>
                {/each}

                <!--TODO: Only show spinner when files are being processed by the LLM-->
                <div role="status" class="flex justify-center m-1">
                    <svg aria-hidden="true" class="w-8 h-8 text-gray-200 animate-spin dark:text-gray-600 fill-blue-600" viewBox="0 0 100 101" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M100 50.5908C100 78.2051 77.6142 100.591 50 100.591C22.3858 100.591 0 78.2051 0 50.5908C0 22.9766 22.3858 0.59082 50 0.59082C77.6142 0.59082 100 22.9766 100 50.5908ZM9.08144 50.5908C9.08144 73.1895 27.4013 91.5094 50 91.5094C72.5987 91.5094 90.9186 73.1895 90.9186 50.5908C90.9186 27.9921 72.5987 9.67226 50 9.67226C27.4013 9.67226 9.08144 27.9921 9.08144 50.5908Z" fill="currentColor"/>
                        <path d="M93.9676 39.0409C96.393 38.4038 97.8624 35.9116 97.0079 33.5539C95.2932 28.8227 92.871 24.3692 89.8167 20.348C85.8452 15.1192 80.8826 10.7238 75.2124 7.41289C69.5422 4.10194 63.2754 1.94025 56.7698 1.05124C51.7666 0.367541 46.6976 0.446843 41.7345 1.27873C39.2613 1.69328 37.813 4.19778 38.4501 6.62326C39.0873 9.04874 41.5694 10.4717 44.0505 10.1071C47.8511 9.54855 51.7191 9.52689 55.5402 10.0491C60.8642 10.7766 65.9928 12.5457 70.6331 15.2552C75.2735 17.9648 79.3347 21.5619 82.5849 25.841C84.9175 28.9121 86.7997 32.2913 88.1811 35.8758C89.083 38.2158 91.5421 39.6781 93.9676 39.0409Z" fill="currentFill"/>
                    </svg>
                </div>
            </div>

        <Code diffItem="{selectedDiffItem}" index="{selectedDiffItemIndex}" />

        </div>
        <button class="bg-blue-700 hover:bg-blue-800 text-white font-bold py-4 px-4 m-1 shadow-lg fixed bottom-4 right-0 transform -translate-x-1/2 rounded-full"
                on:click={() => window.scrollTo({ top: 0, left: 0, behavior: 'smooth'})}>
            <svg xmlns="http://www.w3.org/2000/svg" height="16" width="16" viewBox="0 0 512 512">
                <!--!Font Awesome Free 6.5.1 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free Copyright 2023 Fonticons, Inc.-->
                <path fill="#ffffff" d="M233.4 105.4c12.5-12.5 32.8-12.5 45.3 0l192 192c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L256 173.3 86.6 342.6c-12.5 12.5-32.8 12.5-45.3 0s-12.5-32.8 0-45.3l192-192z"/>
            </svg>
        </button>
    {:else}
        <Loading />
    {/if}
</div>