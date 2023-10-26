import { writable } from "svelte/store";

export const oldCodeStore = writable(null);
export const newCodeStore = writable(null);
export const successMessage = writable(null);
export const errorMessage = writable(null);