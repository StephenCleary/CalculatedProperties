![Logo](Icon.128.png)

# Calculated Properties

Easy-to-use calculated properties for MVVM apps (.NET 4, MonoTouch, MonoDroid, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8.0, and Silverlight 5).

## Quick Start

Install the [NuGet package](https://www.nuget.org/packages/Nito.CalculatedProperties).

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

`MyValue` and `MyCalculatedValue` will automatically raise `PropertyChanged` appropriately. Any time `MyValue` is set, both property values notify that they have been updated. This works even if they are properties on different ViewModels. The only thing you have to be careful about is to only access these properties from the UI thread.

It's magic!

## How It Works

Property relationships are determined using dependency tracking.

### Terminology

A **source property** is a property whose value is used in the calculation of another property. Every source property has a collection of target properties that it influences.

A **target property** is a property whose value is determined by executing a delegate. Every target property has a collection of source properties whose value it depends on.

A **trigger property** is a read/write source property.

A **calculated property** is a read-only target property that is *also* a source property.

Source properties and target properties are internal concepts (they're not exposed to you), but they help simplify the dependency tracking discussion below. Just keep in mind that a target property is always a calculated property, but a source property may be a trigger property *or* a calculated property.

### Invalidation

Invalidation can start one of several ways:

- Writing a trigger property.
- Calling `Invalidate()` or `InvalidateTargets()` on a trigger property or calculated property.
- Modifying an observable collection that is the current value of a trigger property or calculated property. See "Collections", below.

When a trigger property is written, that trigger property and the transitive closure of all its target properties are invalidated. Calling `Invalidate()` on a trigger property or calculated property has the same effect.

Invalidation always defers `PropertyChanged` notification while the affected properties are being invalidated, and resumes notification when the invalidations are complete. See "Notification", below.

### Calculated Values

Calculated properties will calculate their value on demand (i.e., when read). They also remember the calculated value and will not re-evaluate it as long as it is valid. So, if a calculated property is retrieved and then retrieved again immediately, the cached value is returned the second time.

Invalidating a calculated property merely marks the property as invalid. It will not actually recalculate its value until its getter is called. When calculated properties are initially constructed, they are always invalid.

### Dependency Tracking

When a calculated property calculates its value, it establishes a dependency tracking scope with itself as the target property. While the calulated property delegate is being evaluated, any trigger property getters or calculated property getters will register those properties as source properties within that scope.

Since a calculated property may depend on other calculated properties, dependency scopes can be nested (internally, there's a *stack* of dependency tracking scopes).

When the dependency tracking scope is completed, it updates the calculated property's collection of source properties as well as the source properties' collections of target properties. This ensures that each property knows all of its target properties (and source properties, if applicable).

### Notification

`PropertyChanged` notification normally happens at the end of invalidation, after all affected properties have been invalidated. After all affected properties have been invalidated, then all invalidated properties raise `PropertyChanged`.

The whole invalidation and dependency tracking system is independent of `PropertyChanged`. Raising `PropertyChanged` is a separate step in the process.

> Why use a separate system instead of building dependency tracking into `PropertyChanged`? Good question!
>
> A separate invalidation/dependency tracking system is more efficient than one that is based on `PropertyChanged`. As a separate system, it avoids string comparisons and spurious notifications within the system. Also, the separation permits consolidation of both the invalidation phase and the notification phase, eliminating spurious notifications produced by the system.
> 
> A final benefit of this separation is that it results in a cleaner and more predictable execution of complex scenarios. E.g., if you choose to use the `PropertyChanged` of a calculated property to (manually) update a different trigger property. If invalidation and dependency tracking were implemented using `PropertyChanged`, then that would be a hairy scenario to untangle and predict which properties will be updated when. By separating invalidation/dependency tracking from `PropertyChanged`, the execution is much more predictable (and efficient). Of course, this is still an unusual test case, and not recommented; I recommend replacing manual `PropertyChanged` handlers with calculated properties as much as possible.

During the invalidation process, `PropertyChanged` notifications are **deferred**. The deferring of `PropertyChanged` notifications can be safely nested; internally, there is a reference count of deferrals, and the `PropertyChanged` events will only fire once this reference count reaches zero.

You can defer notifications manually. You would want to do this, for example, if you are setting several different trigger values and want to do so in the most efficient manner:

    using (PropertyChangedNotificationManager.Instance.DeferNotifications())
    {
        vm.SomeProperty = someValue;
        vm.OtherProperty = otherValue;
        // At this point, all affected properties are invalidated.
    }
    // At this point, all PropertyChanged events have been raised
    //  (assuming there are no deferrals further up the stack).

This is especially useful if you have calculated properties that depend on multiple values that you're setting; by deferring the `PropertyChanged` notifications, you're consolidating the multiple `PropertyChanged` events into a single one.

### Example Update Lifecycle

A walk-through should help take the abstract concepts above and translate them to a concrete description of how the property values work. Take the introductory example:

    public int MyValue
    {
        get { return Properties.Get(7); }
        set { Properties.Set(value); }
    }

    public int MyCalculatedValue
    {
        get { return Properties.Calculated(() => MyValue * 2); }
    }

We'll say that `MyViewModel.MyCalculatedValue` is data-bound to some UI control, like a label.

When the ViewModel is constructed, neither of the properties technically exist yet. When the ViewModel is bound to the View, then the UI invokes the getter on `MyViewModel.MyCalculatedValue`, and here is where things get interesting:

- `Properties.Calculated` will create the actual calculated property (named `"MyCalculatedValue"`) the first time it is invoked. Calculated properties are always created in an invalid state.
- `Properties.Calculated` will then invoke the `CalculatedProperty.GetValue` method of that property.
- `CalculatedProperty.GetValue` will see that it is in an invalid state, so it decides to calculate its value.
  - `CalculatedProperty.GetValue` establishes a dependency tracking scope and invokes its delegate (`() => MyValue * 2`).
    - The delegate invokes the getter of `MyViewModel.MyValue`, which calls `Properties.Get`.
    - `Properties.Get` will create the actual trigger property (named `"MyValue"`) the first time it is invoked. The trigger property value is `7`.
    - `Properties.Get` will then invoke the `TriggerProperty.GetValue` method of that trigger property.
    - `TriggerProperty.GetValue` registers that property with the dependency tracking scope, and then returns `7`.
    - The delegate completes executing, returning the value `14`.
  - `CalculatedProperty.GetValue` completes the dependency tracking scope.
    - `MyCalculatedValue` now has a collection of sources: `[MyValue]`.
    - `MyValue` now has a collection of targets: `[MyCalculatedValue]`.
  - The calculated property now has a value of `14` and is marked valid.
- Finally, the `MyViewModel.MyCalculatedValue` getter returns the value `14` to the UI.

Fun, eh? Now, let's observe how an update works. Let's set a source property:

    vm.MyValue = 13;

- `Properties.Set` will invoke the `TriggerProperty.SetValue` method of the `"MyValue"` property.
- The new value `13` is not equal to the current value `7`, so the trigger property has to update.
- The trigger property defers notifications.
  - The trigger property invalidates itself and the transitive closure of all its target properties.
  - As each property is invalidated, it adds itself to the deferred notification collection.
  - The trigger property does not actually enter an invalid state; it just updates its value to `13` and adds itself to the deferred collection.
  - The calculated property enters an invalid state.
  - The deferred notification property collection is now: `[MyValue, MyCalculatedValue]`.
- The trigger property resumes notifications.
- Since notifications are no longer deferred, `PropertyChanged` events are raised for the properties in the deferred collection (`MyValue` and `MyCalculatedValue`).
- The UI detects the `PropertyChanged` notification for `MyCalculatedValue` and invokes the getter for that property.
- `Properties.Calculated` will invoke the `CalculatedProperty.GetValue` method of the `"MyCalculatedValue"` property.
- `CalculatedProperty.GetValue` will see that it is in an invalid state, so it decides to (re-)calculate its value.
  - `CalculatedProperty.GetValue` establishes a dependency tracking scope and invokes its delegate (`() => MyValue * 2`).
    - The delegate invokes the getter of `MyViewModel.MyValue`, which calls `Properties.Get`.
    - `Properties.Get` will invoke the `TriggerProperty.GetValue` method of that trigger property.
    - `TriggerProperty.GetValue` registers that property with the dependency tracking scope, and then returns `13`.
    - The delegate completes executing, returning the value `26`.
  - `CalculatedProperty.GetValue` completes the dependency tracking scope.
    - `MyCalculatedValue` has the same collection of sources: `[MyValue]`.
    - `MyValue` has the same collection of targets: `[MyCalculatedValue]`.
  - The calculated property now has a value of `26` and is marked valid.
- Finally, the `MyViewModel.MyCalculatedValue` getter returns the value `26` to the UI.

This sounds like a lot of work, but in reality it is *extremely* fast, as well as flexible.

### Collections

When the current value of a trigger property or calculated property is a collection that implements `INotifyCollectionChanged`, then that property will subscribe to `CollectionChanged` and invalidate the transitive closure of all its target properties whenever the collection changes in any way. Note that in this case, the property value is not actually changed (it still refers to the same collection), so only the target properties are invalidated, not the property whose value is the collection. Calling `InvalidateTargets()` on a trigger property or calculated property has the same effect.

This allows calculated properties to use observable collections and Just Work:

    public ObservableCollection<int> MyValue
    {
        get { return Properties.Get(() => new ObservableCollection<int>()); }
        set { Properties.Set(value); }
    }

    public int MyCalculatedValue
    {
        get { return Properties.Calculated(() => MyValue.Count == 0 ? 13 : MyValue.First()); }
    }

`IBindingList` is supported in the same way as `INotifyCollectionChanged`; however, `IBindingList` is less efficient. Use `ObservableCollection<T>` instead of `BindingList<T>` if possible.

### Comparers

When a trigger property is set, it will first evaluate its old value against its new value to determine whether it needs to update. You can specify a comparer to the trigger property to override how this comparison is done:

    private static readonly IEqualityComparer<int> Comparer = ...;
    public int MyValue
    {
        get { return Properties.Get(7, Comparer); }
        set { Properties.Set(value, Comparer); }
    }

Note that the same comparer instance should be passed into both `Get` and `Set`.

If a trigger property is set to an "equal" value, then it does *not* enter the invalidation phase; neither the trigger property nor any of its targets are invalidated. However, it does overwrite its old value with the new value, even if the are "equal".

If you'd like a simple library with a fluent API for creating comparers and equality comparers, try the [Comparers NuGet package](https://www.nuget.org/packages/Comparers/).

### Miscellany

Trigger properties and calculated properties are **not threadsafe**. They are not specifically tied to a UI thread (they don't use `Dispatcher` or anything like that), but they do expect that they will all be written to from the same thread (which, in practice, is the UI thread). Some MVVM platforms (most notably WPF) will do automatic cross-thread marshaling for simple property updates, but that *will not work* for updating calculated properties. Trigger properties and calculated properties will detect cross-thread access and will throw `InvalidOperationException`.

Like regular data binding, trigger properties and calculated properties will write non-fatal errors to the debugger output window, so if you are not seeing updates when you think you should, check there first. However, if you use the `PropertyHelper` class like all the examples do, it's very difficult to actually cause those errors. :)

Dependency loops (e.g., a calculated property indirectly depending on its own value) will result in a stack overflow exception. I have no intention of adding explicit checks for this, since I expect it to be a rare scenario.

## Alternatives

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

However, this is not maintainable. The core problem is that the calculation of `MyCalculatedValue` and the declaration of the dependency are far apart, in different properties. As the calculations change and grow more complex, eventually the dependencies will not be correct.

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