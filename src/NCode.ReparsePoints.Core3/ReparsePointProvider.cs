#region Copyright Preamble
//
//    Copyright © 2015 NCode Group
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using NCode.ReparsePoints.Win32;

namespace NCode.ReparsePoints.Core3
{
  /// <summary>
  /// Contains the methods to create and inspect win32 file and folder reparse
  /// points such as hard links, junctions (aka soft links), and symbolic links.
  /// </summary>
  /// <remarks>
  /// Technically speaking, hard links are not reparse points but this library
  /// supports them too.
  /// </remarks>
  public interface IReparsePointProvider
  {
    /// <summary>
    /// Given a path, determines the type of reparse point.
    /// </summary>
    /// <param name="path">The path to inspect.</param>
    /// <returns>A <see cref="LinkType"/> enumeration.</returns>
    LinkType GetLinkType(string path);

    /// <summary>
    /// Given a path, returns the information about a reparse point.
    /// </summary>
    /// <param name="path">The path to inspect.</param>
    /// <returns>A <see cref="ReparseLink"/> that contains the information
    /// about a reparse point.</returns>
    ReparseLink GetLink(string path);

    /// <summary>
    /// Creates a new reparse point such as a hard link, junction (aka soft
    /// link), or symoblic link.
    /// </summary>
    /// <remarks>
    /// In order to create symbolic links, the current user must either be an
    /// administrator running with elevated privileges or a non-admin user that
    /// has the SeCreateSymbolicLinkPrivilege right in local security policy.
    /// </remarks>
    /// <param name="path">The path of reparse point to create.</param>
    /// <param name="target">The target for the reparse point.</param>
    /// <param name="overwrite">Overwrite reparse point (Default=false).</param>
    /// <param name="type">A <see cref="LinkType"/> enumeration that specifies
    /// the type of reparse point to create.</param>
    void CreateLink(string path, string target, LinkType type, bool overwrite = false);
  }

  /// <summary>
  /// Contains the default implementation for <see cref="IReparsePointProvider"/>.
  /// </summary>
  public class ReparsePointProvider : IReparsePointProvider
  {
    #region IReparsePointProvider Members

    /// <summary>
    /// Given a path, determines the type of reparse point.
    /// </summary>
    /// <param name="path">The path to inspect.</param>
    /// <returns>A <see cref="LinkType"/> enumeration.</returns>
    public virtual LinkType GetLinkType(string path)
    {
      using (var handle = NativeMethods.FindFirstFile(path, out var data))
      {
        if (handle.IsInvalid) return LinkType.Unknown;

        if (!data.FileAttributes.HasFlag(FileAttributes.ReparsePoint))
          return data.FileAttributes.HasFlag(FileAttributes.Directory) ? LinkType.Unknown : LinkType.HardLink;

        switch (data.Reserved0)
        {
          case Win32Constants.IO_REPARSE_TAG_SYMLINK: return LinkType.Symbolic;
          case Win32Constants.IO_REPARSE_TAG_MOUNT_POINT: return LinkType.Junction;
        }
      }
      return LinkType.Unknown;
    }

