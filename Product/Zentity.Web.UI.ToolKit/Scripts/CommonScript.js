// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

function CauseValidations(validatorIds) {
    var validators = validatorIds.split(",");
    if (validators.length > 0) {
        var i = 0;
        for (i = 0; i < validators.length; i++) {
            var objValidator = document.getElementById(validators[i]);
            if (objValidator != null) {

                ValidatorValidate(objValidator);
            }
        }
    }
}

function CauseValidation(validatorId) {
    var objValidator = document.getElementById(validatorId);
    if (objValidator != null) { 
        ValidatorValidate(objValidator);
    }
}


//Changes visibility of a control based on input parameter
function ShowHideControl(controlId, visibility) {
    var filterCriteira = document.getElementById(controlId);
    if (filterCriteira != null) {

        if (visibility == "true") {
            filterCriteira.style.display = "block";
        }
        else {
            filterCriteira.style.display = "none";
        }
    }
}

function LongTypeRangeValidatorScript(source, clientside_arguments) {
    var isvalid = /^[-]?[0-9]+$/.test(clientside_arguments.Value);
    if (isvalid == true) {
        if (clientside_arguments.Value >= -9223372036854775808
            && clientside_arguments.Value <= 922372036854775807) {
            clientside_arguments.IsValid = true;
        }
        else {
            clientside_arguments.IsValid = false;
        }
    }
    else {
        clientside_arguments.IsValid = false;
    }
}

function ViewPanel(id, imageId, plusImagePath, minusImagePath) {
    var objectId = document.getElementById(id);
    var image = document.getElementById(imageId);
    if (objectId.style.display == 'inline') {
        objectId.style.display = 'none';
        image.src = plusImagePath; 
    }
    else {
        objectId.style.display = 'inline';
        image.src = minusImagePath;
    }
}
