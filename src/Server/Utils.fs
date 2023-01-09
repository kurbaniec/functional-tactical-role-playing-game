namespace Utils

// Weird, but it seems this is missing
// TODO: Some functions seem to be missing?
// See: https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-resultmodule.html
module Result =
    // See: https://github.com/dotnet/fsharp/blob/main/src/FSharp.Core/result.fs#L39-39
    let defaultValue value result =
        match result with
        | Error _ -> value
        | Ok v -> v
