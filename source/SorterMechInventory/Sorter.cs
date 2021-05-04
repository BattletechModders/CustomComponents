﻿using System;

namespace CustomComponents
{
    [CustomComponent("Sorter")]
    public class Sorter : SimpleCustomComponent, ISorter, IValueComponent
    {
        public int Order { get; set; }
        public void LoadValue(object value)
        {
            Order = value is Int64 i ? (int)i : 0;
        }
    }
}