using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitControl
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance;
        public Transform SelectionArea;

        public Mesh SelectCircleMesh;
        public Material SelectCircleMaterial;

        private void Awake()
        {
            Instance = this;
        }

        public void SetSelectionAreaPosition(Vector3 position)
        {
            SelectionArea.position = new Vector3(position.x, SelectionArea.position.y, position.z);
        }

        public void SetSelectionAreaScale(Vector3 scale)
        {
            SelectionArea.localScale = new Vector3(scale.x, 0, scale.z);
        }
    }
}