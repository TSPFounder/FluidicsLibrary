#nullable enable

using System;
using SE_Library;
using CAD;
using Mathematics;

namespace Fluidics;

public class Fluid
{
    public Fluid()
    {
        MyMaterial = new Material();
    }

    public Material MyMaterial { get; set; }
}
