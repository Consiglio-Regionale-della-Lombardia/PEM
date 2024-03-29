﻿/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using log4net;
using System;

namespace PortaleRegione.Logger
{
    public class Log
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Log));

        private Log()
        {
        }

        public static void Initialize()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        ///     Used to log Debug messages in an explicit Debug Logger
        /// </summary>
        /// <param name="message">The object message to log</param>
        public static void Debug(string message)
        {
            log.Debug(message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        public static void Error(string message)
        {
            log.Error(message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        /// <param name="exception">The exception to log, including its stack trace </param>
        public static void Error(string message, Exception exception)
        {
            log.Error(message, exception);
        }
    }
}