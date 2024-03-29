using System.Collections.Generic;
using System;
using System.Linq;

namespace VUtils {
    namespace UnityUtils {
        using UnityEngine;
        using Unity.Mathematics;
        using Unity.Transforms;
        namespace ECS {
            using Unity.Collections;
            using Unity.Entities;
            using Unity.Physics;
            public static class Extensions {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="state"></param>
                /// <param name="singleton"></param>
                /// <returns></returns>
                public static EntityCommandBuffer.ParallelWriter GetECBSingleton(ref SystemState state, BeginSimulationEntityCommandBufferSystem.Singleton singleton)
                {
                    var ecbSingleton = singleton;
                    ecbSingleton.SetAllocator(Allocator.TempJob);
                    var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                    return ecb.AsParallelWriter();
                }
            }
            public static class Transforms {
                public static float3 ClampMagnitude(float3 vector, float maxMagnitude) {
                    float sqrMagnitude = math.lengthsq(vector);
                    if (sqrMagnitude > maxMagnitude)
                    {
                        float scale = math.sqrt(sqrMagnitude) / maxMagnitude;
                        vector /= scale;
                    }
                    return vector;
                }
                public static quaternion RotateTowards(quaternion from, float3 to, float delta) {
                    var _look = quaternion.LookRotationSafe(math.forward(), to);
                    return math.slerp(from, _look, math.radians(delta));
                }
            }
            public static class Singularity {
                /// <summary>
                /// Adds and sets the following components: PhysicsVelocity(add), PhysicsCollier(add), LocalTransform(set), ShipMovementComponent(add), ThrustComponent(add), ReplicantMatrix(add).
                /// </summary>
                /// <param name="writer"></param>
                /// <param name="chunkIdx"></param>
                /// <param name="entity"></param>
                /// <param name="spawnPosition"></param>
                /// <param name="initialVelocity"></param>
                /// <param name="range"></param>
                /// <param name="sense"></param>
                public static void CreateShip(this ref EntityCommandBuffer.ParallelWriter writer, int chunkIdx, Entity entity, float3 spawnPosition, float3 initialVelocity, ShipClass type, float range = 0, float sense = 0) {
                    // Tag Components
                    writer.AddComponent<ShipTag>(chunkIdx, entity);
                    writer.AddComponent(chunkIdx, entity, new ShipType() { Value = type });

                    // Physics Components
                    writer.SetComponent(chunkIdx, entity, LocalTransform.FromPositionRotationScale(spawnPosition, quaternion.identity, 50f));
                    writer.AddComponent(chunkIdx, entity, new Thrust() { Value = float3.zero });
                    writer.SetComponent(chunkIdx, entity, new PhysicsVelocity() { Linear = initialVelocity });
                    // Ship Attribute Components
                    writer.AddComponent(chunkIdx, entity, new Sensor() { Range = range, Sensitivity = sense });
                }
                /// <summary>
                /// Adds necessary Dynamic buffers for the replicant matrix
                /// </summary>
                /// <param name="writer"></param>
                /// <param name="chunkIdx"></param>
                /// <param name="entity"></param>
                /// <param name="inputs"></param>
                /// <param name="outputs"></param>
                /// <param name="interactions"></param>
                public static void AddMatrixBuffers(this ref EntityCommandBuffer.ParallelWriter writer, int chunkIdx, Entity entity, NativeArray<InputNode> inputs, NativeArray<OutputNode> outputs, NativeArray<Interaction> interactions) {
                    var _inputs = writer.AddBuffer<InputNode>(chunkIdx, entity);
                    var _outputs = writer.AddBuffer<OutputNode>(chunkIdx, entity);
                    writer.AddBuffer<HiddenNode>(chunkIdx, entity);

                    var _interactions = writer.AddBuffer<Interaction>(chunkIdx, entity);

                    _inputs.CopyFrom(inputs);
                    _outputs.CopyFrom(outputs);
                    _interactions.CopyFrom(interactions);
                }
            }
            public static class Constants {
                /// <summary>
                /// Float3 that represents world forward (0,0,1)
                /// </summary>
                public static readonly float3 Forward = new(0, 0, 1);
                /// <summary>
                /// Float3 that represents world up (0,1,0)
                /// </summary>
                public static readonly float3 Up = new(0, 1, 0);
            }
        }
        namespace GO {
            public static class Utilities { }
        }
        namespace UIToolkit {
            using UnityEngine.UIElements;
            public class UI {
                public static VisualElement Create(params string[] classes) { return Create<VisualElement>(classes); }
                public static T Create<T>(params string[] classes) where T : VisualElement, new()
                {
                    var element = new T();
                    foreach (var className in classes)
                    {
                        element.AddToClassList(className);
                    }
                    return element;
                }
                public class LineChart : VisualElement {
                    public List<float> Values;
                    private float width;
                    private float height;
                    public int NumVisible = -1;
                    [SerializeField]
                    public float XSize;
                    public float YBotMargin;

