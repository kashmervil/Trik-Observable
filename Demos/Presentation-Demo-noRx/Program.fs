﻿open Trik
open Trik.Collections

[<EntryPoint>]
let main _ = 
    use model = new Model()
    let motorL = model.Motor.[M2]//Two ports in the controller's behind
    let motorR = model.Motor.[M1]//you can change ports to any of the range M1 .. M4 
    //for full ports description go http://goo.gl/jRWJ4j
    let power = model.AnalogSensor.[A1].ToObservable() 
                |> Observable.map (fun d -> if d < 450 then 100 elif d > 550 then -100 else 0) 
                |> Observable.DistinctUntilChanged

    use l_disp = power.Subscribe(motorL)
    use r_disp = power.Subscribe(motorR)
    let ledStream = power |> Observable.map (function | 100 -> LedColor.Green 
                                                      | 0   -> LedColor.Orange 
                                                      | _   -> LedColor.Red)
    use led_disp = ledStream.Subscribe(model.Led)
    System.Console.ReadKey() |> ignore
    0
