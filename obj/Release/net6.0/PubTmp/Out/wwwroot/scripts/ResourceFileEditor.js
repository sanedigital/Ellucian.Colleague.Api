// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

(function (resourcefileeditor, $, undefined) {
    //Viewmodel for single resource entry
    var resourceItemViewModel = function (jsResItem) {
        var self = this;
        self.Key = ko.observable(jsResItem.Key);
        self.Value = ko.observable(jsResItem.Value);
        self.Comment = ko.observable(jsResItem.Comment);
        self.OriginalValue = ko.observable(jsResItem.OriginalValue);

        //Dirty flag to track if changes have been made and to skip the server request the first time values are loaded
        self.dirtyFlag = new ko.dirtyFlag(self);

        //Compare if current value is same as Ellucian-delivered value
        self.hasChanged = function () {
            return self.Value() != self.OriginalValue();
        }

        self.restoreOriginalValue = function (data) {
            self.Value(self.OriginalValue());
        }
    }

    //Viewmodel for single resource file
    var resourceFileViewModel = function (data) {
        var self = this;
        self.ResourceFilePath = ko.observable();
        self.ResourceFileName = ko.observable(data.ResourceFileName ? data.ResourceFileName : "");
        self.ResourceFileEntries = ko.observableArray(
            ko.utils.arrayMap(data.ResourceFileEntries, function (resEntry) { return new resourceItemViewModel(resEntry) }));

        self.dirtyItems = ko.computed(function () {
            return ko.utils.arrayFilter(self.ResourceFileEntries(), function (item) {
                return item.dirtyFlag.isDirty();
            });
        }, this);

        self.isDirty = ko.computed(function () {
            return self.dirtyItems().length > 0;
        }, this);


        //Set the unSavedData flag whenever the isDirty is being set to true, to alert during window unload
        self.isDirty.subscribe(function () {
            if (self.isDirty() == true) {
                unSavedDataFlag(true);
            }
        }
        );

        //Computed function to track changes in resource entry values
        self.ResourceItemValueChanged = ko.pureComputed(function () {
            return {
                ResourceFilePath: self.ResourceFilePath(),
                ResourceFileEntries: self.ResourceFileEntries().map(function (resItem) {
                    return { Key: resItem.Key(), Value: resItem.Value(), OriginalValue: resItem.OriginalValue(), Comment: resItem.Comment() };
                })
            };
        });


    }

    resourcefileeditor.ViewModel = function () {
        var self = this;
        self.SelectedFilePath = ko.observable();
        self.SelectedResourceFile = ko.observable();
        self.SelectedResourceCulture = ko.observable();
        self.PreviousSelectedResxFile = ko.observable();
        self.ResourceFilesAvailable = ko.observableArray();
        self.ResourceCulturesAvailable = ko.observableArray();
        self.RelativeFilePath = ko.observable();
        self.showDialog = ko.observable(false);
        self.cancelUrl = '';
        self.cancelLocalSettings = function () {
            location.href = self.cancelUrl;
        }

        self.cancelResourceFileEditor = function () {
            location.href = self.cancelUrl;
        }

        self.isLoading = ko.observable(false);
        self.isUpdating = ko.observable(false);

        //Get list of all resource files in the current working directory
        self.GetResourceFileNames = function () {
            $.ajax({
                type: 'GET',
                url: 'GetResourceFiles',
                contentType: 'application/json',
                data: { cultureCode: self.SelectedResourceCulture() },
                success: function (data) {
                    self.ResourceFilesAvailable(data);
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
                        alert(message);
                    }
                },
                complete: function () { }

            })
        };

        // Get current ui culture from settings which is set in configuration
        self.GetCurrentUICulture = function () {
            $.getJSON('GetCurrentUICulture', function (data) {
                self.SelectedResourceCulture(data);
            })
        };

        //Get list of all supported cultures as a key value pair of display name and culture code
        $.getJSON('GetSupportedUICultures', function (data) {
            self.ResourceCulturesAvailable(data);
            self.GetCurrentUICulture();
        });

        //self.OnResourceCultureChange
        self.SelectedResourceCulture.subscribe(function (cultureCode) {
            if (cultureCode) {
                self.GetResourceFileNames(cultureCode);
            }
        });

        //On selection change, refresh the resourceItems to the new list
        self.SelectedFilePath.subscribe(function (item) {
            if (item != undefined) //Get the resource items if a valid file is selected
            {
                var data = { filePath: item.toString() };

                $.ajax({
                    type: 'GET',
                    url: 'GetResourceItemsByFile',
                    contentType: 'application/json',
                    data: data,
                    beforeSend: function () {
                        self.isLoading(true);
                    }
                }).done(
                    function (data) {
                        var mappedData = new resourceFileViewModel(data);
                        var relativePathOfFile = self.SelectedFilePath().substring(self.SelectedFilePath().indexOf("Resources"), self.SelectedFilePath().length);

                        self.isLoading(false);
                        self.PreviousSelectedResxFile(self.SelectedResourceFile());
                        self.SelectedResourceFile(mappedData);
                        self.SelectedResourceFile().ResourceFilePath(self.SelectedFilePath());
                        self.RelativeFilePath(relativePathOfFile);//Display only relative path of selected file
                        $("table").makeTableResponsive();
                    })
                    .fail(function () { })
                    .always(function () { })

            }
            else //Clear the table binding to hide the table
            {
                self.SelectedResourceFile(null);
            }
        });

        self.SelectedFilePath.subscribe(function (item, e) {
            //Check if there are unsaved values before new resx is selected
            //self.checkUnsavedData();
            if (self.SelectedResourceFile() != undefined) //First time a file is selected, the selectedResourceFile will be undefined. Skip the unsaved data check
            {
                if (self.SelectedResourceFile().isDirty() == true) {
                    unSavedDataFlag(true);
                    self.showDialog(true);
                    $("#dialog-form").dialog();

                }
            }

        }, null, 'beforeChange');


        //Unsaved data check before selecting a new resx file to edit
        //self.checkUnsavedData = function (data) {
        //    if (self.SelectedResourceFile() != undefined) //First time a file is selected, the selectedResourceFile will be undefined. Skip the unsaved data check
        //    {
        //        if (self.SelectedResourceFile().isDirty() == true) {
        //            unSavedDataFlag(true);
        //            self.showDialog(true);
        //            $("#dialog-form").dialog();

        //        }
        //    }
        //}

        self.cancelBtnClick = function () {
            self.showDialog(false);
            $('#dialog-form').dialog('close');
        }

        self.saveResourceFile = function (fileToSave) {
            var file = ko.toJSON(fileToSave.ResourceItemValueChanged());

            //Save the updated resource file
            $.ajax({
                type: 'POST',
                url: 'SaveResourceFile',
                data: { model: file },
                beforeSend: function () {
                    self.isUpdating(true);
                    document.onkeydown = function (e) { return false; }
                },
                success: function (data) {
                    unSavedDataFlag(false);
                    ko.utils.arrayForEach(fileToSave.dirtyItems(), function (changedItem) {
                        changedItem.dirtyFlag.reset();
                    });
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
                        alert(message);
                    }
                },
                complete: function () {
                    self.isUpdating(false);
                    document.onkeydown = function (e) { return true; }
                }
            })
        }

        //Save kicked off from Save button on page
        self.saveResxFile = function () {
            if (unSavedDataFlag() == true) {
                var fileToSave = self.SelectedResourceFile();
                self.saveResourceFile(fileToSave);

            }
        }

        //Save kicked off from the modal popup
        self.saveUnsavedData = function () {
            if (unSavedDataFlag() == true) {
                var fileToSave = self.PreviousSelectedResxFile();
                self.saveResourceFile(fileToSave);
                $('#dialog-form').dialog('close');
            }
        }

    }


    var unSavedDataFlag = ko.observable(false);
    //Alert user for unsaved changes 
    window.onbeforeunload = confirmExit;

    function confirmExit(e) {
        if (unSavedDataFlag() == true) {
            return 'Changes will be lost if you leave the page. Do you want to save ?';
        }
    }


    //Deep validation for nested viewmodels
    ko.validation.init({
        grouping: {
            deep: true, live: true, observable: true

        }
    }, true);


    //Check if the values are dirty before attempting to save - since the save auotmatically fires during initialization too
    ko.dirtyFlag = function (root, isInitiallyDirty) {
        var result = function () { },
            _initialState = ko.observable(ko.toJSON(root)),
            _isInitiallyDirty = ko.observable(isInitiallyDirty);

        result.isDirty = ko.computed(function () {
            return _isInitiallyDirty() || _initialState() !== ko.toJSON(root);
        });

        result.reset = function () {
            _initialState(ko.toJSON(root));
            _isInitiallyDirty(false);
        };

        return result;
    };


}(window.resourcefileeditor = window.resourcefileeditor || {}, jQuery));