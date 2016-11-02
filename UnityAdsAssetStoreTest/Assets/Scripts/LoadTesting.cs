using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class LoadTesting : MonoBehaviour
{
	private List<byte[]> memoryConsumingArrays = new List<byte[]> ();
	private long mbAllocated = 0;

	const int mbToAllocate = 100;
	const int bytesToAllocate = mbToAllocate * 1024 * 1024;

	public void Allocate100MB ()
	{
		mbAllocated += mbToAllocate;

		var memoryConsumingArray = new byte[bytesToAllocate];
		memoryConsumingArrays.Add (memoryConsumingArray);

		UIController.Instance.Log (string.Format("Allocated {0} MB in total", mbAllocated));
	}

	public void ToggleCPULoad ()
	{
		var prefab = Resources.Load ("UnityBall");
		for (int i = 0; i < 50; i++)
		{
			Instantiate (
				prefab, 
				new Vector2 (UnityEngine.Random.Range (-2f, 2f), UnityEngine.Random.Range (0f, 2f)),
				Quaternion.identity);
		}
		StartCoroutine (StartCpuIntensiveOperation ());
	}

	private IEnumerator StartCpuIntensiveOperation()
	{
		while (true)
		{
			for (int i = 0; i < memoryConsumingArrays.Count; i++)
			{
				for (int j = 0; j < bytesToAllocate; j++)
				{
					memoryConsumingArrays[i][j] = (byte)UnityEngine.Random.Range (0, 255);

					if (j % (bytesToAllocate / 20) == 0)
					{
						yield return null;
					}
				}
				UIController.Instance.Log (string.Format("Wrote random bytes to memory block #{0}", i));
				yield return null;
			}
			yield return null;
		}
	}
}
