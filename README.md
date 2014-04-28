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

Property relationships are determined using dependency tracking. For more information, [see the wiki](https://github.com/StephenCleary/CalculatedProperties/wiki/How-It-Works).

The only really important thing you should know is that the `PropertyChanged` notifications are not raised immediately; they're deferred and then all raised together (combining any duplicate notifications).

You can defer notifications manually. You would want to do this, for example, if you are setting several different trigger values and want to do so in the most efficient manner:

    using (PropertyChangedNotificationManager.Instance.DeferNotifications())
    {
        vm.SomeProperty = someValue;
        vm.OtherProperty = otherValue;
        // At this point, no PropertyChanged events have been raised.
    }
    // At this point, all PropertyChanged events have been raised
    //  (assuming there are no deferrals further up the stack).

This is especially useful if you have calculated properties that depend on multiple values that you're setting; by deferring the `PropertyChanged` notifications, you're consolidating the multiple `PropertyChanged` events into a single one.

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