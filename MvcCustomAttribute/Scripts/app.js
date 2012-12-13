$(function () {
    var $carrier = $("#Carrier");
    var $ship = $("#ShipMethod");
    enableList($ship, false);
    enableSurcharge(null);
    $carrier.change(function () {
        listChanged($(this), $ship, "/ShipRate/GetShipMethodsByCarrier");
        enableSurcharge($(this).val());
    });

    $("#rate-form select").change(function () {
        clearResults();
    });

    $("#rate-form input").change(function () {
        clearResults();
    });
});

function enableSurcharge(val) {
    var $surcharge = $("#Surcharge");
    if (val == "UPS") {
        $surcharge.removeAttr("disabled");
    } else {
        $surcharge.attr("disabled", "disabled");
    }
}

function clearResults() {
    $("#rate-detail").html("").hide();
}

function complete() {
    $("#rate-detail").show();
    $('#Carrier').focus();
}

$.validator.addMethod("maximumweight",
    function (value, element, parameters) {
        var carrier = $("#" + parameters["dependentproperty"]).val();
        var carriervalue = parameters["dependentvalue"].toString();
        var weightvalue = Number(parameters["weightvalue"]);
        if (carrier == carriervalue && value > weightvalue) {
            return false;
        }
        $('.validation-summary-errors')
            .removeClass('validation-summary-errors')
            .addClass('validation-summary-valid');
        return true;
    }
);

$.validator.unobtrusive.adapters.add(
    "maximumweight",
    ["weightvalue", "dependentproperty", "dependentvalue"],
    function (options) {
        options.rules["maximumweight"] = {
            weightvalue: options.params["weightvalue"],
            dependentproperty: options.params["dependentproperty"],
            dependentvalue: options.params["dependentvalue"]
        };
        options.messages["maximumweight"] = options.message;
    }
);