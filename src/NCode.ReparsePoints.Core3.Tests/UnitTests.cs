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
using System.IO;
using NUnit.Framework;

namespace NCode.ReparsePoints.Core3.Tests
{
  [TestFixture]
  public class UnitTests
  {
    [Test]
    public void HardLink()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);

        if (File.Exists(expectedSource)) File.Delete(expectedSource);

        var provider = ReparsePointFactory.Create();
        provider.CreateLink(expectedSource, expectedTarget, LinkType.HardLink);

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type);
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource));
        Assert.IsNull(link.Target);
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource); // If the file to be deleted does not exist, no exception is thrown.
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void HardLinkExistsDir()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.HardLink));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Unknown, link.Type); // Hmm..
        Assert.AreEqual(LinkType.Unknown, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.IsNull(link.Target);
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void HardLinkExistsDir_Overwrite()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.HardLink, overwrite: true));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Unknown, link.Type); // Hmm..
        Assert.AreEqual(LinkType.Unknown, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.IsNull(link.Target);
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void HardLinkExistsFile()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.HardLink));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type);
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource));
        Assert.IsNull(link.Target);
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void HardLinkExistsFile_Overwrite()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.HardLink, overwrite: true));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type);
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource));
        Assert.IsNull(link.Target);
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource); // If the file to be deleted does not exist, no exception is thrown.
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void Junction()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);

        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource);

        var provider = ReparsePointFactory.Create();
        provider.CreateLink(expectedSource, expectedTarget, LinkType.Junction);

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Junction, link.Type);
        Assert.AreEqual(LinkType.Junction, provider.GetLinkType(expectedSource));
        Assert.AreEqual(expectedTarget, link.Target);
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void JunctionExistsDir()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<InvalidOperationException>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Junction));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Unknown, link.Type); // Hmm..
        Assert.AreEqual(LinkType.Unknown, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }
    [Test]
    public void JunctionExistsDir_Overwrite_Ok()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        provider.CreateLink(expectedSource, expectedTarget, LinkType.Junction, overwrite: true);

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Junction, link.Type);
        Assert.AreEqual(LinkType.Junction, provider.GetLinkType(expectedSource));
        Assert.AreEqual(expectedTarget, link.Target);
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void JunctionExistsFile()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<InvalidOperationException>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Junction));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type); // Hmm..
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void JunctionExistsFile_Overwrite()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<InvalidOperationException>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Junction, overwrite: true));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type); // Hmm..
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void SymbolicDir()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);

        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource);

        var provider = ReparsePointFactory.Create();
        provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic);

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Symbolic, link.Type);
        Assert.AreEqual(LinkType.Symbolic, provider.GetLinkType(expectedSource));
        Assert.AreEqual(expectedTarget, link.Target);
      }
      finally
      {
        //if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        //if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void SymbolicDirExistsDir()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Unknown, link.Type); // Hmm..
        Assert.AreEqual(LinkType.Unknown, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void SymbolicDirExistsDir_Overwrite_Ok()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic, overwrite: true);

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Symbolic, link.Type);
        Assert.AreEqual(LinkType.Symbolic, provider.GetLinkType(expectedSource));
        Assert.AreEqual(expectedTarget, link.Target);
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void SymbolicDirExistsFile()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type); // Hmm..
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void SymbolicDirExistsFile_Overwrite()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        Directory.CreateDirectory(expectedTarget);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic, overwrite: true));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type); // Hmm..
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (Directory.Exists(expectedTarget)) Directory.Delete(expectedTarget, true);
      }
    }

    [Test]
    public void SymbolicFile()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);

        if (File.Exists(expectedSource)) File.Delete(expectedSource);

        var provider = ReparsePointFactory.Create();
        provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic);

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Symbolic, link.Type);
        Assert.AreEqual(LinkType.Symbolic, provider.GetLinkType(expectedSource));
        Assert.AreEqual(expectedTarget, link.Target);
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void SymbolicFileExistsDir()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Unknown, link.Type); // Hmm..
        Assert.AreEqual(LinkType.Unknown, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedSource)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void SymbolicFileExistsDir_Overwrite_Ok()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        Directory.CreateDirectory(expectedSource);

        var provider = ReparsePointFactory.Create();
        provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic, overwrite: true);

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.Symbolic, link.Type);
        Assert.AreEqual(LinkType.Symbolic, provider.GetLinkType(expectedSource));
        Assert.AreEqual(expectedTarget, link.Target);
      }
      finally
      {
        if (Directory.Exists(expectedSource)) Directory.Delete(expectedSource, true);
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedSource)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void SymbolicFileExistsFile()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type); // Hmm..
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }

    [Test]
    public void SymbolicFileExistsFile_Overwrite()
    {
      var context = TestContext.CurrentContext;
      var expectedSource = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      var expectedTarget = Path.Combine(context.WorkDirectory, Guid.NewGuid().ToString("N"));
      try
      {
        File.WriteAllText(expectedTarget, string.Empty);
        File.WriteAllText(expectedSource, string.Empty);

        var provider = ReparsePointFactory.Create();
        Assert.Throws<Win32Exception>(() => provider.CreateLink(expectedSource, expectedTarget, LinkType.Symbolic, overwrite: true));

        var link = provider.GetLink(expectedSource);
        Assert.IsNotNull(link);
        Assert.AreEqual(LinkType.HardLink, link.Type); // Hmm..
        Assert.AreEqual(LinkType.HardLink, provider.GetLinkType(expectedSource)); // Hmm..
        Assert.AreEqual(null, link.Target); // Hmm..
      }
      finally
      {
        if (File.Exists(expectedSource)) File.Delete(expectedSource);
        if (File.Exists(expectedTarget)) File.Delete(expectedTarget);
      }
    }
  }
}
