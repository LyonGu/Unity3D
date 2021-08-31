using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class WaypointBlobAssetAuthoring_Done : MonoBehaviour, IDeclareReferencedPrefabs {

    public Transform[] transformArray;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        for (int i = 0; i < transformArray.Length; i++) {
            referencedPrefabs.Add(transformArray[i].gameObject);
        }
    }

    private void OnDrawGizmos() {
        if (transformArray != null) {
            for (int i = 0; i < transformArray.Length; i++) {
                Transform transform = transformArray[i];
                if (transform == null) return;
                Gizmos.DrawWireCube(transform.position, Vector3.one * .3f);
                if (i > 0) {
                    Gizmos.DrawLine(transform.position, transformArray[i-1].position);
                }
                if (i == transformArray.Length - 1) {
                    Gizmos.DrawLine(transform.position, transformArray[0].position);
                }
            }
        }
    }

}
