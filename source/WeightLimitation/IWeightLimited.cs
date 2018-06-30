using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    public interface IWeightLimited
    {
        int MinTonnage { get; }
        int MaxTonnage { get; }
    }
}
