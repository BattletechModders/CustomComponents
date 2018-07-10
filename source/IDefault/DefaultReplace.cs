using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    [CustomComponent("DefaultReplace")]
    public class DefaultReplace : SimpleCustomComponent
    {
        public string DefaultID { get; set; }
    }
}
