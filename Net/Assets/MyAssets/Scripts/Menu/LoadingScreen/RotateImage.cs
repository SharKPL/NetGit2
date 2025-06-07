using UnityEngine;

namespace MUSOAR
{
    public class RotateImage : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 100f;
        
        private void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}
