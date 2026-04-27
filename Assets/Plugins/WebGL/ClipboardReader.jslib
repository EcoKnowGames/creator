mergeInto(LibraryManager.library, {
    ReadClipboardText: function (gameObjectNamePtr, callbackMethodPtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var callbackMethod = UTF8ToString(callbackMethodPtr);
        navigator.clipboard.readText().then(function (text) {
            SendMessage(gameObjectName, callbackMethod, text);
        }).catch(function (err) {
            console.error("Clipboard read failed: " + err);
            SendMessage(gameObjectName, callbackMethod, "");
        });
    }
});
