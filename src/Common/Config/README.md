# Configuration Patterns
This may be one of my favorites. A surprisingly small amount of code that enables a different way of thinking
about configuration. It's not exactly configuration by convention�that would probably rest directly on the
configuration class names. But even with allowing some variance between the section-path and the configuration
class name, it still "solidifies" it in a way that is clear and follows VSA.

## vertical-slice mindset
This is sort of an example of VSA "in miniature". Consider the usual naive configuration example:

```csharp
// a configuration specified in a library, perhaps
public record ThingSettings(int Maximum, string Specifier, string ImplType);

// meanwhile, in Program.cs or ServiceCollection extensions...

services.Configure<ThingSettings>(configuration.GetSection("thing"));
// - and/or -
var thingSettings = configuration.GetSection("Thing").Get<ThingSettings>();
```

This may even be extended across library boundaries, where no DI is established by the library and is built by
hand in every consuming project.

So, let's move it closer to the related object. Along with sensitivity to the "magic string" smell, it might end
up in a somewhat closer `static class Constants { public const string ThingSettingsSectionName = "thing"; }`.

But the settings class is _the_ related class. There really is no other.

```csharp
public record ThingSettings(int Maximum, string Specifier, string ImplType){
	public const string SectionName = "thing";
}
```

This is organizing for use. It leaves no doubt and is easily discoverable. Before `net7.0`/C# 11 this was our best option.
In fact even then I would favor this because of the "const" and backward compatibility, and the pattern used in my equivalent implementations of `ConfigurationExt` would be based on
reflection with a runtime error if `SectionName` wasn't available or misspelled.
But now that 8.0/12 are well-established, the contractual, compile-time enforcement of `static abstract` interface members completely close the circle.

## normalizing "inconsistent" behavior
Another aspect of these relatively simple extensions is the insistent `new()` constraint.
This enforces (again, at compile-time) that there are cogent defaults on all configuration object properties.
It also ensures that whatever the state of the underlying `IConfiguration` section the consumer can be sure of receiving an
object without an exception.
If you take a look at the [default hypotheses](/test/CommonTests/Config/ConfigDefaultsHypotheses.cs) you can see that may
not be the case depending on combinations of how the config object was coded and the state of the `IConfiguration`.
This favors forgiveness and flexibility in the configuration "code", which is in some senses "at odds" with the more
strict sensibility of C# generally.

However, I have found that this leads to a better configuration experience.
A strict-typing and nullable-annotation environment like C# ideally highlights errors at compile-time.
With a modern IDE, we'll see red and cyan squiggles before we even initiate a compile, at code-authoring time.
However, when this strictness extends to external input (of which configuration is a form) the late-bound, runtime
errors are more frustrating than helpful.
The system is "grading" for form rather than allowing for flexibility in communicating intent.
It's a strictness that _feels_ passive-aggressive because there is no warning or error until the project is run.
When the error does occur it has the taste of being deliberately obtuse: the failure could have been
averted with more flexible—yet still semantically unambiguous—parsing rules.

## What about configurable sub-functions?
This isn't answerable directly with consumable code. This is part of the reason it doesn't make sense to package
this as a library. Instead, this is a pattern that uses the flexibility of C# generics to enable conventional
configuration as a subsection of another configuration. You can see examples in the Resilience and Configuration
test code.
