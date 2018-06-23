using System;

public class Job
{
    public Action Invoker { get; set; }
    public bool IsCompleted { get; set; }

    public Job(Action action)
    {
        Invoker = action;
    }
}