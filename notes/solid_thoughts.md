# Good acronym, bad ordering?

SOLID principles are helpful to remember when crafting and reviewing new code with an eye towards maintainability. When coding in C# though, the way that I consider the ordering is very different to the acronym itself.

- **S**ingle responsibility (SRP) - a method (or class) should do only one thing and do it well.
- **O**pen/closed - open for extension, Closed for modification.
- **L**iskov substitution - subclasses should be substitutable for base classes without changes in behavior of the consuming code.
- **I**nterface segregation - "Many client-specific interfaces are better than one general-purpose interface."
- **D**ependency inversion - wherever possible, dependencies should be abstractions, not concrete implementations.

The acronym is cool, but does it reflect reality? That is:

- Will all of these be used every day in a given language? 
  - Does the language syntax itself guide along these lines?
  - Do these affect our daily program-structure decisions?
- Are there language patterns that are easier to understand but still foster these goals, even if they don't map cleanly to the SOLID principles?
  - Are there other principles that might be less opaque AND cover these easily?
- Are there any SOLID principles that in practice follow directly from another?

I think any and all of these may be the case.

# Let's explore other candidates

There's a lot of advice out there. These are a few for C#,that for me have stood the test of time. Let's unpack and see how these might map onto SOLID.

## Advice #1: Use Dependency Injection (DI)

From the beginning: a _dependency_ is code or functionality external to code within some boundary (method, class, namespace, assembly) that it needs to complete its assigned responsibility.

(Nothing here _enforces_ that singular "responsibility", but I'm standing by it.)

Dependency _injection_ is the practice of managing (creating; destroying) dependencies externally to the dependent code and only injecting references to it. For classes strictest form enforces **constructor injection**: dependencies are consumed through constructor parameters; conversely, constructor parameters are only for consuming dependencies. The consumption point is clear and descriptive, and the class is fully "prepped" for activity on construction.

Note that I have _not_ mentioned the use of any particular DI (AKA "IoC") container/framework, or even _any_ framework. That is possible and sometime preferrable in situations with small dependency graphs and short program lifetime: using constructor injection and just "`new`ing up" the graph as needed. Using a framework can save brain-cycles, but when CPU cycles are at a premium you can stay close to the metal.

In order to see the benefits, let's consider prior alternatives. I'll try my best to "steel-man" these models, not undercut them with bad practices.

### How we got here

Let's wind back the C# clock: before there were even shortcuts for this (or probably almost contemporaneously with the shortcuts, because we software engineers are lazy and too clever by half) it became clear that classes that created and managed their dependencies internally were too complex, even when **Single Responsibility** was otherwise followed. On top of their core purpose, they also had to handle things like object lifetimes and complex cleanup. Should that boilerplate/plumbing really have been part of their job? 

This also implicitly breaks **Dependency Inversion** because even if we've "coded to interfaces" the class needs to know what implementation to use so it can build and manage it. 

Maybe this is _somewhat_ acceptable. Can this leakage be "contained" in the constructor and `Dispose()`?

``` csharp
public class BasicDep { ... }

public class ManagedDep : IDispose { ... }

public class Dependent : IDispose // has to; it owns ManagedDep
{
  private readonly BasicDep basic;
  private readonly ManagedDep managed;

  public Dependent()
  {
    // easy peasy
    basic = new();

    // a little harder...
    managed = new();
  }

  public void Dispose() 
  {
    // skipping over the full Dispose pattern...
    managed.Dispose();
  }
}
```

Okay, not too bad. For configuration, you could go global (a la `app.config` 'n' `ConfiurationManager`) or just pass it through every later that needs it. That can get ugly though, and isn't strictly "uninjected". It does allow more flexibility in configuration though.

We could also add interfaces for the dependencies, 

```csharp
public class Dependent : IDispose // has to; it owns ManagedDep
{
  private readonly IBasic basic;

  public Dependent()
  {
    // contained in constructor, but so?
    basic = new BasicDep();
  }
}
```

but it's less clear what utility that provides if the concrete constructor is being called directly anyway. 

### Difficulties even with non-inverted strict style

As the scenarios get more complicated the boilerplate does as well. This impinges more and more on the "single responsibility" of the class.

If there are alternate implementations of dependencies the class has to "know" how to make decisions about which to choose. If that is

Testing ...

Alternate lifetimes: singleton dependencies; factories ...

### Factories point the way

([service locator](why_not_service_locator.md)) ...

Move dependency construction out of the constructor ...

Move cleanup out of `Dispose()` (lifetime scopes) ...

### Mapping back to SOLID

Dependency management become a "single responsibility" managed elsewhere. ...

Very clean inversion: concrete dependecies nowhere in sight. ...

### Other benefits

Clear boundaries. ...

Clear listing of dependencies. ...

### Go heavy on the "USE"

Really dig in; once you've got one or two tools (along with just "newing up," don't ignore that!) really seek to deepen your understanding of what they can do and how they should be properly used. 


## Advice #2: Code to interfaces

In a sense, this is the component- or module-oriented analogue to pure functions in FP. ...


## Advice #3: Favor composition over inheritance

...


## Advice #4: You're not writing code for the machine, you're writing it for other developers

Focus on optimizing the _developer experience_ before optimizing for the machine. ...