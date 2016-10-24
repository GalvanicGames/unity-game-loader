var unityLoaderJS = {
    isVisible: null,

    ulInitialize: function() {
        this.isVisible = (function(){
            var stateKey, eventKey, keys = {
                hidden: "visibilitychange",
                webkitHidden: "webkitvisibilitychange",
                mozHidden: "mozvisibilitychange",
                msHidden: "msvisibilitychange"
            };

            for (stateKey in keys) {
                if (stateKey in document) {
                    eventKey = keys[stateKey];
                    break;
                }
            }
    
            return function(c) {
                if (c) document.addEventListener(eventKey, c);
                return !document[stateKey];
            }
        })();
    },

        ulIsTabActive: function() {
            if (this.isVisible == null) {
                console.error("ulIsTabActive invoked but has not been initialized!");
                return false;
            }

            return this.isVisible();
    }
};

mergeInto(LibraryManager.library, unityLoaderJS);
