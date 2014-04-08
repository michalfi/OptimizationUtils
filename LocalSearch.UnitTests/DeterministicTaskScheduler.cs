﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch.UnitTests
{
    /// <summary>
    /// TaskScheduler for executing tasks on the same thread that calls RunTasksUntilIdle() or RunPendingTasks() 
    /// taken from http://svengrand.blogspot.cz/2013/12/unit-testing-c-code-which-starts-new.html
    /// </summary>
    public class DeterministicTaskScheduler : TaskScheduler
    {
        private List<Task> scheduledTasks = new List<Task>();

        #region TaskScheduler methods

        protected override void QueueTask(Task task)
        {
            scheduledTasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            scheduledTasks.Add(task);
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return scheduledTasks;
        }

        public override int MaximumConcurrencyLevel { get { return 1; } }

        #endregion

        /// <summary>
        /// Executes the scheduled Tasks synchronously on the current thread. If those tasks schedule new tasks
        /// they will also be executed until no pending tasks are left.
        /// </summary>
        public void RunTasksUntilIdle()
        {
            while (scheduledTasks.Any())
            {
                this.RunPendingTasks();
            }
        }

        /// <summary>
        /// Executes the scheduled Tasks synchronously on the current thread. If those tasks schedule new tasks
        /// they will only be executed with the next call to RunTasksUntilIdle() or RunPendingTasks(). 
        /// </summary>
        public void RunPendingTasks()
        {
            foreach (var task in scheduledTasks.ToArray())
            {
                this.TryExecuteTask(task);
                scheduledTasks.Remove(task);
            }
        }
    }

}
