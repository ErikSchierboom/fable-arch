module Fable.Arch.Virtualdom

open Fable.Core
open Fable.Core.JsInterop

open Html
open App

[<Import("h","virtual-dom")>]
let h(arg1: string, arg2: obj, arg3: obj[]): obj = failwith "JS only"

[<Import("diff","virtual-dom")>]
let diff (tree1:obj) (tree2:obj): obj = failwith "JS only"

[<Import("patch","virtual-dom")>]
let patch (node:obj) (patches:obj): Fable.Import.Browser.Node = failwith "JS only"

[<Import("create","virtual-dom")>]
let createElement (e:obj): Fable.Import.Browser.Node = failwith "JS only"

let createTree<'T> (handler:'T -> unit) tag (attributes:Attribute<'T> list) children =
    let toAttrs (attrs:Attribute<'T> list) =
        let elAttributes = 
            attrs
            |> List.map (function
                | Attribute (k,v) -> (k ==> v) |> Some
                | _ -> None)
            |> List.choose id
            |> (function | [] -> None | v -> Some ("attributes" ==> (createObj(v))))
        let props =
            attrs
            |> List.filter (function | Attribute _ -> false | _ -> true)
            |> List.map (function
                | Attribute _ -> failwith "Shouldn't happen"
                | Style style -> "style" ==> createObj(unbox style)
                | Property (k,v) -> k ==> v
                | EventHandler(ev,f) -> ev ==> ((f >> handler) :> obj)
            )

        match elAttributes with
        | None -> props
        | Some x -> x::props
        |> createObj
    let elem = h(tag, toAttrs attributes, List.toArray children)
    elem

let rec render handler node =
    match node with
    | Element((tag,attrs), nodes)
    | Svg((tag,attrs), nodes) -> createTree handler tag attrs (nodes |> List.map (render handler))
    | VoidElement (tag, attrs) -> createTree handler tag attrs []
    | Text str -> box(string str)
    | WhiteSpace str -> box(string str)

let renderer =
    {
        Render = render
        Diff = diff
        Patch = patch
        CreateElement = createElement
    }