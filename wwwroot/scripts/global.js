// Add an antiforgerytoken request header to all ajax requests. 

$(document).ready(function () {
    var antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
    $.ajaxSetup({
        headers: {
            __RequestVerificationToken: antiForgeryToken
        },
        converters: {
            "text json": function (msg) {
                return $.parseJSON(msg, true);
            }
        }
    });
});