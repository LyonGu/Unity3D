using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Testing : MonoBehaviour {

    private void Start() {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = entityManager.CreateEntity();

        Application.targetFrameRate = -1;
        //DynamicBuffer<IntBufferElement> dynamicBuffer = entityManager.AddBuffer<IntBufferElement>(entity);
        
        entityManager.AddBuffer<IntBufferElement>(entity);
        DynamicBuffer<IntBufferElement> dynamicBuffer = entityManager.GetBuffer<IntBufferElement>(entity);

        dynamicBuffer.Add(new IntBufferElement { Value = 1 });
        dynamicBuffer.Add(new IntBufferElement { Value = 2 });
        dynamicBuffer.Add(new IntBufferElement { Value = 3 });

        DynamicBuffer<int> intDynamicBuffer = dynamicBuffer.Reinterpret<int>();
        intDynamicBuffer[1] = 5;

        for (int i = 0; i < intDynamicBuffer.Length; i++) {
            intDynamicBuffer[i]++;
        }

    }
}
