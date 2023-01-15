namespace Utils

open FSharpx.Collections

module Map =
    // See: https://stackoverflow.com/a/3974842
    let join (p: Map<'a, 'b>) (q: Map<'a, 'b>) =
        Map.fold (fun acc key value -> Map.add key value acc) p q

// Weird, but it seems this is missing
// See: https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-resultmodule.html
module Result =
    // See: https://github.com/dotnet/fsharp/blob/main/src/FSharp.Core/result.fs#L39-39
    let defaultValue value result =
        match result with
        | Error _ -> value
        | Ok v -> v

module Dictionary =
    type Dictionary<'K, 'V> = System.Collections.Generic.Dictionary<'K, 'V>
    type KeyValuePair<'K, 'V> = System.Collections.Generic.KeyValuePair<'K, 'V>
    let add (key) (value) (table: Dictionary<'K, 'V>): Dictionary<'K, 'V> =
        table.Add(key, value)
        table

    let ofMap (table: Map<'K, 'V>): Dictionary<'K, 'V> =
        // See: https://stackoverflow.com/a/27109405/12347616
        Dictionary(table)

    let ofList (list: list<('K*'V)>) =
        // See: http://www.fssnip.net/1t/title/Dictionary-extensions
        Dictionary(list |> Map.ofList)

    let ofSeq (seq: seq<('K*'V)>) =
        Dictionary(seq |> Map.ofSeq)
