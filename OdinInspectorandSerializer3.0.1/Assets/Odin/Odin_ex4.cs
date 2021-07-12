using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

/*
 Detailed Info Box Attribute

 DetailedInfoBox is used on any property, and displays a message box that can be expanded to show more details. 
 Use this to convey a message to a user, and give them the option to see more details.
     
     */
public class Odin_ex4 : MonoBehaviour
{
    [DetailedInfoBox("Click the DetailedInfoBox...",
    "... to reveal more information!\n" +
    "This allows you to reduce unnecessary clutter in your editors, and still have all the relavant information available when required.")]
    public int Field;
}
