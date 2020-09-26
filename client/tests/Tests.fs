module Tests

open Fable.Mocha
open App


let allTests = testList "All" [
    
]

[<EntryPoint>]
let main (args: string[]) = Mocha.runTests allTests