// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//
// *******************************************************

//TODO : Change the name of mmethod
function ValidateLongDataType(source, clientside_arguments)
{ 
    var isvalid = /^[-]?[0-9]+$/.test(clientside_arguments.Value);        
    if (isvalid==true)
    {
        if (clientside_arguments.Value >= -9223372036854775808
            && clientside_arguments.Value <= 922372036854775807 )
        { 
            clientside_arguments.IsValid=true;
        }
        else
        { 
            clientside_arguments.IsValid=false;
        }
    }
    else
    { 
        clientside_arguments.IsValid=false;
    }
}

function ValidateDecimalDataType(source, clientside_arguments)
{
    var isvalid = /^[-]?[0-9]+$/.test(clientside_arguments.Value);
    if (isvalid == true) {
        if (clientside_arguments.Value >= -999999999999999999
            && clientside_arguments.Value <= 999999999999999999) {
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



function MultilineTextValidate(txt,maxLen)
{
    try
    {
        if (txt.value.length > (maxLen - 1)) {
            txt.value = (txt.value).substring(0,maxLen)
            return false;
        }
    } 
    catch(e)
    { 
    }
} 
 