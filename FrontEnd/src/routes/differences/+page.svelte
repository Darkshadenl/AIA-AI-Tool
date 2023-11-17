<script>
  import { errorMessageStore, newCodeStore, progressInformationMessageStore } from "../../store.js";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    let code;
    newCodeStore.subscribe((value) => code = value);

    let selection = [];

    console.log("newcode")
    console.log(code)

    $: if (code) {
        code.forEach(code => {
            code.diff.forEach(diff => {
                selection[diff.value] = selection[diff.value] || false;
            });
        });
    }

    /**
     * @param {{removed: any, added: any, value:string}} val
     */
    const addToCodeSelection = (val) => {
        if (val.added) {
            if (val.value in selection) {
                selection[val.value] = !selection[val.value];
                console.log(selection[val.value])
            }
            selection = {...selection};
        }
    }

</script>

<h1>Differences</h1>

{#if progressInformationMessage && errorMessage === null}
    <p>{progressInformationMessage}</p>
{/if}

{#if errorMessage}
    <p>An exception occurred: {errorMessage}</p>
{/if}

<div class="differences-container">
    <div class="code">
        <!--{#if oldCode}-->
        <!--        <h2>{oldCode.fileName}</h2>-->

        <!--        {#each oldCode.diff as diff}-->
        <!--            {#if !diff.added}-->
        <!--                <div class={diff.removed ? 'removed' : 'unchanged'} role="button"-->
        <!--                     class:selected-old={oldCodeSelection[diff.value]}-->
        <!--                     on:click={() => addToOldCodeSelection(diff)}-->
        <!--                     on:keydown={() => addToOldCodeSelection(diff)} tabindex="0">-->
        <!--                    <pre>{diff.value}</pre>-->
        <!--                </div>-->
        <!--            {/if}-->
        <!--        {/each}-->
        <!--{/if}-->
    </div>

    <div class="code">
        <!--{#if newCode}-->
        <!--        <h2>{newCode.fileName}</h2>-->

        <!--        {#each newCode.diff as diff}-->
        <!--            {#if !diff.removed}-->
        <!--                <div class={diff.added ? 'added' : 'unchanged'} role="button"-->
        <!--                     class:selected-new={selection[diff.value]}-->
        <!--                     on:click={() => addToCodeSelection(diff)}-->
        <!--                     on:keydown={() => addToCodeSelection(diff)} tabindex="0">-->
        <!--                    <pre>{diff.value}</pre>-->
        <!--                </div>-->
        <!--            {/if}-->
        <!--        {/each}-->
        <!--{/if}-->
    </div>
</div>

<style>
    .column-container {
        column-count: 2;
        column-gap: 20px;
    }

    .code {
        width: 48%;
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

    .removed {
        background-color: rgba(241, 113, 130, 0.65);
        color: #24292e;
        text-decoration: line-through;
    }

    .unchanged {
        color: #24292e;
    }

    pre {
        white-space: pre-wrap;
        margin: 0;
    }
</style>
