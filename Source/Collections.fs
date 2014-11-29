﻿namespace Trik
open System

[<AutoOpen>]
module Collections = 
    
    [<Struct>]
    type Point(x: int, y: int, z: int) =
        member self.X = x
        member self.Y = y
        member self.Z = z
        override self.ToString() = sprintf "X= %d Y= %d Z= %d" x y z

        static member Zero = new Point(0,0,0)
        static member (+) (c1: Point, c2: Point) = new Point(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z)
        static member (-) (c1: Point, c2: Point) = new Point(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z)
        static member (*) (c1: Point, c2: Point) = new Point(c1.X * c2.X, c1.Y * c2.Y, c1.Z * c2.Z)

    type ButtonEventCode  = 
     | Sync  = 0
     | Esc   = 1
     | Enter = 28 
     | Up    = 103 
     | Left  = 105 
     | Right = 106
     | Down  = 108
     | Power = 116
     //| Menu  = 139
      
    [<Struct>]
    type ButtonEvent(button: ButtonEventCode, isPressed: bool) = //, timeStamp: double) = 
        member self.Button = button
        member self.IsPressed = isPressed
        //member self.TimeStamp = timeStamp
        member self.AsTuple = button, isPressed//, timeStamp

        new (code: int, isPressed: bool) = //, timeStamp: double) = 
            ButtonEvent(enum<ButtonEventCode> code, isPressed)//, timeStamp)
        new (code: uint16, isPressed: bool) = //, timeStamp: double) = 
            ButtonEvent(int code, isPressed) //, timeStamp)
        override self.ToString() = button.ToString() + " " + isPressed.ToString()// + " " + timeStamp.ToString() 

    [<AutoOpen>]
    module ServoMotor =
        type Kind = {
            min: int 
            max: int
            zero: int
            stop: int
            period:int
            }
        let Servo180 = { stop = 0; zero = 1600000; min = 800000; max = 2400000; period = 20000000 }
        let Servo2   = { Servo180 with zero = 1700000; min = 900000; max = 2300000 } 
        let LEDControllerServo = { stop = 0; zero = 0; min = 0; max = 2000000; period = 2000000 }
        let observerEps = 100
    
    /// Numbers of I2C ports associated with Stripe  
    type LedStripePorts = {
        Red: int
        Green: int
        Blue: int
        Ground: int
        }
    [<RequireQualifiedAccess>]
    type PadEvent = 
        | Pad of int * ( int * int ) option
        | Button of int
        | Wheel of int
        | Stop
    
    [<Flags>]
    type LedColor = Green = 1 | Red = 2 | Orange = 3 | Off = 0
    
    let inline private parse x = Trik.Helpers.fastInt32Parse x

    [<Struct>]
    type LineLocation(x: int, crossroad: int, mass: int) = 
        member self.X = x
        member self.Crossroad = crossroad
        member self.Mass = mass
        new(x: string, c: string, m: string) = new LineLocation(parse x, parse c, parse m)
        override self.ToString() = sprintf "loc: %d %d %d\n\n" self.X self.Crossroad self.Mass

    [<Struct>]
    type ObjectLocation(x: int, y: int, mass: int) = 
        member self.X = x
        member self.Y = y
        member self.Mass = mass
        new(x: string, y: string, m: string) = new ObjectLocation(parse x, parse y, parse m)
        override self.ToString() = sprintf "loc: %d %d %d\n\n" self.X self.Y self.Mass

    [<Struct>]
    type DetectTarget(hue: int, hueTolerance: int, saturation: int, saturationTolerance: int, value: int, valueTolerance: int) =
        member self.Hue = hue
        member self.HueTolerance = hueTolerance
        member self.Saturation = saturation
        member self.SaturationTolerance = saturationTolerance
        member self.Value = value
        member self.ValueTolerance = valueTolerance
        new (hue: string, hueTolerance: string
            , saturation: string, saturationTolerance: string
            , value: string, valueTolerance: string) = new DetectTarget(parse hue
                                                        , parse hueTolerance, parse saturation
                                                        , parse saturationTolerance, parse value
                                                        , parse valueTolerance)

        override self.ToString() = String.Format("hsv {0} {1} {2} {3} {4} {5}", hue, hueTolerance, saturation, saturationTolerance, value, valueTolerance)

    type VideoSensorOutput<'Location> = 
            Location of 'Location
            | Target of DetectTarget with 
        member self.TryGetLocation = match self with
                                     | Location l -> Some l
                                     | Target _ -> None
        member self.TryGetTarget = match self with
                                   | Location _ -> None
                                   | Target t -> Some t