using System;
using System.Collections.Generic;
using System.Linq;
using Commands;
using EventBus;
using Events;
using Units;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private new Camera camera;
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private LayerMask selectableUnitsLayers;
        [SerializeField] private LayerMask floorLayers;
        [SerializeField] private RectTransform selectionBox;

        private Vector2 startingMousePosition;

        private ActionBase activeAction;
        private bool wasMouseDownOnUi;
        private CinemachineFollow _cinemachineFollow;
        private float _zoomStartTime;
        private float _rotationStartTime;
        private Vector3 _startingFollowOffset;
        private float maxRotationAmount;
        private HashSet<AUnit> aliveUnits = new(100);
        private HashSet<AUnit> addedUnits = new(24);
        private List<ISelectable> selectedUnits = new(12);

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out _cinemachineFollow))
            {
                Debug.LogError("Cinemachine Cmera did not hve CinemachineFloowo! No Zoom working!");
            }

            _startingFollowOffset = _cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(_cinemachineFollow.FollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
            Bus<ActionSelectedEvent>.OnEvent += HandleActionSelected;
        }


        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
            Bus<ActionSelectedEvent>.OnEvent -= HandleActionSelected;
        }

        private void HandleUnitSelected(UnitSelectedEvent args) => selectedUnits.Add(args.Unit);

        private void HandleUnitDeselected(UnitDeselectedEvent args) => selectedUnits.Remove(args.Unit);

        private void HandleUnitSpawn(UnitSpawnEvent args) => aliveUnits.Add(args.Unit);

        private void HandleActionSelected(ActionSelectedEvent evt)
        {
            activeAction = evt.Action;
            if (!activeAction.RequiresClickToActivate)
            {
                ActivateAction(new RaycastHit());
            }
        }


        // Update is called once per frame
        void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if (selectionBox == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDown();
            }
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDrag();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseUp()
        {
            if (!wasMouseDownOnUi && activeAction == null && !Keyboard.current.shiftKey.isPressed)
                DeselectAllUnits();
            HandleLeftClick();
            foreach (AUnit unit in addedUnits)
            {
                unit.Select();
            }

            selectionBox.gameObject.SetActive(false);
        }

        private void HandleMouseDrag()
        {
            if (activeAction != null || wasMouseDownOnUi) return;

            Bounds selectionBoxBounds = ResizeSelectionBox();
            foreach (AUnit unit in aliveUnits)
            {
                Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
                if (selectionBoxBounds.Contains(unitPosition))
                {
                    addedUnits.Add(unit);
                }
            }
        }

        private void HandleMouseDown()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
            startingMousePosition = Mouse.current.position.ReadValue();
            addedUnits.Clear();
            wasMouseDownOnUi = EventSystem.current.IsPointerOverGameObject();
        }

        private void DeselectAllUnits()
        {
            ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
            foreach (ISelectable selectable in currentlySelectedUnits)
            {
                selectable.Deselect();
            }
        }

        private Bounds ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float width = mousePosition.x - startingMousePosition.x;
            float height = mousePosition.y - startingMousePosition.y;

            selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

            return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
        }

        private void HandleRightClick()
        {
            if (selectedUnits.Count == 0) return;

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.wasReleasedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
            {
                List<AUnit> abstractUnits = new(selectedUnits.Count);
                foreach (ISelectable selectable in selectedUnits)
                {
                    if (selectable is AUnit unit)
                    {
                        abstractUnits.Add(unit);
                    }
                }


                for (int i = 0; i < abstractUnits.Count; i++)
                {
                    CommandContext context = new(abstractUnits[i], hit, i);

                    foreach (ICommand command in abstractUnits[i].AvailableCommands)
                    {
                        if (command.CanHandle(context))
                        {
                            command.Handle(context);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleLeftClick()
        {
            if (camera == null) return;

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (activeAction == null
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
            }
            else if (activeAction != null
                     && !EventSystem.current.IsPointerOverGameObject()
                     && Physics.Raycast(cameraRay, out hit, float.MaxValue, floorLayers))
            {
                ActivateAction(hit);
            }
        }

        private void ActivateAction(RaycastHit hit)
        {
            List<ACommandable> abstractCommandables = selectedUnits
                .Where((unit) => unit is ACommandable)
                .Cast<ACommandable>()
                .ToList();
            for (int i = 0; i < abstractCommandables.Count; i++)
            {
                CommandContext context = new(abstractCommandables[i], hit, i);
                activeAction.Handle(context);
            }

            activeAction = null;
        }

        private void HandleRotation()
        {
            if (ShouldSetRotationStartTime())
            {
                _rotationStartTime = Time.time;
            }

            float rotationTime = Mathf.Clamp01((Time.time - _rotationStartTime) * _cameraConfig.RotationSpeed);
            Vector3 targetFollowOffset;

            if (Keyboard.current.pageDownKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0);
            }
            else if (Keyboard.current.pageUpKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    -maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0);
            }
            else
            {
                targetFollowOffset = new Vector3(
                    _startingFollowOffset.x,
                    _cinemachineFollow.FollowOffset.y,
                    _startingFollowOffset.z
                );
            }

            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                _cinemachineFollow.FollowOffset,
                targetFollowOffset,
                rotationTime
            );
        }

        private bool ShouldSetRotationStartTime()
        {
            return Keyboard.current.pageUpKey.wasPressedThisFrame
                   || Keyboard.current.pageDownKey.wasPressedThisFrame
                   || Keyboard.current.pageUpKey.wasReleasedThisFrame
                   || Keyboard.current.pageDownKey.wasReleasedThisFrame;
        }

        private void HandleZooming()
        {
            if (ShouldSetZoomStartTime())
            {
                _zoomStartTime = Time.time;
            }

            Vector3 targetFollowOffset;
            float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * _cameraConfig.ZoomSpeed);

            if (Keyboard.current.endKey.isPressed)
            {
                targetFollowOffset = new(
                    _cinemachineFollow.FollowOffset.x,
                    _cameraConfig.MinZoomDistance,
                    _cinemachineFollow.FollowOffset.z
                );
            }
            else
            {
                targetFollowOffset = new(
                    _cinemachineFollow.FollowOffset.x,
                    _startingFollowOffset.y,
                    _cinemachineFollow.FollowOffset.z
                );
            }

            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                _cinemachineFollow.FollowOffset,
                targetFollowOffset,
                zoomTime);
        }

        private static bool ShouldSetZoomStartTime()
        {
            return Keyboard.current.endKey.wasPressedThisFrame || Keyboard.current.endKey.wasReleasedThisFrame;
        }

        private void HandlePanning()
        {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
            // cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (!_cameraConfig.EnableEdgePan)
                return moveAmount;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            int screenWidth = Screen.width; // 1920
            int screenHeight = Screen.height; // 1080

            if (mousePosition.x <= _cameraConfig.EdgePanSize)
                moveAmount.x -= _cameraConfig.MousePanSpeed;
            else if (mousePosition.x >= screenWidth - _cameraConfig.EdgePanSize)
                moveAmount.x += _cameraConfig.MousePanSpeed;

            if (mousePosition.y <= _cameraConfig.EdgePanSize)
                moveAmount.y -= _cameraConfig.MousePanSpeed;
            else if (mousePosition.y >= screenHeight - _cameraConfig.EdgePanSize)
                moveAmount.y += _cameraConfig.MousePanSpeed;


            return moveAmount;
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += _cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= _cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= _cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += _cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }
    }
}