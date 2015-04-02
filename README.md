![Logo](Icon.128.png)

# Calculated Properties [![Gratipay](https://img.shields.io/gratipay/StephenCleary.svg?style=plastic)](https://gratipay.com/StephenCleary)

Easy-to-use calculated properties for MVVM apps (.NET 4, MonoTouch, MonoDroid, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8.0, and Silverlight 5).

> ## .NET Core / ASP.NET vNext Status
>  [![AppVeyor](https://img.shields.io/appveyor/ci/StephenCleary/CalculatedProperties.svg?style=plastic)](https://ci.appveyor.com/project/StephenCleary/CalculatedProperties) [![Coveralls](https://img.shields.io/coveralls/StephenCleary/CalculatedProperties.svg?style=plastic)](https://coveralls.io/r/StephenCleary/CalculatedProperties)
> [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Nito.CalculatedProperties.svg?style=plastic)](https://www.nuget.org/packages/Nito.CalculatedProperties/)
>
> Support for `IBindingList` has been dropped in the .NET Core / ASP.NET vNext version.

## Quick Start

Install the [NuGet package](https://www.nuget.org/packages/Nito.CalculatedProperties).

Add a `PropertyHelper` instance to your view model (pass in a delegate that raises `PropertyChanged` for that instance):

    private readonly PropertyHelper Property;

    public MyViewModel()
    {
        Property = new PropertyHelper(RaisePropertyChanged);
    }

    private void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, args);
    }

Next, create read/write _trigger_ properties (pass the default value in the getter):

    public string Name
    {
        get { return Property.Get(string.Empty); }
        set { Property.Set(value); }
    }

Now you can create read-only _calculated_ properties:

    public string Greeting
    {
        get { return Property.Calculated(() => "Hello, " + Name + "!"); }
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

Before writing this library, I looked [pretty hard](https://github.com/StephenCleary/CalculatedProperties/wiki/Alternatives) for something that already existed.
