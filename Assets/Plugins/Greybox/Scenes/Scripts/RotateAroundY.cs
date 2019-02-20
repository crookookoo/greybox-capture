using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GBXT
{
    public class RotateAroundY : MonoBehaviour
    {
        [Range(0,10)]
        public float rotateSpeed = 1;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

}
