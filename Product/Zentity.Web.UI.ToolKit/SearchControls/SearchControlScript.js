// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//
// *******************************************************

//Check or uncheck Checkboxes in the Delete column based status of Header Checkbox
function SelectDeselectAll(gridViewId, headerCheckBoxId) {
    var table = document.getElementById(gridViewId);
    var headerCheckBox = document.getElementById(headerCheckBoxId);

    for (var rowIndex = 0; rowIndex < table.rows.length; rowIndex++) {
        var row = table.rows[rowIndex];
        for (var cellIndex = 0; cellIndex < row.cells.length; cellIndex++) {
            var cell = row.cells[cellIndex];

            for (controlIndex = 0; controlIndex < cell.childNodes.length; controlIndex++) {
                var control = cell.childNodes[controlIndex];
                if (control.type == "checkbox") {
                    control.checked = headerCheckBox.checked;
                }
            }
        }
    }
}

//Check or uncheck header Checkbox based on status of child checkboxes in the Delete column
function SelectDeselectHeaderCheckBox(gridViewId, headerCheckBoxId) {
    var table = document.getElementById(gridViewId);
    var headerCheckBox = document.getElementById(headerCheckBoxId);

    var allChecked = true;
    for (var rowIndex = 0; rowIndex < table.rows.length; rowIndex++) {
        var row = table.rows[rowIndex];
        for (var cellIndex = 0; cellIndex < row.cells.length; cellIndex++) {
            var cell = row.cells[cellIndex];

            for (controlIndex = 0; controlIndex < cell.childNodes.length; controlIndex++) {
                var control = cell.childNodes[controlIndex];
                if (control.type == "checkbox" && control.id != headerCheckBox.id && control.checked == false) {
                    allChecked = false;
                    break;
                }
            }
        }
    }

    if (allChecked) {
        headerCheckBox.checked = true;
    }
    else {
        headerCheckBox.checked = false;
    }
}

//Validates CheckBox status and displayed appropriate message.
function ValidateDeleteCheckBoxSelection(gridViewId, confirmationMessage, errorMessage) {

    var table = document.getElementById(gridViewId);

    var checked = false;
    for (var rowIndex = 0; rowIndex < table.rows.length; rowIndex++) {
        var row = table.rows[rowIndex];
        for (var cellIndex = 0; cellIndex < row.cells.length; cellIndex++) {
            var cell = row.cells[cellIndex];

            for (controlIndex = 0; controlIndex < cell.childNodes.length; controlIndex++) {
                var control = cell.childNodes[controlIndex];
                if (control.type == "checkbox") {
                    if (control.checked) {
                        checked = true;
                        break;
                    }
                }
            }
        }
    }
    
    if (checked) {
        return confirm(confirmationMessage);
    }
    else {
        alert(errorMessage);
        return false;
    }
}

//Allows only digits to enter and value less than on equal to given maxValue. 
//Calls PostBack event if Enter button clicked
function HandleKeyPressEvent(textboxKey, minValue, maxValue) {
    var textbox = document.getElementById(textboxKey);

    if (event.keyCode == 13) {
        __doPostBack(textboxKey, '');
    }
    else if (event.keyCode >= 48 && event.keyCode <= 58) {

        var currentValue = 0;
        try {
            var currentValue = parseInt(textbox.value + String.fromCharCode(event.keyCode));
        }
        catch (Err) {
            textbox.value = minValue;
        }
        if (currentValue >= minValue && currentValue <= maxValue) {
            return true;
        }
    }

    return false;
}