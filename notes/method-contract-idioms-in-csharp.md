Every language has idioms: sayings or constructions that form more or less organically over time, often becoming a _de facto_ part of grammar.

This develops differently in human languages than in "machine" language...

C# has a long history ...

# _on hold_
Kind of forgot about this one before starting [OneOf](/notes/oneof-reflections.md). 
It turns out that apparently I've been mulling the linguistic-idiom analogy for a while. Also, exploring discriminated unions covers a subset of the topic here.
They work to form a tension against looser, undisciplined syntactic forms; 
a discipline outside C# that can nonetheless be put in dialog with it,
and allows us to "come to rest" on a set of cogent practices within C#.

(In a very real sense, this is my primary, up-'til-now inchoate reason for exploring ideas from functional programming.
Exploring another country to see my own with new eyes.)

So in that article we should see 
- a pretty complete set of return value type-discipline; 
- _maybe_ an exploration of parameter type-discipline.

That leaves for this article the larger exploration of method discipline.
- Contractual approach (even when interfaces aren't present);
- naming/abstraction;
- allowing pure functions (more FP) to inspire but not formally dictate our approach;
- minimal requirements, maximal return;
- ... _more tbd_?
- as at least a corollary—let's just say it—death to `ref`.