= CalculatedProperties

Easy-to-use calculated properties for MVVM apps (.NET 4, MonoTouch, MonoDroid, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8.0, and Silverlight 5).

== Quick Start

Install the NuGet package.

Add a `PropertyHelper` instance to your view model (pass in a delegate that raises `PropertyChanged` for that instance):

    private readonly PropertyHelper Properties;

    public MyViewModel()
    {
        Properties = new PropertyHelper(RaisePropertyChanged);
    }

    private void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, args);
    }

Next, create read/write _trigger_ properties (pass the default value in the getter):

    public int MyValue
    {
        get { return Properties.Get(7); }
        set { Properties.Set(value); }
    }

Now you can create read-only _calculated_ properties:

    public int MyCalculatedValue
    {
        get { return Properties.Calculated(() => MyValue * 2); }
    }

Done.

No, seriously. That's it.

`MyValue` and `MyCalculatedValue` will automatically raise `PropertyChanged` appropriately. Any time `MyValue` is set, both property values notify that they have been updated.

It's magic!

== How It Works

Property relationships are determined using dependency tracking.

=== Terminology

A **source property** is a property whose value is used in the calculation of another property. Every source property has a collection of target properties that it influences.

A **target property** is a property whose value is determined by executing a delegate. Every target property has a collection of source properties whose value it depends on.

A **trigger property** is a read/write source property.

A **calculated property** is a read-only target property that is *also* a source property.

Source properties and target properties are internal concepts (they're not exposed to you), but they help simplify the dependency tracking discussion below. Just keep in mind that a target property is always a calculated property, but a source property may be a trigger property *or* a calculated property.

=== Invalidation

Invalidation can start one of several ways:

- Writing a trigger property.
- Calling `Invalidate()` or `InvalidateTargets()` on a trigger property or calculated property.
- Modifying an observable collection that is the current value of a trigger property or calculated property.

When a trigger property is written, that trigger property and the transitive closure of all its target properties are invalidated. Calling `Invalidate()` on a trigger property or calculated property has the same effect.

When the current value of a trigger property or calculated property is a collection that implements `INotifyCollectionChanged`, then that property will subscribe to `CollectionChanged` and invalidate the transitive closure of all its target properties whenever the collection changes in any way. Note that in this case, the property value is not actually changed (it still refers to the same collection), so only the target properties are invalidated, not the property whose value is the collection. Calling `InvalidateTargets()` on a trigger property or calculated property has the same effect.

Note: `IBindingList` is not currently supported, but it wouldn't be too hard to add it in if someone needs it.

Invalidation always delays `PropertyChanged` notification while the affected properties are being invalidated, and resumes notification when the invalidations are complete. See "Notification", below.

=== Calculation

- when calculation takes place

When a property value is calculated, any properties used to determine its value are linked to that property. When the linked properties change, the related property is recalculated.

, which can depend on trigger properties or other calculated properties:

- invalidation
- propertychanged notification
- like an AngularJS digest loop

- threading

- Collection monitoring.
- Does not support IBindingList, but it wouldn't be too hard to add if someone needs it.

== Alternatives

Before writing this library, I looked pretty hard for something that already existed.

Manual implementation is a possibility. Calculated properties can use expression lambdas to look like this:

    private int _myValue;
    public int MyValue
    {
        get { return _myValue; }
        set
        {
            _myValue = value;
            OnPropertyChanged();
            OnPropertyChanged(() => MyCalculatedValue);
        }
    }
    public int MyCalculatedValue
    {
        get { return MyValue * 2; }
    }

However, this is not maintainable. The core problem is that the calculation of +MyCalculatedValue+ and the declaration of the dependency are far apart, in different properties. As the calculations change and grow more complex, eventually the dependencies will not be correct.

An alternative is to define a property dependency mapping in code, as [this StackOverflow answer](http://stackoverflow.com/a/4596666/263693) suggests. A concrete implementation is available [in a Code Project article](http://www.codeproject.com/Articles/375192/WPF-The-calculated-property-dependency-problem). This solution makes the properties look like:

    private int _myValue;
    public int MyValue
    {
        get { return _myValue; }
        set { _myValue = value; OnPropertyChanged(); }
    }
    [DependentOn("MyValue")]
    public int MyCalculatedValue
    {
        get { return MyValue * 2; }
    }

This solution moves the dependency declaration near the actual dependency, so it's a great step in the right direction. Unfortunately, lambdas cannot be used in attributes, so this solution is forced to use magic strings for all the dependency properties. Also, it's not possible to have a dependency across different ViewModel instances; a parent or child ViewModel cannot depend on the other's properties. It would be possible but difficult to extend the attribute solution to support property paths in addition to simple property names.

A more exhaustive solution is [UpdateControls](http://updatecontrols.net/cs/). The problem with UpdateControls is that each ViewModel is all-or-nothing; if your type implements `INotifyPropertyChanged` then you have to raise it yourself for all your properties. Also, UpdateControls will wrap your actual ViewModel within a data-bindable wrapper that is actually bound to the View. Finally, UpdateControls tries too hard to be a comprehensive MVVM framework; I just want a little library that can work with any framework, and allow me to gradually transition.