## About
CustomComponents - is a library that allows you to create custom ComponentsDef, that can contain any custom data and load them from JSON definitions.
## How it works
Loading each type of resource controled by apopriate DataManager.%%DefLoadRequest class. after loading Def file stored in DataManager and can be retrieved by id. its unchanged throughout the life time. So library hooks to

1. DataManager.%%DefLoadRequest method OnLoadedWithJSON(string json), check if loading json have "CustomType" tag and then load custom component according this tag. 
2. %%Def.ToJSON(), where check if component is cutomized and send customized object to serialization.

## What you need to create own definition

First register your mod assembly to CustomComponents somewhere in Init Method
```
CustomComponents.Control.RegisterCustomTypes(Assembly.GetExecutingAssembly())
```

Second create new type and inherite it from apopriate Custom%%Def class. There is implemented 5 types of custom ressources based on default component types.
- CustomAmmunitionBoxDef<T> : BattleTech.AmmunitionBoxDef
- CustomJumpJetDef<T> : BattleTech.JumpJetDef
- CustomHeatSinkDef<T> : BattleTech.HeatSinkDef
- CustomUpgradeDef<T> : BattleTech.UpgradeDef 
- CustomWeaponDef<T> : BattleTech.WeaponDef

add CustomAttribute with unique name for type
add any fields/properties to class
```
[Custom("Engine")]
public class EngineDef : CustomHeatSinkDef<EngineDef>
{
   public int EngineRating {get;set;}
   public String SomeValue;
}
```

and now you can take HeatSinkTemplate.json from game, add your parameters to main scope and change standard parameters (at least Description.Id) for your needs
```
{
"CustomType" : "Engine",
"EngineRating" : 200,
"SomeValue" : "Custom String",
// rest of heatsink deffinition
}
```
you can check if component is Engine and cast it to Engine using standart C# operators **is** and **as**
```
if (componentDef is EngineDef)
{
   var engine = componentDef as EngineDef;
   movementSpeed = engine.EngineRating / mechTonnage
}
```
## Generalization

You can create and implement interfaces to create some general component behaviour. For example provided with library IColorComponent interface, that change color of component in component list or mech equipment
```
    interface IColorComponent
    {
        UIColor Color { get; }
    }
```
for example to make Engine orange all you need is add to it definition
```
[Custom("Engine")]
public class EngineDef : CustomHeatSinkDef<MyComponentDef>, IColorComponent
{
   UIColor Color {get;set;}
...
}
```
and add color to JSon
```
"Color" : "Orange"
```
_note: UIColor is buildin enum with pretty limited set of colors and most of them are unusable, it has element UIColor.Custom, but i dont found how to define custom colors. also it have Half variants (e.g. UIColor.RedHalf) with semitransparent alpha_

## Validator

based on [CptMoore](https://github.com/CptMoore/MechEngineer) validation. 

This class designed to provide support for custom validation checks for components and mechs

### ValidateAdd

This is a way to check if component can be mounted in provided location

There is two ways to do it
1. IValidateAdd interface. Attach it to your class and ValidateAdd() will be called when game need this check(e.g. when you drop component to location)
_note: this check called every time when you drop component for each present location include trashcan_
2. Validator.RegisterAddValidator() and ValidateAddDelegate - register external validator for provided type. delegete will be called when dropped component can be cast to this type(designed for interfaces as the cannot define methods itself)

Parameters:
- MechLabLocationWidget widget - location where you drop component.
- bool current_result - result of previous checks
- ref string errorMessage - error message to show if check failed
- MechLabPanel mechlab - link to mechlab
- MechComponentDef component(for delegate) - validated component

Return Value:
- bool - can this component can be mounted(can be ovveride by later checks)
_note: game can ignore this value for weapons, it additionaly recheck hardpoins in OnMechLabDrop when result is false and replace weapon if can_
- ref string errorMessage - warning message, that game show to user when placement canceled.

Order of validation
1. Standard game validation
2. Validation for all added external delegates
3. IValidateAdd.ValidateAdd() 

### Mech Validation

Call whe game need to check if current mech design is valid. 
Some two ways for do custom check

1. IMechValidate with ValidateMech() will be called when game checks design **and this design has this component attached**
2. Validator.ValidateMech() and ValidateMechDelegate - register external validator, **will be called at each mech validation**

Parameters
- Dictionary<MechValidationType, List<string>> errors - list of errors
- MechValidationLevel validationLevel - level of validation, mechlab use MechValidationLevel.MechLab, so if you do apopriate ValidationAdd checks 
- MechDef mechDef - mech config to check

How game interpretate results

1. check each MechValidationType category, show icon if error messages exist. 
2. if total count of critical errors > 0 (red icons) - it dont allow save design and leave mechlab
3. if there is no critical errors, but other error present(yellow icons) - it show warning, but allow save design and use it in battle

Error categories, they pretty selfexplainatory

Critical errors:
- MechValidationType.ValidManifest - something wrong with mech descriptor at all. usualy with this kind of error mech cannot be loaded by game
- MechValidationType.Overweight
- MechValidationType.WeaponsMissing
- MechValidationType.InvalidInventorySlots - generic eqipment error
- MechValidationType.InvalidHardpoints - weapons cannot be placed in this location cose dont have apropeate hardpoint
- MechValidationType.InvalidJumpjets - wrong number of jumjets
- MechValidationType.StructureDestroyed - part of mech destroyed and need to be replaced(last 4 error have one "broken" icon)

Warning:
- MechValidationType.Underweight
- MechValidationType.StructureDamaged
- MechValidationType.AmmoMissing
- MechValidationType.AmmoUnneeded

## Included interfaces

### ICustomColor

Allow to change color of component

### IWeightLimit

Limit usage of component to Mech tonnage and filter it in mechlab like JumpJets

## Expand to other resource type

under construction