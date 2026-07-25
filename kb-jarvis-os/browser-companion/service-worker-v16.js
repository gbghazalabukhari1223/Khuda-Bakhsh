importScripts('service-worker.js', 'wordpress-v16.js');

const executeOperationV15Preserved = executeOperation;
executeOperation = async function executeOperationOperationalV16(operation, payload) {
  if (operation === 'wordpress.content') {
    return await operateWordPressContentV16(payload ?? {});
  }
  return await executeOperationV15Preserved(operation, payload ?? {});
};
