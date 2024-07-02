using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace cYo.Common.Win32
{
	public static class ShellFile
	{
		private static class UnsafeNativeMethods
		{
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SHFILEOPSTRUCT
			{
				public IntPtr hwnd;
				public int wFunc;
				public string pFrom;
				public string pTo;
				public short fFlags;
				public bool fAnyOperationsAborted;
				public IntPtr hNameMappings;
				public string lpszProgressTitle;
			}

			public const int FO_DELETE = 3;
			public const short FOF_SILENT = 4;
			public const short FOF_ALLOWUNDO = 0x40;
			public const short FOF_NOCONFIRMATION = 0x10;
			public const short FOF_NOERRORUI = 0x400;

			[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
			public static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);
		}

		public static void DeleteFile(IWin32Window window, ShellFileDeleteOptions options, params string[] files)
		{
			if (files == null || files.Length == 0)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string value in files)
			{
				if (!string.IsNullOrEmpty(value))
				{
					stringBuilder.Append(value);
					stringBuilder.Append('\0');
				}
			}
			if (stringBuilder.Length != 0)
            {
                stringBuilder.Append('\0');
                UnsafeNativeMethods.SHFILEOPSTRUCT lpFileOp = default(UnsafeNativeMethods.SHFILEOPSTRUCT);
                lpFileOp.hwnd = window?.Handle ?? IntPtr.Zero;
                lpFileOp.wFunc = UnsafeNativeMethods.FO_DELETE;
                lpFileOp.fFlags = UnsafeNativeMethods.FOF_SILENT | UnsafeNativeMethods.FOF_NOERRORUI;
                if (!options.HasFlag(ShellFileDeleteOptions.NoRecycleBin))
                {
                    lpFileOp.fFlags |= UnsafeNativeMethods.FOF_ALLOWUNDO;
                }
                if (!options.HasFlag(ShellFileDeleteOptions.Confirmation))
                {
                    lpFileOp.fFlags |= UnsafeNativeMethods.FOF_NOCONFIRMATION;
                }
                lpFileOp.pFrom = stringBuilder.ToString();
                lpFileOp.fAnyOperationsAborted = false;
                lpFileOp.hNameMappings = IntPtr.Zero;
                if (UnsafeNativeMethods.SHFileOperation(ref lpFileOp) != 0 || lpFileOp.fAnyOperationsAborted)
                {
					throw new Win32Exception();
                }
            }
        }

		public static void DeleteFile(ShellFileDeleteOptions options, params string[] files)
		{
			DeleteFile(null, options, files);
		}

		public static void DeleteFile(params string[] files)
		{
			DeleteFile(ShellFileDeleteOptions.None, files);
		}
	}
}
