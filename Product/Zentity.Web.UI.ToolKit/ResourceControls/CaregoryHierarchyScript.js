// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

function ShowArrowImage(id) {
    var menuobj = document.getElementById(id);
    menuobj.style.visibility = 'visible';

    return false;
}

function HideArrowImage(id, panelId, event) {
    var menuobj = document.getElementById(id);

    if (CheckMousePosition(panelId, event) == false) {
        menuobj.style.visibility = 'hidden';
        document.getElementById(panelId).style.visibility = 'hidden';
    }
    else {
        if (document.getElementById(panelId).style.visibility = 'hidden') {
            menuobj.style.visibility = 'hidden';
        }
    }
}


function PerformOpration(id) {
    var menuobj = document.getElementById(id);
    menuobj.style.display = 'inline';

    return true;
}

function ShowContextMenu(id, event) {

    var contextMenu = document.getElementById(id);

    var right = document.body.clientWidth - event.clientX;

    var newLeft;
    if (right < contextMenu.offsetWidth) {
        newLeft = document.documentElement.scrollLeft + event.clientX - contextMenu.offsetWidth;
    }
    else {
        newLeft = document.documentElement.scrollLeft + event.clientX;
    }
    contextMenu.style.left = newLeft + "px";

    var bottom = document.documentElement.clientHeight - event.clientY;

    var height = 0;
    var contextHeight = contextMenu.style.height;
    var contextHeightPxIndex = contextHeight.indexOf('px', 0);
    height = contextHeight.substring(0, contextHeightPxIndex);

    var newTop;
    if (bottom < height) {
        newTop = event.clientY - (height - bottom);
    }
    else {
        newTop = document.documentElement.scrollTop + event.clientY;
    }
    contextMenu.style.top = newTop + "px";    
    
    contextMenu.style.display = "inline";
    contextMenu.style.visibility = 'visible';
    return false;
}

function HideContextMenu(id, event) {
    var contextMenu = document.getElementById(id);

    var leftPosition = contextMenu.style.left;
    var topPosition = contextMenu.style.top;
    var contextWidth = contextMenu.style.width;
    var H = contextMenu.style.height;

    var left = 0;
    var top = 0;
    var width = 0;
    var height = 0;

    var leftPositionPxIndex = leftPosition.indexOf('px', 0);
    left = leftPosition.substring(0, leftPositionPxIndex);

    var topPositionPxIndex = topPosition.indexOf('px', 0);
    top = topPosition.substring(0, topPositionPxIndex);

    var contextWidthPxIndex = contextWidth.indexOf('px', 0);
    width = contextWidth.substring(0, contextWidthPxIndex);

    var h1 = H.indexOf('px', 0);
    height = H.substring(0, h1);

    var finalWidth = parseInt(left) + parseInt(width);
    var finalHeight = parseInt(top) + parseInt(height);


    var x = (event.clientX + document.documentElement.scrollLeft) + 5;
    var y = (event.clientY + document.documentElement.scrollTop) + 5;

    if ((x > parseInt(left)) && (x < finalWidth)) {
        if ((y > parseInt(top)) && (y < finalHeight)) {
            contextMenu.style.visibility = 'visible';
        }
        else {
            contextMenu.style.visibility = 'hidden';
        }
    }
    else {
        contextMenu.style.visibility = 'hidden';
    }
}

function CheckMousePosition(panelId, event) {
    var contextMenu = document.getElementById(panelId);

    var leftPosition = contextMenu.style.left;
    var topPosition = contextMenu.style.top;
    var contextWidth = contextMenu.style.width;
    var contextHeight = contextMenu.style.height;

    var left = 0;
    var top = 0;
    var width = 0;
    var height = 0;

    var leftPositionPxIndex = leftPosition.indexOf('px', 0);
    left = leftPosition.substring(0, leftPositionPxIndex);

    var topPositionPxIndex = topPosition.indexOf('px', 0);
    top = topPosition.substring(0, topPositionPxIndex);

    var contextWidthPxIndex = contextWidth.indexOf('px', 0);
    width = contextWidth.substring(0, contextWidthPxIndex);

    var contextHeightPxIndex = contextHeight.indexOf('px', 0);
    height = contextHeight.substring(0, contextHeightPxIndex);

    var finalWidth = parseInt(left) + parseInt(width);
    var finalHeight = parseInt(top) + parseInt(height);

    var x = (event.clientX + document.documentElement.scrollLeft) + 2;
    var y = (event.clientY + document.documentElement.scrollTop) + 2;

    if ((x > parseInt(left)) && (x < finalWidth)) {
        if ((y > parseInt(top)) && (y < finalHeight)) {
            return true;
        }
        else {
            return false;
        }
    }
    else {
        return false;
    }
}

function DeleteResource(text) {
    var deleteCategory = confirm(text);
    if (deleteCategory == true) {
        return true;
    }
    else {
        return false;
    }
}

function SelectMenu(id, CssClass) {
    id.className = CssClass;
}

function DeselectMenu(id, CssClass) {
    id.className = CssClass;
}
 