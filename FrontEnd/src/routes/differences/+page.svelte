<script>
    import {diffStore, errorMessageStore, progressInformationMessageStore} from "../../store.js";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    /**
     * @type {
     * Array.<{id: number, fileName: string,
     * diffs: Array.<{id: number,
     * oldValue?: Array.<{value: string, selected?: string}>,
     * newValue?: Array.<{value: string, selected?: string}>}>}>
     * }
     */
    let diffDataStruct;
    /**
     * @type {
     * Array.<{id: number, fileName: string,
     * diffs: Array.<{id: number,
     * merged?: Array.<{value: string, selected?: string}>,
     * oldValue?: Array.<{value: string, selected?: string}>,
     * newValue?: Array.<{value: string, selected?: string}>}>}>
     * }
     */
    let mergedStruct;
    diffStore.subscribe((value) => {
        diffDataStruct = value;

        if (diffDataStruct) {
            mergedStruct = JSON.parse(JSON.stringify(diffDataStruct));
            mergedStruct.forEach(diffItem => {
                diffItem.diffs.forEach(diff => {
                    if (diff.newValue)
                        diff.merged = []
                })
            });
        }
    });

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

        let contains;
        if (merged) contains = merged.some(item => item.value === lineObject.value);
        if (!contains) diffs[diffId].merged = [...merged, JSON.parse(JSON.stringify(lineObject))];
        else diffs[diffId].merged = merged.filter(item => item.value !== lineObject.value);

        mergedStruct = [...mergedStruct];
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

    let localMergedStruct = mergedStruct;

    /**
     * Handles text edits on an input element.
     *
     * @param {number} diffId - The ID of the current diff.
     * @param {number} diffItemId - The ID of the current diff item.
     * @param {number} index - The index of the merged code in the diff.
     * @param {InputEvent} event - The input event from the contenteditable element.
     */
    function handleTextEdit(diffId, diffItemId, index, event) {
        // Werk de lokale kopie bij zonder de originele mergedStruct aan te raken
        localMergedStruct[diffItemId].diffs[diffId].merged[index].value = event.target.value;
        console.log()
    }

    function handleTextBlur(diffId, diffItemId, index, event) {
        mergedStruct[diffItemId].diffs[diffId].merged[index].value = event.target.value;
        mergedStruct = [...mergedStruct];
        console.info('diffDataStruct', diffDataStruct)
        console.info('mergedStruct', mergedStruct);
    }

    function autoGrow(event) {
        event.target.style.height = '1rem';
        event.target.style.height = `${event.target.scrollHeight}px`;
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
    {#if diffDataStruct && mergedStruct}
        <div class="code maxxed">
            {#each diffDataStruct as diffItem}
                <h2>{diffItem.fileName}</h2>

                {#each diffItem.diffs as diff}
                    {#each diff.oldValue as oldCode, oldIndex}
                        <div class="code-diff wrap {diff.oldValue && diff.newValue ? 'removed' : 'unchanged'}"
                             tabindex="0"
                             class:selected-old={oldCode.selected === "old"}
                             on:click={() => handleClick(diff.id, diffItem.id, oldIndex, true)}
                             on:keydown={() => handleClick(diff.id, diffItem.id, oldIndex, true)}
                             role="button">
                            <pre>{oldCode.value}</pre>
                        </div>
                    {/each}
                {/each}
            {/each}
        </div>

        <div class="code maxxed">
            {#each diffDataStruct as diffItem}
                <h2>{diffItem.fileName}</h2>

                {#each diffItem.diffs as diff}
                    {#if diff.newValue}
                        {#each diff.newValue as newCode, newIndex}
                            <div class="code-diff wrap {diff.oldValue && diff.newValue ? 'added' : 'unchanged'}"
                                 tabindex="0"
                                 class:selected-new={newCode.selected === "new"}
                                 on:click={() => handleClick(diff.id, diffItem.id, newIndex, false)}
                                 on:keydown={() => handleClick(diff.id, diffItem.id, newIndex, false)}
                                 role="button">
                                <pre>{newCode.value}</pre>
                            </div>
                        {/each}
                    {:else}
                        {#each diff.oldValue as old}
                            <div class="code-diff unchanged wrap" role="button">
                                <pre>{old.value}</pre>
                            </div>
                        {/each}
                    {/if}
                {/each}
            {/each}
        </div>

        <div class="code maxxed">
            {#each mergedStruct as diffItem}
                <h2>{diffItem.fileName}</h2>
                {#each diffItem.diffs as diff}
                    {#if diff.merged}
                        {#if diff.merged.length > 0}
                            {#each diff.merged as mergedCode, mergedIndex}
                                <div class="code-diff merged-item removable-merge-item"
                                     tabindex="0"
                                     role="button">
                                    <span>X</span>
                                    <textarea class="merge-input"
                                              bind:value={mergedCode.value}
                                              on:input={(event) => handleTextEdit(diff.id, diffItem.id, mergedIndex, event)}
                                              on:input={autoGrow}
                                              on:blur={(event) => handleTextBlur(diff.id, diffItem.id, mergedIndex, event)}
                                    />
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
                            <div class="code-diff unchanged wrap" role="button">
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

    .wrap {
        /*white-space: pre-wrap;*/
        word-break: break-word;
        max-width: 100%;
    }



    .column-container {
        column-count: 2;
        column-gap: 30px;
        display: flex;
        justify-content: space-between;
    }

    .removable-merge-item {
        display: flex;
        flex-direction: row;
    }

    .removable-merge-item > span {
       margin: 2px 10px 0 5px;
    }

    .merge-input {
        border: none;
        width: 100%;
        background-color: #ff5b14;
        color: wheat;
        font-family: monospace;
    }

    .removable-merge-item > textarea {
        height: 1rem;
        overflow-y: hidden;
    }

    .code {
        flex: 1;
        display: flex;
        height: 100%;
        width: 99%;
        flex-direction: column;
        margin: 0 2px 0 5px;
    }

    .maxxed {
        max-width: 70rem;
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
