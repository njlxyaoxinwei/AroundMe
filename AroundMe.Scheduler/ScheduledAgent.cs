﻿using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System;
using AroundMe.Core;

namespace AroundMe.Scheduler
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override async void OnInvoke(ScheduledTask task)
        {
            //TODO: Add code to perform your task in background
            //set random image from local storage
            await LockscreenHelpers.SetRandomImageFromLocalStorage();
           // ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(30)); //Only for debugging. Release version can only update every 20-40min
            NotifyComplete();
        }
    }
}