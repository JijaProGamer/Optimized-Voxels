using UnityEngine;
using System.Threading;
using System;

public class CustomThreading
{
    public Action<int> func;
    public Action finished;
    public bool useMainThread;

    public bool isWorking = false;
    public bool finishedWorking = false;

    int threads_used;
    int current_batch;
    int batches;
    int extra_work;

    int threads;
    int workPerThread;
    int workAmount;
    int workFinished;

    public void setData(int _threads, int _workPerThread, int _workAmount)
    {
        threads = _threads;
        workPerThread = _workPerThread;
        workAmount = _workAmount;

        batches = (_workAmount / _workPerThread) - 1;
        extra_work = _workAmount % _workPerThread;

        threads_used = 0;
        current_batch = 0;
        workFinished = 0;

        isWorking = true;
        finishedWorking = false;
    }

    void run(object state)
    {
        int[] range = (int[])state;
        int start = range[0];
        int end = range[1];

        for (int j = start; j < end; j++)
        {
            func(j);

            Interlocked.Increment(ref workFinished);
        }

        if (Interlocked.CompareExchange(ref workFinished, 0, 0) == workAmount)
        {
            isWorking = false;
            finishedWorking = true;
            finished?.Invoke();
        }

        Interlocked.Decrement(ref threads_used);
    }

    public void Update()
    {
        if (isWorking && !finishedWorking && threads_used < threads)
        {
            int availableThreads = threads - threads_used;

            for (int i = 0; i < availableThreads; i++)
            {
                if (current_batch <= batches || ((batches + 1) == current_batch && extra_work > 0))
                {
                    int start = current_batch * workPerThread;
                    int end = (current_batch + 1) * workPerThread;

                    if (current_batch == (batches + 1) && extra_work > 0)
                    {
                        end = start + extra_work;
                    }

                    if (!useMainThread)
                    {
                        ThreadPool.QueueUserWorkItem(run, new int[] { start, end });
                    }
                    else
                    {
                        run(new int[] { start, end });
                    }

                    current_batch++;
                    Interlocked.Increment(ref threads_used);
                }
            }
        }
    }
}
