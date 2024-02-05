module Theme

module Theme =
    let color (nr: int option) =
        let nr =
            match nr with
            | Some nr -> nr
            | _ -> System.Convert.ToInt32((new System.Random()).NextDouble() * 8.0)

        match nr with
        | 0 -> "#ffc83d"
        | 1 -> "#e3008c"
        | 2 -> "#4f6bed"
        | 3 -> "#ca5010"
        | 4 -> "#00b294"
        | 5 -> "#498205"
        | 6 -> "#881798"
        | _ -> "#986f0b"
