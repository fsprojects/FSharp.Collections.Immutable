#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

[<AutoOpen>]
module FunctionalUtils =
    let inline flip f a b = f b a
    let inline uncurry f (a, b) = f a b

    let inline applyOverFuncs f g h x = f (g x) (h x)
    let inline applyOverArgs f g x y = f (g x) (g y)

    let inline fst3 (a, _, _) = a
    let inline snd3 (_, a, _) = a
    let inline thd3 (_, _, a) = a
