// Copyright 2017 Ellucian Company L.P. and its affiliates.
// 
// Message - a generic message UI element 
// Parameters:
// isVisible  (required) - boolean - is the message visible at this time?
// icon       (required) - text/enum - 'info', 'warning', 'error', etc
//                         See Ellucian style guide for full list of available icons
// color      (optional) - text/enum - 'light', 'neutral', 'info', 'info-dark', 'warning',
//                                     'warning-dark', 'error', 'error-dark', 'success', 'success-dark'
//                         Default (no value) is no visible container
// direction  (optional) - text/enum - 'right', 'down', 'left', 'up'
// size       (optional) - text/enum - 'xsmall', 'small', 'medium', 'large'
//                         Note: medium is actually larger than the default
// Usage:
//
// <icon params="isVisible: showNow, icon:'add', container: 'dark'"></message>
//
// Notes:
// The template (markup) for this component can be found in ./_Icon.html
//
// When placing icons on a page, be cognizant of the "scope" of the icon. Icons don't typically convery information 
// on their own; they should usually be placed alongside meaningful content (for accessibility).
//
define(['text!Icon/_Icon.html'], function (markup) {
    function IconViewModel(params) {
        var self = this;

        try {

            self.isVisible = null;
            if (typeof (params.isVisible) === "undefined") {
                throw "Please provide a valid isVisible value.";
            } else {
                self.isVisible = ko.isObservable(params.isVisible) ? params.isVisible : ko.observable(params.isVisible);
            }

            self.iconId = null;
            if (typeof (params.icon) === "undefined") {
                throw "Please provide a valid icon value.";
            } else {
                self.iconId = ko.computed(function () {
                    var value = "";
                    switch (ko.unwrap(params.icon)) {
                        case "error":
                            value = "#icon-error";
                            break;
                        case "warning":
                            value = "#icon-warning";
                            break;
                        case "check":
                            value = "#icon-check";
                            break;
                        case "info":
                            value = "#icon-info";
                            break;
                        case "arrow":
                            value = "#icon-arrow";
                            break;
                        case "avatar":
                            value = "#icon-avatar";
                            break;
                        case "calendar":
                            value = "#icon-calendar";
                            break;
                        case "clock":
                            value = "#icon-clock";
                            break;
                        case "clear":
                            value = "#icon-clear";
                            break;
                        case "close":
                            value = "#icon-close";
                            break;
                        case "delete":
                            value = "#icon-delete";
                            break;
                        case "document":
                            value = "#icon-document";
                            break;
                        case "email":
                            value = "#icon-email";
                            break;
                        case "export":
                            value = "#icon-export";
                            break;
                        case "filter":
                            value = "#icon-filter";
                            break;
                        case "help":
                            value = "#icon-help";
                            break;
                        case "home":
                            value = "#icon-home";
                            break;
                        case "menu":
                            value = "#icon-menu";
                            break;
                        case "more":
                            value = "#icon-more";
                            break;
                        case "print":
                            value = "#icon-print";
                            break;
                        case "refresh":
                            value = "#icon-refresh";
                            break;
                        case "save":
                            value = "#icon-save";
                            break;
                        case "search":
                            value = "#icon-search";
                            break;
                        case "skip":
                            value = "#icon-skip";
                            break;
                        default:
                            throw "Invalid icon value provided!"
                            break;
                    }
                    return value;
                });
            }

            self.iconClasses = ko.computed(function () {
                var value = "";
                if (typeof (params.direction) !== "undefined") {
                    switch (ko.unwrap(params.direction)) {
                        case "right":
                            value += " esg-icon--right";
                            break;
                        case "down":
                            value += " esg-icon--down";
                            break;
                        case "left":
                            value += " esg-icon--left";
                            break;
                        case "up":
                            value += " esg-icon--up";
                            break;
                        default:
                            throw "Invalid direction value provided!";
                            break;
                    }
                }
                if (typeof (params.size) !== "undefined") {
                    switch (ko.unwrap(params.size)) {
                        case "xsmall":
                            value += " esg-icon--xsmall";
                            break;
                        case "small":
                            value += " esg-icon--small";
                            break;
                        case "medium":
                            value += " esg-icon--medium";
                            break;
                        case "large":
                            value += " esg-icon--large";
                            break;
                        default:
                            throw "Invalid size value provided!";
                            break;
                    }
                }
                return value;
            });

            self.iconContainerClasses = ko.computed(function () {
                var value = "";
                if (typeof (params.color) !== "undefined") {
                    switch (ko.unwrap(params.color)) {
                        case 'light':
                            value += " esg-icon__container--light";
                            break;
                        case 'neutral':
                            value += " esg-icon__container--neutral";
                            break;
                        case 'info':
                            value += " esg-icon__container--info";
                            break;
                        case 'info-dark':
                            value += " esg-icon__container--info-dark";
                            break;
                        case 'warning':
                            value += " esg-icon__container--warning";
                            break;
                        case 'warning-dark':
                            value += " esg-icon__container--warning-dark";
                            break;
                        case 'error':
                            value += " esg-icon__container--error";
                            break;
                        case 'error-dark':
                            value += " esg-icon__container--error-dark";
                            break;
                        case 'success':
                            value += " esg-icon__container--success";
                            break;
                        case 'success-dark':
                            value += " esg-icon__container--success-dark";
                            break;
                        default:
                            throw "Invalid container color value provided!";
                            break;
                    }
                    return value;
                }
            });
        } catch (error) {
            console.log(error);
        }
    }
    return { viewModel: IconViewModel, template: markup };
});