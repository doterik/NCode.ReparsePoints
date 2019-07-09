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


namespace NCode.ReparsePoints.Core3
{
  /// <summary>
  /// Factory methods to create the default implementation of <see cref="IReparsePointProvider"/>.
  /// </summary>
  public static class ReparsePointFactory
  {
    private static IReparsePointProvider provider;

    /// <summary>
    /// Instantiates the default implementation of <see cref="IReparsePointProvider"/>.
    /// </summary>
    public static IReparsePointProvider Create() => new ReparsePointProvider();

    /// <summary>
    /// Singleton instance for the default implementation of <see cref="IReparsePointProvider"/>.
    /// </summary>
    public static IReparsePointProvider Provider
    {
      get => provider ?? (provider = Create());
      set => provider = value;
    }
  }
}
