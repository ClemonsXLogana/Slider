using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Basically is a power source that will turn on if the gate is on

//Shouldn't need this, but I'm leaving it in until my fix is stable.
public class TripleWirePatch : ElectricalNode
{
    public TimedGate gateToWatch;
    private bool signalStarted;
    
    protected override void Update()
    {
        base.Update();

        if (gateToWatch.Powered && !signalStarted)
        {
            signalStarted = true;
            StartSignal(true);
        }
    }
}
