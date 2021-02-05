// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

namespace Zentity.Web.UI.ToolKit
{
    internal class LengthValidator : BaseValidator
    {
        int _maxLength;
        string _maxLengthMessage = string.Empty;

        public int MaxLength
        {
            get
            {
                return _maxLength;
            }
            set
            {
                _maxLength = value;
            }
        }

        public string MaxLengthMessage
        {
            get
            {
                return _maxLengthMessage;
            }
            set
            {
                _maxLengthMessage = value;
            }
        }

        protected override bool EvaluateIsValid()
        {
            string valueToValidate = this.GetControlValidationValue(this.ControlToValidate);

            bool result = false;

            if (valueToValidate.Length > MaxLength)
            {
                this.Text = MaxLengthMessage;
            }
            else
            {
                result = true;
            }

            return result;
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            if (base.RenderUplevel)
            {
                writer.AddAttribute("evaluationfunction", "TextBoxLengthValidatorIsValid");
                writer.AddAttribute("maximumlength", this.MaxLength.ToString(CultureInfo.InvariantCulture));
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (base.RenderUplevel)
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "TxtBxLngthValIsValid",
                   @"<script language='javascript'>
               function TextBoxLengthValidatorIsValid(val) 
               { 
                  var value = ValidatorGetValue(val.controltovalidate); 
                  if (ValidatorTrim(value).length == 0) return true; 
                  if (val.maximumlength < 0) return true; 
                  return (value.length <= val.maximumlength);
               }
            </script>");
        }



    }

}
