<script>
  import Code from '$lib/+code.svelte';
  import { errorMessageStore, newCodeStore, oldCodeStore, progressInformationMessageStore } from "../../store.js";

    let progressInformationMessage;
    let errorMessage;
    progressInformationMessageStore.subscribe((value) => progressInformationMessage = value);
    errorMessageStore.subscribe((value) => errorMessage = value);

    let oldCode;
    let newCode;
    oldCodeStore.subscribe((value) => oldCode = value);
    newCodeStore.subscribe((value) => newCode = value);

    let newCodeSelection = [];
    let oldCodeSelection = [];

    console.log("oldcode:");
    console.log(oldCode)
    console.log("newcode")
    console.log(newCode)

    $: if (newCode) {
        newCode.forEach(code => {
            code.diff.forEach(diff => {
                if (!diff.removed) {
                    newCodeSelection[diff.value] = newCodeSelection[diff.value] || false;
                }
            });
        });
    }

    $: if (oldCode) {
        oldCode.forEach(code => {
            code.diff.forEach(diff => {
                if (!diff.added && diff.removed) {
                    newCodeSelection[diff.value] = newCodeSelection[diff.value] || false;
                }
            });
        });
    }

    /**
     * @param {{removed: any, added: any, value:string}} val
     */
    const addToNewCodeSelection = (val) => {
        if (val.added) {
            if (val.value in newCodeSelection) {
                newCodeSelection[val.value] = !newCodeSelection[val.value];
                console.log(newCodeSelection[val.value])
            }
            newCodeSelection = {...newCodeSelection};
        }
    }

    /**
     * @param {{removed: any, added: any, value:string}} val
     */
    const addToOldCodeSelection = (val) => {
        if (!val.added) {
            if (val.value in newCodeSelection) {
                oldCodeSelection[val.value] = !oldCodeSelection[val.value];
                console.log(oldCodeSelection[val.value])
            }
            oldCodeSelection = {...oldCodeSelection};
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
        <!--                     class:selected-new={newCodeSelection[diff.value]}-->
        <!--                     on:click={() => addToNewCodeSelection(diff)}-->
        <!--                     on:keydown={() => addToNewCodeSelection(diff)} tabindex="0">-->
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
