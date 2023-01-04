using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RMEGo.ShibaLib.Win32Utils.Structs;
using Win32Utils;
using static RMEGo.ShibaLib.Win32Utils.Win32Constants;

namespace RMEGo.ShibaLib.Win32Utils.Managed
{
    public class MemoryScanner : IDisposable
    {
        public int PID { get; private set; }
        public IntPtr Handle { get; private set; }
        public MemoryScanner(int pid)
        {
            this.PID = pid;
            this.Handle = Win32ProcessUtils.OpenProcess(PROCESS.QUERY_INFORMATION | PROCESS.WM_READ, false, pid);
            if (this.Handle == IntPtr.Zero) {
                throw new EntryPointNotFoundException($"Could not found a running process with PID: {pid}");
            }
        }

        #region === Private Members ===
        enum DigitFlag
        {
            Byte,
            WildCard,
            Target,
            Not
        }

        struct Digit
        {
            public DigitFlag Flag { get; set; }
            public byte? Data { get; set; }
        }

        private static List<Digit> BuildPattern(string pattern)
        {
            var p = pattern.Replace(" ", "");
            var buffer = new List<Digit>(p.Length / 2);
            var i = 0;
            while (i < p.Length)
            {
                if (i + 1 >= p.Length) break;
                if (p[i] == '^')
                {
                    if (i + 2 >= p.Length) break;
                    buffer.Add(new Digit { Flag = DigitFlag.Not, Data = Convert.ToByte($"{p[1]}{p[2]}", 16) });
                    i += 3;
                    continue;
                }
                if (p[i] == '_' && p[i + 1] == '_')
                {
                    buffer.Add(new Digit { Flag = DigitFlag.WildCard });
                    i += 2;
                    continue;
                }
                if (p[i] == '*' && p[i + 1] == '*')
                {
                    buffer.Add(new Digit { Flag = DigitFlag.Target });
                    i += 2;
                    continue;
                }
                buffer.Add(new Digit { Flag = DigitFlag.Byte, Data = Convert.ToByte($"{p[i]}{p[i + 1]}", 16) });
                i += 2;
            }
            return buffer;
        }

        private static int Find(byte[] buf, List<Digit> pattern, int start, int length)
        {
            var pos = start;
            var index = -1;
            var noTarget = pattern.FindIndex(x=>x.Flag is DigitFlag.Target) == -1;
            while (pos <= length - pattern.Count)
            {
                var failed = true;
                for (var i = 0; i < pattern.Count; i++)
                {
                    var d = pattern[i];
                    if (d.Flag == DigitFlag.WildCard) continue;
                    if (d.Flag == DigitFlag.Target)
                    {
                        index = pos + i;
                        failed = false;
                        continue;
                    }
                    if (d.Flag == DigitFlag.Byte && buf[pos + i] != d.Data)
                    {
                        index = -1;
                        failed = true;
                        break;
                    }
                    if (d.Flag == DigitFlag.Not && buf[pos + i] == d.Data)
                    {
                        index = -1;
                        failed = true;
                        break;
                    }
                    if (noTarget && i == pattern.Count - 1 && buf[pos + i] == d.Data)
                    {
                        index = pos;
                        failed = false;
                        break;
                    }
                }

                if (!failed)
                {
                    return index;
                }

                pos += 4;
            }
            return index;
        }

        #endregion

        public record struct ScanResult(long RegionStart, long RegionEnd, long Position);

        public record struct ScanRegion(long RegionStart, long RegionEnd)
        {
            public static implicit operator ScanRegion(ScanResult x) => new(x.RegionStart, x.RegionEnd);
        }

        public ScanResult? Scan(string pattern)
        {
            Win32ProcessUtils.GetSystemInfo(out var sysInfo);
            var min = (long)sysInfo.minimumApplicationAddress;
            var max = (long)sysInfo.maximumApplicationAddress;
            return Scan(pattern, new ScanRegion(min, max));
        }

        public ScanResult? Scan(string pattern, ScanRegion region)
        {
            var min = region.RegionStart;
            var max = region.RegionEnd;
            var p = BuildPattern(pattern);
            var readed = 0;
            while (min < max)
            {
                Win32ProcessUtils.VirtualQueryEx(this.Handle, new IntPtr(min), out MemoryBasicInformation64 memInfo, (uint)Marshal.SizeOf(typeof(MemoryBasicInformation64)));
                if (memInfo.Protect == PAGE.READWRITE && memInfo.State == MEM.COMMIT)
                {
                    var end = (long)(memInfo.BaseAddress + memInfo.RegionSize);
                    //Console.WriteLine($"[VERBOSE] SEARCH [{memInfo.BaseAddress:X2}~{memInfo.RegionSize}]...");
                    var buf = new byte[memInfo.RegionSize];
                    var based = (long)memInfo.BaseAddress;
                    if (Win32ProcessUtils.ReadProcessMemory(this.Handle, based, buf, (int)memInfo.RegionSize, ref readed))
                    {
                        var index = Find(buf, p, 0, buf.Length);
                        if (index != -1)
                        {
                            return new ScanResult
                            {
                                RegionStart = based,
                                RegionEnd = end,
                                Position = based + index
                            };
                        }
                    }
                }
                min += (long)memInfo.RegionSize;
            }
            return null;
        }

        #region === Debug Interfaces
#if DEBUG
        public int DebugSearchAll(string pattern)
        {
            var count = 0;
            Win32ProcessUtils.GetSystemInfo(out var sysInfo);

            var min = (long)sysInfo.minimumApplicationAddress;
            var max = (long)sysInfo.maximumApplicationAddress;
            var p = BuildPattern(pattern);
            var readed = 0;
            while (min < max)
            {
                Win32ProcessUtils.VirtualQueryEx(this.Handle, new IntPtr(min), out MemoryBasicInformation64 memInfo, (uint)Marshal.SizeOf(typeof(MemoryBasicInformation64)));
                if (memInfo.Protect == PAGE.READWRITE && memInfo.State == MEM.COMMIT)
                {
                    var end = (long)(memInfo.BaseAddress + memInfo.RegionSize);
                    var buf = new byte[memInfo.RegionSize];
                    var based = (long)memInfo.BaseAddress;
                    if (Win32ProcessUtils.ReadProcessMemory(this.Handle, based, buf, (int)memInfo.RegionSize, ref readed))
                    {
                        var index = Find(buf, p, 0, buf.Length);
                        if (index != -1)
                        {
                            var indexOf = p.FindIndex(x=> x.Flag is DigitFlag.Target);
                            var targetOffset = index;
                            if (indexOf < 0)
                            {
                                targetOffset = 0;
                            }
                            var start = Math.Max(0, index - targetOffset);
                            var part = buf[start..(Math.Min(start+p.Count, buf.Length))];
                            Console.WriteLine($"[DEBUG] Found @ 0x{based + index:X2}.");
                            Console.WriteLine(string.Join(' ', part.Select(x => x.ToString("X2"))));
                            count += 1;
                        }
                    }
                }
                min += (long)memInfo.RegionSize;
            }
            return count;
        }
#endif
        #endregion

        public void Dispose()
        {
            Win32HandleUtils.CloseHandle(this.Handle);
            this.Handle = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
    }
}
