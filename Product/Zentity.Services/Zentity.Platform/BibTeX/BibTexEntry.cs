// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform 
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Represents a BibTeX Entry. 
	/// It holds the name of the entry, a key and a collection of all the fields as key-value pairs.
	/// </summary> 
	[Serializable]
	public class BibTeXEntry
	{
		private string name;
		private string key;
		private Dictionary<string, string> properties = new Dictionary<string,string>();

		/// <summary>
		/// Gets or sets the BibTeX entity type name.
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				this.name = value;
			}
		}

		/// <summary>
		/// Gets or sets the key for the BibTeX entry.
		/// </summary>
		public string Key
		{
			get
			{
				return key;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				this.key = value;
			}
		}

		/// <summary>
		/// Gets the name and value pair of fields in BibTeX entry.
		/// </summary>
		public Dictionary<string, string> Properties
		{
			get
			{
				return properties;
			}
		}

		/// <summary>
		/// Returns string representation of the BibTeX entry.
		/// </summary>
		/// <returns>Returns name of BibTeX entry.</returns> 
		public override string ToString()
		{
			return this.Name;
		}
	}
}
