I recently reviewed a small project led by a teammate who is on fire for functional programming, with the zeal of a neophyte among heathens in a heathen land (read: F# fanboi in C# shop).

Since the work was fairly deep but constrained within a contiguous part of the codebase, he asked and I allowed him to try out the [`OneOf<>` library](https://github.com/mcintyre321/OneOf). Despite the [limitations we read about](https://matteland.medium.com/oneof-discriminated-unions-in-c-132e534bda99) I thought it would be interesting to see what he came up with, both on `OneOf<..>`'s own terms but also in exploring discriminated unions (**DU**s) more generally.

## Ordering

First a kind of obvious thing: the order-dependence of the types in `OneOf<..>` means it is very helpful if you establish a ordering-pattern early on. I suggested the "least-to-most happy" ordering, e.g. `OneOf<YuckyError, NotFound, ActualValue>`.

This allows for sucessive short-circuit handling blocks with `TryPickT0(out var badOutcome, out var remainingOutcomes)`. It still isn't very pretty though.

And unfortunately, this is a bit of a red herring. DUs will shine where you have multiple possible _successful_ return types that will have different handling in the consuming method. That will call for a different ordering-pattern. Perhaps "general-to-specific"? But what if they are peer-types? 

Or is that itself a red herring?

Either way, the limitations of the project itself (mainly a port of existing backend-automation code) did not lend itself to deeper exploration of the question. The goal was mainly to use `OneOf` to replace existing error handling. Probably not the best use of the tool. 

## Error handling highlights

It wasn't an exercise without benefit, though. Even though my teammate was entering into the problem from an overzealous position ("exceptions are just glorified `GOTO`s") it is area in which C# isn't doing itself any favors. Linguistically and historically, C# exceptions tend to paper over some important distinctions.  

### DUs: not an exception alternative

This might have been a simple rediscovery of a trivial fact of DU usage: There's really no point in catching system-level exceptions in the interior only to wrap them and pass them back up the call stack. Exception-bubbling ain't half bad for that. Whatever expense was involved in `throw`ing and `catch`ing is already paid by that point, so there's no savings.

But note that distinction...

### Error levels!

This experience did bring new light to the distinction between application errors and system errors. This is one of those areas where framework designers themselves tried to make a clean distinction and failed ([remember `ApplicationException`](https://stackoverflow.com/questions/5685923/what-is-applicationexception-for-in-net)?).

But even with new guidance (derive from `System.Exception`; define your own app-error exception base class) there's the sneaking suspicion that applications shouldn't _need_ the expense of the exception mechanism for errors that aren't really... exceptional.

So what error "levels" are there? From bare metal, upward:

- System errors that we can do nothing about: memory corruption, stack frame access, out-of-bounds memory access. These win your process a one-way ticket to SIGKILL, or maybe someone just tripped over a power cord.
- Environmental errors that are out of our control: hard I/O, out-of-memory (which maybe belongs above). You may be able to log, it may or may not mean the end of your process, and might be open to retry.
- Code (look in the mirror) errors. These come in two flavors. 
  - Domain-level errors: null-ref, arg-null, `ArgumentException`, overflows (numeric or stack). Some of these may even be intentionally composed and thrown by our code, but even then only as a guard against other poorly written code that didn't properly guard its own inputs. _And it's usually our own poorly written code._ You know you've done it. Bow your heads. _Mea culpa, mea culpa, mea maxima culpa._
  - Infrastructure errors: API call errors (`400` mainly; `404` is _not_ an error), database errors. These are still our own fault, but because they involve references to other "living" systems there can often be a "historical bad data" (feels like someone else's fault) aspect to them. The path to fixing may be somewhat harder to navigate and have unique catch-22s. These are in a sense true "_application_ errors," as opposed to 
- User input errors: invalid or otherwise corrupt input. It's our job to catch the bad ones (user's fault) and return a useful message _before_ they cause exceptions (our fault).
- Non-error "errors": e.g., authn, authz, not-found (`404`). These are only errors in the sense that some user expectation was dashed. From the engineering perspective these outcomes are (should be) fully expected and accounted for.

The upshot here is that most non-happy path flows that we need to worry about aren't really "exceptional". Maybe half-and-half. The point is that if we are hewing to the recommendation that "exceptions should not be used to control logical code flow; they should be used for _exceptional_ circumstances," then we should either be working to **avoid** them (code errors) or simply add them to our flow (validation and non-error).

"Ah ha!," my FP zealot says, "but _that's precisely why_ we need DUs in C# in the first place!!"

But do we?

## Existing C# idioms: don't spurn existing wisdom

"Wisdom" may be a bit overwrought here, but there's ways to use the language we have to effect/cover many of the cases that DUs solve outright. The trouble is, as "idioms" they are often subtle and easy to get misuse—the same way a human-language learner might trip over difficult idioms in their second language. 

### Just let exceptions happen
...

### `bool TryXxx(out ...)`
...

### `Result?`
...

### `ICommonInterface`
...