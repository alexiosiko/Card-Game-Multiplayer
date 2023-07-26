using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Players : MonoBehaviour
{
	public static Players Singleton;
	void Awake()
	{
		Singleton = this;
	}
}