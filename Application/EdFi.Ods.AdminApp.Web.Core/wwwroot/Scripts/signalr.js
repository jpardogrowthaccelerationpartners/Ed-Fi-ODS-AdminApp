﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

$.extend(true, edfiODS, {
    signalR: {
        setStatusText: function (text, error) {
            var $statusMessage = $("#signalr-status-message");
            $statusMessage.html(text);

            if (error) {
                $statusMessage.addClass("note");
            } else {
                $statusMessage.removeClass("note");
            }
        },

        setProgress: function (percentComplete, error, warning) {
            var $progressBar = $("#signalr-status-progress-bar");

            var progressBarStyle = percentComplete === 0 ? "0" : percentComplete + "%";
            $progressBar.css("width", progressBarStyle).attr("aria-value-now", percentComplete);
            if (percentComplete === 100 && !error) {
                if (!warning) {
                    $progressBar.removeClass("progress-bar-striped active").text("Completed Successfully");
                } else {
                    $progressBar.removeClass("progress-bar-success").addClass("progress-bar-warning");
                    $progressBar.removeClass("progress-bar-striped active").text("Completed with warning");
                }
            }

            if (error) {
                $progressBar.removeClass("progress-bar-success").addClass("progress-bar-warning");

                var errorBarStyle = (100 - percentComplete) + "%";
                $progressBar.css("width", errorBarStyle);

                if (percentComplete === 0) {
                    $progressBar.removeClass("progress-bar-striped active").text("Completed with Error");
                }
            }
        },

        showProgress: function() {
            $("#signalr-progress").show();
        },

        hideProgress: function () {
            $("#signalr-progress").hide();
        },

        setFinalRedirectUrl: function (url) {
            if (StringIsNullOrWhitespace(url)) {
                $("#signalr-done-button").text("Reload");
                $("#signalr-done-button").click(function () {
                    location.reload();
                });
            } else {
                $("#signalr-done-button").attr("href", url);
                $("#signalr-done-button").text("View");
            }
        },

        showFinalStatus: function (finalRedirectUrl) {
            $("#signalr-progress").show();
            var waiting = $("#waiting-msg");
            if (waiting.length > 0) {
                waiting.hide();
                $("#completed-msg").show();
            }
            edfiODS.signalR.setFinalRedirectUrl(finalRedirectUrl);
            $("#signalr-done-button").show();
            $("#signalr-done-button").parents(".modal").on("hidden.bs.modal", function() {
                location.reload();
            });
        },

        showError: function(errorMessage) {
            edfiODS.signalR.setProgress(0, true, false);
            edfiODS.signalR.setStatusText(errorMessage, true);
            edfiODS.signalR.showFinalStatus();
        },

        stopListener: function() {
            $.connection.hub.stop();
        },

        startListener: function ($hub, finalRedirectUrl) {
            $hub.client.updateStatus = function (status) {
                if (status.Complete) {
                    $hub.server.unsubscribe();

                    if (status.Error) {
                        if (!StringIsNullOrWhitespace(status.ErrorMessage)) {
                            edfiODS.signalR.setStatusText(status.ErrorMessage, true);
                        } else {
                            edfiODS.signalR.setStatusText("Error - Operation could not be completed", true);
                        }
                        edfiODS.signalR.setProgress(0, true, false);
                        edfiODS.signalR.showFinalStatus();

                    } else {
                        edfiODS.signalR.setStatusText(status.StatusMessage);
                        if (status.Warning) {
                            edfiODS.signalR.setProgress(100, false, true);
                        } else {
                            edfiODS.signalR.setProgress(100, false, false);
                        }
                        edfiODS.signalR.showFinalStatus(finalRedirectUrl);
                    }
                } else {
                    var percentComplete = status.TotalSteps ? Math.max(Math.round((status.CurrentStep / status.TotalSteps) * 100), 1) : 1;
                    edfiODS.signalR.setProgress(percentComplete, status.Error, false);
                    edfiODS.signalR.setStatusText(status.StatusMessage, status.Error);
                }
            };

            $.connection.hub.disconnected(function () {
                setTimeout(function () {
                    $.connection.hub.start().done(function () {
                        $hub.server.subscribe();
                    });
                }, 5000); // Restart connection after 5 seconds.
            });

            $.connection.hub.start().done(function () {
                $hub.server.subscribe();
            });

            $(window).unload(function () {
                $.connection.hub.stop();
            });
        },

        changeStatusMessage: function(newStatusMessage) {
            $("#signalr-status-message").text(newStatusMessage);
        }
    }
});
