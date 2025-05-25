# Configuration patterns
## `ISettingPointer` and `ISettings`
These are wafer-thin interfaces that enable a few handy startup-time extensions for reading and registering
configuration objects. It's kinda sneaky though, because they subtly enforce conventional and centralized
configuration. Let's say, "encourage."

The simplest case for use of the two interfaces is shown in `GenericNestedConfig_SingleUse`.

To see the value of the method, consider the more "traditional" way of relating Parent/Sub. Without a way of
separating the different consumers of `SubService`, what would our options be? Usually, it would be assumed that
`ParentService` would assume the load of hosting the configuration entities and then building `SubService`. This
might be done directly or through a factory object. If there are lifetime considerations for `SubService`,
`ParentService` owns those as well, because it owns the `SubService` instance that it created. Note how this
undercuts SRP within Parent. It also leads to abstraction-leak: even if it can use the factory to rely only on
the contract `ISubService`, it still needs to "know" about the configuration.

By allowing the `SubService` to manage its own configuration and conventions, `ParentService` is now released
from the responsibilities of managing it at all. It doesn't need to define configuration it doesn't need, just
a path where sub-configurations will be stored. It
doesn't need to "know" about `SubService`'s configuration needs. And all the `ISubService` lifecycle concerns
(which might be different for different `ISubService` implementations!) are left
to the DI container.

## Storage Service Example

A "storage service" is a bread-and-butter bit of infrastructure code that is necessary for almost any non-trivial
enterprise project. However, there really is no point in attempting to release one as a library: one would end up
building both too much and too little. The variety of storage architectures and possible use-cases are maddening,
but the actual requirements of a given project may be rather modest. Why take on a massive third-party dependency
of which only a fraction of the code is needed, and yet fails to implement that _one edge case_...?

I submit, however, that there are some very predictable patterns to how a storage library is built and bound as a
dependency in code. I will walk through each of these.

First note the fundamental assumption of the use of a "strategy"
pattern: different needs and environments will dictate different storage requirements; these differing requirements
are best met through differing implementations of the same underlying contract.
1. **Startup-time** - the strategy is fixed in configuration.
   - **Single-dependency** - there is need for access to only one configured storage endpoint.
   - **Multiple-fixed** - there is a need to access a fixed-in-code number of configured storage endpoints.
   - **Multiple-variable** - the endpoints are pulled from a configuration list.
2. **Application-time** - the strategy-configurations are determined in the "user-code" of the application.
   - Same multiplicities.

The trick is finding a "strategy framework" that will support all of these from within a single structure. Let's see if
we can find one.

### The shape of the grain of salt
I want to stress that this is not meant as even an outline of a proposed `IStorageService` framework except in
the most remote fashion. Again, there are a multitude of possible variations that I would not guess at until presented
with an actual use case.[^1] Perhaps it would be best to see this as a simple stand-in example to demonstrate somewhat
complex strategy-pattern concerns in a real-world C# application, and nothing more. `ISettingPointer` is the
lesson. Everything else is left as an exercise for the reader.

This is the crux of the need for cogent library-authoring skills in any given organization. There are a lot of great code
tools that we can get off the Nuget shelf, and C# itself is a very powerful tool. But even with a powerful toolset,
there will always be a need for base-level code that is conditioned for unique infrastructure needs and domain
problems, maintained and fully understood in house.

[^1]: This is generally why it is so hard to author a meaningful example of a complex language or framework feature like
dependency injection, which only really shines in a complex domain. Sadly, this may be why there is such proliferation
of badly implemented dependency injection in the wild.
