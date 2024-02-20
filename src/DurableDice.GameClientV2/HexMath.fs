module HexMath

[<Measure>] type px
[<Measure>] type area = px ^ 2

module HexMath =
    let width = 1200.0<px>
    let paddingTop = 60.0<px>
    let paddingLeft = 60.0<px>
    let x = round (1000.0 / sqrt 3.0) / 1000.0
    let r = 9.0<px>
    let r2 = r * 2.0
    let rx = r * x
