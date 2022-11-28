using Units.Movement;
using Units.Movement.Behaviours;
using UnityEngine;

namespace Controls
{
    public class RtsController : MonoBehaviour
    {
        private Ray _ray;
        private RaycastHit _hitData;

        private SelectedDictionary _selectedDictionary;
        private bool _dragSelect;
        private Vector3 _selectStart;

        private static readonly int[] Triangles = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };
        private static readonly int SelectableLayer = 3;
        private static readonly int LayerMask = (1 << SelectableLayer);
        void Start()
        {
            _selectedDictionary = GetComponent<SelectedDictionary>();
            _dragSelect = false;
        }
    
        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _selectStart = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                if ((_selectStart - Input.mousePosition).magnitude > 40)
                {
                    _dragSelect = true;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_dragSelect == false)
                {
                    _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(_ray, out _hitData, 50000.0f, LayerMask))
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            _selectedDictionary.AddSelected(_hitData.transform.gameObject);
                        }
                        else
                        {
                            _selectedDictionary.DeselectAll();
                            _selectedDictionary.AddSelected(_hitData.transform.gameObject);
                        }
                    }
                    else
                    {
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            _selectedDictionary.DeselectAll();
                        }
                    }
                }
                else
                {
                    Vector2[] corners = GenerateBoundingBox(_selectStart, Input.mousePosition);
                    Vector3[] bottom = new Vector3[4];
                    Vector3[] top = new Vector3[4];

                    int i = 0;

                    var mainCamera = Camera.main;
                    foreach (Vector2 corner in corners)
                    {
                        Ray ray = mainCamera!.ScreenPointToRay(corner);

                        if (Physics.Raycast(ray, out _hitData, 50000.0f, LayerMask))
                        {
                            bottom[i] = new Vector3(_hitData.point.x, _hitData.point.y, _hitData.point.z);
                            top[i] = ray.origin - _hitData.point;
                        }
                        i++;
                    }
                    
                    //generate the mesh
                    Mesh selectionMesh = GenerateSelectionMesh(bottom,top);

                    MeshCollider selectionBox = gameObject.AddComponent<MeshCollider>();
                    selectionBox.sharedMesh = selectionMesh;
                    selectionBox.convex = true;
                    selectionBox.isTrigger = true;

                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        _selectedDictionary.DeselectAll();
                    }

                    Destroy(selectionBox, 0.02f);
                }

                _dragSelect = false;
            }

            if (Input.GetMouseButton(1))
            {
                _ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _hitData, 50000.0f, LayerMask))
                {
                    foreach (var agent in _selectedDictionary.GetSelectedObjects().Values)
                    {
                        if (agent.GetComponent<Agent>() != null)
                        {
                            if (agent.GetComponent<Seek>() == null)
                            {
                                agent.AddComponent<Seek>().target = _hitData.point;
                            }
                            else
                            {
                                agent.GetComponent<Seek>().SetTarget(_hitData.point);
                            }
                        }
                    }
                }
            }
        }
        
        void OnGUI()
        {
            if (_dragSelect)
            {
                var rect = Utils.GetScreenRect(_selectStart, Input.mousePosition);
                Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }
        
        Vector2[] GenerateBoundingBox(Vector2 p1,Vector2 p2)
        {
            // Min and Max to get 2 corners of rectangle regardless of drag direction.
            var bottomLeft = Vector3.Min(p1, p2);
            var topRight = Vector3.Max(p1, p2);

            // 0 = top left; 1 = top right; 2 = bottom left; 3 = bottom right;
            Vector2[] corners =
            {
                new (bottomLeft.x, topRight.y),
                new (topRight.x, topRight.y),
                new (bottomLeft.x, bottomLeft.y),
                new (topRight.x, bottomLeft.y)
            };
            return corners;

        }

        Mesh GenerateSelectionMesh(Vector3[] bottom, Vector3[] top)
        {
            Vector3[] vertices = new Vector3[8];

            for(int i = 0; i < 8; i++)
            {
                vertices[i] = (i < 4) ? bottom[i] : bottom[i - 4] + top[i - 4];
            }

            Mesh selectionMesh = new Mesh();
            selectionMesh.vertices = vertices;
            selectionMesh.triangles = Triangles;

            return selectionMesh;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            _selectedDictionary.AddSelected(other.gameObject);
        }
    }
}
