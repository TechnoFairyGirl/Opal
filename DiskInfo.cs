using System;
using System.Runtime.InteropServices;

namespace Opal
{
	class DiskInfoInterop
	{
		public struct _statvfs
		{
			public ulong f_bsize;    // file system block size
			public ulong f_frsize;   // fragment size
			public ulong f_blocks;   // size of fs in f_frsize units
			public ulong f_bfree;    // # free blocks
			public ulong f_bavail;   // # free blocks for non-root
			public ulong f_files;    // # inodes
			public ulong f_ffree;    // # free inodes
			public ulong f_favail;   // # free inodes for non-root
			public ulong f_fsid;     // file system id
			public ulong f_flag;     // mount flags
			public ulong f_namemax;  // maximum filename length
		}

		[DllImport("MonoPosixHelper", SetLastError = true, EntryPoint = "Mono_Posix_Syscall_statvfs")]
		public static extern int statvfs(string path, out _statvfs buf);
	}

	class DiskInfo
	{
		public static void GetDiskDetails(string path, out string description, out double percentUsed)
		{
			DiskInfoInterop.statvfs(path, out var stat);

			var bytesTotal = stat.f_bsize * stat.f_blocks;
			var bytesFree = stat.f_bsize * stat.f_bfree;
			var bytesUsed = bytesTotal - bytesFree;

			var gbTotal = bytesTotal / (1024d * 1024d * 1024d);
			var tbTotal = gbTotal / 1024d;
			var gbUsed = bytesUsed / (1024d * 1024d * 1024d);
			var tbUsed = gbUsed / 1024d;

			description = tbTotal >= 1 ?
				$"{Math.Round(tbUsed, 2)} TB / {Math.Round(tbTotal, 2)} TB" :
				$"{Math.Round(gbUsed)} GB / {Math.Round(gbTotal)} GB";

			percentUsed = bytesTotal > 0 ? ((double)bytesUsed / bytesTotal) * 100 : 100;
		}
	}
}
