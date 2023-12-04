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
    {:else}
        <Loading />
    {/if}
</div>