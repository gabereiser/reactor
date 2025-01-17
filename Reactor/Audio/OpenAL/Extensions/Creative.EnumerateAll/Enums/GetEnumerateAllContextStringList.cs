﻿//
// GetEnumerateAllContextStringList.cs
//
// Copyright (C) 2020 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

namespace Reactor.Audio.OpenAL
{
    /// <summary>
    ///     Defines available parameters for
    ///     <see cref="ALC.EnumerateAll.GetStringList(ALDevice, GetEnumerateAllContextStringList)" />.
    /// </summary>
    public enum GetEnumerateAllContextStringList
    {
        /// <summary>
        ///     Gets the specifier strings for all available devices.
        /// </summary>
        AllDevicesSpecifier = 0x1013
    }
}