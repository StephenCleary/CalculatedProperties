CalculatedProperties
====================

Easy to use calculated properties for MVVM apps (.NET 4, MonoTouch, MonoDroid, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8.0, Silverlight 5).




- Like UpdateControls but without the ForView.Wrap thing.

ViewModelBase.CreateIndependent<T>(T value = default(T));

Independent<string> _name = CreateIndependent("none");
// TODO: IndependentList<T>?
Dependent<string> _fullName = CreateDependent(() => "Mr. " + Name);

// how does Dependent know which string to pass to OnPropertyChanged?
// capture [CallerMemberName] on first "get"?
// probably should have our own subscription system, not tied to OnPropertyChanged
// queue up change notifications & trigger them all at the end, like Angular

Call OnPropertyChanged for other properties.
- Unmaintainable.

http://www.codeproject.com/Articles/375192/WPF-The-calculated-property-dependency-problem
- Attribute-based approach
- Only handles computed properties from other properties *on the same object*; extending this would be difficult.

http://updatecontrols.net/cs/
- Nice, but requires ViewModels to follow their own special pattern; at runtime there's a "wrapper" created via reflection around your VMs.
- Also, tries too hard to be a framework; I want a library.