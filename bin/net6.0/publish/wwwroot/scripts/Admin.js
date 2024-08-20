// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
// base admin namespace
(function (admin, $, undefined) {
}(window.admin = window.admin || {}, jQuery));


// configuration namespace
(function (configuration, $, undefined) {

    // local settings viewmodel: Connection Settings
    configuration.LocalSettingsViewModel = function (localSettings) {
        var self = this;

        self.isUpdating = ko.observable(localSettings.isUpdating);
        self.isUpdating(false);
        self.showValidationSummary = ko.observable(localSettings.showValidationSummary);
        self.showValidationSummary(false);//Initially, hide validation summary block
        self.validationSummary = ko.observable(localSettings.validationSummary);
        self.hideDialog = ko.observable(localSettings.hideDialog);
        self.hideDialog(false);

        // APP listener settings
        self.accountName = ko.observable(localSettings.AccountName)
            .extend(
            {
                required: { message: "Account DMI Registry Name is required" }
            });
        self.connectionPoolSize = ko.observable(localSettings.ConnectionPoolSize)
            .extend(
            {
                required: { message: "Connection Pool Size is required" },
                min: { params: 0, message: "App Connection Pool Size should be greater than or equal to 0" }
            });
        self.hostNameOverride = ko.observable(localSettings.HostNameOverride);
        self.ipAddress = ko.observable(localSettings.IpAddress)
            .extend(
            {
                required: { message: "DMI Application Listener IP Address is required" }
            });
        self.port = ko.observable(localSettings.Port)
            .extend(
            {
                required: { message: "DMI Application Listener Port is required" },
                min: { params: 1, message: "DMI App Listener Port should be greater than or equal to 1" },
                max: { params: 65535, message: "DMI App Listener Port should be less than or equal to 65535" }
            });
        self.secure = ko.observable(localSettings.Secure);

        // DAS listener settings
        self.useDasDatareader = ko.observable(localSettings.UseDasDatareader);
        self.dasAccountName = ko.observable(localSettings.DasAccountName)
            .extend(
            {
                required: { message: "Account DAS Registry Name is required", onlyIf: function () { return self.useDasDatareader() === true } }
            });
        self.dasIpAddress = ko.observable(localSettings.DasIpAddress)
            .extend(
            {
                required: { message: "DAS Listener IP Address is required", onlyIf: function () { return self.useDasDatareader() === true } }
            });
        var dasPort = (localSettings.DasPort === 0) ? "" : localSettings.DasPort;
        self.dasPort = ko.observable(dasPort)
            .extend(
            {
                required: { message: "DAS Listener Port is required", onlyIf: function () { return self.useDasDatareader() === true } },
                min: { params: 1, message: "DAS Listener Port should be greater than or equal to 1", onlyIf: function () { return self.useDasDatareader() === true } },
                max: { params: 65535, message: "DAS App Listener Port should be less than or equal to 65535", onlyIf: function () { return self.useDasDatareader() === true } }
            });
        self.dasSecure = ko.observable(localSettings.DasSecure);
        self.dasHostNameOverride = ko.observable(localSettings.DasHostNameOverride);
        var dasConnectionPoolSize = (localSettings.DasConnectionPoolSize === 0) ? "" : localSettings.DasConnectionPoolSize;
        self.dasConnectionPoolSize = ko.observable(dasConnectionPoolSize)
            .extend(
            {
                required: { message: "DAS Connection Pool Size is required", onlyIf: function () { return self.useDasDatareader() === true } },
                min: { params: 1, message: "DAS Connection Pool Size should be greater than or equal to 1", onlyIf: function () { return self.useDasDatareader() === true } }
            });
        self.dasUsername = ko.observable(localSettings.DasUsername)
            .extend(
            {
                required: { message: "DAS Username is required", onlyIf: function () { return self.useDasDatareader() === true } }
            });
        self.dasPassword = ko.observable(localSettings.DasPassword)
            .extend(
            {
                required: { message: "DAS Password is required", onlyIf: function () { return self.useDasDatareader() === true } }
                });

        // OAUTH settings
        self.oauthIssuerUrl = ko.observable(localSettings.OauthIssuerUrl);
        self.oauthProxyUsername = ko.observable(localSettings.OauthProxyUsername);
        self.oauthProxyPassword = ko.observable(localSettings.OauthProxyPassword);

        // Shared secret
        self.sharedSecret1 = ko.observable(localSettings.SharedSecret1)
            .extend(
            {
                required: { message: "Shared Secret is required" }, minLength: 8, maxLength: 20
            });
        self.sharedSecret2 = ko.observable(localSettings.SharedSecret2)
            .extend(
            {
                required: { message: "Confirm Shared Secret is required" },
                equal: { params: self.sharedSecret1, message: "Confirm Shared Secret and Shared Secret must match" }
            });

        console.log(localSettings);
        self.logLevels = ko.observableArray(localSettings.LogLevels);
        var currentlogLevel = ko.utils.arrayFilter(self.logLevels(), function (level) {
            return level.Value === localSettings.LogLevel;
        });
        self.logLevel = ko.observable(currentlogLevel[0]);


        self.cultures = ko.observableArray(localSettings.SupportedCultures);
        var currentCulture = ko.utils.arrayFilter(self.cultures(), function (culture) {
            return culture.Value === localSettings.DefaultCulture;
        });
        self.culture = ko.observable(currentCulture[0]);

        self.culturesUi = ko.observableArray(localSettings.SupportedUiCultures);
        var currentUiCulture = ko.utils.arrayFilter(self.culturesUi(), function (culture) {
            return culture.Value === localSettings.DefaultUiCulture;
        });
        self.uiCulture = ko.observable(currentUiCulture[0]);

        self.profileName = ko.observable(localSettings.ProfileName);

        // Error profiles for each section of connection settings page
        self.appErrors = ko.validatedObservable({ p1: self.accountName, p2: self.ipAddress, p3: self.port, p4: self.secure, p5: self.hostNameOverride, p6: self.connectionPoolSize });
        self.dasErrors = ko.validatedObservable({ p1: self.dasAccountName, p2: self.dasIpAddress, p3: self.dasPort, p4: self.dasSecure, p5: self.dasConnectionPoolSize, p6: self.dasHostNameOverride, p7: self.dasUsername, p8: self.dasPassword });
        self.secretErrors = ko.validatedObservable({ p1: self.sharedSecret1, p2: self.sharedSecret2 });

        self.saveLocalSettings = function (validate) {
            var valid = true;
            if (validate) {
                valid = self.isValid();
            }
            if (valid) {

                self.isUpdating(true);

                var model = ko.toJS(self);
                console.log(model);
                if (model.logLevel.Value) {
                    model.logLevel = model.logLevel.Value;
                }
                if (model.culture.Value) {
                    model.defaultCulture = model.culture.Value;
                }
                if (model.uiCulture.Value) {
                    model.defaultUiCulture = model.uiCulture.Value;
                }
                ko.utils.postJson(location.href, { model: model });

            }
        }

        self.cancelUrl = "";
        self.cancelLocalSettings = function () {
            location.href = self.cancelUrl;
        }

        self.testAppConnection = function () {
            if (!self.appErrors.isValid() || !self.secretErrors.isValid()) {
                var message = "There are errors on this page\n\n";
                if (self.appErrors.errors().length > 0) {
                    message += self.appErrors.errors().join("\n") + "\n";
                }
                if (self.secretErrors.errors().length > 0) {
                    message += self.secretErrors.errors().join("\n") + "\n";
                }
                message += "\nPlease correct and try again.";

                alert(message);
            } else {
                $("#dialog-form").dialog();
            }
        }

        //Test App connection Test Button Click
        self.testAppConnection_TestBtnClick = function () {
            var model = new Object();
            model.AccountName = self.accountName();
            model.IpAddress = self.ipAddress();
            model.Port = self.port();
            model.Secure = self.secure();
            model.ConnectionPoolSize = self.connectionPoolSize();
            model.HostNameOverride = self.hostNameOverride();
            model.SharedSecret1 = self.sharedSecret1();
            model.SharedSecret2 = self.sharedSecret1();
            model.UserId = $('#UserId').val();
            model.Password = $('#Password').val();
            var theDialog = $(this);
            $.ajax({
                url: 'TestAppConnection',
                data: JSON.stringify(model),
                type: 'POST',
                contentType: 'application/json',
                dataType: 'json',
                success: function (data, textStatus, jqXHR) {
                    alert(data);
                    $('#dialog-form').dialog('close');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    var responseText = null;
                    try {
                        responseText = $.parseJSON(jqXHR.responseText);
                    }
                    catch (e) {
                        responseText = null;
                    }

                    if (responseText != null) {
                        alert(responseText);
                    }
                    else {
                        var message = "HTTP Error " + jqXHR.status + ": " + jqXHR.statusText;
                        if (jqXHR.status == "404") {
                            message += ".  You may get this error if you recently upgrade the Colleague Web API and did not clear your browser cache.";
                        }
                        alert(message);
                    }
                },
                complete: function () { }
            });
        }

        //Test App connection Test Button Click
        self.testAppConnection_CancelBtnClick = function () {
            $('#dialog-form').dialog('close');
        }
        // Test DAS listener connection
        self.testDASConnection = function () {
            if (!self.dasErrors.isValid()) {
                alert("There are errors on this page\n\n" + self.dasErrors.errors().join("\n") + "\n\nPlease correct and try again.");
            } else {
                var model = new Object();
                model.DasAccountName = self.dasAccountName();
                model.DasIpAddress = self.dasIpAddress();
                model.DasPort = self.dasPort();
                model.DasSecure = self.dasSecure();
                model.DasConnectionPoolSize = self.dasConnectionPoolSize();
                model.DasHostNameOverride = self.dasHostNameOverride();
                model.DasUsername = self.dasUsername();
                model.DasPassword = self.dasPassword();
                var theDialog = $(this);
                $.ajax({
                    url: 'TestDASConnection',
                    data: JSON.stringify(model),
                    type: 'POST',
                    contentType: 'application/json',
                    dataType: 'json',
                    success: function (data, textStatus, jqXHR) {
                        alert(data);
                        theDialog.dialog("close");
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        var responseText = null;
                        try {
                            responseText = $.parseJSON(jqXHR.responseText);
                        }
                        catch (e) {
                            responseText = null;
                        }

                        if (responseText != null) {
                            alert(responseText);
                        }
                        else {
                            var message = "HTTP Error " + jqXHR.status + ": " + jqXHR.statusText;
                            if (jqXHR.status == "404") {
                                message += ".  You may get this error if you recently upgrade the Colleague Web API and did not clear your browser cache.";
                            }
                            alert(message);
                        }
                    },
                    complete: function () { }
                });
            }
        }

        //Test OAuth connection Test Button Click
        self.testOauthConnection_TestBtnClick = function () {
            var model = new Object();
            model.OauthIssuerUrl = self.oauthIssuerUrl();
            model.UserId = self.oauthProxyUsername();
            model.Password = self.oauthProxyPassword();
            var theDialog = $(this);
            $.ajax({
                url: 'TestOauthSettings',
                data: JSON.stringify(model),
                type: 'POST',
                contentType: 'application/json',
                dataType: 'json',
                success: function (data, textStatus, jqXHR) {
                    alert(data);
                    $('#dialog-form').dialog('close');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    var responseText = null;
                    try {
                        responseText = $.parseJSON(jqXHR.responseText);
                    }
                    catch (e) {
                        responseText = null;
                    }

                    if (responseText != null) {
                        alert(responseText);
                    }
                    else {
                        var message = "HTTP Error " + jqXHR.status + ": " + jqXHR.statusText;
                        if (jqXHR.status == "404") {
                            message += ".  You may get this error if you recently upgrade the Colleague Web API and did not clear your browser cache.";
                        }
                        alert(message);
                    }
                },
                complete: function () { }
            });
        }

        self.errors = ko.validation.group(self);
        self.isValid = function () {
            if (self.useDasDatareader() === true) {
                var message = "";
                if (self.errors().length > 0) {
                    message += self.errors().join("\n");
                    self.validationSummary(message);
                    self.showValidationSummary(true);
                    return false;
                } else {
                    return true;
                }
            } else {
                // Otherwise, only validate App and shared secret portions
                if ((self.appErrors.errors().length > 0) || (self.secretErrors.errors().length > 0)) {
                    var message = "";
                    if (self.appErrors.errors().length > 0) {
                        message += self.appErrors.errors().join("\n") + "\n";
                    }
                    if (self.secretErrors.errors().length > 0) {
                        message += self.secretErrors.errors().join("\n") + "\n";
                    }
                    self.validationSummary(message);
                    self.showValidationSummary(true);
                    return false;
                } else {
                    return true;
                }
            }
        }

        // If the shared secret is more than 100 characters long, it wasn't decrypted successfully
        // (encrypted strings are at least 160 characters long, but we'll use 100 just in case).
        // The shared secret is not supposed to be longer than 20 anyway (form validation allows up to 20 chars).
        // The cause is likely that the machine key used to encrypt the shared secret has been reset,
        // so we let the admin know that the shared secret needs to be re-entered.
        self.showSharedSecretDecryptionError = ko.observable(false);
        if (localSettings.SharedSecret1.length > 100) {
            self.showSharedSecretDecryptionError = ko.observable(true);
        }

        self.showMachineKeySettingError = ko.observable(false);
        if (localSettings.MachineKeySettingError.length > 0) {
            self.showMachineKeySettingError = ko.observable(true);
        }
        self.machineKeySettingError = ko.observable(localSettings.MachineKeySettingError);

        self.showMachineKeySettingWarning = ko.observable(false);
        if (localSettings.MachineKeySettingWarning.length > 0) {
            self.showMachineKeySettingWarning = ko.observable(true);
        }
        self.machineKeySettingWarning = ko.observable(localSettings.MachineKeySettingWarning);
    }
    // end local settings viewmodel

    // api settings profile viewmodel
    configuration.ApiSettingsProfileViewModel = function (apiSettingsProfileModel) {
        var self = this;

        self.currentProfileName = ko.observable(apiSettingsProfileModel.CurrentProfileName);
        self.existingProfileNames = ko.observableArray(apiSettingsProfileModel.ExistingProfileNames);
        self.selectedExistingProfileName = ko.observable(apiSettingsProfileModel.SelectedExistingProfileName);

        self.newProfileName = ko.observable(apiSettingsProfileModel.NewProfileName).extend({
            validation: {
                validator: function (val, otherVal) {
                    var rt = true;
                    if (val != null && val != "") {
                        $.each(self.existingProfileNames(), function (index, value) {
                            if (index > 0) {
                                var x = value.Text.toUpperCase().replace(new RegExp(" ", 'g'), "");
                                if (x == val) {
                                    rt = false;
                                }
                            }
                        });
                        if (rt) {
                            var x = self.currentProfileName().toUpperCase().replace(new RegExp(" ", 'g'), "");
                            if (x == val) {
                                rt = false;
                            }
                        }
                    }
                    return rt;
                },
                message: 'New Profile Name exists in Colleague!',
                params: null
            }
        });

        self.errorMessage = apiSettingsProfileModel.ErrorMessage;

        self.selectedExistingProfileName.subscribe(function () {
            if (self.selectedExistingProfileName().Value != "") {
                self.newProfileName("");
            }
        });

        self.newProfileName.subscribe(function () {
            if (self.newProfileName() != "") {
                var value = self.newProfileName();
                value = value.toUpperCase();
                value = value.replace(new RegExp(" ", 'g'), "");
                self.newProfileName(value);
                // clear dropdown
                self.selectedExistingProfileName(self.existingProfileNames()[0]);
            }
        });

        self.saveApiSettingsProfile = function () {
            if (self.isValid()) {
                var model = ko.toJS(self);
                ko.utils.postJson(location.href, { model: model });
            }
        }
        self.cancelUrl = "";
        self.cancelApiSettingsProfile = function () {
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
    // end api settings profile viewmodel

    // api settings (Cachings, Photos, Reports tabs) viewmodel
    configuration.ApiSettingsViewModel = function (apiSettings) {
        var self = this;

        self.id = ko.observable(apiSettings.Id);
        self.version = ko.observable(apiSettings.Version);
        self.profileName = ko.observable(apiSettings.ProfileName);
        self.TabSelected = ko.observable(1);

        // photo settings
        var photoSettings = apiSettings.PhotoSettings;
        self.PhotoSettings = new Object();
        self.PhotoSettings.basePhotoUrl = ko.observable(photoSettings.BasePhotoUrl);

        self.PhotoSettings.imageExtension = ko.observable(photoSettings.ImageExtension);
        self.PhotoSettings.imageTypes = ko.observableArray(photoSettings.ImageTypes);
        var x = ko.utils.arrayFirst(self.PhotoSettings.imageTypes(), function (imageType) {
            return (imageType.Key == photoSettings.SelectedImageType.Key);
        });
        self.PhotoSettings.selectedImageType = ko.observable(x);

        self.PhotoSettings.customHeaders = ko.observableArray(ko.utils.arrayMap(photoSettings.CustomHeaders, function (customHeader) {
            return {
                Key: customHeader.Key, Value: customHeader.Value
            };
        }));

        self.PhotoSettings.addCustomHeader = function () {
            // Allow custom headers only when there is a base url
            if (self.PhotoSettings.basePhotoUrl().length <= 0) {
                var message = "Please specify an Image Server Base URL to add Custom Headers\n";
                alert(message);
            }
            else {
                self.PhotoSettings.customHeaders.push({
                    Key: "",
                    Value: ""
                });
            }
        };

        self.PhotoSettings.removeCustomHeaders = function (customHeader) {
            self.PhotoSettings.customHeaders.remove(customHeader);
        };

        self.PhotoSettings.urlPreview = ko.computed(function () {
            var result = "Enter a Photo Server Base URL to produce a preview..."
            if (self.PhotoSettings.basePhotoUrl() != null && self.PhotoSettings.basePhotoUrl() != "") {
                result = self.PhotoSettings.basePhotoUrl() + "1234567";
                if (self.PhotoSettings.imageExtension() != null && self.PhotoSettings.imageExtension() != "") {
                    result += "." + self.PhotoSettings.imageExtension();
                }
            }
            return result;
        });
        // end photo settings

        // report settings
        var reportSettings = apiSettings.ReportSettings;
        self.ReportSettings = new Object();
        self.ReportSettings.reportLogoPath = ko.observable(reportSettings.ReportLogoPath);
        self.ReportSettings.unofficialWatermarkPath = ko.observable(reportSettings.UnofficialWatermarkPath);

        self.ReportSettings.logoSrc = ko.computed(function () {
            var baseUrl = document.location.href;
            baseUrl = baseUrl.replace("Admin/ApiSettings", "");
            var logoPath = self.ReportSettings.reportLogoPath();
            if (logoPath !== null && logoPath.length > 0) {
                return baseUrl + logoPath;
            } else {
                return "";
            }
        });
        self.ReportSettings.watermarkSrc = ko.computed(function () {
            var baseUrl = document.location.href;
            baseUrl = baseUrl.replace("Admin/ApiSettings", "");
            var watermarkPath = self.ReportSettings.unofficialWatermarkPath();
            if (watermarkPath !== null && watermarkPath.length > 0) {
                return baseUrl + watermarkPath;
            } else {
                return "";
            }
        });
        // end report settings

        // save api settings
        self.saveApiSettings = function () {
            if (self.isValid()) {
                var model = ko.toJS(self);
                ko.utils.postJson(location.href, { model: model });
            }
        }

        // cancel from page
        self.cancelUrl = "";
        self.cancelApiSettings = function () {
            location.href = self.cancelUrl;
        }

        //
        // Validation functions 
        //
        self.errors = ko.validation.group(self);
        self.isValid = function () {
            isValid = true;
            var tabsValid = self.PhotoSettings.isValid() && self.ReportSettings.isValid();
            if ((self.errors().length > 0) || !(tabsValid)) {
                isValid = false;
            };
            return isValid;
        };

        self.PhotoSettings.isValid = function () {
            // Ensure that all photo settings are valid
            var message = "There are errors on this page on the Photos tab.\n";

            isValid = true;

            var photoUrl = self.PhotoSettings.basePhotoUrl();
            if (photoUrl != "") {
                // Only need to check the remaining settings when a base URL is specified
                if (self.PhotoSettings.imageExtension()) {
                    var origString = self.PhotoSettings.imageExtension();
                    // Check if there are non-alphanumeric characters in the image extension
                    var testString = origString.replace(/[^a-zA-Z0-9]/g, "");
                    if (testString != origString) {
                        message = message + "   Image Extension contains non-alphanumeric characters.\n";
                        isValid = false;
                    }
                }
                var headerArray = self.PhotoSettings.customHeaders();
                var headerCount = headerArray.length;
                if (headerCount > 0) {
                    var headerIndex = 0;
                    while (headerIndex < headerCount) {
                        var headerName = headerArray[headerIndex].Key;
                        var headerValue = headerArray[headerIndex].Value;
                        // Check for empty Header Name/Value pairs
                        if ((headerValue == "") && (headerName == "")) {
                            message = message + "   Please remove empty Custom Header row " + (headerIndex + 1) + "\n";
                            isValid = false;
                        }
                            // Check that each Header Value has a Name
                        else if ((headerValue !== "") && (headerName == "")) {
                            message = message + "   Value in Custom Header row " + (headerIndex + 1) + " has no Name\n";
                            isValid = false;
                        }
                        ++headerIndex;
                    }
                }
            }
            message = message + "\nPlease correct and try again.";
            if (isValid) {
                return true;
            } else {
                alert(message);
                return false;
            }
        }

        self.ReportSettings.isValid = function () {
            // Ensure that all report settings are valid
            var message = "There are errors on this page on the Reports tab:\n";

            isValid = true;

            message = message + "\nPlease correct and try again.";

            // The Logo Path and Watermark Path for reports are optional and self-testing

            if (isValid) {
                return true;
            } else {
                alert(message);
                return false;
            }
        }
    }

    configuration.KeyManagementViewModel = function (keyManagementModel) {
        var self = this;

        self.fixedKey = ko.observable(keyManagementModel.FixedKey);
        self.keyStrategy = ko.observable(keyManagementModel.KeyStrategy);
        self.keyStrategies = ko.observableArray(keyManagementModel.KeyStrategies);
        self.keyPath = ko.observable(keyManagementModel.KeyPath);
        self.errorMessage = ko.observable(keyManagementModel.ErrorMessage);
        self.confirmationUrl = keyManagementModel.ConfirmationUrl;
        self.savedComplete = ko.observable(false);

        self.saveKeyManagement = function () {
            if (self.isValid()) {
                var model = ko.toJSON(self);
                //ko.utils.postJson(location.href, { model: model });
                console.log(model);
                $.ajax({
                    url: location.href,
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: model,
                    success: function (result) {

                    },
                    error: function(jqXHR, exception) {
                        if (jqXHR.status == 503) {
                            self.savedComplete(true);
                            // this is what we expect from an application restart.
                            // wait for several seconds for application to restart then redirect
                            setTimeout(() => {
                                location.href = self.confirmationUrl;
                            }, 3000);
                        }
                    }
                });
            }
        }
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

}(window.admin.configuration = window.admin.configuration || {}, jQuery));