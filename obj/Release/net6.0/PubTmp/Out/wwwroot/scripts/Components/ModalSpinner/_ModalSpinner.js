//Copyright 2016 Ellucian Company L.P. and its affiliates.
// 
// ModalSpinner - a "data loading" modal spinner UI element 
// Parameters:
// isVisible  (required) - boolean - is the message visible at this time?
// message    (required) - text - the message to be displayed
//
// Usage:
//
// <modal-spinner params="isVisible: showNow, message: someText"></modal-spinner>
//
// Notes:
// The template (markup) for this component can be found in ./ModalSpinner.html
//
define(['text!ModalSpinner/_ModalSpinner.html'], function (markup) {
    function ModalSpinnerViewModel(params) {
        var self = this;
        try {
            if (typeof params.isVisible === "undefined") {
                throw "Please provide a valid isVisible parameter."
            }

            self.isVisible = ko.isObservable(params.isVisible) ? params.isVisible : ko.observable(params.isVisible);
            if (typeof (params.message) === "undefined") {
                self.message = "Loading Data...";
            } else {
                self.message = ko.isObservable(params.message) ? params.message : ko.observable(params.message);
            }
        } catch (error) {
            console.log(error);
        }
    }
    return { viewModel: ModalSpinnerViewModel, template: markup };
});