module Movement

module Type =
    let private footDistance: Lazy<Distance> = Lazy.Create(fun () -> Distance 2)
    let private mountDistance: Lazy<Distance> = Lazy.Create(fun () -> Distance 3)
    let private flyDistance: Lazy<Distance> = Lazy.Create(fun () -> Distance 3)

    let distance (m: MovementType) =
        match m with
        | Foot -> footDistance.Value
        | Mount -> mountDistance.Value
        | Fly -> flyDistance.Value