                    public LineChart() { generateVisualContent += DrawLineGraph; }
                    private void DrawLineGraph(MeshGenerationContext ctx) {
                        width = layout.width;
                        height = layout.height;
                        var painter = ctx.painter2D; // set painter style
                        painter.lineWidth = 2.0f;
                        painter.strokeColor = Color.green;
                        painter.lineJoin = LineJoin.Round;
                        painter.lineCap = LineCap.Round;
                        painter.BeginPath();

                        var yUpper = Values[0]; //arbitrary starting values
                        var yLower = Values[0];

                        if (NumVisible <= 0) NumVisible = Values.Count; // display the whole list if the count is less 0..duh. just catching errors
                        XSize = width / NumVisible;
                        for (int i = Mathf.Max(Values.Count - NumVisible, 0); i < Values.Count; i++) // adjust yMin and yMax as the list is cycled through
                        {
                            float testValue = Values[i];
                            if (testValue > yLower) yLower = testValue;
                            if (testValue < yUpper) yUpper = testValue;
                        }
                        float yDiff = yLower - yUpper;
                        if (yDiff <= 0) yDiff = 5f;
                        YBotMargin = yLower;
                        int xIdx = 0;

                        yUpper = 0f;
                        painter.MoveTo(new Vector2(XSize / 2, (yLower - (yUpper + Values[Mathf.Max(Values.Count - NumVisible, 0)])) / yDiff * height));
                        for (int i = Mathf.Max(Values.Count - NumVisible, 0); i < Values.Count; i++)
                        {
                            float xPos = (XSize / 2) + xIdx * XSize;
                            float yPos = (yLower - (yUpper + Values[i])) / yDiff * height;

                            painter.LineTo(new Vector2(xPos, yPos));
                            xIdx++;
                        }
                        painter.Stroke();
                    }
                }
            }
        }
        namespace MathHelpers {
            public class Maths {
                public static float TAU = 6.2831855f;
                public static float AngleBetweenPositions2D(Vector2 pos1, Vector2 pos2) {
                    float angle = Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * 180 / Mathf.PI;
                    return angle;
                }
                public static float AngleBetweenPositions3D(Vector3 pos1, Vector3 pos2) {
                    Vector3 diffVector = pos2 - pos1;
                    float dot = Vector3.Dot(pos1, diffVector);
                    float mag1 = pos1.magnitude;
                    float mag2 = diffVector.magnitude;

                    float angle = Mathf.Acos(dot / (mag1 * mag2));
                    return angle;
                }
                public static Vector3 ConvertToVector3(float theta) {
                    Vector3 vec = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
                    return vec;
                }
                public static float Average(List<float> collection) {
                    float total = 0;
                    for (int i = 0; i < collection.Count; i++)
                    {
                        total += collection[i];
                    }
                    float avg = total / collection.Count;

                    return avg;
                }
                public static float Average(float[] collection) {
                    float total = 0;
                    for (int i = 0; i < collection.Length; i++)
                    {
                        total += collection[i];
                    }
                    float avg = total / collection.Length;

                    return avg;
                }
                public static float Sigmoid(float input) { return 1 / (1 + Mathf.Exp(-input)); }
                public static float Normalize(float input, float min, float max) { return (input - min) / (max - min); }
                public static Vector3 NormalizeVector3ByScale(Vector3 input, float scale) {
                    // normalize each element multiply by scale factor
                    var _ratio = input.magnitude / scale;
                    return new Vector3(input.x * _ratio, input.y * _ratio, 0);
                }
            }
        }
        namespace CollectionHelpers {
            using UnityEngine;
            public class Collections {
                public static float FindLowest(List<float> list, int numVisible = -1) {
                    float lowest = Mathf.Infinity;
                    if (numVisible <= 0) numVisible = list.Count;
                    for (int i = Mathf.Max(list.Count - numVisible, 0); i < list.Count; i++) if (list[i] < lowest) lowest = list[i];
                    return lowest;
                }
                public static float FindLowest(float[] list, int numVisible = -1) {
                    float lowest = Mathf.Infinity;
                    if (numVisible <= 0) numVisible = list.Length;
                    for (int i = Mathf.Max(list.Length - numVisible, 0); i < list.Length; i++) if (list[i] < lowest) lowest = list[i];
                    return lowest;
                }
                public static float FindHighest(float[] list, int numVisible = -1) {
                    float highest = 0;
                    if (numVisible <= 0) numVisible = list.Length;
                    for (int i = Mathf.Max(list.Length - numVisible, 0); i < list.Length; i++) if (list[i] > highest) highest = list[i];
                    return highest;
                }
                public static float FindHighest(List<float> list, int numVisible = -1) {
                    float highest = 0;
                    if (numVisible <= 0) numVisible = list.Count;
                    for (int i = Mathf.Max(list.Count - numVisible, 0); i < list.Count; i++) if (list[i] > highest) highest = list[i];
                    return highest;
                }
                public static float FindMedian(float[] list, int numVisible = -1) {
                    if (numVisible <= list.Length) {
                        Array.Sort(list);
                        var idx = list.Length - numVisible;
                        if (idx <= 0) idx = list.Length;
                        var i = idx / 2;
                        return list[i];
                    }
                    else return 0;

                }
                public static float FindMedian(List<float> list, int numVisible = -1) {
                    var sorted = list.OrderBy(i => i);
                    var med = 0f;
                    if (numVisible <= list.Count) {
                        if (numVisible <= 0) numVisible = list.Count;
                        var idx = list.Count - numVisible;
                        for (int i = idx; i < list.Count; i++) med += list[i];
                        med /= numVisible;
                        return med;
                    }
                    else return 0;
                }
                public static int GetFirstNullIndex<T>(T[] hold)
                {
                    for (int i = 0; i < hold.Length; i++) if (hold[i] == null) return i;
                    return -1;
                }
                public static int GetLastNonNullIndex<T>(T[] hold)
                {
                    for (int i = hold.Length - 1; i < 0; i++) if (hold[i] != null) return i;
                    return -1;
                }
            }
        }
    }
}