    /// <summary>
    /// Given a path, returns the information about a reparse point.
    /// </summary>
    /// <param name="path">The path to inspect.</param>
    /// <returns>A <see cref="ReparseLink"/> that contains the information
    /// about a reparse point.</returns>
    public virtual ReparseLink GetLink(string path)
    {
      FileAttributes attributes;
      try
      {
        attributes = File.GetAttributes(path);
      }
      catch (DirectoryNotFoundException) { return new ReparseLink(); }
      catch (FileNotFoundException) { return new ReparseLink(); }

      var link = new ReparseLink
      {
        Attributes = attributes
      };

      if (!attributes.HasFlag(FileAttributes.ReparsePoint))
      {
        link.Type = attributes.HasFlag(FileAttributes.Directory)
          ? LinkType.Unknown
          : LinkType.HardLink;

        return link;
      }

      var encoding = Encoding.Unicode;
      var reparseHeaderSize = Marshal.SizeOf(typeof(ReparseHeader));
      var bufferLength = reparseHeaderSize + 2048;

      using (var hReparsePoint = OpenReparsePoint(path, AccessRights.GenericRead))
      {
        int error;
        do
          using (var buffer = SafeLocalAllocHandle.Allocate(bufferLength))
          {
            var b = NativeMethods.DeviceIoControl(
              hReparsePoint,
              Win32Constants.FSCTL_GET_REPARSE_POINT,
              SafeLocalAllocHandle.InvalidHandle,
              0,
              buffer,
              bufferLength,
              out int bytesReturned,
              IntPtr.Zero);

            error = Marshal.GetLastWin32Error();

            if (b)
            {
              var reparseHeader = buffer.Read<ReparseHeader>(0);

              IReparseData data;
              switch (reparseHeader.ReparseTag)
              {
                case Win32Constants.IO_REPARSE_TAG_MOUNT_POINT:
                  data = buffer.Read<JunctionData>(reparseHeaderSize);
                  link.Type = LinkType.Junction;
                  break;

                case Win32Constants.IO_REPARSE_TAG_SYMLINK:
                  data = buffer.Read<SymbolicData>(reparseHeaderSize);
                  link.Type = LinkType.Symbolic;
                  break;

                default:
                  throw new InvalidOperationException($"An unknown reparse tag {reparseHeader.ReparseTag:X} was encountered.");
              }

              var offset = Marshal.SizeOf(data) + reparseHeaderSize;
              var target = buffer.ReadString(offset + data.SubstituteNameOffset, data.SubstituteNameLength, encoding);

              link.Target = ParseDosDevicePath(target);
              return link;
            }

            if (error == Win32Constants.ERROR_INSUFFICIENT_BUFFER)
              bufferLength = buffer.Read<ReparseHeader>(0).ReparseDataLength;
            else
              throw new Win32Exception(error);
          }
        while (error == Win32Constants.ERROR_INSUFFICIENT_BUFFER);
      }
      return link;
    }

    /// <summary>
    /// Creates a new reparse point such as a hard link, junction (aka soft
    /// link), or symoblic link.
    /// </summary>
    /// <remarks>
    /// In order to create symbolic links, the current user must either be an
    /// administrator running with elevated privileges or a non-admin user that
    /// has the SeCreateSymbolicLinkPrivilege right in local security policy.
    /// </remarks>
    /// <param name="path">The path of reparse point to create.</param>
    /// <param name="target">The target for the reparse point.</param>
    /// <param name="overwrite">Overwrite reparse point (Default=false).</param>
    /// <param name="type">A <see cref="LinkType"/> enumeration that specifies
    /// the type of reparse point to create.</param>
    public virtual void CreateLink(string path, string target, LinkType type, bool overwrite = false)
    {
      switch (type)
      {
        case LinkType.HardLink:
          CreateHardLink(path, target);
          break;

        case LinkType.Junction:
          if (overwrite && Directory.Exists(path)) Directory.Delete(path);
          CreateJunction(path, target);
          break;

        case LinkType.Symbolic:
          if (overwrite && Directory.Exists(path)) Directory.Delete(path);
          CreateSymbolicLink(path, target);
          break;

        default:
          throw new ArgumentException($"Invalid Type '{type}' was specified.", nameof(type));
      }
    }

    /// <summary>
    /// Helper method to create a hard link.
    /// </summary>
    /// <param name="file">The path of the hard link to create.</param>
    /// <param name="target">The target for the link.</param>
    public virtual void CreateHardLink(string file, string target)
    {
      if (!NativeMethods.CreateHardLink(file, target, IntPtr.Zero))
        throw new Win32Exception();
    }

    /// <summary>
    /// Helper method to create a symbolic link.
    /// </summary>
    /// <param name="path">The path of the symbolic link to create.</param>
    /// <param name="target">The target for the link.</param>
    public virtual void CreateSymbolicLink(string path, string target)
    {
      var attributes = File.GetAttributes(target);
      var flags = attributes.HasFlag(FileAttributes.Directory)
        ? SymbolicLinkFlag.Directory
        : SymbolicLinkFlag.File;

      if (!NativeMethods.CreateSymbolicLink(path, target, flags))
        throw new Win32Exception();
    }

