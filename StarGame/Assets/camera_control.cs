﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_control : MonoBehaviour {
    private Camera m_camera;
    float y = 0;
    // Use this for initialization

    private void Awake()
    {
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        y += Time.deltaTime * 10;
        transform.rotation = Quaternion.Euler(0, y, 0);

    }
}