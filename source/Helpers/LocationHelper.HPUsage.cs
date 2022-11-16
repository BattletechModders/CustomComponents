using System;

namespace CustomComponents;

public class HPUsage : IComparable
{
    public int Total;
    public int Used;
    public HardpointInfo hpInfo;
    public int WeaponCategoryID
        => hpInfo?.WeaponCategory?.ID ?? -1;

    public HPUsage()
    {
    }

    public HPUsage(HardpointInfo hp, int total, int used = 0)
    {
        hpInfo = hp;
        Total = total;
        Used = used;
    }

    public HPUsage(HPUsage other, bool reset = false)
    {
        hpInfo = other.hpInfo;
        Total = other.Total;
        Used = reset ? 0 : other.Used;
    }

    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;

        if (obj is HPUsage hp)
        {
            if (hpInfo == null)
                return hp.hpInfo == null ? 0 : -1;
            if (hp.hpInfo == null)
                return 1;

            return hpInfo.CompatibleID.Count.CompareTo(hp.hpInfo.CompatibleID.Count);
        }

        throw new ArgumentException();
    }
}