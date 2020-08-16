# How to find out why my application is leaking

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

3. Check the dumpheap and see if you can check what is increasing between dumps

    With `!dumpheap -stat`, we can see the count of objects and the total size from
    each class name. So, we can observe the number of objects of
    `MemoryLeakApplication.Customer`

    | Object | First (count) | First (size) | Second (count) | Second (size) |
    | --- | --- | --- | --- | --- |
    | Customer | 136 | 3264 |496 | 11904 |
    | Customer[] | 8 | 2128 | 9 | 4188 |

    ```text
    0:000> !dumpheap -stat
    Statistics:
        MT    Count    TotalSize Class Name
    011b7998       10          116      Free
    6c185074        2          144 System.Globalization.CultureInfo
    6c1826f8        2          168 System.Threading.ThreadAbortException
    6c185700        2          264 System.Globalization.NumberFormatInfo
    6c185c40        1          269 System.Byte[]
    6c1844d4        3          468 System.Collections.Generic.Dictionary`2+Entry[[System.Type, mscorlib],[System.Security.Policy.EvidenceTypeDescriptor, mscorlib]][]
    00f75040       16          512 MemoryLeakApplication.Program+<GetCustomers>d__1
    6c18512c        2          616 System.Globalization.CultureData
    6c18426c       16          644 System.Int32[]
    6c182d74       20          712 System.String[]
    6c183698       26          728 System.RuntimeType
    6c182c60        6          750 System.Char[]
    00f754b4        8         2128 MemoryLeakApplication.Customer[]
    00f74e38      136         3264 MemoryLeakApplication.Customer
    6c1824e4      203         6374 System.String
    6c182788        6        17748 System.Object[]
    ```

    ```text
    0:000> !dumpheap -stat
    Statistics:
        MT    Count    TotalSize Class Name
    011b7998       12          136      Free
    6c185074        2          144 System.Globalization.CultureInfo
    6c1826f8        2          168 System.Threading.ThreadAbortException
    6c185700        2          264 System.Globalization.NumberFormatInfo
    6c185c40        1          269 System.Byte[]
    6c1844d4        3          468 System.Collections.Generic.Dictionary`2+Entry[[System.Type, mscorlib],[System.Security.Policy.EvidenceTypeDescriptor, mscorlib]][]
    6c18512c        2          616 System.Globalization.CultureData
    6c18426c       16          644 System.Int32[]
    6c182d74       20          712 System.String[]
    6c183698       26          728 System.RuntimeType
    6c182c60        6          750 System.Char[]
    00f75040       31          992 MemoryLeakApplication.Program+<GetCustomers>d__1
    00f754b4        9         4188 MemoryLeakApplication.Customer[]
    6c1824e4      218         6674 System.String
    00f74e38      496        11904 MemoryLeakApplication.Customer
    6c182788        6        17748 System.Object[]
    Total 917 objects
    ```
