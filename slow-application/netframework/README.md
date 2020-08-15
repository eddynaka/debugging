# How to find out why my application is slowing

## What tool should I use to understand what is happening

To understand what is happening, you need to generate a dump of your process. To
do that, you can use the following tools:

- [DebugDiag](https://debugdiag.com)
- [ProcDump](https://docs.microsoft.com/en-us/sysinternals/downloads/procdump)

In this case, since it's a scenario of slowness, you should generate more than
one dump.

## How to analyze the dump file

1. Open
   [WinDbgX](https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/windbg-command-line-preview)
2. Load the generated dump
3. Check if
   [SOS](https://github.com/dotnet/diagnostics/blob/master/documentation/sos-debugging-extension-windows.md)
   loaded correctly:

    ```text
    0:000> .chain
    Extension DLL chain:
        C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll: image 4.8.4200.0, API 1.0.0, built Tue Jun  9 22:07:50 2020
            [path: C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll]
    ```

4. Checking user mode time from each thread:

    Looking at the `.time` and `!runaway`, we can see that the execution is fast
    and isn't stuck anywhere.

    4.1. First dump:

    ```text
    0:000> !runaway
    User Mode Time
    Thread       Time
        5:3894     0 days 0:00:00.000
        4:244c     0 days 0:00:00.000
        3:4348     0 days 0:00:00.000
        2:3734     0 days 0:00:00.000
        1:4444     0 days 0:00:00.000
        0:36d4     0 days 0:00:00.000

    0:000> .time
    Debug session time: Sat Aug 15 13:47:47.000 2020 (UTC - 3:00)
    System Uptime: 0 days 23:53:32.544
    Process Uptime: 0 days 0:00:14.000
    Kernel time: 0 days 0:00:00.000
    User time: 0 days 0:00:00.000
    ```

    4.2. Second dump:

    ```text
    0:000> !runaway
    User Mode Time
    Thread       Time
        7:2588     0 days 0:00:00.000
        6:4618     0 days 0:00:00.000
        5:3894     0 days 0:00:00.000
        4:244c     0 days 0:00:00.000
        3:4348     0 days 0:00:00.000
        2:3734     0 days 0:00:00.000
        1:4444     0 days 0:00:00.000
        0:36d4     0 days 0:00:00.000

    0:000> .time
    Debug session time: Sat Aug 15 13:48:11.000 2020 (UTC - 3:00)
    System Uptime: 0 days 23:53:55.811
    Process Uptime: 0 days 0:00:38.000
    Kernel time: 0 days 0:00:00.000
    User time: 0 days 0:00:00.000
    ```

5. Check the stack to see what is happening:

    Looking at the `!clrstack`, we can see that both threads are in the same
    position in different dumps sleeping.

    5.1. First dump:

    ```text
    0:000> !clrstack
    OS Thread Id: 0x1818 (0)
    Child SP       IP Call Site
    010fed00 7720181c [HelperMethodFrame: 010fed00] System.Threading.Thread.SleepInternal(Int32)
    010fed84 6c53be7b System.Threading.Thread.Sleep(Int32) [f:\dd\ndp\clr\src\BCL\system\threading\thread.cs @ 725]
    010fed8c 02fe088a *** WARNING: Unable to verify checksum for SlowApplication.exe
    SlowApplication.Program.Main(System.String[])
    010fef0c 6d65f036 [GCFrame: 010fef0c]
    ```

    5.2. Second dump:

    ```text
    0:000> !clrstack
    OS Thread Id: 0x1818 (0)
    Child SP       IP Call Site
    010fed00 7720181c [HelperMethodFrame: 010fed00] System.Threading.Thread.SleepInternal(Int32)
    010fed84 6c53be7b System.Threading.Thread.Sleep(Int32) [f:\dd\ndp\clr\src\BCL\system\threading\thread.cs @ 725]
    010fed8c 02fe088a SlowApplication.Program.Main(System.String[])
    010fef0c 6d65f036 [GCFrame: 010fef0c]
    ```
