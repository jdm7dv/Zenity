// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zentity.Web.UI.ToolKit
{
    internal class BrowsableNodeInfo
    {
        private string _value;
        private string _text;
        private int _associatedResourceCount;
        private List<BrowsableNodeInfo> _childNodesInfo;


        public BrowsableNodeInfo(string value, string text,
            int associatedResourceCount)
        {
            _value = value;
            _text = text;
            _associatedResourceCount = associatedResourceCount;
            _childNodesInfo = new List<BrowsableNodeInfo>();
        }

        public BrowsableNodeInfo()
        {            
        }


        public string Value
        {
            get
            {
                return _value;
            }            
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }

        public int AssociatedResourceCount
        {
            get
            {
                return _associatedResourceCount;
            }
        }


        public List<BrowsableNodeInfo> ChildNodesInfo
        {
            get
            {
                return _childNodesInfo;
            }
        }
    }
}
