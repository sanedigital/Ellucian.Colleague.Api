// Copyright 2023-2024 Ellucian Company L.P. and its affiliates.
// base cacheManagement namespace
(function (admin, $, undefined) {
}(window.cacheManagement = window.cacheManagement || {}, jQuery));


// configuration namespace
(function (configuration, $, undefined) {

    configuration.CacheManagementViewModel = function (cacheManagementModel) {
        var self = this;

        self.application = ko.observable(cacheManagementModel.Application);
        self.host = ko.observable(cacheManagementModel.Host);
        self.cacheKeys = ko.observableArray(cacheManagementModel.CacheKeys);
        self.errorMessage = ko.observable(cacheManagementModel.ErrorMessage);
        self.confirmationUrl = cacheManagementModel.ConfirmationUrl;
        self.cacheClearedComplete = ko.observable(false);
        self.processing = ko.observable(false);
        self.isRetrievingCachedValue = ko.observable(false);
        self.viewCacheUrl = "";
        self.clearCacheUrl = "";
        self.loadedKeysForView = ko.observable({});
        self.filterText = ko.observable("");

        self.filteredCacheKeys = ko.computed(function () {
            return self.cacheKeys().filter(function (str) {
                return str.includes(self.filterText());
            });
        });
        self.viewCacheContents = function (key) {
            if (self.isValid() && !self.loadedKeysForView()[key]) {
                self.isRetrievingCachedValue(true);
                self.errorMessage('');
                $.ajax({
                    url: self.viewCacheUrl + '?key=' + key,
                    type: 'GET',
                    success: function (result) {
                        if (result.Result) {
                            self.loadedKeysForView()[key] = result.Result;
                            self.loadedKeysForView.valueHasMutated();
                        }
                        self.isRetrievingCachedValue(false);

                    },
                    error: function (jqXHR, exception) {
                        self.errorMessage(exception);
                        console.error(exception);
                        self.isRetrievingCachedValue(false);
                    }
                });

            }
        }
        self.clearCacheKeys = function (keys) {
            if (!self.processing() && keys && keys.length > 0) {
                self.processing(true);
                $.ajax({
                    url: self.clearCacheUrl,
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify(keys),
                    success: function (result) {

                        if (result.RemovedKeys && result.RemovedKeys.length > 0) {
                            result.RemovedKeys.forEach((key) => {
                                self.cacheKeys.remove(key);
                            });
                        }

                        self.processing(false);
                        self.cacheClearedComplete(true);
                        setTimeout(function () {
                           self.cacheClearedComplete(false);
                        }, 500);  
                    },
                    error: function (jqXHR, exception) {
                        if (jqXHR.status == 503) {
                            setTimeout(() => {
                                location.href = self.confirmationUrl;
                            }, 3000);
                        }
                        self.processing(false);
                    }
                });
            }
        };

        self.cancelUrl = "";
        self.cancelKeyManagement = function () {
            location.href = self.cancelUrl;
        }

        self.errors = ko.validation.group(self);
        self.isValid = function () {
            if (self.errors().length > 0) {
                var message = "There are errors on this page\n\n" + self.errors().join("\n") + "\n\nPlease correct and try again.";
                alert(message);
                return false;
            } else {
                return true;
            }
        }
    }

}(window.cacheManagement.configuration = window.cacheManagement.configuration || {}, jQuery));