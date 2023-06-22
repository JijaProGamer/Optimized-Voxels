using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

struct Triangle
{
    public Vector3 a,
        b,
        c;
    public int chunk;
}

public class Renderer
{
    public int threads;
    public ComputeShader shader;

    List<Chunk> chunksToTransform;
    Triangle[] result;

    CustomThreading threading = new CustomThreading();

    ComputeBuffer outputBuffer;
    ComputeBuffer countBuffer;
    AsyncGPUReadbackRequest request;

    bool started;
    public bool finished;
    public bool finished_requests;

    void transformMesh(int i)
    {
        Triangle tri = result[i];
        Chunk chunk = chunksToTransform[tri.chunk];

        chunk.mesh.vertices.Add(tri.a);
        chunk.mesh.vertices.Add(tri.b);
        chunk.mesh.vertices.Add(tri.c);

        chunk.mesh.triangles.Add(chunk.mesh.vertices.Count - 3);
        chunk.mesh.triangles.Add(chunk.mesh.vertices.Count - 2);
        chunk.mesh.triangles.Add(chunk.mesh.vertices.Count - 1);
    }

    public void Update()
    {
        if (started && request.done && !request.hasError && !finished_requests)
        {
            finished_requests = true;
            result = request.GetData<Triangle>().ToArray();

            int[] triCount = { 0 };
            ComputeBuffer.CopyCount(outputBuffer, countBuffer, 0);
            countBuffer.GetData(triCount);

            threading.finished = () =>
            {
                finished = true;
            };

            threading.func = transformMesh;
            threading.SetData(threads, 5000, triCount[0]);

            outputBuffer.Release();
            countBuffer.Release();
        }

        threading.Update();
    }

    public void generate(float[] densities, List<Chunk> chunks, List<Vector3Int> positions)
    {
        Debug.Log("OK"); 
        outputBuffer = new ComputeBuffer(
            positions.Count * 512 * 5,
            sizeof(float) * 9 + sizeof(int),
            ComputeBufferType.Append
        );

        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer positionBuffer = new ComputeBuffer(positions.Count, sizeof(int) * 3);
        ComputeBuffer densitiesBuffer = new ComputeBuffer(positions.Count * 512, sizeof(float));
        positionBuffer.SetData(positions);
        densitiesBuffer.SetData(densities);
        started = true;
        chunksToTransform = chunks;

        shader.SetBuffer(0, "positions", positionBuffer);
        shader.SetBuffer(0, "Densities", densitiesBuffer);
        shader.SetBuffer(0, "Result", outputBuffer);

        shader.Dispatch(0, positions.Count, 1, 1);
        request = AsyncGPUReadback.Request(outputBuffer);
        positionBuffer.Release();
        densitiesBuffer.Release();
    }
};
