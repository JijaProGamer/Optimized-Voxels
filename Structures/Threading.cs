using UnityEngine;
using System.Threading;
using System;

public class CustomThreading
{
    public Action<int> func;
    public Action finished;

    public bool isWorking = false;
    public bool finishedWorking = false;

    int threads_used;
    int current_batch;
    int batches;
    int extra_work;

    int threads;
    int workPerThread;
    int workAmount;

    public void setData(int _threads, int _workPerThread, int _workAmount)
    {
        threads = _threads - 1;
        workPerThread = _workPerThread;
        workAmount = _workAmount;

        batches = (_workAmount / _workPerThread) - 1;
        extra_work = _workAmount % _workPerThread;

        threads_used = 0;
        current_batch = 0;

        isWorking = true;
        finishedWorking = false;
    }

    void run(int start, int end)
    {
        for (int j = start; j < end; j++)
        {
            func(j);

            if(j == end - 1 && j == workAmount - 1){
                isWorking = false;
                finishedWorking = true;
                finished();
            }
        }

        threads_used--;
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

                    Thread thread = new Thread(() => run(start, end));
                    thread.Name = "Threading";
                    thread.IsBackground = true;
                    thread.Start();

                    current_batch++;
                    threads_used++;
                }
            }
        }
    }
}
