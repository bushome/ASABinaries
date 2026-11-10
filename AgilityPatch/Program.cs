// AgilityPatch: sets an exe's exported D3D12SDKVersion to match the
// DirectX 12 Agility SDK (D3D12Core.dll) in the folder named by its own
// exported D3D12SDKPath. Works on the ASA game client, dedicated server and
// DevKit editor exes (any 64-bit exe that exports both symbols).
using System.Diagnostics;
using System.Text;

namespace AgilityPatch;

internal static class Program
{
    private const int RetailMin = 600;  // 1.6xx = retail Agility SDK
    private const int PreviewMin = 700; // 1.7xx = preview (needs Developer Mode)

    private static int Main(string[] argv)
    {
        var targets = new List<string>();
        int? forcedVersion = null;
        bool check = false, noBackup = false, allowPreview = false;

        for (int i = 0; i < argv.Length; i++)
        {
            string a = argv[i];
            switch (a.ToLowerInvariant())
            {
                case "-h": case "--help": case "/?":
                    PrintHelp(); return 0;
                case "--check":
                    check = true; break;
                case "--no-backup":
                    noBackup = true; break;
                case "--allow-preview":
                    allowPreview = true; break;
                case "--version":
                    if (i + 1 >= argv.Length || !int.TryParse(argv[++i], out int v))
                    { Console.Error.WriteLine("--version needs a number, e.g. --version 620"); return 2; }
                    forcedVersion = v; break;
                default:
                    if (a.StartsWith('-')) { Console.Error.WriteLine($"Unknown option: {a}"); return 2; }
                    targets.Add(a); break;
            }
        }

        if (targets.Count == 0) { PrintHelp(); return 2; }

        if (forcedVersion is int fv && !VersionAllowed(fv, allowPreview, out string why))
        { Console.Error.WriteLine(why); return 2; }

        // Expand folders into the exes inside them (top level only).
        var exes = new List<(string Path, bool Explicit)>();
        foreach (string t in targets)
        {
            string full = Path.GetFullPath(t.Trim('"'));
            if (Directory.Exists(full))
            {
                foreach (string f in Directory.GetFiles(full, "*.exe").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    exes.Add((f, false));
            }
            else if (File.Exists(full)) exes.Add((full, true));
            else { Console.Error.WriteLine($"Not found: {full}"); return 2; }
        }

        int errors = 0, patched = 0, ok = 0;
        foreach (var (exe, isExplicit) in exes)
        {
            try
            {
                var r = PatchOne(exe, isExplicit, forcedVersion, check, noBackup, allowPreview);
                if (r == Result.Patched) patched++;
                else if (r == Result.AlreadyCurrent || r == Result.WouldPatch) ok++;
            }
            catch (Exception ex)
            {
                errors++;
                Console.Error.WriteLine($"ERROR  {exe}");
                Console.Error.WriteLine($"       {ex.Message}");
            }
        }

        if (exes.Count > 0 && patched + ok + errors == 0)
            Console.WriteLine("No exes in the given folder(s) export D3D12SDKVersion.");

        Console.WriteLine();
        Console.WriteLine(check
            ? $"Check finished: {errors} error(s)."
            : $"Done: {patched} patched, {ok} already current, {errors} error(s).");
        return errors > 0 ? 1 : 0;
    }

    private enum Result { Skipped, AlreadyCurrent, WouldPatch, Patched }

    private static Result PatchOne(string exe, bool isExplicit, int? forcedVersion,
                                   bool check, bool noBackup, bool allowPreview)
    {
        bool battlEye = Path.GetFileNameWithoutExtension(exe).EndsWith("_BE", StringComparison.OrdinalIgnoreCase);
        if (battlEye)
        {
            // BattlEye launcher: a separate 32-bit exe that refuses modified game files anyway.
            if (isExplicit) throw new InvalidDataException("This is the BattlEye launcher. It doesn't use the Agility SDK, and BattlEye won't run a patched game exe. Patch the non-BE exe and launch without BattlEye.");
            return Result.Skipped;
        }

        byte[] data = File.ReadAllBytes(exe);
        PeFile pe;
        try { pe = PeFile.Parse(data); }
        catch (InvalidDataException) when (!isExplicit)
        {
            return Result.Skipped; // folder scan: not a 64-bit PE we can use
        }

        if (!pe.Exports.TryGetValue("D3D12SDKVersion", out uint verRva))
        {
            if (isExplicit) throw new InvalidDataException("This exe does not export D3D12SDKVersion, so it can't use a redistributable Agility SDK.");
            return Result.Skipped; // folder scan: quietly skip other exes
        }
        if (!pe.Exports.TryGetValue("D3D12SDKPath", out uint pathRva))
            throw new InvalidDataException("Exports D3D12SDKVersion but not D3D12SDKPath.");

        int verOff = pe.RvaToOffset(verRva);
        int current = BitConverter.ToInt32(data, verOff);
        string sdkPath = pe.ReadSdkPath(pathRva);

        string exeDir = Path.GetDirectoryName(exe)!;
        string coreDll = Path.GetFullPath(Path.Combine(exeDir, sdkPath, "D3D12Core.dll"));

        int target;
        if (forcedVersion is int fv) target = fv;
        else
        {
            if (!File.Exists(coreDll))
                throw new FileNotFoundException($"D3D12Core.dll not found where the exe looks for it: {coreDll}  (copy the Agility SDK DLLs there, or pass --version N)");
            target = FileVersionInfo.GetVersionInfo(coreDll).FileMinorPart; // 1.620.1.0 -> 620
            if (!VersionAllowed(target, allowPreview, out string why))
                throw new InvalidDataException($"{coreDll} is version 1.{target}: {why}");
        }

        string label = $"{Path.GetFileName(exe)}  (SDK path {sdkPath})";
        if (current == target)
        {
            Console.WriteLine($"OK     {label}: already {current}");
            return Result.AlreadyCurrent;
        }
        if (check)
        {
            Console.WriteLine($"NEEDS  {label}: {current} -> {target}");
            return Result.WouldPatch;
        }

        if (!noBackup)
            File.Copy(exe, exe + ".bak", overwrite: true); // state right before this patch

        BitConverter.TryWriteBytes(data.AsSpan(verOff, 4), target);
        bool checksumUpdated = pe.UpdateChecksumIfSet(data);

        // FileShare.None: fails cleanly if the game/editor is still running.
        using (var fs = new FileStream(exe, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            fs.Position = verOff;
            fs.Write(data, verOff, 4);
            if (checksumUpdated)
            {
                fs.Position = pe.ChecksumOffset;
                fs.Write(data, pe.ChecksumOffset, 4);
            }
        }

        // Verify by re-reading from disk.
        byte[] after = File.ReadAllBytes(exe);
        int written = BitConverter.ToInt32(after, verOff);
        if (written != target) throw new IOException($"Verification failed: read back {written}, expected {target}.");

        Console.WriteLine($"PATCH  {label}: {current} -> {target}" + (noBackup ? "" : "  (backup: .bak)"));
        return Result.Patched;
    }

    private static bool VersionAllowed(int v, bool allowPreview, out string why)
    {
        why = "";
        if (v < RetailMin) { why = $"SDK version {v} is older than the Agility SDK range (600+)."; return false; }
        if (v >= PreviewMin && !allowPreview)
        { why = $"SDK version {v} is a preview release (1.7xx). It needs Windows Developer Mode; pass --allow-preview to use it anyway."; return false; }
        if (v >= 1000) { why = $"SDK version {v} doesn't look valid."; return false; }
        return true;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
AgilityPatch 1.0 - match an exe's D3D12SDKVersion to its Agility SDK DLLs

Usage:
  AgilityPatch <exe or folder> [more...] [options]

  A folder patches every exe in it (top level only) that exports
  D3D12SDKVersion; other exes are skipped.

  The target version is read from D3D12Core.dll in the folder the exe
  itself names (its D3D12SDKPath, e.g. .\D3D12\ or .\D3D12\x64\), so
  copy the new Agility SDK DLLs in first, then run this.

Options:
  --check          Only report; change nothing.
  --version N      Use N instead of reading D3D12Core.dll (e.g. 620).
  --no-backup      Don't write <exe>.bak before patching.
  --allow-preview  Allow 1.7xx preview SDKs (need Windows Developer Mode).

Examples:
  AgilityPatch "D:\SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64"
  AgilityPatch "E:\ARKDevkit\Engine\Binaries\Win64" --check

Exit code: 0 = success, 1 = an exe failed, 2 = bad arguments.
Close the game/editor first. Game updates replace the exe; run this again after each one.
""");
    }
}

/// Minimal PE32+ reader: sections, export table, checksum.
internal sealed class PeFile
{
    private readonly byte[] _d;
    private readonly ulong _imageBase;
    private readonly (uint Va, uint VSize, uint RawPtr, uint RawSize)[] _sections;
    public Dictionary<string, uint> Exports { get; } = new(StringComparer.Ordinal);
    public int ChecksumOffset { get; }
    private readonly uint _origChecksum;

    private PeFile(byte[] d)
    {
        _d = d;
        if (d.Length < 0x40 || d[0] != 'M' || d[1] != 'Z') throw new InvalidDataException("Not an exe (no MZ header).");
        int pe = BitConverter.ToInt32(d, 0x3C);
        if (pe <= 0 || pe + 24 > d.Length || BitConverter.ToUInt32(d, pe) != 0x00004550)
            throw new InvalidDataException("Not a PE file.");
        int nsec = BitConverter.ToUInt16(d, pe + 6);
        int optSize = BitConverter.ToUInt16(d, pe + 20);
        int opt = pe + 24;
        if (BitConverter.ToUInt16(d, opt) != 0x20B) throw new InvalidDataException("Not a 64-bit (PE32+) exe.");

        _imageBase = BitConverter.ToUInt64(d, opt + 24);
        ChecksumOffset = opt + 64;
        _origChecksum = BitConverter.ToUInt32(d, ChecksumOffset);

        _sections = new (uint, uint, uint, uint)[nsec];
        int sh = opt + optSize;
        for (int i = 0; i < nsec; i++)
        {
            int s = sh + i * 40;
            _sections[i] = (BitConverter.ToUInt32(d, s + 12), BitConverter.ToUInt32(d, s + 8),
                            BitConverter.ToUInt32(d, s + 20), BitConverter.ToUInt32(d, s + 16));
        }

        uint numDirs = BitConverter.ToUInt32(d, opt + 108);
        if (numDirs == 0) return;
        uint expRva = BitConverter.ToUInt32(d, opt + 112);
        if (expRva == 0) return;

        int e = RvaToOffset(expRva);
        uint numFuncs = BitConverter.ToUInt32(d, e + 20);
        uint numNames = BitConverter.ToUInt32(d, e + 24);
        int funcs = RvaToOffset(BitConverter.ToUInt32(d, e + 28));
        int names = RvaToOffset(BitConverter.ToUInt32(d, e + 32));
        int ords = RvaToOffset(BitConverter.ToUInt32(d, e + 36));
        for (int i = 0; i < numNames; i++)
        {
            string name = ReadCString(RvaToOffset(BitConverter.ToUInt32(d, names + 4 * i)));
            int ord = BitConverter.ToUInt16(d, ords + 2 * i);
            if (ord < numFuncs) Exports[name] = BitConverter.ToUInt32(d, funcs + 4 * ord);
        }
    }

    public static PeFile Parse(byte[] d) => new(d);

    public int RvaToOffset(uint rva)
    {
        foreach (var s in _sections)
        {
            uint size = Math.Max(s.VSize, s.RawSize);
            if (rva >= s.Va && rva < s.Va + size)
            {
                uint off = rva - s.Va;
                if (off >= s.RawSize) throw new InvalidDataException($"RVA 0x{rva:X} has no file data (uninitialized section).");
                return checked((int)(s.RawPtr + off));
            }
        }
        throw new InvalidDataException($"RVA 0x{rva:X} is outside every section.");
    }

    /// D3D12SDKPath is normally `const char*` (8-byte pointer to the string);
    /// also accept a char array exported directly.
    public string ReadSdkPath(uint exportRva)
    {
        int off = RvaToOffset(exportRva);
        ulong ptr = BitConverter.ToUInt64(_d, off);
        if (ptr > _imageBase && ptr - _imageBase < uint.MaxValue)
        {
            try { return ReadCString(RvaToOffset((uint)(ptr - _imageBase))); }
            catch (InvalidDataException) { /* fall through to char array */ }
        }
        return ReadCString(off);
    }

    private string ReadCString(int off)
    {
        int end = Array.IndexOf(_d, (byte)0, off);
        if (end < 0 || end - off > 1024) throw new InvalidDataException("Bad string in export data.");
        return Encoding.ASCII.GetString(_d, off, end - off);
    }

    /// Windows only checks the PE checksum for drivers/boot files, but if the
    /// exe has one set, keep it correct. Returns true if it was changed.
    public bool UpdateChecksumIfSet(byte[] d)
    {
        if (_origChecksum == 0) return false;
        ulong sum = 0;
        int len = d.Length;
        for (int i = 0; i < len; i += 2)
        {
            if (i == ChecksumOffset || i == ChecksumOffset + 2) continue;
            uint word = i + 1 < len ? BitConverter.ToUInt16(d, i) : d[i];
            sum += word;
            sum = (sum & 0xFFFF) + (sum >> 16);
        }
        sum = (sum & 0xFFFF) + (sum >> 16);
        uint checksum = (uint)sum + (uint)len;
        BitConverter.TryWriteBytes(d.AsSpan(ChecksumOffset, 4), checksum);
        return checksum != _origChecksum;
    }
}
