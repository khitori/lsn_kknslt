open System
open System.IO
open System.Runtime.InteropServices

module NativeMethods =
    [<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr SetWindowsHookEx(int idHook, IntPtr lpfn, IntPtr hMod, uint dwThreadId)
    [<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern bool UnhookWindowsHookEx(IntPtr hhk)
    [<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
    [<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern int GetMessage(IntPtr& lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
    [<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern bool TranslateMessage(IntPtr& lpMsg)
    [<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern bool DispatchMessage(IntPtr& lpMsg)
    [<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr GetModuleHandle(string lpModuleName)

let WH_KEYBOARD_LL = 13
let WM_KEYDOWN = 0x0100

type LowLevelKeyboardProc = delegate of int * IntPtr * IntPtr -> IntPtr

let hookCallback (nCode: int) (wParam: IntPtr) (lParam: IntPtr) : IntPtr =
    if nCode >= 0 && wParam = IntPtr(WM_KEYDOWN) then
        let vkCode = Marshal.ReadInt32(lParam)
        printfn "Key: %A" (enum<ConsoleKey> vkCode)
    NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam)
let setHook (proc: LowLevelKeyboardProc) =
    let hMod = NativeMethods.GetModuleHandle(null)
    NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, Marshal.GetFunctionPointerForDelegate(proc), hMod, 0u)
let runMessageLoop () =
    let mutable msg = IntPtr.Zero
    while NativeMethods.GetMessage(&msg, IntPtr.Zero, 0u, 0u) <> 0 do
        NativeMethods.TranslateMessage(&msg) |> ignore
        NativeMethods.DispatchMessage(&msg) |> ignore


[<EntryPoint>]
let main argv =
    let hookDelegate = LowLevelKeyboardProc(hookCallback)
    let hookId = setHook hookDelegate
    if hookId = IntPtr.Zero then
        printfn "Failed to set hook"
        1
    else
        runMessageLoop ()
        NativeMethods.UnhookWindowsHookEx(hookId) |> ignore
        0