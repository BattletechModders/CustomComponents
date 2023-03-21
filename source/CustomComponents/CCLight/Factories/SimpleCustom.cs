using fastJSON;

namespace CustomComponents;

public abstract class SimpleCustom<T>: ICustom where T: class
{
    [JsonIgnore]
    public T Def { get; internal set; }

    public override string ToString()
    {
        if (Def == null) // can happen if a custom class was initialized outside CC
        {
            return base.ToString();
        }
        return $"{GetType()} id={Database.Identifier(Def)} type={Def.GetType()} code={Def.GetHashCode()}";
    }
}