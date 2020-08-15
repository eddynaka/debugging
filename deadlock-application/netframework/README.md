# How to find out why my application is hang

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

4. Check the stack to see what is happening:

    We can see in the stack below that it enter twice in two
    `System.Threading.Monitor.Wait`. Now, it seems that the thread is waiting
    something to happen.

    ```text
    0:000> !clrstack
    OS Thread Id: 0x20cc (0)
    Child SP       IP Call Site
    006ff1a4 77201a8c [GCFrame: 006ff1a4]
    006ff254 77201a8c [HelperMethodFrame_1OBJ: 006ff254] System.Threading.Monitor.ObjWait(Boolean, Int32, System.Object)
    006ff2e0 6c5442c8 System.Threading.Monitor.Wait(System.Object, Int32, Boolean) [f:\dd\ndp\clr\src\BCL\system\threading\monitor.cs @ 203]
    006ff2f0 6c5523fd System.Threading.Monitor.Wait(System.Object, Int32) [f:\dd\ndp\clr\src\BCL\system\threading\monitor.cs @ 213]
    006ff2f4 6c5cf5a9 System.Threading.ManualResetEventSlim.Wait(Int32, System.Threading.CancellationToken) [f:\dd\ndp\clr\src\BCL\system\threading\ManualResetEventSlim.cs @ 521]
    006ff348 6ce222f9 System.Threading.Tasks.Task.WaitAllBlockingCore(System.Collections.Generic.List`1<System.Threading.Tasks.Task>, Int32, System.Threading.CancellationToken) [f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 5193]
    006ff3c8 6ce21fa1 System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], Int32, System.Threading.CancellationToken) [f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 5109]
    006ff470 6ce21d11 System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[], Int32) [f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 4971]
    006ff484 6ce21c3c System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[]) [f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 4898]
    006ff490 00a10a67 DeadlockApplication.Program.Main(System.String[])
    006ff640 6d65f036 [GCFrame: 006ff640]
    ```

5. With that in mind, let's check syncblk:

    Here we can see that we have two threads waiting for an object.

    ```text
    0:000> !syncblk
    Index         SyncBlock MonitorHeld Recursion Owning Thread Info          SyncBlock Owner
        7 00857114            3         1 00869e60 2c68  10   0276242c System.Object
        8 00857148            3         1 00868d28 28c4   9   02762420 System.Object
    -----------------------------
    Total           8
    CCW             1
    RCW             2
    ComClassFactory 0
    Free            0
    ```

6. If you have mex, go to thread 9/10 and do `clkrstack2`:

    We can see that the thread 9 is waiting for an object and the owner is
    thread `2c68`

    ```text
    0:000> ~9s
    eax=00000000 ebx=00000001 ecx=00000000 edx=00000000 esi=00000001 edi=00000001
    eip=77201a8c esp=0516f280 ebp=0516f410 iopl=0         nv up ei pl nz ac po nc
    cs=0023  ss=002b  ds=002b  es=002b  fs=0053  gs=002b             efl=00000212
    ntdll!NtWaitForMultipleObjects+0xc:
    77201a8c c21400          ret     14h
    0:009> !clrstack2
    DbgId ThreadId Apartment Kind   CLR          GC Mode    GC Suspending?
        9     28c4 MTA       Worker v4.8.4200.00 Preemptive no

    Lock
    ----
    Method         System.Threading.Monitor.Enter(System.Object, Boolean ByRef)
    Object         0x000000000276242c
    Owner Thread 1 2c68

    SP       IP       Function                                                                                                                                   Source
    0516f5dc 00000000 GCFrame
    0516f6bc 00000000 GCFrame
    0516f6d8 00000000 HelperMethodFrame_1OBJ [System.Threading.Monitor.ReliableEnter(System.Object, Boolean ByRef)]
    0516f754 6c568468 System.Threading.Monitor.Enter(System.Object, Boolean ByRef)                                                                               f:\dd\ndp\clr\src\BCL\system\threading\monitor.cs @ 62
    0516f764 00a10b31 DeadlockApplication.Program.MethodA()
    0516f794 00a10aac DeadlockApplication.Program+<>c.<Main>b__2_0()
    0516f7a0 6c5cd4bb System.Threading.Tasks.Task.InnerInvoke()                                                                                                  f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2884
    0516f7ac 6c5cb731 System.Threading.Tasks.Task.Execute()                                                                                                      f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2498
    0516f7d0 6c5cb6fc System.Threading.Tasks.Task.ExecutionContextCallback(System.Object)                                                                        f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2861
    0516f7d4 6c568604 System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean) f:\dd\ndp\clr\src\BCL\system\threading\executioncontext.cs @ 980
    0516f840 6c568537 System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)         f:\dd\ndp\clr\src\BCL\system\threading\executioncontext.cs @ 928
    0516f854 6c5cb4b2 System.Threading.Tasks.Task.ExecuteWithThreadLocal(System.Threading.Tasks.Task ByRef)                                                      f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2827
    0516f8b8 6c5cb357 System.Threading.Tasks.Task.ExecuteEntry(Boolean)                                                                                          f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2767
    0516f8c8 6c5cb29d System.Threading.Tasks.Task.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()                                                         f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2704
    0516f8cc 6c53eb7d System.Threading.ThreadPoolWorkQueue.Dispatch()                                                                                            f:\dd\ndp\clr\src\BCL\system\threading\threadpool.cs @ 820
    0516f91c 6c53e9db System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()                                                                             f:\dd\ndp\clr\src\BCL\system\threading\threadpool.cs @ 1161
    0516fb3c 00000000 DebuggerU2MCatchHandlerFrame
    ```

    We can see that the thread 10 is waiting for an object and the owner is
    thread `28c4`

    ```text
    0:010> ~10s
    eax=00000000 ebx=00000001 ecx=00000000 edx=00000000 esi=00000001 edi=00000001
    eip=77201a8c esp=0532efb0 ebp=0532f140 iopl=0         nv up ei pl nz ac po nc
    cs=0023  ss=002b  ds=002b  es=002b  fs=0053  gs=002b             efl=00000212
    ntdll!NtWaitForMultipleObjects+0xc:
    77201a8c c21400          ret     14h
    0:010> !clrstack2
    DbgId ThreadId Apartment Kind   CLR          GC Mode    GC Suspending?
    10     2c68 MTA       Worker v4.8.4200.00 Preemptive no

    Lock
    ----
    Method         System.Threading.Monitor.Enter(System.Object, Boolean ByRef)
    Object         0x0000000002762420
    Owner Thread 1 28c4

    SP       IP       Function                                                                                                                                   Source
    0532f30c 00000000 GCFrame
    0532f3ec 00000000 GCFrame
    0532f408 00000000 HelperMethodFrame_1OBJ [System.Threading.Monitor.ReliableEnter(System.Object, Boolean ByRef)]
    0532f484 6c568468 System.Threading.Monitor.Enter(System.Object, Boolean ByRef)                                                                               f:\dd\ndp\clr\src\BCL\system\threading\monitor.cs @ 62
    0532f494 00a10c61 DeadlockApplication.Program.MethodB()
    0532f4c4 00a10bdc DeadlockApplication.Program+<>c.<Main>b__2_1()
    0532f4d0 6c5cd4bb System.Threading.Tasks.Task.InnerInvoke()                                                                                                  f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2884
    0532f4dc 6c5cb731 System.Threading.Tasks.Task.Execute()                                                                                                      f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2498
    0532f500 6c5cb6fc System.Threading.Tasks.Task.ExecutionContextCallback(System.Object)                                                                        f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2861
    0532f504 6c568604 System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean) f:\dd\ndp\clr\src\BCL\system\threading\executioncontext.cs @ 980
    0532f570 6c568537 System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)         f:\dd\ndp\clr\src\BCL\system\threading\executioncontext.cs @ 928
    0532f584 6c5cb4b2 System.Threading.Tasks.Task.ExecuteWithThreadLocal(System.Threading.Tasks.Task ByRef)                                                      f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2827
    0532f5e8 6c5cb357 System.Threading.Tasks.Task.ExecuteEntry(Boolean)                                                                                          f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2767
    0532f5f8 6c5cb29d System.Threading.Tasks.Task.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()                                                         f:\dd\ndp\clr\src\BCL\system\threading\Tasks\Task.cs @ 2704
    0532f5fc 6c53eb7d System.Threading.ThreadPoolWorkQueue.Dispatch()                                                                                            f:\dd\ndp\clr\src\BCL\system\threading\threadpool.cs @ 820
    0532f64c 6c53e9db System.Threading._ThreadPoolWaitCallback.PerformWaitCallback()                                                                             f:\dd\ndp\clr\src\BCL\system\threading\threadpool.cs @ 1161
    0532f86c 00000000 DebuggerU2MCatchHandlerFrame
    ```

7. With those stacks we can see that one thread is waiting the resource from the
   other thread, generating a deadlock in the application.
