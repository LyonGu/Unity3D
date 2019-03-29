using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SwapCubemaps1 : MonoBehaviour {


	public Cubemap cubeA;
	public Cubemap cubeB;

	public Transform posA;
	public Transform posB;

	private Material curMat;
	private Cubemap curCube;

	// Use this for initialization
	void Awake () {
        Renderer renderer = this.GetComponent<Renderer>();
        curMat = renderer.sharedMaterial;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(curMat)
		{
			curCube = CheckProbeDistance();
			curMat.SetTexture("_Cubemap", curCube);
			
		}
	}


	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		
		if(posA)
		{
			Gizmos.DrawWireSphere(posA.position, 0.5f);
		}
		
		if(posB)
		{
			Gizmos.DrawWireSphere(posB.position, 0.5f);
		}
	}

	private Cubemap CheckProbeDistance()
	{
		float distA = Vector3.Distance(transform.position, posA.position);
		float distB = Vector3.Distance(transform.position, posB.position);
		
		if(distA < distB)
		{
			return cubeA;
		}
		else if(distB < distA)
		{
			return cubeB;
		}
		else
		{
			return cubeA;
		}
		
	}


}
