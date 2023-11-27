<script>
    import {v4 as uuidv4} from "uuid";
    import {diffStore, errorMessageStore, progressInformationMessageStore} from "../../store.js";
    import JSZip from "jszip";
    import AutoGrowingTextArea from "$lib/components/AutoGrowingTextArea.svelte";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    /** @type {Array<DiffData>}*/
    let diffDataStruct;
    /** @type {Array<DiffData>}*/
    let mergedStruct;
    diffStore.subscribe((value) => {
        diffDataStruct = value;

        if (diffDataStruct) {
            mergedStruct = JSON.parse(JSON.stringify(diffDataStruct));
            mergedStruct.forEach(diffItem => {
                diffItem.diffs.forEach(diff => {
                    if (diff.newValue)
                        diff.merged = [];
                });
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
        const diffs = mergedStruct[diffItemId].diffs;
        const merged = diffs[diffId].merged;

        let contains;
        if (merged) contains = merged.some(item => item.id === lineObject.id);

        calculateLineNumber(diffs, diffId, lineObject, contains);

        if (!contains) {
            lineObject.id = uuidv4();
            diffs[diffId].merged = [...merged, JSON.parse(JSON.stringify(lineObject))];
        } else {
            diffs[diffId].merged = merged.filter(item => item.id !== lineObject.id);
        }

        diffs[diffId].merged.sort((a, b) => a.newLineNumber - b.newLineNumber);
        mergedStruct = [...mergedStruct];
    };

    function calculateLineNumber(diffs, diffId, lineObject, contains) {
        if (diffId >= diffs.length) return;

        for (let i = diffId; i < diffs.length; i++) {
            diffs[i].oldValue.forEach(value => contains ? value.oldLineNumber-- : value.oldLineNumber++);
        }

        lineObject.lineNumber = contains ? lineObject.newLineNumber - 1 : lineObject.newLineNumber + 1;
    }


    const handleClick = (diffId, diffItemId, index, old) => {
        let diff = diffDataStruct[diffItemId].diffs[diffId];
        if (!(diff.oldValue && diff.newValue)) {
            console.log('returning. No new AND old found');
            return;
        }
        console.log(`click diffitemid:${diffItemId} diffid:${diffId} index:${index} old:${old}`)
        const lineObject = old === true ? diffDataStruct[diffItemId].diffs[diffId].oldValue[index] :
            diffDataStruct[diffItemId].diffs[diffId].newValue[index];
        changeLineColor(lineObject, old, diffId, diffItemId, index);
        moveToAndFromMerged(lineObject, diffId, diffItemId);
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
    }

    /**
     * Handles text edits on an input element.
     *
     * @param {number} diffId - The ID of the current diff.
     * @param {number} diffItemId - The ID of the current diff item.
     * @param {number} index - The index of the merged code in the diff.
     * @param {InputEvent} event - The input event from the contenteditable element.
     */
    function handleTextBlur(diffId, diffItemId, index, event) {
        mergedStruct[diffItemId].diffs[diffId].merged[index].value = event.target.value;
        mergedStruct = [...mergedStruct];
    }

    function autoGrow(event) {
        event.target.style.height = '1rem';
        event.target.style.height = `${event.target.scrollHeight}px`;
    }

    function submit() {
        console.log(mergedStruct);
        let showError = false;
        const download = {}
        mergedStruct.forEach(current => {
            let dataString = "";

            current.diffs.forEach(current => {
                if (current.merged && current.merged.length > 0) {
                    // use merged values
                    let mergedValues = ""
                    current.merged.forEach(data => mergedValues = mergedValues === "" ? `${data.value}` :`${mergedValues}\n${data.value}`);
                    dataString = dataString === "" ? `${mergedValues}` : `${dataString}\n${mergedValues}`;
                } else if (current.oldValue && current.newValue) {
                    // if both oldValue and newValue exist, user forgot to select an option
                    showError = true;
                } else {
                    // just take oldValue
                    let mergedValues = ""
                    current.oldValue.forEach(data => mergedValues = mergedValues === "" ? `${data.value}` :`${mergedValues}\n${data.value}`);
                    dataString = dataString === "" ? `${mergedValues}` : `${dataString}\n${mergedValues}`;
                }
            })
            download[current.fileName] = dataString;
        });
        if (showError) {
            alert("You forgot to select one or more old/new options.");
        }
        createZip(download);
        console.log(download);
    }

    function createZip(dataToZip) {
        const zip = new JSZip();
        for (const [fileName, content] of Object.entries(dataToZip)) {
            const blob = createBlob(content);
            zip.file(fileName, blob);
        }

        zip.generateAsync({ type: "blob" }).then((zipBlob) => {
            const zipUrl = URL.createObjectURL(zipBlob);
            const link = document.createElement("a");
            link.href = zipUrl;
            link.download = "bestanden.zip";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(zipUrl);
        });
    }

    function createBlob(textContent) {
        return new Blob([textContent], { type: "text/plain" });
    }

</script>

<div>
    <h1>Differences</h1>
</div>

<div class="submit-button">
    <button type="submit" on:click={submit}>submit</button>
</div>

{#if progressInformationMessage && errorMessage === null}
    <p>{progressInformationMessage}</p>
{/if}

{#if errorMessage}
    <p>An exception occurred: {errorMessage}</p>
{/if}

{#if diffDataStruct && mergedStruct}
    {#each diffDataStruct as diffItem, index}
        <div class="column-container">
            <div class="code maxxed">
                <h2>{diffItem.fileName}</h2>

                {#each diffItem.diffs as diff}
                    {#each diff.oldValue as oldCode, oldIndex}
                        <div class="code-diff wrap {diff.oldValue && diff.newValue ? 'removed' : 'unchanged'}"
                             tabindex="0"
                             class:selected-old={oldCode.selected === "old"}
                             on:click={() => handleClick(diff.id, diffItem.id, oldIndex, true)}
                             on:keydown={() => handleClick(diff.id, diffItem.id, oldIndex, true)}
                             role="button">
                            <p>{oldCode.oldLineNumber}</p> <pre>{oldCode.value}</pre>
                            <span>{oldCode.lineNumber}</span>
                            <pre>{oldCode.value}</pre>
                        </div>
                    {/each}
                {/each}
            </div>

            <div class="code maxxed">
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
                                 <p>{newCode.newLineNumber}</p> <pre>{newCode.value}</pre>
                                <span>{newCode.lineNumber}</span>
                                <pre>{newCode.value}</pre>
                            </div>
                        {/each}
                    {:else}
                        {#each diff.oldValue as oldCode}
                            <div class="code-diff unchanged wrap" role="button">
                                <p>{oldCode.newLineNumber}</p> <pre>{oldCode.value}</pre>
                                <span>{oldCode.lineNumber}</span>
                                <pre>{oldCode.value}</pre>

                            </div>
                        {/each}
                    {/if}
                {/each}
            </div>
            
            <div class="code maxxed">
                <h2>{mergedStruct[index].fileName}</h2>

                {#each mergedStruct[index].diffs as diff}
                    {#if diff.merged}
                        {#if diff.merged.length > 0}
                            {#each diff.merged as mergedCode, mergedIndex}
                                <div class="code-diff merged-item removable-merge-item"
                                     tabindex="0"
                                     role="button">
                                    <p>{mergedCode.lineNumber}</p>
                                    <textarea class="merge-input"
                                              bind:value={mergedCode.value}
                                              on:input={(event) => handleTextEdit(diff.id, diffItem.id, mergedIndex, event)}
                                              on:input={autoGrow}
                                              on:focus={autoGrow}
                                              on:blur={(event) => handleTextBlur(diff.id, diffItem.id, mergedIndex, event)}
                                    />
                                    <span>{mergedCode.lineNumber}</span>
                                    <AutoGrowingTextArea textValue="{mergedCode.value}" parentFunc="{(event) => handleTextBlur(diff.id, diffItem.id, mergedIndex, event)}" />
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
                        {#each diff.oldValue as oldCode}
                            <div class="code-diff unchanged wrap" role="button">
                                <p>{oldCode.oldLineNumber}</p><pre>{oldCode.value}</pre>
                                <span>{oldCode.lineNumber}</span>
                                <pre>{oldCode.value}</pre>
                            </div>
                        {/each}
                    {/if}
                {/each}
            </div>
        </div>
    {/each}
{/if}

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

    .merge-input {
        border: none;
        width: 100%;
        background-color: #ff5b14;
        color: wheat;
        font-family: monospace;
    }

    .removable-merge-item > textarea {
        height: 1rem;
        resize: vertical;
        overflow-y: hidden;

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
        display: flex;
    }

    .code-diff p {
        margin: 0;
        padding-right: 10px;
        word-break: keep-all;
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
    
    .added:hover, .removed:hover {
        cursor: pointer;
    }

    /*pre {*/
    /*    white-space: pre-wrap;*/
    /*    margin: 0;*/
    /*}*/
</style>
