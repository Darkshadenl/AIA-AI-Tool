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
        <div>
            {#each diffDataStruct as diffItem, index}
                <button class="bg-blue-400 hover:bg-blue-800 text-white font-bold py-2 px-4 m-1 shadow-lg"
                        on:click={() => setDiffItemsOnClick(index)}>
                    {diffItem.fileName}
                </button>
            {/each}
        </div>

        <Code diffItem="{selectedDiffItem}" index="{selectedDiffItemIndex}" />

        <button class="bg-blue-400 hover:bg-blue-800 text-white font-bold py-4 px-4 m-1 shadow-lg fixed bottom-4 right-0 transform -translate-x-1/2 rounded-full"
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