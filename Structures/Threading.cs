using UnityEngine;
using System.Threading;
using System;

public class CustomThreading
{
    public Action<int> func;
    public Action finished;
    public bool useMainThread;

    private int session = 0;
    private int threads;
    private int workPerThread;
    private int workAmount;
    private int workFinished;

    private bool isWorking = false;
    private bool finishedWorking = false;

    private int threadsUsed;
    private int currentBatch;
    private int batches;
    private int extraWork;

    bool newError;
    string lastError;

    public void SetData(int threads, int workPerThread, int workAmount)
    {
        session++;
        this.threads = threads;
        this.workPerThread = workPerThread;
        this.workAmount = workAmount;

        batches = (workAmount / workPerThread) - 1;
        extraWork = workAmount % workPerThread;

        threadsUsed = 0;
        currentBatch = 0;
        workFinished = 0;

        isWorking = true;
        finishedWorking = false;
    }

    private void Run(object state)
    {
        int[] range = (int[])state;
        int start = range[0];
        int end = range[1];

        for (int j = start; j < end; j++)
        {
            try {
                func(j);
                int currentWorkFinished = Interlocked.Increment(ref workFinished);

                if (currentWorkFinished >= workAmount)
                {
                    isWorking = false;
                    finishedWorking = true;
                    finished?.Invoke();
                }
            } catch (Exception err){
                lastError = err.ToString();
                newError = true;
            }
        }

        Interlocked.Decrement(ref threadsUsed);
    }

    public void Update()
    {
        if(newError){
            Debug.Log(lastError);
            newError = false;
        }

        if (isWorking && !finishedWorking && threadsUsed < threads)
        {
            if (workAmount == 0)
            {
                isWorking = false;
                finishedWorking = true;
                finished?.Invoke();
            }

            int availableThreads = threads - threadsUsed;

            for (int i = 0; i < availableThreads; i++)
            {
                if (currentBatch <= batches || ((batches + 1) == currentBatch && extraWork > 0))
                {
                    int start = currentBatch * workPerThread;
                    int end = (currentBatch + 1) * workPerThread;

                    if (currentBatch == (batches + 1) && extraWork > 0)
                    {
                        end = start + extraWork;
                    }

                    if (!useMainThread)
                    {
                        ThreadPool.QueueUserWorkItem(Run, new int[] { start, end });
                    }
                    else
                    {
                        Run(new int[] { start, end });
                    }

                    currentBatch++;
                    Interlocked.Increment(ref threadsUsed);
                }
            }
        }
    }
}
