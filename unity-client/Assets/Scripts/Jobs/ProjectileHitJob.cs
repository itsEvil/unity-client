using Game.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs {
    public class ProjectileHitJob : IDisposable {
        private readonly Transform[] Transforms;
        private readonly Entity[] Entities;
        private TransformAccessArray TransformAccessArray;
        private NativeArray<float> Distances;
        private NativeArray<bool> WithinHitDistances;
        private readonly Transform To;
        private readonly float HitDistance;
        public ProjectileHitJob(Dictionary<int, Entity> entities, Transform to, float hitDistance) {
            To = to;
            Transforms = new Transform[entities.Count];
            Entities = new Entity[entities.Count];
            int index = 0;
            foreach(var (_, entity) in entities) {
                Entities[index] = entity;
                Transforms[index++] = entity.Transform;
            }

            TransformAccessArray = new TransformAccessArray(Transforms);
            Distances = new(Transforms.Length, Allocator.Persistent);
            WithinHitDistances = new(Transforms.Length, Allocator.Persistent);
        }
        public Result Execute() {
            TransformDistanceCheckJob job = new TransformDistanceCheckJob() {
                Distances = Distances,
                WithinHitDistances = WithinHitDistances,
                ToPosition = To.position,
                MinDistance = 0.4f,
            };
            JobHandle jobHandle = job.Schedule(TransformAccessArray);
            jobHandle.Complete();

            float minDistance = float.MaxValue;
            int minIndex = 0;
            for(int i = 0; i < Distances.Length; i++) {
                if (!job.WithinHitDistances[i])
                    continue;

                if (job.Distances[i] < minDistance) {
                    minDistance = job.Distances[i];
                    minIndex = i;
                }
            }

            return new Result(Entities[minIndex], minDistance, job.WithinHitDistances[minIndex]);
        }
        public void Dispose() {
            Distances.Dispose();
            WithinHitDistances.Dispose();
        }
        public readonly struct Result {
            public readonly Entity Closest;
            public readonly float Distance;
            public readonly bool WithinHit;
            public Result(Entity ent, float distance, bool within) {
                Closest = ent;
                Distance = distance;
                WithinHit = within;
            }
        }
        [BurstCompile]
        struct TransformDistanceCheckJob : IJobParallelForTransform {
            [WriteOnly] public NativeArray<float> Distances;
            [WriteOnly] public NativeArray<bool> WithinHitDistances;
            [ReadOnly] public Vector3 ToPosition;
            [ReadOnly] public float MinDistance;
            public void Execute(int i, TransformAccess transform) {
                Distances[i] = Vector3.Distance(ToPosition, transform.position);
                if (math.abs(ToPosition.x - transform.position.x) <= MinDistance &&
                   math.abs(ToPosition.y - transform.position.y) <= MinDistance) {
                    WithinHitDistances[i] = true;
                }
                else {
                    WithinHitDistances[i] = false;
                }
            }
        }
    }
}
