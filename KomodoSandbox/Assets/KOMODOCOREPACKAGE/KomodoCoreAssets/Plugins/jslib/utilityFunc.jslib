var utilityFunc = {

// Example method for getting the parent URL
    GetParentURL: function() {
        return document.referrer;
    }
 
};

mergeInto(LibraryManager.library, utilityFunc);
