/*
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

using System;
using log4net;
using log4net.Config;

namespace PortaleRegione.Logger
{
    public class Log
    {
        private static readonly Log _instance = new Log();
        protected static ILog debugLogger;
        protected ILog monitoringLogger;

        private Log()
        {
            monitoringLogger = LogManager.GetLogger("MonitoringLogger");
            debugLogger = LogManager.GetLogger("DebugLogger");
        }

        public static void Initialize()
        {
            XmlConfigurator.Configure();
        }

        /// <summary>
        ///     Used to log Debug messages in an explicit Debug Logger
        /// </summary>
        /// <param name="message">The object message to log</param>
        public static void Debug(string message)
        {
            debugLogger.Debug(message);
        }


        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        /// <param name="exception">The exception to log, including its stack trace </param>
        public static void Debug(string message, Exception exception)
        {
            debugLogger.Debug(message, exception);
        }


        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        public static void Info(string message)
        {
            _instance.monitoringLogger.Info(message);
        }


        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        /// <param name="exception">The exception to log, including its stack trace </param>
        public static void Info(string message, Exception exception)
        {
            _instance.monitoringLogger.Info(message, exception);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        public static void Warn(string message)
        {
            _instance.monitoringLogger.Warn(message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        /// <param name="exception">The exception to log, including its stack trace </param>
        public static void Warn(string message, Exception exception)
        {
            _instance.monitoringLogger.Warn(message, exception);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        public static void Error(string message)
        {
            _instance.monitoringLogger.Error(message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        /// <param name="exception">The exception to log, including its stack trace </param>
        public static void Error(string message, Exception exception)
        {
            _instance.monitoringLogger.Error(message, exception);
        }


        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        public static void Fatal(string message)
        {
            _instance.monitoringLogger.Fatal(message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message">The object message to log</param>
        /// <param name="exception">The exception to log, including its stack trace </param>
        public static void Fatal(string message, Exception exception)
        {
            _instance.monitoringLogger.Fatal(message, exception);
        }
    }
}