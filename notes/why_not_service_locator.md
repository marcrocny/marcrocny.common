# Why not Service Locator

A pattern/tool that seems to fix the problem of binding implementations to concrete classes is a Service Locator (which most any DI container can be subverted to implement). Instead of the class breaking Dependency Inversion, it has access to a component (usually global) that allows it to *resolve* an implementation when given a contract/interface. Inversion remains intact.

However, the class still has to manage everything it creates. This isn't just a extra plumbing either, it leads to a problems of leaky abstractions and doesn't solve the coupling issues. 

## Resource Management Leakiness

Consider a service `ProfileService` that has a dependency on `IImageHandler`. Following good DDD it has several such dependencies, and tests are written for it and everything works. It doesn't need to know the internal details of `IImageHandler` or its other dependencies, it can be fully complete (up to Unit Tests) before they are written.

Now we move on to implement `IImageHandler`. We build `FileImageHandler` in a similar DDD fashion, relying on `IFileStore`. We create a `BasicFileStoreImpl`, it has a dependency on singleton configuration, easy peasy. In every case, the implementations build their dependencies by proxy, calling `ServiceLocator.Resolve<T>()`. Everything works.

::: mermaid
classDiagram
  class PS["ProfileService"]
  class IH["IImageHandler"]
  class FIH["FileImageHandler"]
  class IFS["IFileStore"]
  class BFS["BasicFileStoreImpl"]

  PS --> IH
  FIH --|> IH
  FIH --> IFS
  BFS --|> IFS
:::

### What about managed resources?

Now we want to introduce `CloudFileStoreImpl`. It contains a managed resource and must implement `IDisposable`.

::: mermaid
classDiagram
  class PS["ProfileService"]
  class IH["IImageHandler"]
  class FIH["FileImageHandler"]
  class IFS["IFileStore"]
  class BFS["BasicFileStoreImpl"]
  class CFS["CloudFileStoreImpl"]

  PS --> IH
  FIH --|> IH
  FIH --> IFS
  BFS --|> IFS
  CFS --|> IFS
  CFS --|> IDisposable
:::

Now the quandary begins: this method of `IDisposable` cannot be treated as an implementation detail. With `ServiceLocator` services build their own dependencies by proxy. They own them, so they have to clean them up. They can't simply let them get collected when de-referenced, disposal must be deterministic. First `IFileStore` will have to extend `IDisposable` to communicate this requirement upward. `BasicFileStoreImpl` will have to contain a no-op `Dispose()` implementation. `FileImageHandler` will have to implement it, communicate it through `IImageHandler` and then `ProfileService` itself will have to implement it to clean up its `IImageHandler` dependency.

::: mermaid
classDiagram
  class PS["ProfileService"]
  class IH["IImageHandler"]
  class FIH["FileImageHandler"]
  class IFS["IFileStore"]
  class BFS["BasicFileStoreImpl"]
  class CFS["CloudFileStoreImpl"]

  PS --> IH
  FIH --|> IH
  FIH --> IFS
  BFS --|> IFS
  CFS --|> IFS
  CFS --|> IDisposable
  IFS --|> IDisposable
  BFS --|> IDisposable
  FIH --|> IDisposable
  IH --|> IDisposable
  PS --|> IDisposable
:::

This lifetime requirement of one low-level implementation has now has infected every other service and contract with `IDisposable`, many of which do not even need it. Not only does it infect upwards to dependent services, but also sideways. Once one implementation of a contract is managed, `IDisposable` becomes bound to the contract itself.

## Lack of Lifetime Control

Not sure if this is true, let's see. Is there a way to implement sophisticated lifetime control with a Service Locator?

What for? This would be a resolution scenario such as for web requests, or multiple services running alongside each other that need parallel but independent resources (e.g., parallel database connections). In this case, objects resolved within the scope have the option of being constructed once within the scope and shared among dependents built within the scope. This is more granular than Singleton scope (one instance for the entire process lifetime) or Transient scope (a new instance on every resolution).

### "Strict" SL
So far, we've looked at a particular "strict" form of Service Locator usage: the Locator is a global singleton that is directly referenced by a dependent's constructor to resolve its dependencies. At first it would seem that only singleton and transient are available: lacking any way to communicate through the parameterless constructor to the `Resolve(..)` operations within it, there's no way to identify the scope of the object.

But the object itself could be tagged. What would this look like?

``` csharp
// basic dependency & impl, scoped?
interface IService { string Foo(); }
class ScopedServiceImpl : IService { ... }

// basic SL dependency resolution
class Dependent
{ 
    readonly IService service;
    public Dependent() => service = Locator.Instance.Resolve<IService>();
}

// thought 1: crib Scope
interface ILocatorScope
{
    ILocatorScope ChildScope();
    T Resolve<T>();
}

//show usage ...

```

...

## Less Strict Issues

### Bleeding out of the constructor

