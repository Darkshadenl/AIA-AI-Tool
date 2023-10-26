//@ts-check

import { oldCodeStore, newCodeStore, successMessage, errorMessage } from "../../store.js";

export const load = (async () => {
  let oldCode;
  oldCodeStore.subscribe((value) => {
    oldCode = value;
  });

  let newCode;
  newCodeStore.subscribe((value) => {
		newCode = value;
	});

  let success;
  successMessage.subscribe((value) => {
    success = value;
  });

  let error;
  errorMessage.subscribe((value) => {
    error = value;
  });

  if (oldCode && newCode) return { oldCode: oldCode, newCode: newCode };
  if (success) return { successMessage: success};
  if (error) return { errorMessage: error };
});