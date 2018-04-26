using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/*!
 * \brief attribute for marking property or field that you want to save .
 * 
 * * Attribute for marking and give name public properties or field that you want to save. 
 * * Name is for field on save database
 * * Currently only int, float, string, and double data types
 * 
 *  usage: 
 *>    [SaveManager(NameSave = "Status")] public int Status;
 *    
 */
public class SaveManager : System.Attribute {

	public string NameSave { get; set; } //! This is name of field 

}
