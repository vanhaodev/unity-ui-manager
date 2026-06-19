---
description: A full-screen "please wait" overlay that blocks input while work runs.
---

# Loading Block

A **loading block** is the overlay you show while something is happening — a network call, a save, a
scene load. It covers the screen so the player can't tap anything until the work finishes.

## The easy way — wrap your work

Pass your async work to `LoadingBlock` and the overlay shows before it starts and hides when it ends,
even if the work throws:

```csharp
await ui.LoadingBlock<LoadingBlockDefault>(async () =>
{
    await api.SaveGame();
});
```

Need a result back? Use the overload that returns a value:

```csharp
var profile = await ui.LoadingBlock<LoadingBlockDefault, Profile>(async () =>
{
    return await api.FetchProfile();
});
```

## The manual way — show and hide yourself

When the work isn't a single `await`, open it with a handle and dispose it when you're done:

```csharp
using (ui.LoadingBlock<LoadingBlockDefault>())
{
    DoSomeWork();
}   // overlay hides here
```

## It's safe to nest

The loading block counts how many times it's been opened, so overlapping requests just work — the
overlay stays up until the **last** one finishes. Two systems can both show it at once and neither
will hide it out from under the other.

```csharp
ui.LoadingBlock<LoadingBlockDefault>();  // shown
ui.LoadingBlock<LoadingBlockDefault>();  // still one overlay
// …both finish… → overlay hides
```

If something goes wrong and you need to force it closed regardless of the count, call
`ui.ForceHideLoadingBlock()`.

## Custom look

Use the built-in `LoadingBlockDefault`, or make your own by inheriting from `BaseLoadingBlock`,
designing the prefab (spinner, tip text, progress bar…), and adding it to the **Loading Blocks**
list on your UILibrary. The optional setup callback lets you configure it as it opens:

```csharp
ui.LoadingBlock<MyLoadingBlock>(onSetup: b => b.SetTip("Loading shop…"));
```

## Handy members

| Member | Description |
| --- | --- |
| `ui.LoadingBlock<T>(setup)` | Show the overlay; returns a handle — dispose to hide. |
| `ui.LoadingBlock<T>(asyncWork, setup)` | Show, run the work, hide automatically. |
| `ui.LoadingBlock<T, R>(asyncWork, setup)` | Same, but returns the work's result. |
| `ui.ForceHideLoadingBlock()` | Hide immediately, ignoring the open count. |
| `ui.IsLoadingBlockActive` | Is the overlay currently up? |
