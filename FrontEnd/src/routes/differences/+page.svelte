<script>
    import {errorMessageStore, diffStore, progressInformationMessageStore} from "../../store.js";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    /**
     * @type {Array.<{id: number, fileName: string,
     * diffs: Array.<{id: number, oldValue?: {value: string, selected?: string},
     * newValue?: {value: string, selected?: string}}>}>}
     */
    let diffDataStruct;
    let mergedStruct;
    diffStore.subscribe((value) => diffDataStruct = value);

    if (diffDataStruct) {
        console.log(diffDataStruct)
        mergedStruct = diffDataStruct.map(chunk => ({ ...chunk }));
        mergedStruct.forEach(diffItem => {
            diffItem.diffs.forEach(diff => {
                if (diff.newValue)
                    diff.merged = []
            })
        })
        console.log('mergedStruct')
        console.log(mergedStruct)
    }
    let selection = [];

    const changeLineColor = (lineObject, old, diffId, diffItemId, index) => {
        if (old) {
            diffDataStruct[diffItemId].diffs[diffId].oldValue[index].selected =
                lineObject.selected === "old" ? undefined : "old";
        } else if (!old) {
            diffDataStruct[diffItemId].diffs[diffId].newValue[index].selected =
                lineObject.selected === "new" ? undefined : "new";
        }
    }

    const moveToAndFromMerged = (lineObject, diffId, diffItemId) => {
        let diffs = mergedStruct[diffItemId].diffs;
        const merged = diffs[diffId].merged;
        const contains = merged.some(item => item.value === lineObject.value);

        if (!contains) {
            diffs[diffId].merged = [...merged, lineObject]; // Voeg toe met nieuwe referentie
        } else {
            diffs[diffId].merged = merged.filter(item => item.value !== lineObject.value); // Verwijder met nieuwe referentie
        }

        // Svelte's reactivity werkt door toewijzing, update de mergedStruct met een nieuwe referentie
        mergedStruct[diffItemId].diffs = diffs;
        mergedStruct = [...mergedStruct]; // Trigger reactivity door toewijzing op het hoogste niveau
    };


    const handleClick = (diffId, diffItemId, index, old) => {
        let diff = diffDataStruct[diffItemId].diffs[diffId];
        if (!(diff.oldValue && diff.newValue)){
            console.log('returning. No new AND old found')
            return;
        }
        console.log(`click diffitemid:${diffItemId} diffid:${diffId} index:${index} old:${old}`)
        const lineObject = old === true ? diffDataStruct[diffItemId].diffs[diffId].oldValue[index] :
            diffDataStruct[diffItemId].diffs[diffId].newValue[index];
        changeLineColor(lineObject, old, diffId, diffItemId, index)
        moveToAndFromMerged(lineObject, diffId, diffItemId)
    }

</script>

<div>
    <h1>Differences</h1>
</div>

<div class="submit-button">
    <button type="submit">submit</button>
</div>

{#if progressInformationMessage && errorMessage === null}
    <p>{progressInformationMessage}</p>
{/if}

{#if errorMessage}
    <p>An exception occurred: {errorMessage}</p>
{/if}

<div class="column-container">
    {#if diffDataStruct}
        <div class="code">
            {#each diffDataStruct as diffItem}
                <h2>{diffItem.fileName}</h2>

                {#each diffItem.diffs as diff}
                    {#each diff.oldValue as oldCode, oldIndex}
                        <div class="code-diff {diff.oldValue && diff.newValue ? 'removed' : 'unchanged'}"
                             tabindex="0"
                             class:selected-old={oldCode.selected === "old"}
                             on:click={handleClick(diff.id, diffItem.id, oldIndex, true)}
                             on:keydown={handleClick(diff.id, diffItem.id, oldIndex, true)}
                             role="button">
                            <pre>{oldCode.value}</pre>
                        </div>
                    {/each}
                {/each}
            {/each}
        </div>

        <div class="code">
            {#each diffDataStruct as diffItem}
                <h2>{diffItem.fileName}</h2>

                {#each diffItem.diffs as diff}
                    {#if diff.newValue}
                        {#each diff.newValue as newCode, newIndex}
                            <div class="code-diff {diff.oldValue && diff.newValue ? 'added' : 'unchanged'}"
                                 tabindex="0"
                                 class:selected-new={newCode.selected === "new"}
                                 on:click={handleClick(diff.id, diffItem.id, newIndex, false)}
                                 on:keydown={handleClick(diff.id, diffItem.id, newIndex, false)}
                                 role="button">
                                <pre>{newCode.value}</pre>
                            </div>
                        {/each}
                    {:else}
                        {#each diff.oldValue as old}
                            <div class="code-diff unchanged" role="button">
                                <pre>{old.value}</pre>
                            </div>
                        {/each}
                    {/if}
                {/each}
            {/each}
        </div>

        <div class="code">
            {#each mergedStruct as diffItem}
                <h2>{diffItem.fileName}</h2>
                {#each diffItem.diffs as diff}
                    {#if diff.merged}
                        {#if diff.merged.length > 0}
                            {#each diff.merged as mergedCode}
                                <div class="code-diff merged-item"
                                     tabindex="0"
                                     role="button">
                                    <pre>{mergedCode.value}</pre>
                                </div>
                            {/each}
                        {:else}
                            <div class="code-diff merged-item"
                                 tabindex="0"
                                 role="button">
                                <pre> </pre>
                            </div>
                        {/if}
                    {:else}
                        {#each diff.oldValue as old}
                            <div class="code-diff unchanged" role="button">
                                <pre>{old.value}</pre>
                            </div>
                        {/each}
                    {/if}
                {/each}
            {/each}
        </div>
    {/if}
</div>

<style>
    .submit-button {
        display: flex;
        justify-content: center;
        position: fixed;
        bottom: 95%;
        width: 100%;
    }

    .column-container {
        column-count: 2;
        column-gap: 30px;
        display: flex;
        justify-content: space-between;
    }

    .code {
        flex: 1;
        display: flex;
        flex-wrap: nowrap;
        height: 100%;
        width: 99%;
        flex-direction: column;
    }

    .code-diff {
        margin: 0 0 3px 0;
    }

    .selected-new {
        background-color: #2eef00 !important;
        font-weight: bold;
    }

    .selected-old {
        background-color: #ff1414 !important;
        font-weight: bold;
    }

    .merged-item {
        background-color: #ff5b14 !important;
        font-weight: bold;
    }

    .added {
        background-color: rgba(117, 243, 155, 0.49);
        color: #24292e;
        text-decoration: none;
    }

    .unchanged {
        color: #24292e;
    }

    .removed {
        background-color: rgba(241, 113, 130, 0.65);
        color: #24292e;
        text-decoration: line-through;
    }


    pre {
        white-space: pre-wrap;
        margin: 0;
    }
</style>
