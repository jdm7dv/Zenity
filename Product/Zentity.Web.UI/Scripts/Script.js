// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

//Execute Validation for given Validators
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

// Displays a confirmation box to the user.
function ShowConfirmDialog(confirmationMessage) {
    return confirm(confirmationMessage);
}

// Set specified value to control if it contains empty.
function textChange(Id, actualValue) {
    if (document.getElementById(Id).value.replace(/(^\s*)|(\s*$)/g, "") == '') {
        document.getElementById(Id).value = actualValue;
    }
}

// Adjust width of top tag if it is out the control boundary. 
// Invisible tags if exceed control boundary.
function AdjustTopTags(tagCloudContainerClientID, adjustHeight) {
    var tagCloudContainer = document.getElementById(tagCloudContainerClientID);
    var link, i = 0;
    var links = tagCloudContainer.getElementsByTagName('a');

    var linkText;
    while (link = links.item(i++)) {
        // Truncate text if it is out of the boundary of Div container.
        var linkModified = false;
        while (link.offsetWidth >= tagCloudContainer.offsetWidth) {
            linkModified = true;
            linkText = link.firstChild.nodeValue;
            link.firstChild.nodeValue = linkText.substring(0, linkText.length - 1);
        }
        if (linkModified) {
            linkText = link.firstChild.nodeValue;
            linkText = linkText.substring(0, linkText.length - 3) + '...';
            link.firstChild.nodeValue = linkText;
        }
    }

    if (adjustHeight) {
        var offsetHeightMet = false;
        i = 0;
        while (link = links.item(i++)) {
            if (offsetHeightMet) {
                link.style.visibility = 'hidden';
                continue;
            }
            // Hide tag if it is out of boundary of tag cloud control. 
            // A link that represents a tag, will visible if its height and top margin is
            // inside the Div container.
            if ((link.offsetTop + link.offsetHeight) > tagCloudContainer.offsetHeight) {
                link.style.visibility = 'hidden';
                offsetHeightMet = true;
                continue;
            }
        }
    }
}

function ToggleDiv(contentDivId, additionalFiltersStatusHiddenId, toggleImageId, toggleImagePathHiddenId, expandedImagePath, collapsedImagePath) {
    var contentDiv = document.getElementById(contentDivId);
    var additionalFiltersStatusHidden = document.getElementById(additionalFiltersStatusHiddenId);
    var toggleImage = document.getElementById(toggleImageId);
    var toggleImagePathHidden = document.getElementById(toggleImagePathHiddenId);

    if (contentDiv != null) {
        if (contentDiv.className == 'Collapsed') {
            contentDiv.className = 'Expanded';
            toggleImage.src = expandedImagePath;
        }
        else {
            contentDiv.className = 'Collapsed';
            toggleImage.src = collapsedImagePath;
        }
        if (additionalFiltersStatusHidden != null) {
            additionalFiltersStatusHidden.value = contentDiv.className;
        }
        if (toggleImagePathHidden != null) {
            toggleImagePathHidden.value = toggleImage.src;
        }
    }
}