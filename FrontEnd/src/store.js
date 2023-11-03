import { persisted } from "svelte-local-storage-store";

export const progressInformationMessageStore = persisted('progressInformationMessageStore', null);
export const errorMessageStore = persisted('errorMessageStore', null);
export const oldCodeStore = persisted('oldCodeStore', null);
export const newCodeStore = persisted('newCodeStore', null);