    /// <summary>
    /// Helper method to create a junction.
    /// </summary>
    /// <param name="path">The path of the junction to create.</param>
    /// <param name="target">The target for the junction.</param>
    public virtual void CreateJunction(string path, string target)
    {
      path = Path.GetFullPath(path);
      target = Path.GetFullPath(target);

      using (var handle = NativeMethods.FindFirstFile(path, out Win32FindData data))
        if (!handle.IsInvalid)
          throw new InvalidOperationException("A file or folder already exists with the same name as the junction.");

      Directory.CreateDirectory(path);

      var encoding = Encoding.Unicode;
      var nullChar = new byte[] { 0, 0 };

      var printName = ParseDosDevicePath(target);
      var printNameBytes = encoding.GetBytes(printName);
      var printNameLength = printNameBytes.Length;

      var substituteName = FormatDosDevicePath(printName, false);
      var substituteNameBytes = encoding.GetBytes(substituteName);
      var substituteNameLength = substituteNameBytes.Length;

      var junction = new JunctionData
      {
        SubstituteNameOffset = 0,
        SubstituteNameLength = checked((ushort)substituteNameLength),
        PrintNameOffset = checked((ushort)(substituteNameLength + nullChar.Length)),
        PrintNameLength = checked((ushort)printNameLength)
      };

      var junctionLength = Marshal.SizeOf(junction) + nullChar.Length * 2;
      var reparseLength = junctionLength + junction.SubstituteNameLength + junction.PrintNameLength;

      var reparse = new ReparseHeader
      {
        ReparseTag = Win32Constants.IO_REPARSE_TAG_MOUNT_POINT,
        ReparseDataLength = checked((ushort)reparseLength),
        Reserved = 0,
      };

      var bufferLength = Marshal.SizeOf(reparse) + reparse.ReparseDataLength;

      using var hReparsePoint = OpenReparsePoint(path, AccessRights.GenericWrite);
      using var buffer = SafeLocalAllocHandle.Allocate(bufferLength);
      var offset = buffer.Write(0, reparse);
      offset += buffer.Write(offset, junction);
      offset += buffer.Write(offset, substituteNameBytes, 0, substituteNameBytes.Length);
      offset += buffer.Write(offset, nullChar, 0, nullChar.Length);
      offset += buffer.Write(offset, printNameBytes, 0, printNameBytes.Length);
      offset += buffer.Write(offset, nullChar, 0, nullChar.Length);
      Debug.Assert(offset == bufferLength);

      var b = NativeMethods.DeviceIoControl(
        hReparsePoint,
        Win32Constants.FSCTL_SET_REPARSE_POINT,
        buffer,
        bufferLength,
        SafeLocalAllocHandle.InvalidHandle,
        0,
        out int bytesReturned,
        IntPtr.Zero);

      if (!b) throw new Win32Exception();
    }

    #endregion

    private static string FormatDosDevicePath(string path, bool sanitize)
    {
      if (sanitize)
        path = ParseDosDevicePath(path);

      return Win32Constants.NonInterpretedPathPrefix + path + "\\";
    }

    private static string ParseDosDevicePath(string path)
    {
      var result = Win32Constants
        .DosDevicePrefixes
        .Where(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        .Aggregate(path, (current, prefix) => current.Remove(0, prefix.Length));

      while (result.EndsWith("\\"))
        result = result.Remove(result.Length - 1);

      return result;
    }

    private static SafeFileHandle OpenReparsePoint(string reparsePoint, AccessRights accessRights)
    {
      var hFile = NativeMethods.CreateFile(
        reparsePoint,
        accessRights,
        FileShareMode.FileShareRead | FileShareMode.FileShareWrite,
        IntPtr.Zero,
        FileCreationDisposition.OpenExisting,
        FileAttributeFlags.FileFlagBackupSemantics | FileAttributeFlags.FileFlagOpenReparsePoint,
        IntPtr.Zero);

      if (hFile.IsInvalid)
        throw new Win32Exception();

      return hFile;
    }

  }
}
