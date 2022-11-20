using System.Collections.Generic;
using UnityEngine;

namespace Controls
{
    public class SelectedDictionary : MonoBehaviour
    {
        private readonly Dictionary<int, GameObject> _selectedObjects = new Dictionary<int, GameObject>();

        public void AddSelected(GameObject go)
        {
            int id = go.GetInstanceID();

            if (!_selectedObjects.ContainsKey(id))
            {
                if (!go.tag.Equals("Ground"))
                {
                    _selectedObjects.Add(id, go);
                    go.AddComponent<SelectionComponent>();
                }
            }
        }

        public void Deselect(int id)
        {
            _selectedObjects.Remove(id);
        }

        public void DeselectAll()
        {
            foreach (KeyValuePair<int,GameObject> pair in _selectedObjects)
            {
                if (pair.Value != null)
                {
                    Component.Destroy(_selectedObjects[pair.Key].GetComponent<SelectionComponent>());
                }
            }
            _selectedObjects.Clear();
        }
    }
}
