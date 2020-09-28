using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace imageStacker.Core.Extensions
{
    public static class TaskListExtension
    {
        public static async Task WaitForFinishingTasks(this List<Task> queue, int queueCapacity)
        {
            while (queue.Count > queueCapacity)
            {
                var completedTask = await Task.WhenAny(queue);
                queue.Remove(completedTask);
                queue.FindAll(item => item.IsCompleted).ForEach(item => queue.Remove(item));
            }
        }

    }
}
