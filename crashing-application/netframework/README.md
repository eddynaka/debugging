# How to find out why my application is crashing

## What tool should I use to understand what is happening

To understand what is happening, you need to generate a dump of your process. To do that, you can use the following tools:

- [DebugDiag](https://debugdiag.com)
- [ProcDump](https://docs.microsoft.com/en-us/sysinternals/downloads/procdump)

## How to analyze the dump file

1. Open [WinDbgX](https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/windbg-command-line-preview)
2. Load the generated dump
3. Check if [SOS](https://github.com/dotnet/diagnostics/blob/master/documentation/sos-debugging-extension-windows.md) loaded correctly:

    ```text
    0:000> .chain
    Extension DLL chain:
        C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll: image 4.8.4200.0, API 1.0.0, built Tue Jun  9 22:07:50 2020
            [path: C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll]
    ```

4. Retrieve the last exception:

    ```text
    0:000> !pe
    Exception object: 02af41a4
    Exception type:   System.Exception
    Message:          crashing my application
    InnerException:   <none>
    StackTrace (generated):
        SP       IP       Function
        00AFF1F4 0113089A CrashingApplication!CrashingApplication.Program.Main(System.String[])+0x52

    StackTraceString: <none>
    HResult: 80131500
    ```

5. Check the stack to see how it happened:

    ```text
    0:000> !clrstack
    OS Thread Id: 0x4dd4 (0)
    Child SP       IP Call Site
    00aff144 76279962 [HelperMethodFrame: 00aff144]
    00aff1f4 0113089a CrashingApplication.Program.Main(System.String[]) [C:\Microsoft\debugging\crashing-application\netframework\Program.cs @ 10]
    00aff368 6d65f036 [GCFrame: 00aff368]
    ```
