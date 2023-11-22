<script>
    import {errorMessageStore, diffStore, progressInformationMessageStore} from "../../store.js";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    let diffDataStruct;
    diffStore.subscribe((value) => diffDataStruct = value);
    if (diffDataStruct) {
        console.log(diffDataStruct)
    }
    let selection = [];

    const handleClick = (diffId, diffItemId, old) => {
        let item = diffDataStruct[diffItemId].diffs[diffId];
        if (!(item.old && item.new))
            return;

        if (old) {
            diffDataStruct[diffItemId].diffs[diffId].selected =
                diffDataStruct[diffItemId].diffs[diffId].selected === "old" ? undefined : "old";
        } else if (!old) {
            diffDataStruct[diffItemId].diffs[diffId].selected =
                diffDataStruct[diffItemId].diffs[diffId].selected === "new" ? undefined : "new";
        }
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
                    <div class="code-diff {diff.old && diff.new ? 'removed' : 'unchanged'}"
                         tabindex="0"
                         class:selected-old={diff.selected === "old"}
                         on:click={handleClick(diff.id, diffItem.id, true)}
                         on:keydown={handleClick(diff.id, diffItem.id, true)}
                         role="button">
                        <pre>{diff.old}</pre>
                    </div>
                {/each}
            {/each}
        </div>

        <div class="code">
            {#each diffDataStruct as diffItem}
                <h2>{diffItem.fileName}</h2>

                {#each diffItem.diffs as diff}
                    {#if diff.old && diff.new}
                        <div class="code-diff {diff.old && diff.new ? 'added' : 'unchanged'}"
                             tabindex="0"
                             class:selected-new={diff.selected === "new"}
                             on:click={handleClick(diff.id, diffItem.id, false)}
                             on:keydown={handleClick(diff.id, diffItem.id, false)}
                             role="button">
                            <pre>{diff.new}</pre>
                        </div>
                    {:else}
                        <div class="code-diff unchanged" role="button">
                            <pre>{diff.old}</pre>
                        </div>
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
