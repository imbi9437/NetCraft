using _Project.Script.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CHJMentalEvent : IEvent
{
    public MentalState mentalState;


    public CHJMentalEvent(MentalState mentalState)
    {
        this.mentalState = mentalState;
    }



}
