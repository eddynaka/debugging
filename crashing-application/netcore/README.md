# How to find out why my application is crashing

## What tool should I use to understand what is happening

To understand what is happening, you need to generate a dump of your process. To
do that, you can use the following tools:

- [DebugDiag](https://debugdiag.com)
- [ProcDump](https://docs.microsoft.com/en-us/sysinternals/downloads/procdump)
- [dotnet-dump](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-dump)

## How to analyze the dump file using WinDbgX

1. Open
   [WinDbgX](https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/windbg-command-line-preview)
2. Load the generated dump
3. Check if
   [SOS](https://github.com/dotnet/diagnostics/blob/master/documentation/sos-debugging-extension-windows.md)
   loaded correctly:

    ```text
    0:000> .chain
    Extension DLL chain:
        sos: image 3.1.137102+c62ab0fb98a60d9c889b3db47d2a4e56d5c69321, API 2.0.0, built Wed Jul 22 00:09:27 2020
            [path: C:\path\Local\DBG\ExtRepository\EG\cache2\Packages\SOS\3.1.2.37102\win-x64\sos.dll]
    ```

4. Retrieve the last exception:

    ```text
    0:000> !pe
    Exception object: 0000027c4a620078
    Exception type:   System.Exception
    Message:          crashing my application
    InnerException:   <none>
    StackTrace (generated):
        SP               IP               Function
        000000F3A777E2E0 00007FFB3B0A552F CrashingApplication!CrashingApplication.Program.Main(System.String[])+0x7f

    StackTraceString: <none>
    HResult: 80131500
    ```

5. Check the stack to see how it happened:

    ```text
    0:000> !clrstack
    OS Thread Id: 0x23f0 (0)
            Child SP               IP Call Site
    000000F3A777E1E8 00007ffc072f3e49 [HelperMethodFrame: 000000f3a777e1e8]
    000000F3A777E2E0 00007ffb3b0a552f CrashingApplication.Program.Main(System.String[]) [C:\Microsoft\debugging\crashing-application\netcore\Program.cs @ 14]
    ```

## How to analyze the dump file using dotnet-dump

1. Load the dump file:

    ```shell
    dotnet-dump analyze dump.dmp
    ```

2. Retrieve the exception last exception:

    ```text
    > pe -lines
    Exception object: 0000027c4a620078
    Exception type:   System.Exception
    Message:          crashing my application
    InnerException:   <none>
    StackTrace (generated):
        SP               IP               Function
        000000F3A777E2E0 00007FFB3B0A552F CrashingApplication!CrashingApplication.Program.Main(System.String[])+0x7f [C:\Microsoft\debugging\crashing-application\netcore\Program.cs @ 14]

    StackTraceString: <none>
    HResult: 80131500
    ```

3. Check the stack to see how it happened:

    ```text
    > clrstack
    OS Thread Id: 0x23f0 (0)
            Child SP               IP Call Site
    000000F3A777E1E8 00007ffc072f3e49 [HelperMethodFrame: 000000f3a777e1e8]
    000000F3A777E2E0 00007FFB3B0A552F CrashingApplication.Program.Main(System.String[]) [C:\Microsoft\debugging\crashing-application\netcore\Program.cs @ 14]
    ```
