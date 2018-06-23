using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThreadJobs
{
    private Queue<Job> tasks { get; set; }
    private Thread taskThread { get; set; }
    private bool isRunning = false;

    public void Enqueue(ref Job task)
    {
        tasks.Enqueue(task);
    }

    public Job Deqeue()
    {
        return tasks.Dequeue();
    }

    private void Update()
    {
        while (true)
        {
            if (tasks.Count <= 0 || isRunning) continue;
            isRunning = true;
            Job task = Deqeue();
            Task current = Task.Run(task.Invoker);
            while (!current.IsCompleted) continue;
            task.IsCompleted = true;
        }
    }

    public void Run()
    {
        Dispose();
        taskThread = new Thread(new ThreadStart(Update));
        taskThread.Start();
    }

    public void Dispose()
    {
        if (taskThread != null && taskThread.IsAlive) taskThread.Abort();
    }

    public ThreadJobs()
    {
        tasks = new Queue<Job>();
    }
}