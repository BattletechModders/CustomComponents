using fastJSON;

namespace CustomComponents
{
    public abstract class SimpleCustom<T>: ICustom where T: class
    {
        [JsonIgnore]
        public T Def { get; internal set; }

        public override string ToString()
        {
            return $"{GetType()} id={Database.Identifier(this)} type={Def.GetType()} code={Def.GetHashCode()}";
        }
    }
}