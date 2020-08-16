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

    We can see that in the last exception we have a nested one. So, let's check.

    ```text
    0:000> !pe
    Exception object: 02653c80
    Exception type:   System.UnauthorizedAccessException
    Message:          Access to the path 'C:\Windows\private.txt' is denied.
    InnerException:   <none>
    StackTrace (generated):
        SP       IP       Function
        004FEDC4 6D1CCC60 mscorlib_ni!System.IO.__Error.WinIOError(Int32, System.String)+0xc04910
        004FEDE0 6C56AD83 mscorlib_ni!System.IO.FileStream.Init(System.String, System.IO.FileMode, System.IO.FileAccess, Int32, Boolean, System.IO.FileShare, Int32, System.IO.FileOptions, SECURITY_ATTRIBUTES, System.String, Boolean, Boolean, Boolean)+0x2e3
        004FEEA4 6C599C21 mscorlib_ni!System.IO.FileStream..ctor(System.String, System.IO.FileMode, System.IO.FileAccess, System.IO.FileShare, Int32, System.IO.FileOptions, System.String, Boolean, Boolean, Boolean)+0x45
        004FEEDC 6C6268A5 mscorlib_ni!System.IO.StreamWriter.CreateFile(System.String, Boolean, Boolean)+0x4d
        004FEEF4 6C626840 mscorlib_ni!System.IO.StreamWriter..ctor(System.String, Boolean, System.Text.Encoding, Int32, Boolean)+0x4c
        004FEF18 6C5AA4DF mscorlib_ni!System.IO.File.InternalWriteAllText(System.String, System.String, System.Text.Encoding, Boolean)+0x43
        004FEF4C 6CCFE1CA mscorlib_ni!System.IO.File.WriteAllText(System.String, System.String)+0x2e
        004FEF5C 00B808BC CrashingApplication!CrashingApplication.Program.Main(System.String[])+0x74

    StackTraceString: <none>
    HResult: 80070005
    There are nested exceptions on this thread. Run with -nested for details
    ```

4. Retrieve the last exception with nested option:

    ```text
    0:000> !pe -nested
    Exception object: 02653c80
    Exception type:   System.UnauthorizedAccessException
    Message:          Access to the path 'C:\Windows\private.txt' is denied.
    InnerException:   <none>
    StackTrace (generated):
        SP       IP       Function
        004FEDC4 6D1CCC60 mscorlib_ni!System.IO.__Error.WinIOError(Int32, System.String)+0xc04910
        004FEDE0 6C56AD83 mscorlib_ni!System.IO.FileStream.Init(System.String, System.IO.FileMode, System.IO.FileAccess, Int32, Boolean, System.IO.FileShare, Int32, System.IO.FileOptions, SECURITY_ATTRIBUTES, System.String, Boolean, Boolean, Boolean)+0x2e3
        004FEEA4 6C599C21 mscorlib_ni!System.IO.FileStream..ctor(System.String, System.IO.FileMode, System.IO.FileAccess, System.IO.FileShare, Int32, System.IO.FileOptions, System.String, Boolean, Boolean, Boolean)+0x45
        004FEEDC 6C6268A5 mscorlib_ni!System.IO.StreamWriter.CreateFile(System.String, Boolean, Boolean)+0x4d
        004FEEF4 6C626840 mscorlib_ni!System.IO.StreamWriter..ctor(System.String, Boolean, System.Text.Encoding, Int32, Boolean)+0x4c
        004FEF18 6C5AA4DF mscorlib_ni!System.IO.File.InternalWriteAllText(System.String, System.String, System.Text.Encoding, Boolean)+0x43
        004FEF4C 6CCFE1CA mscorlib_ni!System.IO.File.WriteAllText(System.String, System.String)+0x2e
        004FEF5C 00B808BC CrashingApplication!CrashingApplication.Program.Main(System.String[])+0x74

    StackTraceString: <none>
    HResult: 80070005

    Nested exception -------------------------------------------------------------
    Exception object: 02647740
    Exception type:   System.Net.Http.HttpRequestException
    Message:          An error occurred while sending the request.
    InnerException:   System.Net.WebException, Use !PrintException a902fae5 to see more.
    StackTrace (generated):
        SP       IP       Function
        004FEFCC 6C5D446B mscorlib_ni!System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(System.Threading.Tasks.Task)+0x67
        004FEFDC 6C5D0CD1 mscorlib_ni!System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(System.Threading.Tasks.Task)+0x41
        004FEFE8 6C5D2EB2 mscorlib_ni!System.Runtime.CompilerServices.TaskAwaiter`1[[System.__Canon, mscorlib]].GetResult()+0x1e
        004FEFF0 00B80965 CrashingApplication!CrashingApplication.Program.HttpRequest()+0x85
        004FF024 00B80889 CrashingApplication!CrashingApplication.Program.Main(System.String[])+0x41

    StackTraceString: <none>
    HResult: 80131500
    ```

5. Problems:

We can see two problems here:

- The application can't right in the path, throwing the exception `System.UnauthorizedAccessException`
- That was caused because the curret `HttpClient.Get` throwed an exception showing that the request wasn't completed with success
