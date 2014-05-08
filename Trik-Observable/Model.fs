﻿namespace Trik
open System
open Trik.ServoMotor

type Model () as model = 
    //do printfn "Creating of model"\
    
    let mutable gyro = None
    let mutable accel = None
    let mutable led = None
    let mutable pad = None
    let mutable motor = None
    let mutable servo = None
    let mutable encoder = lazy (
        model.EncoderConfig 
        |> Array.map (fun (port, cnum) -> (port, new Encoder(cnum)))
        |> dict
    )
    let mutable analogSensor = None

    member val ServoConfig = 
        [| 
          ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
            { stop = 0; zero = 1450000; min = 1200000; max = 1800000; period = 20000000 } )
          ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
            { stop = 0; zero = 1450000; min = 1200000; max = 1800000; period = 20000000 } )
         |] with get, set
    member val EncoderConfig =
        [| 
          ("JB2", 0x31)
          ("JB4", 0x32)
          ("JB3", 0x33)
         |] with get, set
    member val MotorConfig = 
        [| 
          ("JM1", 0x14)
          ("JM2", 0x15)
          ("M1", 0x16)
          ("JM3", 0x17)
         |] with get, set
    member val AnalogSensorConfig = 
        [| 
          ("JA1", 0x25 )
          ("JA2", 0x24 )
          ("JA3", 0x23 )
          ("JA4", 0x22 )
          ("JA5", 0x21 )
          ("JA6", 0x20 )
        |] with get, set
    member x.Motor
        with get() = 
            let motorDefaultInit() = 
                IO.File.WriteAllText("/sys/class/gpio/gpio62/value", "1")
                Helpers.I2C.send 0x10 0x1000 2
                Helpers.I2C.send 0x11 0x1000 2
                Helpers.I2C.send 0x12 0x1000 2
                Helpers.I2C.send 0x13 0x1000 2
                motor <- 
                    x.MotorConfig
                    |> Array.map (fun (port, cnum)  -> (port, new PowerMotor(cnum)))             
                    |> dict
                    |> Some
            defaultArg motor (motorDefaultInit(); motor.Value)
 
         
    member x.Servo
        with get() = 
            let servoDefaultInit() = 
                servo <- 
                    x.ServoConfig
                    |> Array.map (fun (port, path, kind) ->  (port, new Servomotor(path, kind)))             
                    |> dict
                    |> Some
            defaultArg servo (servoDefaultInit(); servo.Value)
        
    
    member x.AnalogSensor
        with get() = 
            let analogSensorDefaultInit() = 
                analogSensor <-
                    x.AnalogSensorConfig
                    |> Array.map (fun (port, cnum) -> (port, new AnalogSensor(cnum)))
                    |> dict
                    |> Some
            defaultArg analogSensor (analogSensorDefaultInit(); analogSensor.Value)
    
    member x.Encoder
        with get() = encoder.Force()

    member x.Gyro
        with get() = 
            let gyroDefaultInit() =
                gyro <- Some(new Trik.Gyroscope(-32767, 32767, "/dev/input/by-path/platform-spi_davinci.1-event"))
            defaultArg gyro (gyroDefaultInit(); gyro.Value)

    member x.Accel
        with get() = 
            let accelDefaultInit() = 
                accel <- Some(new Trik.Accelerometer(-32767, 32767, "/dev/input/event1"))
            defaultArg accel (accelDefaultInit(); accel.Value)
        
    member x.Led 
        with get() = 
            let ledDefaultInit() =
                led <- Some(new Trik.Led("/sys/class/leds/"))
            defaultArg led (ledDefaultInit(); led.Value)
    
    member x.Pad 
        with get() = 
            let padDefaultInit() =
                pad <- Some(new Trik.PadServer(4444))
            defaultArg pad (padDefaultInit(); pad.Value)

    interface IDisposable with
        member x.Dispose() = 
            let inline dispose (device: 'T option when 'T :> IDisposable) = 
                 device |> Option.iter (fun x -> x.Dispose())
            let inline disposeMap (devices: Collections.Generic.IDictionary<string, 'T> option when 'T :> IDisposable) = 
                devices |> Option.iter (Seq.iter (fun x -> x.Value.Dispose()))
            dispose gyro
            dispose accel
            dispose led
            dispose pad
            disposeMap motor
            disposeMap servo
            disposeMap analogSensor 
