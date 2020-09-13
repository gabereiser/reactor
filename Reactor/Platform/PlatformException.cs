﻿/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing detailed licensing details.
 */

using System;

namespace Reactor.Platform
{
    /// <summary>
    /// Defines a plaftorm-specific exception.
    /// </summary>
    public class PlatformException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Reactor.PlatformException"/> class.
        /// </summary>
        public PlatformException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reactor.PlatformException"/> class.
        /// </summary>
        /// <param name="message">A message explaining the cause for this exception.</param>
        public PlatformException(string message) : base(message) { }
    }
}