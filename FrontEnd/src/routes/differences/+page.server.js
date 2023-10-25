//@ts-check

import { oldCodeStore, newCodeStore } from "../../store.js";

/**
 * Create API connection using SignalR
 * @type {import('@sveltejs/kit').Load}
 */
export const load = (async () => {
  let oldCode;
  oldCodeStore.subscribe((value) => oldCode = value);

  let newCode;
  newCodeStore.subscribe((value) =>  newCode = value);

  return { oldCode: oldCode, newCode: newCode }
});