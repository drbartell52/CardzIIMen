using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameboard;
using static Gameboard.DataTypes;
using Gameboard.Objects;
using System.Linq;
using Gameboard.Tools;
namespace Gameboard.Examples
{
    public class NewTouchControllerExample : MonoBehaviour
    {
        public Text LastFound;
        public Text LastUpdated;
        public Text LastDeleted;
        public Text CurrentShapes;
        public RectTransform content;
        private TouchController touchController;
        private Dictionary<uint, GameObject> boardObjectDict = new Dictionary<uint, GameObject>();

        private Gameboard gameboard;

        void Start()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            gameboard = gameboardObject.GetComponent<Gameboard>();
            touchController = gameboardObject.GetComponent<TouchController>();

            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
            touchController.OnShapeFound += OnShapeFound;
            touchController.OnShapeUpdated += OnShapeUpdated;
            touchController.OnShapeLost += OnShapeLost;
            touchController.AllShapesLost += AllShapesLost;
        }

        private void OnDestroy()
        {
            GameboardLogging.Warning($"TouchController: {touchController}");
            touchController.OnShapeFound -= OnShapeFound;
            touchController.OnShapeUpdated -= OnShapeUpdated;
            touchController.OnShapeLost -= OnShapeLost;
            touchController.AllShapesLost -= AllShapesLost;
        }

        void OnGameboardShutdown()
        {
            this.OnDestroy();
        }

        void OnShapeFound(GameboardShape shape)
        {
            LastFound.text = $"LastFound: {shape.id}";
            LastFound.text += $" shapeType: {shape.shapeType}";
        }

        void OnShapeUpdated(GameboardShape shape)
        {
            LastUpdated.text = $"LastUpdated: {shape.id}";
            LastUpdated.text += $" shapeType: {shape.shapeType}";
            UpdateContour(shape);
            displayCurrentShapes();
        }

        private void displayCurrentShapes()
        {
            CurrentShapes.text = "Shapes Objects:\n";
            var shapes = touchController.GetShapes();

            foreach (GameboardShape shape in shapes)
            {
                CurrentShapes.text += $"{shape.shapeType} with Session ID {shape.id}: at Gameboard Coordinates {shape.screenPosition} and World Position {shape.GetWorldPosition()} \n";
            }
        }

        void OnShapeLost(GameboardShape shape)
        {
            LastDeleted.text = $"LastLost: {shape.id}";
            LastDeleted.text += $" shapeType: {shape.shapeType}";

            if (boardObjectDict.ContainsKey(shape.id))
            {
                Destroy(boardObjectDict[shape.id]);
                boardObjectDict.Remove(shape.id);
            }
        }

        void AllShapesLost()
        {
            CurrentShapes.text = "Shapes Objects:\n";
        }

        public void RemoveTokenPairing()
        {
            touchController.UnpairAllPairedPieces();
        }

        private void UpdateContour(GameboardShape shape)
        {
            GameObject lineRendererObject;
            if (!boardObjectDict.ContainsKey(shape.id))
            {
                lineRendererObject = new GameObject("DrawContourLine", typeof(LineRenderer));

                boardObjectDict.Add(shape.id, lineRendererObject);
            }
            else
            {
                lineRendererObject = boardObjectDict[shape.id];
            }

            var lineRenderer = lineRendererObject.GetComponent<LineRenderer>();
            lineRenderer.transform.SetParent(content, false);
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.loop = true;

            var contourWorldVectors3D = shape.GetContourWorldVectors(Camera.main.transform.position.y);

            lineRenderer.positionCount = contourWorldVectors3D.Length;
            for (int i = 0; i < contourWorldVectors3D.Length; i++)
            {
                lineRenderer.SetPosition(i, contourWorldVectors3D[i]);
            }
        }
    }
}