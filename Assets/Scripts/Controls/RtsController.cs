using Menus;
using Units.Movement;
using Units.Movement.Behaviours;
using Unity.VisualScripting;
using UnityEngine;

namespace Controls
{
    public class RtsController : MonoBehaviour
    {
        private Ray _ray;
        private RaycastHit _hitData;

        private SelectedDictionary _selectedDictionary;
        private MenuManager _menuManager;
        private bool _dragSelect;
        private Vector3 _selectStart;
        
        [SerializeField] private GameObject platform;
        [SerializeField] private GameObject unit;
        private float buildingTimeout = 0.0f;
        
        private bool _isBuilding;

        private static readonly int[] Triangles = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };
        private static readonly int SelectableLayer = 3;
        private static readonly int SelectableLayerMask = (1 << SelectableLayer);
        void Start()
        {
            _selectedDictionary = GetComponent<SelectedDictionary>();
            _menuManager = GetComponent<MenuManager>();
            _dragSelect = false;
        }
    
        // Update is called once per frame
        void Update()
        {
            // Start Drag Select
            if (Input.GetMouseButtonDown(0))
            {
                _selectStart = Input.mousePosition;
            }

            // Determine if we are dragging
            if (Input.GetMouseButton(0))
            {
                if ((_selectStart - Input.mousePosition).magnitude > 40)
                {
                    _dragSelect = true;
                }
            }

            // End Drag Select
            if (Input.GetMouseButtonUp(0))
            {
                if (_isBuilding)
                {
                    if (buildingTimeout < Time.time)
                    {
                        _isBuilding = false;
                        _ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(_ray, out _hitData, 50000.0f, SelectableLayerMask))
                        {
                            Instantiate(platform, _hitData.point, Quaternion.identity);
                        }
                    }

                    return;
                }
                
                // If we are not dragging, select a single point
                if (_dragSelect == false)
                {
                    _ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
                    // If we hit a selectable object
                    if (Physics.Raycast(_ray, out _hitData, 50000.0f, SelectableLayerMask))
                    {
                        if (_hitData.transform.CompareTag("Building"))
                        {
                            _menuManager.UnitMenu(unit, _hitData.transform.position);
                        }
                        
                        // Add it to the selected dictionary if pressing shift, otherwise clear the dictionary and add it
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
                        // If we didn't hit a selectable object and we are not pressing shift, deselect all
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            _selectedDictionary.DeselectAll();
                        }
                    }
                }
                else
                {
                    // If we are dragging, create a box and select all objects within it
                    Vector2[] corners = GenerateBoundingBox(_selectStart, Input.mousePosition);
                    Vector3[] bottom = new Vector3[4];
                    Vector3[] top = new Vector3[4];

                    int i = 0;

                    var mainCamera = Camera.main;
                    // Generate the bottom and top corners of the box based on the corners of the screen
                    foreach (Vector2 corner in corners)
                    {
                        Ray ray = mainCamera!.ScreenPointToRay(corner);

                        if (Physics.Raycast(ray, out _hitData, 50000.0f, SelectableLayerMask))
                        {
                            bottom[i] = new Vector3(_hitData.point.x, _hitData.point.y, _hitData.point.z);
                            top[i] = ray.origin - _hitData.point;
                        }
                        i++;
                    }
                    
                    //generate the mesh
                    Mesh selectionMesh = GenerateSelectionMesh(bottom,top);

                    // Collision will eventually trigger the selection of the object
                    MeshCollider selectionBox = gameObject.AddComponent<MeshCollider>();
                    selectionBox.sharedMesh = selectionMesh;
                    selectionBox.convex = true;
                    selectionBox.isTrigger = true;

                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        _selectedDictionary.DeselectAll();
                    }

                    Destroy(selectionBox, 0.02f);
                    _dragSelect = false;
                }
            }

            // If we right click, move the selected units
            if (Input.GetMouseButton(1))
            {
                _ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _hitData, 50000.0f, SelectableLayerMask))
                {
                    // If we hit somenthing else than the ground, make the units attack it
                    bool shouldAttack = !_hitData.transform.GameObject().CompareTag("Ground");

                    foreach (var agent in _selectedDictionary.GetSelectedObjects().Values)
                    {
                        if (agent.GetComponent<Agent>() != null)
                        {
                            if (shouldAttack)
                            {
                                Destroy(agent.GetComponent<SeekBehaviour>());
                                if (agent.GetComponent<AttackBehaviour>() == null)
                                {
                                    agent.AddComponent<AttackBehaviour>().SetTarget(_hitData.transform.gameObject);
                                }
                                else
                                {
                                    agent.GetComponent<AttackBehaviour>().SetTarget(_hitData.transform.gameObject);
                                }
                            }
                            else
                            {
                                Destroy(agent.GetComponent<AttackBehaviour>());
                                if (agent.GetComponent<SeekBehaviour>() == null)
                                {
                                    agent.AddComponent<SeekBehaviour>().SetTargetPosition(_hitData.point);
                                }
                                else
                                {
                                    agent.GetComponent<SeekBehaviour>().SetTargetPosition(_hitData.point);
                                }
                            }
                        }
                    }
                    
                    
                }
                
            }

            if (Input.GetKey(KeyCode.B))
            {
                _menuManager.BuildMenu();
            }
            
            if (Input.GetKey(KeyCode.Escape))
            {
                _menuManager.CloseBuildMenu();
                _isBuilding = false;
                _selectedDictionary.DeselectAll();
            }
        }
        
        void OnGUI()
        {
            if (_dragSelect && !_isBuilding)
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

        public void SetPlatformConstruction()
        {
            _isBuilding = true;
            _selectedDictionary.DeselectAll();
            buildingTimeout = Time.time + 0.5f;
        }
    }
}
