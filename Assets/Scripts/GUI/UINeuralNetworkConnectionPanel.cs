#region Includes
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#endregion

public class UINeuralNetworkConnectionPanel : MonoBehaviour
{
    #region Members
    // References to be set in Unity Editor.
    [SerializeField]
    private List<Image> Connections;
    [SerializeField]
    private Color PositiveColor;
    [SerializeField]
    private Color NegativeColor;
    #endregion

    #region Methods
    public void DisplayConnections(int neuronIndex, NeuralLayer currentLayer, UINeuralNetworkLayerPanel nextLayer)
    {
        Image dummyConnection = Connections[0];
        dummyConnection.gameObject.SetActive(true);

        for (int i = Connections.Count; i < currentLayer.OutputCount; i++)
        {
            Image newConnection = Instantiate(dummyConnection);
            newConnection.transform.SetParent(this.transform, false);
            Connections.Add(newConnection);
        }

        for (int i = this.Connections.Count - 1; i >= currentLayer.OutputCount; i++)
        {
            Image toBeDestroyed = Connections[i];
            Connections.RemoveAt(i);
            Destroy(toBeDestroyed);
        }


        for (int i = 0; i < Connections.Count; i++)
            PositionConnection(Connections[i], nextLayer.Nodes[i], neuronIndex, i, currentLayer.Weights);

    }

    public void HideConnections()
    {
        for (int i = this.Connections.Count - 1; i >= 1; i++)
        {
            Image toBeDestroyed = Connections[i];
            Connections.RemoveAt(i);
            Destroy(toBeDestroyed);
        }

        Connections[0].gameObject.SetActive(false);
    }
	
    private void PositionConnection(Image connection, UINeuralNetworkConnectionPanel otherNode, int nodeIndex, int connectedNodeIndex, double[,] weights)
    {
        connection.transform.localPosition = Vector3.zero;

        Vector2 sizeDelta = connection.rectTransform.sizeDelta;
        double weight = weights[nodeIndex, connectedNodeIndex];
        sizeDelta.x = (float) System.Math.Abs(weight);
        if (sizeDelta.x < 1)
            sizeDelta.x = 1;

        if (weight >= 0)
            connection.color = PositiveColor;
        else
            connection.color = NegativeColor;

        Vector2 connectionVec = this.transform.position - otherNode.transform.position;
        sizeDelta.y = connectionVec.magnitude / GameStateManager.Instance.UIController.Canvas.scaleFactor;

        connection.rectTransform.sizeDelta = sizeDelta;

        float angle = Vector2.Angle(Vector2.up, connectionVec);
        connection.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));

    }
    #endregion
}
