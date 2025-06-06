﻿#region License

//   Copyright 2016 Kastellanos Nikolaos
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nez.ExtendedContent.GeonBit.Graphics;

public class DynamicIndexBufferContent : Collection<int>
{
    public bool IsWriteOnly = false;

    public DynamicIndexBufferContent(Collection<int> source)
    {
        Source = source;
    }

    protected internal Collection<int> Source { get; protected set; }

    public new int Count => Source.Count;

    public new IEnumerator<int> GetEnumerator()
    {
        return Source.GetEnumerator();
    }
}