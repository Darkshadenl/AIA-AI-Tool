<script>
    import {errorMessageStore, newCodeStore, oldCodeStore, progressInformationMessageStore} from "../../store.js";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    let newCode;
    let oldCode;
    newCodeStore.subscribe((value) => newCode = value);
    oldCodeStore.subscribe((value) => oldCode = value);

    let newSelection = {};
    let oldSelection = {};

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
    <div class="code">
        {#if oldCode}
            {#each oldCode as file}
                <h2>{file.fileName}</h2>

                {#each file.diff as diff}
                    {#if !diff.added}
                        <div class="code-diff {diff.removed ? 'removed' : 'unchanged'} {oldSelection[diff.content] ? 'selected-old' : ''}" role="button"
                             class:selected-old={oldSelection[diff.content]}>
                            <pre>{diff.content}</pre>
                        </div>
                    {/if}
                {/each}
            {/each}
        {/if}
    </div>

    <div class="code">
        {#if newCode}
            {#each newCode as file}
                <h2>{file.fileName}</h2>

                {#each file.diff as diff}
                    {#if !diff.removed}
                        <div class="code-diff {diff.added ? 'added' : 'unchanged'} {newSelection[diff.content] ? 'selected-new' : ''}"
                             role="button">
                            <pre>{diff.content}</pre>
                        </div>

                    {/if}
                {/each}
            {/each}
        {/if}
    </div>
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
    }

    .code {
        display: flex;
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
