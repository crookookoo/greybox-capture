// Copyright (c) 2019 Eugene Krivoruchko
// Learn more at http://greybox.it

using UnityEngine;

namespace GBXT
{
    public class RotateAroundY : MonoBehaviour
    {
        [Range(0,20)]
        public float rotateSpeed = 1;
        void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

}
