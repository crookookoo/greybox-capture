using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GBXT
{
    public class RotateAroundY : MonoBehaviour
    {
        [Range(0,20)]
        public float rotateSpeed = 1;
        void Start()
        {
        
        }

        void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

}
