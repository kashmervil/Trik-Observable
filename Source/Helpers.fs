﻿module Trik.Helpers

open System
open System.Runtime.InteropServices
open System.Collections.Generic
open System.IO

[<AutoOpenAttribute>]
module Measures = 
    [<Measure>] type ms
    let millisec = 1<ms>
    [<Measure>] type permil

    [<Measure>] type tick

    [<Measure>] type rad 


let isLinux = not <| Environment.OSVersion.VersionString.StartsWith "Microsoft"
let inline trikSpecific f = if isLinux then f () else ()

let SendToShell cmd  = 
    let args = sprintf "-c '%s'" cmd
    //printfn "%s" args
    trikSpecific <| fun () ->
        let proc = System.Diagnostics.Process.Start("/bin/sh", args)
        proc.WaitForExit()
        if proc.ExitCode  <> 0 then
            printf "Init script failed '%s'" cmd

let AsyncSendToShell cmd = async { SendToShell cmd }
let PostToShell cmd = AsyncSendToShell cmd |> Async.Start

module I2C =
    [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_init(string, int, int)
    [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_SendData(int, int, int) 
    [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern int private wrap_I2c_ReceiveData(int)
    
    let private I2CLockObj = new Object()

    let I2CLockCall f args : 'T = 
        if isLinux then lock I2CLockObj <| fun () -> f args
        else Unchecked.defaultof<'T>

    let inline Init string deviceId forced = I2CLockCall wrap_I2c_init (string, deviceId, forced)
    let inline Send command data len = I2CLockCall wrap_I2c_SendData (command, data, len)  
    let inline Receive (command: int) = I2CLockCall wrap_I2c_ReceiveData command

let loadIniConf path = 
    IO.File.ReadAllLines (path)
        |> Seq.choose(fun s -> 
            if s.Length > 0 then 
                let parts = 
                    s.Split ([| '=' |], StringSplitOptions.RemoveEmptyEntries) 
                    |> Array.map(fun s -> s.Trim([| ' '; '\r' |]) )
                Some(parts.[0], parts.[1]) 
            else None)
        |> dict

let fastInt32Parse (s:string) = 
    let mutable n = 0
    let start = if s.Chars 0 |> Char.IsDigit then 0 else 1
    let sign = if s.Chars 0 = '-' then -1 else 1
    let zero = int '0'
    for i = start to s.Length - 1 do 
        n <- n * 10 + int (s.Chars i) - zero
    sign * n

/// Squishes Value between lowBound and upBound
let inline limit lowBound upBound value = if upBound < value then upBound 
                                          elif lowBound > value then lowBound 
                                          else value  

let inline milliseconds x = 1<ms>*x

let inline permil min max v = 
    let v' = limit min max v
    (1000<permil> * (v' - min))/(max - min)

let HSVtoRGB (h, s, v) =
    if s = 0.0 then (v, v, v) 
    else
        let hs = h / 60.0
        let i = floor (hs)
        let f = hs - i
        let p = v * ( 1.0 - s )
        let q = v * ( 1.0 - s * f )
        let t = v * ( 1.0 - s * ( 1.0 - f ))
        match int i with
            | 0 -> (v, t, p)
            | 1 -> (q, v, p)
            | 2 -> (p, v, t)
            | 3 -> (p, q, v)
            | 4 -> (t, p, v)
            | _ -> (v, p, q)
