using fastJSON;
using System;

namespace CustomComponents
{
    public abstract class SimpleCustom<T>: ICustom where T: class
    {
        [JsonIgnore]
        public T Def { get; internal set; }
    }
}