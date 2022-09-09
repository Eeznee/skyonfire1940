using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class AudioVisual : ObjectElement
{
    protected AVM avm;
    public override void Initialize(ObjectData d,bool firstTime)
    {
        base.Initialize(d,firstTime);
        if (sofObject) avm = sofObject.avm;
    }
}
