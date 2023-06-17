# Good acronym, bad ordering?
SOLID principles are helpful to remember when crafting and reviewing new code with an eye towards maintainability. When coding in C# though, the way that I consider the ordering is very different to the acronym itself.

- **S**ingle responsibility (SRP) - a method (or class) should do only one thing and do it well.
- **O**pen/closed - less opaque: Open for extension, Closed for modification.
- **L**iskov substitution - subclasses should be substitutable for base classes without changes in behavior of the consuming code.
- **I**nterface segregation - "Many client-specific interfaces are better than one general-purpose interface."
- **D**ependency inversion - wherever possible, dependencies should be abstractions, not concrete implementations.

The acronym is cool, but does it reflect reality? That is:

- Will all of these be used every day in a given language? Does the language syntax itself guide along these lines?
- Is there a language pattern that is easy to understand and fosters these goals, but doesn't map cleanly or all to a single SOLID principle?
- Are there any SOLID principles that in practice follow directly from another?
- Are there other principles that might be less opaque AND cover these easily?

I think any and all of these may be the case.

# Let's explore: other candidates

There's a lot of advice out there. Here's a few that I like for C#, let's see how these map onto SOLID, and if they are (in a sense) isomorphic.

## Use Dependency Injection (DI)

Note that this does *not* mean the use of any particular DI (AKA "IoC") container/framework. Microsoft includes its own now, but also note: this doesn't mean that you have to use *any* kind of framework. A while back, folks like Mark Seeman would even advocate for "poor man's DI" in situations with small dependency graphs and short program lifetime: using constructor injection and just "`new`ing up" the graph as needed.

So if we're not joining the container wars (which have probably died down by now; didn't Autofac win?) then what is this about?

Let's wind back the C# clock: before there were even shortcuts for this (or probably almost contemporaneously, because we software engineers are lazy and too clever by half) it became clear that classes that created and managed their dependencies internally were too complex, even when SRP (above) was otherwise followed. On top of their core purpose, they also had to handle things like object lifetimes and complex cleanup. Should those really have been part of their job? 

Also, this implicitly breaks Dependency Inversion (sorry, DI is already taken) because even if we've "coded to interfaces" the class needs to know what implementation to use so it can build and manage it. The alternatives were to either
- accept breakage and further break SRP by just putting the functionality in the class, or
- accept the situation.

### Service Locator to the rescue?

A pattern/tool that seems to fix the problem is using a Service Locator (which most any DI container can be subverted to implement). Instead of the class breaking Dep. Inversion, allow it to access a component (usually global) that allows it to *resolve* an implementation when given a contract/interface. Inversion intact!

But problem: the class still has to manage everything it creates. In C#, this can be an issue if these are **disposable** (that is, require resource cleanup). To any (astute? too clever?) developer, this plumbing just *feels* (one might say "smells") like a concern that could be DRY'd away.

The solution 


## Code to interfaces

## Favor composition over inheritance

