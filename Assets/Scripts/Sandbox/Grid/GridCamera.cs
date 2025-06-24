using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public enum CameraMoveDir
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    public enum CameraZoom
    {
        INZOOM,
        OUTZOOM
    }


    public class GridCamera : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _camera;
        public Camera Camera => _camera;

        [SerializeField] private float _zPlane = -10f;

        private GridManager _gridManager;


        [Header("Controls")]
        private Bounds _cameraBounds;
        private Vector2 _movementUnits;

        [SerializeField] float _movementIntervalDuration;
        private float _movementIntervalTimer;

        private const string LogChannel = "[GridCamera]";

        [SerializeField] private List<float> zoomValues = new();
        private int _currentZoomIndex;

        public void Init(GridManager gridManager)
        {
            _gridManager = gridManager;

            CalculateBounds();
            CalculateMovementUnits();
            CenterCamera();
            _currentZoomIndex = 2;
            _camera.orthographicSize = zoomValues[_currentZoomIndex];
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.A) && Input.GetKeyUp(KeyCode.D) && Input.GetKeyUp(KeyCode.S) && Input.GetKeyUp(KeyCode.W))
            {
                _movementIntervalTimer = 0f;
            }

            _movementIntervalTimer -= Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
            {
                MoveCamera(CameraMoveDir.LEFT);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                MoveCamera(CameraMoveDir.RIGHT);
            }


            if (Input.GetKey(KeyCode.S))
            {
                MoveCamera(CameraMoveDir.DOWN);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                MoveCamera(CameraMoveDir.UP);
            }

            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                UpdateCameraZoom(CameraZoom.INZOOM);
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                UpdateCameraZoom(CameraZoom.OUTZOOM);
            }
        }

        private void CalculateBounds()
        {
            float totalWidth = ((_gridManager.CellScale.x * _gridManager.CellSize.x) + _gridManager.CellGap) * _gridManager.GridSize.x;
            float totalHeight = ((_gridManager.CellScale.y * _gridManager.CellSize.y) + _gridManager.CellGap) * _gridManager.GridSize.y;

            Vector3 center = new Vector3(totalWidth / 2f, -totalHeight / 2f, _zPlane);

            _cameraBounds = new Bounds(center, new Vector3(totalWidth / 2f, totalHeight / 2f, 0f));
        }

        private void CalculateMovementUnits()
        {
            _movementUnits.x = (_gridManager.CellScale.x * _gridManager.CellSize.x) + _gridManager.CellGap;
            _movementUnits.y = (_gridManager.CellScale.y * _gridManager.CellSize.y) + _gridManager.CellGap;
        }

        private void UpdateCameraPosition(float moveX, float moveY)
        {
            Vector3 requestedPosition = new Vector3(this.transform.localPosition.x + moveX, this.transform.localPosition.y + moveY, _zPlane);

            if (_cameraBounds.Contains(requestedPosition))
            {
                this.transform.localPosition = requestedPosition;
                _movementIntervalTimer = _movementIntervalDuration;
            }
            else
            {
                //Camera out of bounds, move to nearest?
            }
        }

        public void MoveCamera(CameraMoveDir dir)
        {
            if (_movementIntervalTimer > 0f)
            {
                return;
            }

            switch (dir)
            {
                case (CameraMoveDir.UP):
                    UpdateCameraPosition(0f, _movementUnits.y);
                    break;
                case (CameraMoveDir.DOWN):
                    UpdateCameraPosition(0f, -_movementUnits.y);
                    break;
                case (CameraMoveDir.LEFT):
                    UpdateCameraPosition(-_movementUnits.x, 0f);
                    break;
                case (CameraMoveDir.RIGHT):
                    UpdateCameraPosition(_movementUnits.x, 0f);
                    break;
                default:
                    break;
            }

        }

        public void FocusCamera(int x, int y)
        {
            this.transform.localPosition = new Vector3(x, -y, _zPlane);
        }

        public void CenterCamera()
        {
            this.transform.localPosition = _cameraBounds.center;
        }

        public void UpdateCameraZoom(CameraZoom zoomDirection)
        {
            switch (zoomDirection)
            {
                case CameraZoom.INZOOM:
                    {
                        if (_currentZoomIndex < zoomValues.Count - 1)
                        {
                            _currentZoomIndex++;
                            //_camera.DOOrthoSize(zoomValues[_currentZoomIndex], 0.4f).SetEase(Ease.OutBack);
                        }
                        break;
                    }
                case CameraZoom.OUTZOOM:
                    {
                        if (_currentZoomIndex > 0)
                        {
                            _currentZoomIndex--;
                            //_camera.DOOrthoSize(zoomValues[_currentZoomIndex], 0.4f).SetEase(Ease.OutBack);;
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(zoomDirection), zoomDirection, null);
            }

            _camera.orthographicSize = zoomValues[_currentZoomIndex];
        }
    }
}
