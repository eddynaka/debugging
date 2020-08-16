# How to find out why my application is crashing

## How to analyze the dump file

1. Open WinDbgX and load the generated dump
2. Check if
   [SOS](https://github.com/dotnet/diagnostics/blob/master/documentation/sos-debugging-extension-windows.md)
   loaded correctly:

    ```text
    0:000> .chain
    Extension DLL chain:
        C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll: image 4.8.4200.0, API 1.0.0, built Tue Jun  9 22:07:50 2020
            [path: C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll]
    ```

3. Retrieve the last exception:

    Here we can see that the application thrown
    `System.StackOverflowException`. But, that isn't the root cause of the
    problem.

    ```text
    0:000> !pe
    Exception object: 033610cc
    Exception type:   System.StackOverflowException
    Message:          <none>
    InnerException:   <none>
    StackTrace (generated):
    <none>
    StackTraceString: <none>
    HResult: 800703e9
    There are nested exceptions on this thread. Run with -nested for details
    ```

4. Check the last exception with nested:

    Now we can see that we have `System.IO.IOException` and that keeps coming.

    ```text
    0:000> !pe -nested
    Exception object: 033610cc
    Exception type:   System.StackOverflowException
    Message:          <none>
    InnerException:   <none>
    StackTrace (generated):
    <none>
    StackTraceString: <none>
    HResult: 800703e9

    Nested exception -------------------------------------------------------------
    Exception object: 033f8f5c
    Exception type:   System.IO.IOException
    Message:          Not Authorized
    InnerException:   <none>
    StackTrace (generated):
        SP       IP       Function
        01204A54 018F0972 CrashingApplication!CrashingApplication.Program.WriteLog(System.Exception)+0x62

    StackTraceString: <none>
    HResult: 80131620
    ...
    ...
    ...
    ```

5. We can see the same thing in the stack:

    ```text
    0:000> !clrstack
    OS Thread Id: 0x5098 (0)
    Child SP       IP Call Site
    012047a4 6ed34cea [HelperMethodFrame: 012047a4]
    01204854 018f0972 CrashingApplication.Program.WriteLog(System.Exception)
    0120488c 018f0985 CrashingApplication.Program.WriteLog(System.Exception)
    ...
    ...
    ...
    012fec8c 018f0985 CrashingApplication.Program.WriteLog(System.Exception)
    012fed8c 018f08f5 CrashingApplication.Program.Process()
    012fee84 018f0870 CrashingApplication.Program.Main(System.String[])
    012feff8 6e86f036 [GCFrame: 012feff8]
    ```
